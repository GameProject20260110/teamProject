using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VContainer;

// 배틀에 필요한 부품을 만들어 연결하고, 턴 진행은 turnOrchestrator에 넘기는 조립 담당
// 게임 규칙을 처리하거나 연출하지 않음
public class CardBattleManager : MonoBehaviour
{
    [Header("드로우 시스템")]
    [SerializeField] private HandLayoutView playerHandLayoutView;
    [SerializeField] private HanddrawConfig drawConfig;

    [Header("멀리건")]
    [SerializeField] private MulliganPanelView mulliganPanelView; // 비워두면 멀리건 생략

    [Header("주사위 굴림 (기본 공격력 결정)")]
    [SerializeField] private DiceRollPanelView diceRollPanelView; // 비워두면 연출 없이 결과만 반영

    [Header("메인 페이즈 (카드 패널 배치)")]
    [SerializeField] private FieldBoardView fieldBoardView;   // 비워두면 배치 페이즈 생략
    [SerializeField] private CardDragView cardDragView;

    [Header("배틀 규칙")]
    [SerializeField] private bool isPlayerFirst = true;
    [SerializeField, Min(0)] private int requiredDeckSize = 20;
    [SerializeField] private bool startOnSceneStart = true;

    private DeckRepository _deckRepository;
    private CardDatabase _cardDatabase;

    private readonly List<CardData> _deckBuffer = new List<CardData>();
    private HandData _playerHand;
    private DrawController _playerDrawController;
    private DrawOrchestrator _playerDrawOrchestrator;
    private MulliganOrchestrator _playerMulliganOrchestrator;
    private DiceRollOrchestrator _playerDiceRollOrchestrator;
    private CardPlacementController _playerPlacementController;
    private CardPlacementOrchestrator _playerPlacementOrchestrator;
    private TurnOrchestrator _turnOrchestrator;
    private CancellationTokenSource _battleCts;

    [Inject]
    public void Construct(DeckRepository deckRepository, CardDatabase cardDatabase)
    {
        _deckRepository = deckRepository;
        _cardDatabase = cardDatabase;
    }

    private void Start()
    {
        if (startOnSceneStart) StartBattleAsync().Forget();
    }

    private void OnDestroy()
    {
        _battleCts?.Cancel();
        _battleCts?.Dispose();
        _battleCts = null;
    }

    public async UniTask StartBattleAsync()
    {
        if (!TrySetUpPlayerDeck()) return;

        _battleCts?.Cancel();
        _battleCts?.Dispose();
        _battleCts = new CancellationTokenSource();

        playerHandLayoutView.ClearHand();
        if (fieldBoardView != null) fieldBoardView.ClearAll();

        _playerDrawOrchestrator = new DrawOrchestrator(_playerDrawController, playerHandLayoutView, drawConfig);
        _playerMulliganOrchestrator = mulliganPanelView != null
            ? new MulliganOrchestrator(_playerHand, new MulliganController(_playerDrawController, _playerHand), playerHandLayoutView, mulliganPanelView)
            : null;
        _playerDiceRollOrchestrator = new DiceRollOrchestrator(_playerHand, new DiceRollController(_playerHand),
                                                               playerHandLayoutView, diceRollPanelView);

        bool canPlace = fieldBoardView != null && cardDragView != null;
        _playerPlacementController = canPlace ? new CardPlacementController(_playerHand, fieldBoardView.SlotCount) : null;
        _playerPlacementOrchestrator = canPlace
            ? new CardPlacementOrchestrator(_playerHand, _playerPlacementController, playerHandLayoutView, fieldBoardView, cardDragView)
            : null;

        // 적은 아직 null → TurnOrchestrator가 적 행동 건너뜀
        _turnOrchestrator = new TurnOrchestrator(isPlayerFirst, _playerDrawOrchestrator, null, _playerMulliganOrchestrator,
                                                 _playerDiceRollOrchestrator, null, _playerPlacementOrchestrator);

        // 지금은 1턴만 실행 (HP 연결 후 반복으로 교체 예정)
        await _turnOrchestrator.RunTurn(_battleCts.Token);
    }

    // "배치 완료" 버튼 OnClick에 연결할 메서드
    public void RequestEndPlacement()
    {
        _playerPlacementOrchestrator?.RequestEndPlacement();
    }

    private bool TrySetUpPlayerDeck()
    {
        if (_deckRepository == null || _cardDatabase == null) { /* 경고 */ return false; }
        if (playerHandLayoutView == null) { /* 경고 */ return false; }

        BattleDeckController deckController = new BattleDeckController(_deckRepository, _cardDatabase);
        if (!deckController.TryBuildBattleDeck(_deckBuffer, out string deckId)) { /* 경고 */ return false; }

        if (_deckBuffer.Count != requiredDeckSize) 
        {
            Debug.LogWarning($"[CardBattleManager] 현재 덱은({deckId}) {_deckBuffer.Count}장 입니다 - 덱은 {requiredDeckSize}장 여야 합니다.");
        }

        _playerHand = new HandData();
        _playerDrawController = new DrawController(drawConfig, _playerHand);
        _playerDrawController.InitializeDeck(_deckBuffer);
        return true;
    }
}
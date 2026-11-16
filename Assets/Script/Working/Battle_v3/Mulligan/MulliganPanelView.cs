using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using TMPro;
using UnityEditor.Overlays;

public class MulliganPanelView : MonoBehaviour
{
    [SerializeField] private PopupOverlayView overlay;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject mulliganCardPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private CanvasGroup confirmButtonGroup;
    [SerializeField] private TMP_Text selectableCountText;
    [SerializeField] private string selectableCountFormat = "교체 가능 {0}장";
    [SerializeField] private MulliganConfig config;


    public event Action<int> OnCardClicked;
    public event Action OnConfirmClicked;

    private readonly List<MulliganCardView> _cards = new List<MulliganCardView>();
    private readonly List<UniTask> _tasks = new List<UniTask>();
    private readonly List<UniTask> _closeTasks = new List<UniTask>();
    private int _sortingLayerId;
    private bool _inputEnabled;

    private void Awake()
    {
        _sortingLayerId = SortingLayerUtility.ResolveId(config != null ? config.sortingLayer : null, nameof(MulliganCardView));
        if (confirmButton != null) confirmButton.onClick.AddListener(HandleConfirmClicked);
        HideImmediate();
    }

    private void OnDestroy()
    {
        
        if (confirmButton != null) confirmButton.onClick.RemoveListener(HandleConfirmClicked);
        
    }

    // 패널 열기
    public async UniTask<bool> ShowAsync(IReadOnlyList<CardRuntime> hand, CancellationToken ct)
    {
        if(config == null || mulliganCardPrefab == null || cardContainer == null || hand == null)
        {
            Debug.LogWarning($"[MulliganPanelView] config / mulliganCardPrefab / cardContainer / 연결을 확인하세요");
            return false;
        }
        if (WorldPoolManager.instance == null)
        {
            Debug.LogWarning($"[MulliganPanelView] Hierarchy에 WorldPoolManager 인스턴스가 없습니다.");
            return false;
        }

        HideImmediate();
        if (overlay != null) overlay.Prepare();
        else Debug.LogWarning("[MulliganPanelView] overlay(PopupOverlayView)가 비어 있어 배경 없이 열립니다.");


        int total = hand.Count;
        for (int i = 0; i < total; i++)
        {
            int visualIndex = HandLayoutView.ToVisualIndex(i, total);
            Vector3 localPos = GetRowPosition(visualIndex, total);

            GameObject obj = WorldPoolManager.instance.Get(mulliganCardPrefab, cardContainer.TransformPoint(localPos), cardContainer.rotation, cardContainer);
            MulliganCardView card = obj != null ? obj.GetComponent<MulliganCardView>() : null;
            if (card == null)
            {
                Debug.LogWarning("[MulliganPanelView] mulliganCardPrefab에 MulliganCardView가 없습니다.");
                if (obj != null) WorldPoolManager.instance.Return(obj);
                _cards.Add(null);
                continue;
            }

            card.Setup(i, hand[i], localPos, config.cardScale, _sortingLayerId, config.cardBaseOrder + visualIndex, config.selectBorderAlpha, config.selectGlowAlpha);
            card.SetAlphaInstant(0f);
            card.OnClicked += HandleCardClicked;
            _cards.Add(card);
        }

        _tasks.Clear();
        if (overlay != null) _tasks.Add(overlay.FadeToAsync(config.overlayAlpha, config.showFadeDuration, ct));
        for(int i = 0; i < _cards.Count; i++)
        {
            if (_cards[i] == null) continue;
            float delay = HandLayoutView.ToVisualIndex(i, total) * config.cardAppearStagger;
            _tasks.Add(FadeCardDelayAsync(_cards[i], 1f, delay, config.cardAppearDuration, ct));
        }
        _tasks.Add(FadeButtonAsync(1f, config.confirmButtonFadeDuraiton, ct));

        await UniTask.WhenAll(_tasks);
        return true;
    }

    // 버튼 활성화
    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
        if (confirmButtonGroup != null)
        {
            confirmButtonGroup.interactable = enabled;
            confirmButtonGroup.blocksRaycasts = enabled;
        }
    }

    // 남은 교체 가능 카드 수 표시
    public void SetSelectableCount(int remaining)
    {
        if (selectableCountText == null) return;
        selectableCountText.text = string.Format(selectableCountFormat, remaining);
    }

    // 카드 선택
    public void SetCardSelected(int handIndex, bool selected)
    {
        MulliganCardView card = GetCard(handIndex);
        if (card == null || config == null) return;
        card.SetSelected(selected, config.selectRaise, config.selectDuration);
    }

    // 확인 버튼 누른 후 1단계 : 카드 사라짐
    public async UniTask PlayExitAsync(IReadOnlyList<int> handIndices, CancellationToken ct)
    {
        if (config == null || handIndices == null) return;

        _tasks.Clear();
        for (int i = 0; i < handIndices.Count; i++)
        {
            MulliganCardView card = GetCard(handIndices[i]);
            if (card != null) _tasks.Add(card.FadeToAsync(0f, config.exitDuration, ct));
        }
        await UniTask.WhenAll(_tasks);
    }

    // 확인 버튼 누른 후 2단계 : 같은 자리에 새 카드 등장 -> 몇 초 후 -> 패널 / 오버레이 페이드 아웃 -> 카드 반납
    public async UniTask PlayEnterAndCloseAsync(IReadOnlyList<int> handIndices, IReadOnlyList<CardRuntime> newCards, CancellationToken ct)
    {
        if (config == null) { HideImmediate(); return; }

        _tasks.Clear();
        if (handIndices != null)
        {
            for (int k = 0; k < handIndices.Count; k++)
            {
                MulliganCardView card = GetCard(handIndices[k]);
                if (card == null) continue;

                CardRuntime newCard = newCards != null && k < newCards.Count ? newCards[k] : null;
                if (newCard != null) card.SetCard(newCard);
                card.ClearSelectedInstant();
                _tasks.Add(card.FadeToAsync(1f, config.enterDuration, ct));
            }
        }

        bool hasEnter = _tasks.Count > 0;
        if (hasEnter)
        {
            await UniTask.WhenAll(_tasks);
        }

        await CloseDelayAsync(hasEnter ? config.closeHoldDelay : 0f, ct);
        HideImmediate();
    }

    // 즉시 정리(취소/재시작 대비) - 트윈 정지, 카드 풀 반납, 오버레이/버튼 숨김
    public void HideImmediate()
    {
        SetInputEnabled(false);
        ReleaseCards();
        if (overlay != null) overlay.HideImmediate();
        if (confirmButtonGroup != null)
        {
            confirmButtonGroup.DOKill();
            confirmButtonGroup.alpha = 0f;
        }
    }

    private async UniTask CloseDelayAsync(float delay, CancellationToken ct)
    {
        if (delay > 0f) await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct);

        float duration = config.closeFadeDuration;
        _closeTasks.Clear();
        if (overlay != null)
        {
            _closeTasks.Add(overlay.FadeToAsync(0f, duration, ct));
        }
        _closeTasks.Add(FadeButtonAsync(0f, duration, ct));
        for (int i = 0; i < _cards.Count; i++)
        {
            if (_cards[i] != null)
            {
                _closeTasks.Add(_cards[i].FadeGroupToAsync(0f, duration, ct));
            }
        }
        await UniTask.WhenAll(_closeTasks);
    }

    private static async UniTask FadeCardDelayAsync(MulliganCardView card, float target, float delay, float duration, CancellationToken ct)
    {
        if (delay > 0f) await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct);
        await card.FadeToAsync(target, duration, ct);
    }

    // 패널 중심 한 줄 배치, 간격 = min(최대 간격, 최대 폭/(장수 - 1))
    private Vector3 GetRowPosition(int visualIndex, int total)
    {
        float spacing = total <= 1 ? 0f : Mathf.Min(config.cardSpacing, config.maxRowWidth / (total - 1));
        return new Vector3((visualIndex - (total - 1) * 0.5f) * spacing, 0f, 0f);
    }

    private async UniTask FadeButtonAsync(float target, float duration, CancellationToken ct)
    {
        if (confirmButtonGroup == null) return;
        confirmButtonGroup.DOKill();
        if(duration <= 0f) { confirmButtonGroup.alpha = target; return; }
        await confirmButtonGroup.DOFade(target, duration).SetEase(Ease.OutQuad).WithCancellation(ct);
    }

    private MulliganCardView GetCard(int handIndex)
    {
        return handIndex >= 0 && handIndex < _cards.Count ? _cards[handIndex] : null;
    }

    private void ReleaseCards()
    {
        for(int i = 0; i < _cards.Count; i++)
        {
            MulliganCardView card = _cards[i];
            if (card == null) continue;
            card.OnClicked -= HandleCardClicked;
            if(WorldPoolManager.instance != null)
            {
                WorldPoolManager.instance.Return(card.gameObject);
            }
        }
        _cards.Clear();
    }

    private void HandleCardClicked(MulliganCardView card)
    {
        if (!_inputEnabled || card == null) return;
        OnCardClicked?.Invoke(card.HandIndex);
    }

    private void HandleConfirmClicked()
    {
        if (!_inputEnabled) return;
        OnConfirmClicked ?. Invoke();
    }
}

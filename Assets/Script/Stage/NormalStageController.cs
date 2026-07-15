using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using VContainer;

public class NormalStageController : BaseStageController
{
    [Header("노말 스테이지 인트로 연출")]
    [SerializeField] private RoundIntro stageStartEffect;
    [SerializeField] private CardAppearEffect cardAppearEffect;
    [SerializeField] private CharacterAppearEffect playerEffect;
    [SerializeField] private CharacterAppearEffect enemyEffect;

    private BattleDataManager _battleDataManager;
    private BattleInitalizer _battleInitalizer;
    private AudioManager _audioManager;

    [Inject]
    public void Construct(BattleDataManager battleDataManager, BattleInitalizer battleInitalizer, AudioManager audioManager)
    {
        _battleDataManager = battleDataManager;
        _battleInitalizer = battleInitalizer;
        _audioManager = audioManager;
        playerEffect.SetDependencies(audioManager, battleInitalizer);
        enemyEffect.SetDependencies(audioManager, battleInitalizer);
    }

    protected override async UniTask PlayIntroEffect(CancellationToken ct)
    {
        _audioManager.PlayBgm("Battle");
        await PlayEffectAsync(onComplete => stageStartEffect.Play(onComplete), ct);
        
        var enemyPrefab = _battleDataManager.GetEnemyPrefab();
        enemyEffect.SetPrefab(enemyPrefab);      
    }

    protected override async UniTask FadeInPlayer(CancellationToken ct)
    { 
        await PlayEffectAsync(cardAppearEffect.Play, ct);
        await PlayEffectAsync(playerEffect.Play, ct);
        var playerCharacter = _battleInitalizer.PlayerCharacter;
        if (playerCharacter != null)
            await playerCharacter.FadeIn(0.5f).AttachExternalCancellation(ct);
    }

    protected override async UniTask FadeInEnemy(CancellationToken ct)
    {
        await PlayEffectAsync(enemyEffect.Play, ct);
        var enemyCharacter = _battleInitalizer.EnemyCharacter;
        if (enemyCharacter != null)
            await enemyCharacter.FadeIn(0.5f).AttachExternalCancellation(ct);
    }

    protected override async UniTask FadeInGameUI(CancellationToken ct)
    {
        await UniTask.WhenAll(HideUIGroup.Select(group => FadeIn(group, ct)));
    }
}

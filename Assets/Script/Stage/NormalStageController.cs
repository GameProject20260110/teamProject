using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;

public class NormalStageController : BaseStageController
{
    [Header("노말 스테이지 인트로 연출")]
    [SerializeField] private RoundIntro stageStartEffect;
    [SerializeField] private CardAppearEffect cardAppearEffect;
    [SerializeField] private RoundCharacter roundCharacter;
    [SerializeField] private CharacterAppearEffect playerEffect;
    [SerializeField] private CharacterAppearEffect enemyEffect;

    protected override async UniTask PlayIntroEffect(CancellationToken ct)
    {
        await PlayEffectAsync(onComplete => stageStartEffect.Play(onComplete), ct);
        await PlayEffectAsync(cardAppearEffect.Play, ct);
        var enemyPrefab = BattleDataManager.instance.GetEnemyPrefab();
        enemyEffect.SetPrefab(enemyPrefab);
        await PlayEffectAsync(enemyEffect.Play, ct);
        await PlayEffectAsync(playerEffect.Play, ct);
    }

    protected override async UniTask FadeInPlayer(CancellationToken ct)
    {
        var playerCharacter = BattleInitalizer.instance.PlayerCharacter;
        if (playerCharacter != null)
            await playerCharacter.FadeIn(0.5f).AttachExternalCancellation(ct);
    }

    protected override async UniTask FadeInEnemy(CancellationToken ct)
    {
        var enemyCharacter = BattleInitalizer.instance.EnemyCharacter;
        if (enemyCharacter != null)
            await enemyCharacter.FadeIn(0.5f).AttachExternalCancellation(ct);
    }

    
    protected override async UniTask FadeInGameUI(CancellationToken ct)
    {
        await UniTask.WhenAll(HideUIGroup.Select(group => FadeIn(group, ct)));
    }
}

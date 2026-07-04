using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Linq;

public class PlayerCharacter : MonoBehaviour
{
    private SpriteRenderer[] _renderers;
    public SpriteRenderer[] Renderers => _renderers;

    private Animator _animator;


    private void Awake()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    public void SetAlpha(float alpha)
    {
        if (_renderers != null && _renderers.Length > 0)
        {
            foreach (var sr in _renderers)
                sr.color = new Color(1, 1, 1, alpha);
        }
    }

    public UniTask FadeIn(float duration = 0.5f)
    {
        if (_renderers != null && _renderers.Length > 0)
        {
            var tasks = _renderers
                .Select(sr => sr.DOFade(1f, duration).SetEase(Ease.OutQuad).ToUniTask());
            return UniTask.WhenAll(tasks);
        }
        return UniTask.CompletedTask;
    }

    public void SubscribeToBattleEvents(BattleEventBus eventBus)
    {
        eventBus.OnPlayerHit += HandleHitPlayer;
        eventBus.OnPlayerAttackStart += HandleEnemyAttack;
    }

    public void UnsubscribeFromBattleEvents(BattleEventBus eventBus)
    {
        eventBus.OnPlayerHit -= HandleHitPlayer;
        eventBus.OnPlayerAttackStart -= HandleEnemyAttack;
    }

    private void HandleEnemyAttack(DiceContext ctx)
    {
        _animator.SetTrigger("Attack");
    }

    private void HandleHitPlayer(DiceContext ctx, int damage)
    {
        _animator.SetTrigger("Hit");
    }
}

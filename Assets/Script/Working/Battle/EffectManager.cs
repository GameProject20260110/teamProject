using UnityEngine;
using VContainer;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [SerializeField] private GameObject burnEffect;
    [SerializeField] private GameObject healEffect;

    private AudioManager _audioManager;

    [Inject]
    public void Construct(AudioManager audioManager)
    {
        _audioManager = audioManager;
        Instance = this;
    }


    public void PlayBurnEffect(IDamageable target, int damage, DiceContext ctx, System.Action onComplete)
    {
        Vector3 pos = ctx.IsPlayer ? ctx.Positions.PlayerPosition : ctx.Positions.EnemyPosition;

        GameObject effect = WorldPoolManager.instance.Get(burnEffect, pos, Quaternion.identity);

        effect.GetComponent<Skill>().Init(new SkillContext
        {
            onHit = () =>
            {
                target.TakeDamageRaw(damage, ctx);
                if(ctx.IsPlayer) ctx.EventBus.TriggerHitPlayer(ctx, damage);
                else ctx.EventBus.TriggerHitEnemy(ctx, damage);
                    
            },
            onEnd = () => onComplete?.Invoke()
        });
    }

    public void PlayHealEffect(Vector3 position)
    {
        _audioManager.PlaySfx("Heal");
        if (healEffect != null)
            WorldPoolManager.instance.Get(healEffect, position, Quaternion.identity);
    }
}

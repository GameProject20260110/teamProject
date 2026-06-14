using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    [SerializeField] private GameObject burnEffect;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void PlayBurnEffect(IDamageable target, int damage, DiceContext ctx, System.Action onComplete)
    {
        GameObject effect = ObjectPool.instance.Get(burnEffect);

        Vector3 pos = ctx.IsPlayer? ctx.Positions.PlayerPosition : ctx.Positions.EnemyPosition;

        Debug.Log(ctx.IsPlayer);
        effect.transform.position = pos;
        effect.GetComponent<Skill>().Init(new SkillContext
        {
            onHit = () =>
            {
                target.TakeDamageRaw(damage);
                Debug.Log(target  + "," + damage + "," + ctx.IsPlayer);

                if(ctx.IsPlayer) ctx.EventBus.TriggerHitEnemy(ctx, damage);
                else ctx.EventBus.TriggerPlayerHit(ctx, damage);
                    
            },
            onEnd = () => onComplete?.Invoke()
        });
    }
}

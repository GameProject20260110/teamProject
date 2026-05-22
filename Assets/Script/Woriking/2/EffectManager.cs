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

    public void PlayBurnEffect(IDamageable target, int damage, BattleContext ctx, System.Action onComplete)
    {
        GameObject effect = ObjectPool.instance.Get(burnEffect);
        effect.transform.position = ctx.EnemyPosition;
        effect.GetComponent<Skill>().Init(new SkillContext
        {
            onHit = () =>
            {
                target.TakeDamageRaw(damage);
                ctx.OnEnemyHit?.Invoke(damage);
            },
            onEnd = () => onComplete?.Invoke()
        });
    }
}

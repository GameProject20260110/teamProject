using UnityEngine;

public class Skill : MonoBehaviour
{
    [SerializeField] AudioClip sfxSound;

    private int damage;
    private System.Action onHitCallback;
    private System.Action onEndCallback;

    public void Init(bool isPlayer, int damage, System.Action onHit, System.Action onEnd)
    {
        AudioManager.instance.PlaySfx(sfxSound);
        this.damage = damage;
        this.onHitCallback = onHit;
        this.onEndCallback = onEnd;
    }

    // Animation Event
    public void HitAnimFrame()
    {
        onHitCallback?.Invoke();
    }

    // Animation Event
    public void OnAttackEnd()
    {
        onEndCallback?.Invoke();
        ObjectPool.instance.Return(gameObject);
    }
}

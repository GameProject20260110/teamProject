using UnityEngine;

public class Skill : MonoBehaviour
{
    private int damage;
    private System.Action onHitCallback;
    private System.Action onEndCallback;

    public void Init(bool isPlayer, int damage, System.Action onHit, System.Action onEnd)
    {
        if(isPlayer) AudioManager.instance.PlaySfx(AudioManager.Sfx.Electric);
        else AudioManager.instance.PlaySfx(AudioManager.Sfx.Void);
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

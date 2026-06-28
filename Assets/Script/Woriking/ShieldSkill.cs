using UnityEngine;

public class ShieldSkill : MonoBehaviour
{
    private int damage;
    private System.Action onShieldCallback;
    private System.Action onEndCallback;

    public void Init(bool isPlayer, int damage, System.Action onHit, System.Action onEnd)
    {
        if(isPlayer) AudioManager.instance.PlaySfx("Electric");
        else AudioManager.instance.PlaySfx("Void");
        this.damage = damage;
        this.onShieldCallback = onHit;
        this.onEndCallback = onEnd;
    }

    public void ShieldUp()
    {

    }
}

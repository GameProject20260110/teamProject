using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class DiceEffectBase : MonoBehaviour
{
    protected DiceVFXBase vfx;

    protected virtual void Awake()
    {
        vfx = GetComponent<DiceVFXBase>();
        if (vfx == null)
            Debug.LogError($"{gameObject.name}에 DiceVFXBase 컴포넌트가 없습니다.");
    }

    public virtual UniTask OnAttack(DiceContext ctx) => UniTask.CompletedTask;
    public virtual UniTask OnDefense(DiceContext ctx) => UniTask.CompletedTask;
}

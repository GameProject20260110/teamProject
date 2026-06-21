using UnityEngine;


[CreateAssetMenu(fileName = "GimmickSo")]
public abstract class GimmickSo : ScriptableObject
{
    public string gimmickName;
    public string description;   // 기본 설명 (비활성 상태에서 표시)
    public Sprite icon;
    public int startTurn = 1;
    public int activateInterval = 1; // 몇 턴마다 발동 (1=매턴, 3=1,4,7턴)

    public abstract void Register(BattleEventBus eventBus);
    public abstract void Unregister(BattleEventBus eventBus);

    protected virtual bool ShouldActivate(int currentTurn)
    {
        if (currentTurn < startTurn) return false;
        return (currentTurn - 1) % activateInterval == 0;
    }

    // 기믹 발동 시 표시할 설명 (override해서 동적 수치 반영 가능)
    public virtual string GetActiveDesc() => description;
}

using UnityEngine;


[CreateAssetMenu(fileName = "GimmickSo")]
public abstract class GimmickSo : ScriptableObject
{
    public string gimmickName;
    public string description;
    public Sprite icon;
    public int startTurn = 1;
    public int activateInterval = 1;

    public abstract void Register(BattleEventBus eventBus);
    public abstract void Unregister(BattleEventBus eventBus);

    protected virtual bool ShouldActivate(int currentTurn)
    {
        if (currentTurn < startTurn) return false;
        return (currentTurn - 1) % activateInterval == 0;
    }

    public virtual string GetActiveDesc() => description;
}

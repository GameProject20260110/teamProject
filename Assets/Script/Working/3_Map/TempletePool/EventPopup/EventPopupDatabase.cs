using UnityEngine;

[CreateAssetMenu(fileName = "EventPopupDatabase", menuName = "Map/EventPopupDatabase")]
public class EventPopupDatabase : ScriptableObject
{
    [SerializeField] private EventPopupSo[] eventPopupDatas;
    
    public EventPopupSo RandomEventPick()
    {
        if (eventPopupDatas == null || eventPopupDatas.Length == 0) return null;

        int idx = Random.Range(0, eventPopupDatas.Length);
        return eventPopupDatas[idx];
    }
}

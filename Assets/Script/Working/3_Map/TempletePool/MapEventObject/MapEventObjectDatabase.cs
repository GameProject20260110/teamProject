using UnityEngine;

[CreateAssetMenu(fileName = "EventObjectDatabase", menuName = "Map/EventObjectDatabase")]
public class EventObjectDatabase : ScriptableObject
{
    [SerializeField] private MapEventObject[] eventPrefabs;

    private MapEventObject lastPicked;

    public MapEventObject RandomEventPick()
    {
        if (eventPrefabs == null || eventPrefabs.Length == 0)
        {
            Debug.LogError($"{name}에 등록된 이벤트 프리팹이 없습니다.");
            return null;
        }

        if (eventPrefabs.Length == 1)
        {
            lastPicked = eventPrefabs[0];
            return lastPicked;
        }

        MapEventObject picked;
        do
        {
            picked = eventPrefabs[Random.Range(0, eventPrefabs.Length)];
        } while (picked == lastPicked);

        lastPicked = picked;
        return picked;
    }
}

using System;
using UnityEngine;

[Serializable]
public class NextEventEntry
{
    public EventPopupSo Event;
    public int weight;
}

public enum EventType
{
    Nothing
}

[Serializable]
public class EventChoice
{
    [SerializeField] private string text;
    [SerializeField] private EventType type;

    public string Text => text;
    public EventType Type => type;

    [Header("next Event")]
    public EventPopupSo nextEventPopup;
    public NextEventEntry[] randomNextEvents;

    public EventPopupSo GetNextEvent()
    {
        if(randomNextEvents != null && randomNextEvents.Length > 0)
        {
            int total = 0;
            foreach (var e in randomNextEvents) total += e.weight;

            if(total > 0)
            {
                int randomNum = UnityEngine.Random.Range(0, total);
                foreach (var e in randomNextEvents)
                {
                    randomNum -= e.weight;
                    if (randomNum <= 0) return e.Event;
                }
            }
        }
        return nextEventPopup;
    }
}

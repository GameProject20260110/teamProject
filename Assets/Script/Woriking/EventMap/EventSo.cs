using UnityEngine;

[CreateAssetMenu(fileName = "EventSo",menuName = "Scriptable Object/Event")]
public abstract class EventSo : ScriptableObject
{
    public string EventName;
    public string EventDescription;
    public Sprite icon;
    
    public abstract void Execute();
}

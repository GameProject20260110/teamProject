using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EventList",menuName ="Scriptable Object/EventScene")]
public class EventSceneSo : ScriptableObject
{
    public string sceneName;
    [TextArea]
    public string sceneDescription;
    public Sprite sceneImage;
    public List<EventSo> eventList;
}

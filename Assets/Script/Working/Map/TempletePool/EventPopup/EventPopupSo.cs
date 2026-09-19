using UnityEngine;

[CreateAssetMenu(fileName = "EventPopupSo",menuName = "Map / EventPopup")]
public class EventPopupSo : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private string title;
    [SerializeField, TextArea] private string desc;
    [SerializeField] private EventChoice[] eventBtns;

    public Sprite Sprite => sprite;
    public string Title => title;
    public string Desc => desc;
    public EventChoice[] EventBtns => eventBtns;
}

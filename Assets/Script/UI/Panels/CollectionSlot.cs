using UnityEngine;
using UnityEngine.UI;

public class CollectionSlot : MonoBehaviour
{
    public DiceData data;
    public Image icon;

    public void Awake()
    {
        icon = GetComponent<Image>();
    }

    public void SetData(DiceData data)
    {
        this.data = data;
        icon.sprite = data.skin.GetSprite(1);
    }
}

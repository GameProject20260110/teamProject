using UnityEngine;
using UnityEngine.UI;

public class CardGhostVisual : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public void SetIcon(Sprite icon)
    {
        if (iconImage == null) return;
        iconImage.sprite = icon;
        iconImage.enabled = icon != null;
    }

    public void OnRent() { }

    public void OnReturn()
    {
        if (iconImage != null) iconImage.enabled = false; 
    }
}

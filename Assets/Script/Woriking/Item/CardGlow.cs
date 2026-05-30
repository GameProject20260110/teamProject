using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardGlow : MonoBehaviour
{
    private Material _glowMaterial;

    private void Awake()
    {
        Image img = GetComponent<Image>();
        _glowMaterial = Instantiate(img.material);
        img.material = _glowMaterial;
    }

    public void SetGlow(bool on)
    {
        float target = on ? 1f : 0f;
        DOTween.To(
            () => _glowMaterial.GetFloat("_GlowIntensity"),
            x => _glowMaterial.SetFloat("_GlowIntensity", x),
            target, 0.2f
        );
    }
}

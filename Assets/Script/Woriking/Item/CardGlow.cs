using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardGlow : MonoBehaviour
{
    private Material _glowMaterial;
    [SerializeField, ColorUsage(true, true)] private Color glowColor1;
    [SerializeField, ColorUsage(true, true)] private Color glowColor2;

    private void Awake()
    {
        Image img = GetComponent<Image>();
        _glowMaterial = Instantiate(img.material);
        img.material = _glowMaterial;
        _glowMaterial.SetColor("_GlowColor1", glowColor1);
        _glowMaterial.SetColor("_GlowColor2", glowColor2);
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

    private void OnDestroy()
    {
        if (_glowMaterial != null)
            Destroy(_glowMaterial);
    }
}

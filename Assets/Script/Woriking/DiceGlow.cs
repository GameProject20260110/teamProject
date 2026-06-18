using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DiceGlow : MonoBehaviour
{
    [Header("페이드 설정")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float maxGlow = 3f;

    [Header("레퍼런스")]
    [SerializeField] private Image glow;
    [SerializeField] private float maxAlpha = 1f;

    private Material materialInstance;
    private Tweener _currentTween;

    private static readonly int GlowID = Shader.PropertyToID("_GlowDiceIntensity");
    //private static readonly int glowSizeID = Shader.PropertyToID("_GlowSize");

    void Awake()
    {
        Image img = GetComponent<Image>();
        materialInstance = Instantiate(img.material);
        img.material = materialInstance;
        materialInstance.SetFloat(GlowID, 0f);
    }

    private void ApplyGlow(float value)
    {
        materialInstance.SetFloat(GlowID, value);

        if (glow != null)
        {
            float ratio = value / maxGlow;
            Color c = glow.color;
            c.a = ratio * maxAlpha;
            glow.color = c;
        }
    }

    public async UniTask ShowGlowAsync(CancellationToken ct = default)
    {       
        float current = materialInstance.GetFloat(GlowID);
        _currentTween?.Kill();

        var completion = new UniTaskCompletionSource<bool>();

        _currentTween = DOVirtual.Float(current, maxGlow, fadeDuration, ApplyGlow)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => completion.TrySetResult(true));

        await completion.Task.AttachExternalCancellation(ct);
    }

    public void HideGlow()
    {       
        float current = materialInstance.GetFloat(GlowID);
        _currentTween?.Kill();

        _currentTween = DOVirtual.Float(current, 0f, fadeDuration, ApplyGlow)
            .SetEase(Ease.OutQuad);
    }

    void OnDestroy()
    {
        _currentTween?.Kill();
        if (materialInstance != null)
            Destroy(materialInstance);
    }
}
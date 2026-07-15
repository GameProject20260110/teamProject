using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System.Linq;

[DisallowMultipleComponent]
public class SpriteGroupAlpha : MonoBehaviour
{
    private SpriteRenderer[] _spriteRenderers;
    private TMP_Text[] _texts;
    [SerializeField] private bool hideOnAwake = true;
    [SerializeField] private string ignoreAlphaTag = "BoardIgnoreAlpha";

    private void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true)
             .Where(sr => !sr.CompareTag(ignoreAlphaTag))
             .ToArray();
        _texts = GetComponentsInChildren<TMP_Text>(true)
            .Where(t => !t.CompareTag(ignoreAlphaTag))
            .ToArray();
        if (hideOnAwake) alpha = 0f;
    }

    public float alpha
    {
        get => _spriteRenderers.Length > 0 ? _spriteRenderers[0].color.a : 1f;
        set
        {
            foreach (var sr in _spriteRenderers)
            {
                Color c = sr.color;
                c.a = value;
                sr.color = c;
            }
            foreach (var t in _texts)
            {
                t.alpha = value;
            }
        }
    }

    public async UniTask FadeAsync(float targetAlpha, float duration, CancellationToken ct, Ease ease = Ease.OutQuad)
    {
        var tcs = new UniTaskCompletionSource<bool>();
        var tween = DOVirtual.Float(alpha, targetAlpha, duration, v => alpha = v)
            .SetEase(ease)
            .OnComplete(() => tcs.TrySetResult(true));

        using (ct.Register(() =>
        {
            tween.Kill();
            tcs.TrySetCanceled(ct);
        }))
        {
            await tcs.Task;
        }
    }
}

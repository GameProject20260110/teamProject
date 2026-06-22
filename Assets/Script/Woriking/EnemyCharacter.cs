using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Linq;

public class EnemyCharacter : MonoBehaviour
{
    // SpriteRenderer 기반 (보스)
    private SpriteRenderer[] _renderers;

    // Image 기반 (일반 적)
    private Image _image;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _image = GetComponentInChildren<Image>();
    }

    public void SetAlpha(float alpha)
    {
        if (_renderers != null && _renderers.Length > 0)
        {
            foreach (var sr in _renderers)
                sr.color = new Color(1, 1, 1, alpha);
        }
        else if (_image != null)
        {
            _image.color = new Color(1, 1, 1, alpha);
        }
    }

    public UniTask FadeIn(float duration = 0.5f)
    {
        if (_renderers != null && _renderers.Length > 0)
        {
            var tasks = _renderers
                .Select(sr => sr.DOFade(1f, duration).SetEase(Ease.OutQuad).ToUniTask());
            return UniTask.WhenAll(tasks);
        }
        else if (_image != null)
        {
            return _image.DOFade(1f, duration).SetEase(Ease.OutQuad).ToUniTask();
        }

        return UniTask.CompletedTask;
    }
}

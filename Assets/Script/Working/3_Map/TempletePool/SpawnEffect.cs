using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SpawnEffect : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform ringPrefab;
    [SerializeField] private RectTransform ringContainer;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float holdDuration = 0.4f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float rotationSpeed = 90f;

    private bool isRotating;
    private RectTransform ringInstance;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        SpawnRing(ringPrefab);
    }

    private void SpawnRing(RectTransform prefab)
    {
        if (ringInstance != null)
            Destroy(ringInstance.gameObject);

        if (prefab == null) return;

        ringInstance = Instantiate(prefab, ringContainer);
        ringInstance.anchoredPosition = Vector2.zero;
    }

    public async UniTask PlayAsync(Action onCharacterAppear, RectTransform overrideRingPrefab = null, CancellationToken token = default)
    {
        if (overrideRingPrefab != null)
            SpawnRing(overrideRingPrefab);

        isRotating = true;
        RotateLoop(token).Forget();

        canvasGroup.DOFade(1f, fadeInDuration);
        await UniTask.Delay(TimeSpan.FromSeconds(fadeInDuration), cancellationToken: token);

        onCharacterAppear?.Invoke();

        await UniTask.Delay(TimeSpan.FromSeconds(holdDuration), cancellationToken: token);

        canvasGroup.DOFade(0f, fadeOutDuration);
        await UniTask.Delay(TimeSpan.FromSeconds(fadeOutDuration), cancellationToken: token);

        isRotating = false;
        Destroy(gameObject);
    }

    private async UniTaskVoid RotateLoop(CancellationToken token)
    {
        while (isRotating && !token.IsCancellationRequested)
        {
            if (ringInstance != null)
                ringInstance.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            await UniTask.Yield(token);
        }
    }
}

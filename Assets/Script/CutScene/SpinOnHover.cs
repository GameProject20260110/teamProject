using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpinOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float spinDuration = 2f;
    [SerializeField] private float spinSpeed = 360f;
    private Button mybtn;

    private CancellationTokenSource cts;

    private void Start()
    {
        mybtn = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!mybtn.interactable) return;

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        SpinAsync(cts.Token).Forget();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;

        transform.localEulerAngles = Vector3.zero;
    }

    private async UniTaskVoid SpinAsync(CancellationToken token)
    {
        float elapsed = 0f;

        try
        {
            while (elapsed < spinDuration)
            {
                float delta = Time.deltaTime;
                transform.Rotate(0f, 0f, spinSpeed * delta);
                elapsed += delta;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            transform.localEulerAngles = Vector3.zero;
        }
        catch (System.OperationCanceledException)
        {
        }
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}

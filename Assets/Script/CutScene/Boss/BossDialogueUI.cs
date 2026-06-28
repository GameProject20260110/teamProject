using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;


public class BossDialogueUI : MonoBehaviour
{
    public static BossDialogueUI instance;

    [SerializeField] private SpeechBubbleView speechBubblePrefab;
    [SerializeField] private Canvas canvas;

    [SerializeField] private float textSpeed = 0.05f;
    [SerializeField] private Vector2 northOffset = new Vector2(0f, 100f);

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (canvas == null) canvas = GetComponentInParent<Canvas>();
    }

    private Vector2 GetBubblePosition(Transform target)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPos,
            canvas.worldCamera,
            out Vector2 localPos
            );

        float targetHeight = 0f;
        var renderers = target.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
            targetHeight = Mathf.Max(targetHeight, sr.bounds.size.y);

        Vector2 topScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position + new Vector3(0, targetHeight * 0.5f, 0));
            

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            topScreenPos,
            canvas.worldCamera,
            out Vector2 topLocalPos
            );

        return topLocalPos + northOffset;
    }

    public async UniTask ShowDialogue(string dialogue, Transform target, CancellationToken ct)
    {
        SpeechBubbleView bubble = Instantiate(speechBubblePrefab, canvas.transform);
        bubble.GetComponent<RectTransform>().anchoredPosition = GetBubblePosition(target);

        bubble.Prepare(dialogue);

        await bubble.FadeIn(ct);

        var typeTask = bubble.TypeWriter(textSpeed, ct);

        while(!bubble._isFullyTyped)
        {
            if(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                bubble.SkipTyping();
                break;
            }
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        await typeTask;

        bubble.StartBlinking();

        await UniTask.Yield(PlayerLoopTiming.Update, ct);
        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space), cancellationToken: ct);

        bubble.StopBlinking();

        await bubble.FadeOut(ct);

        Destroy(bubble.gameObject);
    }

    public async UniTask ShowDialogues(string[] dialogues, Transform target, CancellationToken ct)
    {
        foreach(var dialogue in dialogues)
        {
            await ShowDialogue(dialogue, target, ct);
            await UniTask.Delay(100, cancellationToken: ct);
        }
    }

    
}

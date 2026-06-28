using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using DG.Tweening;

public class SpeechBubbleView : MonoBehaviour
{
    public bool _isFullyTyped = false;

    [SerializeField] private Image bubbleImage;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject tapToContinue;

    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float blinkDuration = 0.7f;

    private CanvasGroup _canvasGroup;
    private CanvasGroup _tapCanvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);

        _tapCanvasGroup = tapToContinue.GetComponent<CanvasGroup>();
        if(_tapCanvasGroup == null) 
            _tapCanvasGroup = tapToContinue.AddComponent<CanvasGroup>();
        tapToContinue.SetActive(false);
    }

    public void Prepare(string dialogue)
    {
        dialogueText.text = dialogue;
        dialogueText.maxVisibleCharacters = 0;
    }


    public async UniTask TypeWriter(float textSpeed, CancellationToken ct)
    {
        _isFullyTyped = false;
        int totalChars = dialogueText.text.Length;

        for (int i = 0; i < totalChars; i++)
        {
            if (ct.IsCancellationRequested) break;
            dialogueText.maxVisibleCharacters = i + 1;
            await UniTask.Delay((int)(textSpeed * 1000), cancellationToken: ct);
        }

        _isFullyTyped = true;
    }

    public void SkipTyping()
    {
        dialogueText.maxVisibleCharacters = dialogueText.text.Length;
        _isFullyTyped = true;
    }

    public void StartBlinking()
    {
        tapToContinue.SetActive(true);
        _tapCanvasGroup.alpha = 1f;
        _tapCanvasGroup.DOKill();
        _tapCanvasGroup.DOFade(0f, blinkDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(tapToContinue);
    }

    public void StopBlinking()
    {
        _tapCanvasGroup.DOKill();
        tapToContinue.SetActive(false);
    }

    public async UniTask FadeIn(CancellationToken ct)
    {
        gameObject.SetActive(true); 
        _canvasGroup.alpha = 0f;
        await _canvasGroup.DOFade(1f, fadeDuration)
            .SetEase(Ease.OutQuad)
            .ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: ct);
    }

    public async UniTask FadeOut(CancellationToken ct)
    {
        await _canvasGroup.DOFade(0f, fadeDuration)
            .SetEase(Ease.InQuad)
            .ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: ct);
    }
}
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.Multiplayer.Center.Common;
using UnityEngine;
using UnityEngine.UI;

public class MapScrollAnimation : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private EventPopupDatabase eventPopupSos;

    private EventPopupSo currentEventSo;

    [Header("Root")]
    [SerializeField] private GameObject popupRoot;

    [Header("Rect")]
    [SerializeField] private Image Panel;
    [SerializeField] private Image magicCircle;
    [SerializeField] private RectTransform leftScroll;
    [SerializeField] private RectTransform rightScroll;
    [SerializeField] private RectTransform EventPopup;
    [SerializeField] private CanvasGroup Popup;

    [Header("EventPopup")]
    [SerializeField] private Image EventSprite;
    [SerializeField] private TextMeshProUGUI EventTitle;
    [SerializeField] private TextMeshProUGUI EventDesc;

    [Header("Test")]
    [SerializeField] private EventPopupSo test;

    [SerializeField] private Button[] Buttons;
    private bool PlayAnim = false;
    private bool FirstAnim = true;

    private async UniTask ScrollOpenAnimation(EventPopupSo eventPopup = null)
    {
        popupRoot.SetActive(true);
        KillTweens();
        Set(eventPopup);
        
        if (!PlayAnim)
        {
            PlayAnim = true;
            FirstAnim = true;
            magicCircle.rectTransform.DOKill();
            _ = magicCircle.rectTransform
                .DORotate(new Vector3(0, 0, -360f), 6f, RotateMode.FastBeyond360)
                .SetRelative()
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
            
        }

        var seq = DOTween.Sequence();
        _ = seq.AppendInterval(0.3f);
        _ = seq.Append(Panel.DOFade(180 / 255f, 0.5f));
        _ = seq.Join(magicCircle.DOFade(1f, 0.5f));
        if (FirstAnim)
        {
            _ = seq.Append(Popup.DOFade(1f, 0.5f));
            FirstAnim = false;
        }

        _ = seq.AppendInterval(1f);
        _ = seq.Join(leftScroll.DOAnchorPos(new Vector2(-700, 0), 0.5f));
        _ = seq.Join(rightScroll.DOAnchorPos(new Vector2(700, 0), 0.5f));
        _ = seq.Join(EventPopup.DOSizeDelta(new Vector2(1350, 950), 0.5f));
        
        await seq.AsyncWaitForCompletion();
    }

    private async UniTask ScrollCloseAnimation()
    {      
        KillTweens();
        
        var seq = DOTween.Sequence();
        _ = seq.AppendInterval(0.3f);

        _ = seq.Append(leftScroll.DOAnchorPos(new Vector2(-50, 0), 0.5f));
        _ = seq.Join(rightScroll.DOAnchorPos(new Vector2(50, 0), 0.5f));
        _ = seq.Join(EventPopup.DOSizeDelta(new Vector2(0, 950), 0.5f));
       
        if (!PlayAnim)
        {
            _ = seq.Append(Popup.DOFade(0f, 0.5f));
            _ = seq.Append(Panel.DOFade(0f, 0.5f));
            _ = seq.Join(magicCircle.DOFade(0f, 0.5f));
        }       
        await seq.AsyncWaitForCompletion();

        if (!PlayAnim)
        {
            magicCircle.rectTransform.DOKill();
            popupRoot.SetActive(false);
        }

    }

    private void KillTweens()
    {
        leftScroll.DOKill(true);
        rightScroll.DOKill(true);
        EventPopup.DOKill(true);
        Panel.DOKill(true);
        
    }

    private void Set(EventPopupSo eventPopup)
    {
        currentEventSo = eventPopup != null ? eventPopup : eventPopupSos.RandomEventPick();
        if (currentEventSo == null) return;
        // ¼¼ÆÃ
        EventSprite.sprite = currentEventSo.Sprite;
        EventTitle.text = currentEventSo.Title;
        EventDesc.text = currentEventSo.Desc;

        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].gameObject.SetActive(false);
            bool hasChoice = (i < currentEventSo.EventBtns.Length);
            if (hasChoice) Buttons[i].gameObject.SetActive(true);
            else continue;

            var ButtonEvent = currentEventSo.EventBtns[i];
            Buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = ButtonEvent.Text;

            Buttons[i].onClick.RemoveAllListeners();
            Buttons[i].onClick.AddListener(() => ButtonChoice(ButtonEvent));
        }
        
        EventPopup.sizeDelta = new Vector2(0, 950);
        leftScroll.anchoredPosition = new Vector2(-50, 0);
        rightScroll.anchoredPosition = new Vector2(50, 0);       
    }

    private async UniTask ButtonChoice(EventChoice buttonEvent)
    {

        var next = buttonEvent.GetNextEvent();

        if (buttonEvent.nextEventPopup == null) 
        {
            FirstAnim = false;
            PlayAnim = false;
            await ScrollCloseAnimation();
        }
        else
        {
            await ScrollCloseAnimation();
            await ScrollOpenAnimation(next);
        }
    }

    [ContextMenu("ScrollOpenAnim")]
    private void TestScrollOpenAnim() => ScrollOpenAnimation(test).Forget();

    [ContextMenu("ScrollCloseAnim")]
    private void TestScrollCloseAnim() => ScrollCloseAnimation().Forget();
}

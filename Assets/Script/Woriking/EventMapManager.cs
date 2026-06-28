using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;

public class EventMapManager : MonoBehaviour
{
    [SerializeField] private EventButton[] eventButtons;
    [SerializeField] private EventSceneSo currentScene;
    [SerializeField] private CanvasGroup[] canvasGroups;
    [SerializeField] private Image image;
    [SerializeField] private TypeWriter typeWriter;
    [SerializeField] private string eventDescription;
    //[SerializeField] private AudioClip audioClip;
    [SerializeField] private string audioClipKey;

    private EventSo[] assignedEvents;

    private void Start()
    {
        AssignRandomEvents();
        SetUpButtons();
        AudioManager.instance.PlayBgm(audioClipKey);
        PlayOpenAnimation().Forget();
    }

    private void AssignRandomEvents()
    {
        var pool = new List<EventSo>(currentScene.eventList);
        assignedEvents = new EventSo[eventButtons.Length];
        image.sprite = currentScene.sceneImage;
        for (int i = 0; i < eventButtons.Length; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            assignedEvents[i] = pool[randomIndex];
            pool.RemoveAt(randomIndex);
        }
    }

    private void SetUpButtons()
    {
        for (int i = 0; i < eventButtons.Length; i++)
        {
            int index = i;
            var eb = eventButtons[index];

            if (eb.descText != null)
                eb.descText.text = assignedEvents[index].EventDescription;
            if (eb.iconImage != null)
                eb.iconImage.sprite = assignedEvents[index].icon;

            eb.button.onClick.AddListener(() =>
            {
                assignedEvents[index].Execute();
                SceneController.instance.LoadMapScene();
            });
        }
    }

    private async UniTaskVoid PlayOpenAnimation()
    {
        ShowPanel().Forget();

        await UniTask.Delay(300);
        typeWriter.Play(currentScene.sceneDescription,
        this.GetCancellationTokenOnDestroy()).Forget();

        await UniTask.Delay(300);
        ShowButtons().Forget();

        await UniTask.Delay(500);
        ShowImage().Forget();
    }

    private async UniTask ShowPanel()
    {
        canvasGroups[0].gameObject.SetActive(true);
        canvasGroups[0].alpha = 0f;
        await canvasGroups[0].DOFade(1f, 0.7f).AsyncWaitForCompletion();
    }

    private async UniTask ShowButtons()
    {
        foreach(var eb in eventButtons)
        {
            eb.button.gameObject.SetActive(true);
            eb.canvasGroup.alpha = 0f;           
            eb.canvasGroup.DOFade(1f, 0.5f);
            await UniTask.Delay(300);
        }
    }

    private async UniTask ShowImage()
    {
        canvasGroups[1].gameObject.SetActive(true);
        canvasGroups[1].alpha = 0f;
        await canvasGroups[1].DOFade(1f, 0.7f).AsyncWaitForCompletion();
    }

}

[System.Serializable]
public class EventButton
{
    public Button button;
    public TextMeshProUGUI descText;
    public Image iconImage;
    public CanvasGroup canvasGroup;
}

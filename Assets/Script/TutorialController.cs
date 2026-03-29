using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialController : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private TutorialMask tutorialMask;
    [SerializeField] private GameObject messageBox;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("튜토리얼 데이터")]
    [SerializeField] private TutorialStepData tutorialData;

    private int currentStepIndex = -1;
    private Button currentTargetButton = null;
    private EventTrigger currentTargetTrigger = null;
    private GameObject currentTargetObject = null;

    void Start()
    {
        messageBox.SetActive(false);
        tutorialMask.gameObject.SetActive(false);
        StartTutorial();
    }

    public void StartTutorial()
    {
        currentStepIndex = -1;
        tutorialMask.Show();
        NextStep();
    }

    public void NextStep()
    {
        RemoveTargetListener();

        currentStepIndex++;

        if (currentStepIndex >= tutorialData.steps.Count)
        {
            CompleteTutorial();
            return;
        }

        ShowStep(tutorialData.steps[currentStepIndex]);
    }

    void ShowStep(TutorialStepData.Step step)
    {
        messageText.text = step.message;
        messageBox.SetActive(true);

        if (!string.IsNullOrEmpty(step.targetUIName))
        {
            GameObject targetObj = GameObject.Find(step.targetUIName);
            if (targetObj != null)
            {
                RectTransform targetRect = targetObj.GetComponent<RectTransform>();
                tutorialMask.FocusOnTarget(targetRect);

                currentTargetObject = targetObj;

                currentTargetButton = targetObj.GetComponent<Button>();
                if (currentTargetButton != null)
                {
                    currentTargetButton.onClick.AddListener(NextStep);
                }
                else
                {
                    AddClickListener(targetObj);
                }
            }
        }
    }

    void AddClickListener(GameObject target)
    {
        currentTargetTrigger = target.GetComponent<EventTrigger>();
        if (currentTargetTrigger == null)
        {
            currentTargetTrigger = target.AddComponent<EventTrigger>();
        }

        currentTargetTrigger.triggers.Clear();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => NextStep());
        currentTargetTrigger.triggers.Add(entry);

        Image image = target.GetComponent<Image>();
        if (image == null)
        {
            image = target.AddComponent<Image>();
            image.color = new Color(1, 1, 1, 0.01f);
        }
        image.raycastTarget = true;
    }

    void RemoveTargetListener()
    {
        if (currentTargetButton != null)
        {
            currentTargetButton.onClick.RemoveListener(NextStep);
            currentTargetButton = null;
        }

        if (currentTargetTrigger != null)
        {
            currentTargetTrigger.triggers.Clear();
            Destroy(currentTargetTrigger);
            currentTargetTrigger = null;
        }

        currentTargetObject = null;
    }

    void CompleteTutorial()
    {
        RemoveTargetListener();

        PlayerPrefs.SetInt("ShopTutorialCompleted", 1);
        PlayerPrefs.Save();

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        RemoveTargetListener();
    }
}
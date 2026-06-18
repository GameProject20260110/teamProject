//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;
//using System.Collections;
//using UnityEngine.EventSystems;

//public class TutorialController : MonoBehaviour
//{
//    [Header("컴포넌트")]
//    [SerializeField] private TutorialMask tutorialMask;
//    [SerializeField] private GameObject messageBox;
//    [SerializeField] private TextMeshProUGUI messageText;

//    [Header("튜토리얼 데이터")]
//    [SerializeField] private TutorialStepData tutorialData;

//    private int currentStepIndex = -1;
//    private Button currentTargetButton = null;
//    private Coroutine waitCoroutine = null;

//    void Start()
//    {
//        messageBox.SetActive(false);
//        tutorialMask.gameObject.SetActive(false);
//        StartTutorial();
//    }

//    public void StartTutorial()
//    {
//        currentStepIndex = -1;
//        tutorialMask.Show();
//        NextStep();
//    }

//    public void NextStep()
//    {
//        RemoveButtonListener();

//        currentStepIndex++;

//        if (currentStepIndex >= tutorialData.steps.Count)
//        {
//            CompleteTutorial();
//            return;
//        }

//        ShowStep(tutorialData.steps[currentStepIndex]);
//    }

//    void ShowStep(TutorialStepData.Step step)
//    {
//        messageText.text = step.message;
//        messageBox.SetActive(true);

//        if (!string.IsNullOrEmpty(step.targetUIName))
//        {
//            GameObject targetObj = GameObject.Find(step.targetUIName);
//            if (targetObj != null)
//            {
//                currentTargetButton = targetObj.GetComponent<Button>();
//                if (currentTargetButton != null)
//                {
//                    Debug.Log(1);
//                    RectTransform targetRect = targetObj.GetComponent<RectTransform>();
//                    tutorialMask.FocusOnTarget(targetRect);
//                    SetMaskClickable(false);
//                    currentTargetButton.onClick.AddListener(NextStep);
//                }
//                else if (targetObj.GetComponent<ItemSlot>() != null)
//                {
//                    Debug.Log(2);
//                    tutorialMask.FocusOnTarget(null, TutorialMask.FocusPreset.Dice);
//                    SetMaskClickable(false);
//                    waitCoroutine = StartCoroutine(WaitForSlotChange(targetObj));
//                }
//                else
//                {
//                    Debug.Log(3);
//                    RectTransform targetRect = targetObj.GetComponent<RectTransform>();
//                    tutorialMask.FocusOnTarget(targetRect);
//                    SetMaskClickable(true);
//                }
//            }
//        }
//        else
//        {
//            Debug.Log(21);
//        }
//    }

//    IEnumerator WaitForSlotChange(GameObject slotObj)
//    {
//        var buyDice = slotObj.GetComponentInChildren<BuyDice>();
//        var buyItem = slotObj.GetComponentInChildren<BuyItem>(true);

//        if (buyDice != null)
//        {
//            // 주사위: diceNum이 0이 아니면 변경된 것
//            int originalDiceNum = buyDice.Data?.diceNum ?? 0;

//            while (true)
//            {
//                if (buyDice.Data != null && buyDice.Data.diceNum != originalDiceNum && buyDice.Data.diceNum != 0)
//                {
//                    NextStep();
//                    yield break;
//                }
//                yield return new WaitForSeconds(0.1f);
//            }
//        }
//        else if (buyItem != null)
//        {
//            // 아이템: active 상태 변경 감지
//            bool originalActive = buyItem.gameObject.activeSelf;

//            while (true)
//            {
//                if (buyItem.gameObject.activeSelf != originalActive && buyItem.gameObject.activeSelf)
//                {
//                    NextStep();
//                    yield break;
//                }
//                yield return new WaitForSeconds(0.1f);
//            }
//        }
//    }

//    void SetMaskClickable(bool clickable)
//    {
//        EventTrigger trigger = tutorialMask.GetComponent<EventTrigger>();
//        if (trigger != null)
//        {
//            trigger.enabled = clickable;
//        }
//    }

//    void RemoveButtonListener()
//    {
//        if (currentTargetButton != null)
//        {
//            currentTargetButton.onClick.RemoveListener(NextStep);
//            currentTargetButton = null;
//        }

//        if (waitCoroutine != null)
//        {
//            StopCoroutine(waitCoroutine);
//            waitCoroutine = null;
//        }
//    }

//    void CompleteTutorial()
//    {
//        RemoveButtonListener();

//        string key = tutorialData.tutorialCompleteName;
//        PlayerPrefs.SetInt(key, 1);
//        PlayerPrefs.Save();

//        Destroy(gameObject);
//    }

//    void OnDestroy()
//    {
//        RemoveButtonListener();
//    }
//}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    void NextStep()
    {
        if (currentTargetButton != null)
        {
            currentTargetButton.onClick.RemoveListener(NextStep);
            currentTargetButton = null;
        }

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

        // 누르는 형식의 튜토리얼 진행
        if (!string.IsNullOrEmpty(step.targetUIName))
        {
            GameObject targetObj = GameObject.Find(step.targetUIName);
            if (targetObj != null)
            {
                RectTransform targetRect = targetObj.GetComponent<RectTransform>();
                tutorialMask.FocusOnTarget(targetRect);
                if(step.autoNextDelay > 0)
                {
                    StartCoroutine(AutoNextCoroutine(step.autoNextDelay));
                }
                else
                {
                    currentTargetButton = targetObj.GetComponent<Button>();
                    if (currentTargetButton != null)
                    {
                        currentTargetButton.onClick.AddListener(NextStep);
                    }
                }

            }


            //if (step.autoNextDelay > 0)
            //{
            //    Debug.Log(step.stepName);
            //    tutorialMask.FocusOnTarget(step.targetUI);
            //    StartCoroutine(AutoNextCoroutine(step.autoNextDelay));
            //}
            //else
            //{
            //    tutorialMask.FocusOnTarget(step.targetUI);

            //    currentTargetButton = step.targetUI.GetComponent<Button>();
            //    if (currentTargetButton != null)
            //    { 
            //        currentTargetButton.onClick.AddListener(NextStep);   
            //    }
            //}    
        }
    }

    IEnumerator AutoNextCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextStep();
    }

    void CompleteTutorial()
    {
        if (currentTargetButton != null)
        {
            currentTargetButton.onClick.RemoveListener(NextStep);
            currentTargetButton = null;
        }

        // 튜토리얼 완료 저장
        PlayerPrefs.SetInt("ShopTutorialCompleted", 1); 
        PlayerPrefs.Save();

        Destroy(gameObject);
    }

    void OnDestroy() // 방어 코드
    {
        if (currentTargetButton != null)
        {
            currentTargetButton.onClick.RemoveListener(NextStep);
        }
    }
}
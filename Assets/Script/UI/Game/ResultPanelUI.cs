using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanelUI : MonoBehaviour
{
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultTargetScoreText;
    public TextMeshProUGUI resultScoreText;
    public Button nextBtn;
    public Button retryBtn;
    //public Transform resultLifeContainer;
    //public Image resultLifePreFab;

    //private List<Image> _resultHeart = new List<Image>();

    public GameObject TextGroup;
    private bool _isListenerAdded = false;

    public void Show(bool isSuccess, int targetScore, int score, int currentLife)
    {
        //if (resultPanel != null)
        //{
        //    resultPanel.SetActive(true);
        //}
        //if(resultTitleText != null)
        //{
        //    resultTitleText.text = isSuccess ? "클리어!" : "실패!";
        //}
        //if(resultTargetScoreText != null)
        //{
        //    resultTargetScoreText.text = $"목표 점수 :  {targetScore}";
        //}
        //if(resultScoreText != null)
        //{
        //    resultScoreText.text = $"내 점수 : {score}";
        //}
        //UpdateHearts(currentLife);

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        if (resultTitleText != null)
        {
            resultTitleText.text = isSuccess ? "DEFENSE!" : "FAILED!";
        }
        if (resultTargetScoreText != null)
        {
            resultTargetScoreText.text = targetScore.ToString();
        }
        if (resultScoreText != null)
        {
            resultScoreText.text = score.ToString();
        }

        if (isSuccess)
        {
            nextBtn.gameObject.SetActive(true);
            retryBtn.gameObject.SetActive(false);
        }
        else
        {
            nextBtn.gameObject.SetActive(false);
            retryBtn.gameObject.SetActive(true);
        }

        TextGroup.SetActive(false);
        UiController.instance.RevealCardHelper(resultPanel.GetComponent<Image>());
    }

    public void Hide()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    //private void UpdateHearts(int currentLife)
    //{
    //    if (resultLifeContainer == null || resultLifePreFab == null) return;
        
    //    while(_resultHeart.Count < currentLife)
    //    {
    //        _resultHeart.Add(Instantiate(resultLifePreFab, resultLifeContainer));
    //    }

    //    for(int i = 0; i < _resultHeart.Count; i++)
    //    {
    //        _resultHeart[i].gameObject.SetActive(i < currentLife);
    //    }
    //}
}

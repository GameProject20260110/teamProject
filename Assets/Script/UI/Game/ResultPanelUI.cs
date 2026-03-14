using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResultPanelUI : MonoBehaviour
{
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultTargetScoreText;
    public TextMeshProUGUI resultScoreText;
    public Transform resultLifeContainer;
    public Image resultLifePreFab;

    private List<Image> _resultHeart = new List<Image>();

    public void Show(bool isSuccess, int targetScore, int score, int currentLife)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        if(resultTitleText != null)
        {
            resultTitleText.text = isSuccess ? "클리어!" : "실패!";
        }
        if(resultTargetScoreText != null)
        {
            resultTargetScoreText.text = $"목표 점수 :  {targetScore}";
        }
        if(resultScoreText != null)
        {
            resultScoreText.text = $"내 점수 : {score}";
        }
        UpdateHearts(currentLife);
    }
    
    public void Hide()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    private void UpdateHearts(int currentLife)
    {
        if (resultLifeContainer == null || resultLifePreFab == null) return;
        
        while(_resultHeart.Count < currentLife)
        {
            _resultHeart.Add(Instantiate(resultLifePreFab, resultLifeContainer));
        }

        for(int i = 0; i < _resultHeart.Count; i++)
        {
            _resultHeart[i].gameObject.SetActive(i < currentLife);
        }
    }
}

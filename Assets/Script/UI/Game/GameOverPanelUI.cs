using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class GameOverPanelUI : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI bestScoreText;
    public Image[] lastDices;
    public DiceSkin defaultDiceSkin;

    public void Show(int round, int bestScore, List<DiceData> diceDatas, List<int> values)
    {
        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (roundText != null)
        {
            roundText.text = $"Round :  {round}";
        }
        if(bestScoreText != null)
        {
            bestScoreText.text = $"BestScore :  {bestScore}";
        }
        if (lastDices == null) return;

        for(int i = 0; i < lastDices.Length; i++)
        {
            if(i < values.Count)
            {
                lastDices[i].gameObject.SetActive(true);
                DiceData data = (diceDatas != null && i < diceDatas.Count) ? diceDatas[i] : null;
                int index = values[i];

                if(data != null && data.skin != null)
                {
                    lastDices[i].sprite = data.skin.GetSprite(index);
                }
                else if(defaultDiceSkin != null)
                {
                    lastDices[i].sprite = defaultDiceSkin.GetSprite(index);
                }
            }
        }
    }
    public void Hide()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
}

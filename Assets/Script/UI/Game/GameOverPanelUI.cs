using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameOverPanelUI : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameEndAnimation anim;
    public GameObject textGroup;
    public TextMeshProUGUI targetScoreText;
    public TextMeshProUGUI myScoreText;
    public Button LobbyBtn;
    public Button RetryBtn;

    public void Show(int round, int Score)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (targetScoreText != null)
        {
            targetScoreText.text = round.ToString();
        }
        if (myScoreText != null)
        {
            myScoreText.text = Score.ToString();
        }

        anim.Show().Forget();
    }

    public void Hide()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
}

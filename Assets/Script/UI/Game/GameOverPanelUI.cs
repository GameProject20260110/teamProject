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
    public Button LobbyBtn;
    public Button RetryBtn;

    public void Show(int round)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        anim.Show().Forget();
    }

    public void Hide()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
}

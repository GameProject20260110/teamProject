using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanelUI : MonoBehaviour
{
    public GameObject resultPanel;
    public CardRevealAnimator anim;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultTargetScoreText;
    public TextMeshProUGUI resultScoreText;
    public Button nextBtn;
    public Button retryBtn;

    public GameObject TextGroup;
    private bool _isListenerAdded = false;

    public void Show(bool isSuccess, int targetScore, int score, int currentLife)
    {
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
        anim.Reveal().Forget();
    }

    public void Hide()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
    }
}

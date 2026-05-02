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

    public void Show(bool isSuccess, int currentLife)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        if (resultTitleText != null)
        {
            resultTitleText.text = isSuccess ? "DEFENSE!" : "FAILED!";
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

using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameOverPanelUI : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameEndAnimation anim;
    public GameObject textGroup;
    [SerializeField] private TextMeshProUGUI descText;
    public Button titleBtn;
    [SerializeField] private Button quitBtn;

    [Header("승리/패배 문구")]
    [SerializeField] private string victoryDesc = "카오스를 물리쳤습니다\n 세상의 구조가 원래대로 돌아가고 있습니다.";
    [SerializeField] private string defeatDesc = "카오스에게 패배하였습니다.";

    private void Awake()
    {
        if(titleBtn)
        {
            titleBtn.onClick.AddListener(() => SceneController.instance.LoadTitleScene());
        }

        if (quitBtn)
        {
            quitBtn.onClick.AddListener(QuitGame);
        }
    }

    public void Show(bool isSuccess = false)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if(descText != null)
        {
            descText.text = isSuccess ? victoryDesc : defeatDesc;
        }
        anim.Show().Forget();
    }

    public void Hide()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

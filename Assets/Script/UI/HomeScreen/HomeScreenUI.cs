using UnityEngine;
using UnityEngine.UI;

public class HomeScreenUI : MonoBehaviour
{
    public static HomeScreenUI instance;

    [Header("Button")]
    [SerializeField] private Button StartBtn;

    //[Header("Text")]
    //[SerializeField] private TextMeshProUGUI bestRound;
    //[SerializeField] private TextMeshProUGUI bestScore;
    //[SerializeField] private TextMeshProUGUI GameClear;
    //[SerializeField] private TextMeshProUGUI totalGamePlayed;
    //[SerializeField] private Image playerImage;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        AudioManager.instance.PlayBgm(AudioManager.Bgm.Home, true);
        StartBtn.onClick.AddListener(() =>
        {
            SceneController.instance.LoadMapScene();
        });

        //bestRound.text = "최고 라운드: " + PlayerStatsManager.instance.bestRound.ToString();
        //bestScore.text = "최고 점수: " + PlayerStatsManager.instance.bestScore.ToString();
        //GameClear.text = "게임 클리어 수: " + PlayerStatsManager.instance.totalClears.ToString();
        //totalGamePlayed.text = "게임 플레이 수: " + PlayerStatsManager.instance.totalGamePlayed.ToString();
    }

}

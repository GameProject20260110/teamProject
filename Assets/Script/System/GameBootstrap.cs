using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("매니저 프리팹")]
    [SerializeField] private GameObject sceneControlPrefab;
    [SerializeField] private GameObject playerManagerPrefab;
    [SerializeField] private GameObject playerShopManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    [SerializeField] private GameObject settingsManagerPrefab;
    [SerializeField] private GameObject playerStatsManagerPrefab;

    private async UniTask Start()
    {
        Instantiate(playerStatsManagerPrefab);
        await UniTask.NextFrame();

        Instantiate(settingsManagerPrefab);
        await UniTask.NextFrame();

        Instantiate(playerManagerPrefab);
        await UniTask.NextFrame();

        Instantiate(playerShopManagerPrefab);
        await UniTask.NextFrame();

        Instantiate(audioManagerPrefab);
        await UniTask.NextFrame();

        Instantiate(sceneControlPrefab);
        await UniTask.NextFrame();

        if (!VaildateManagers()) return;

        Debug.Log("모든 매니저 초기화 완료");

        SceneController.instance.LoadTitleScene();
    }

    private bool VaildateManagers()
    {
        if (PlayerStatsManager.instance == null)
        {
            Debug.LogError("PlayerStatsManager 초기화 실패");
            return false;
        }
        if (SettingsManager.instance == null)
        {
            Debug.LogError("SettingsManager 초기화 실패");
            return false;
        }
        if (SceneController.instance == null)
        {
            Debug.LogError("SceneControl 초기화 실패");
            return false;
        }
        if (PlayerManager.instance == null)
        {
            Debug.LogError("PlayerManager 초기화 실패");
            return false;
        }
        if (PlayerShopManager.instance == null)
        {
            Debug.LogError("PlayerShopManager 초기화 실패");
            return false;
        }
        if (AudioManager.instance == null)
        {
            Debug.LogError("AudioManager 초기화 실패");
            return false;
        }
        return true;
    }
}

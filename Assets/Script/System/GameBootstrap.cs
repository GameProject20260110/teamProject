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

    private async UniTask Start()
    {
        Instantiate(settingsManagerPrefab);
        await UniTask.NextFrame(); // 다음 프레임까지 확실히 대기

        Debug.Log($"SettingsManager: {SettingsManager.instance}"); // null인지 확인

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

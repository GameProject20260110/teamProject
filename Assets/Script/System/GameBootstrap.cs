using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("매니저 프리팹")]
    [SerializeField] private GameObject sceneControlPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    [SerializeField] private GameObject settingsManagerPrefab;
    [SerializeField] private GameObject battleDataManagerPrefab;
    private async UniTask Start()
    {
        Instantiate(settingsManagerPrefab);
        await UniTask.NextFrame();

        Instantiate(audioManagerPrefab);
        await UniTask.NextFrame();

        Instantiate(sceneControlPrefab);
        await UniTask.NextFrame();

        Instantiate(battleDataManagerPrefab);
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
        if (AudioManager.instance == null)
        {
            Debug.LogError("AudioManager 초기화 실패");
            return false;
        }
        if (BattleDataManager.instance == null)
        {
            Debug.LogError("BattleDataManager 초기화 실패");
            return false;
        }
        return true;
    }
}

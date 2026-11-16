using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckSceneLoader
{
    public const string DeckSceneName = "DeckEdit";

    public static bool IsOpen { get; private set; }
    private static bool _isTransitioning;

    public static async UniTask OpenAsync()
    {
        if (IsOpen || _isTransitioning) return;
        _isTransitioning = true;

        try
        {
            if (!SceneManager.GetSceneByName(DeckSceneName).isLoaded)
                await SceneManager.LoadSceneAsync(DeckSceneName, LoadSceneMode.Additive).ToUniTask();

            IsOpen = true;

            if (DeckManagementSceneController.Instance != null)
                await DeckManagementSceneController.Instance.PlayOpenAsync();
        }
        catch(System.Exception e)
        {
            Debug.LogError($"[DeckSceneLoader] µ¦ ÆíÁý ¾À ¿­±â ½ÇÆÐ: {e.Message}");
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    public static async UniTask CloseAsync()
    {
        if (!IsOpen || _isTransitioning) return;
        _isTransitioning = true;

        try
        {
            if (DeckManagementSceneController.Instance != null)
                await DeckManagementSceneController.Instance.PlayCloseAsync();

            Scene scene = SceneManager.GetSceneByName(DeckSceneName);
            if (scene.isLoaded)
                await SceneManager.UnloadSceneAsync(scene).ToUniTask();

            IsOpen = false;
        }

        catch (System.Exception e)
        {
            Debug.LogError($"[DeckSceneLoader] µ¦ ÆíÁý ¾À ´Ý±â ½ÇÆÐ : {e.Message}");
        }
        finally
        {
            _isTransitioning = false;
        }
    }
}

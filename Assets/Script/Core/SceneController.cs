using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;
using TMPro;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    public const string SceneTitle = "Title";
    public const string SceneHome = "HomeScreen";
    public const string SceneBattle = "GameBoard";
    public const string SceneShop = "Shop";

    [Header("로딩 패널")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("페이드 패널")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.4f;

    private SceneInstance? currentAddressableScene;

    public bool IsTransitioning { get; private set; }

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 0f;
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (SettingsManager.instance != null)
            SettingsManager.instance.InitializeUI();
    }

    private void OnEnable()
    {
        if (PlayerShopManager.instance != null)
            PlayerShopManager.instance.OnShopCommitted += LoadGameScene;
    }

    private void OnDisable()
    {
        if (PlayerShopManager.instance != null)
            PlayerShopManager.instance.OnShopCommitted -= LoadGameScene;
    }
    
    private void OnValidate()
    {
        if (fadeCanvasGroup == null)
            Debug.LogWarning("fadeCanvasGroup이 비어있습니다.");
        if (loadingPanel == null)
            Debug.LogWarning("loadingPanel이 비어있습니다.");
    }

    public void ReloadCurrentScene() => LoadAsync(SceneManager.GetActiveScene().name).Forget();
    public void LoadGameScene() => LoadAsync(SceneBattle).Forget(); 
    public void LoadHomeScene() => LoadAsync(SceneHome).Forget();
    public void LoadTitleScene() => LoadAsync(SceneTitle).Forget();
    public void LoadShopScene()
    {
        PlayerShopManager.instance.Open();
        LoadAsync(SceneShop).Forget();
    }


    private async UniTask LoadAsync(string sceneName)
    {
        if (IsTransitioning) return;

        IsTransitioning = true;

        try
        {
            await FadeAsync(0f, 1f);

            SetLoadingUI(true, 0f);

            if (sceneName == SceneShop || sceneName == SceneBattle)
            {
                await LoadAddressableSceneAsync(sceneName);
            }
            else
            {
                await LoadNormalSceneAsync(sceneName);
            }

            SetLoadingUI(false, 0f);
            await FadeAsync(1f, 0f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"씬 로드 실패 ({sceneName}): {e.Message}");
            SetLoadingUI(false, 0f);
        }
        finally
        {
            IsTransitioning = false;
        }
    }

    private async UniTask LoadAddressableSceneAsync(string sceneName)
    {
        // 이전 Addressable 씬 언로드
        if (currentAddressableScene.HasValue)
        {
            await Addressables.UnloadSceneAsync(currentAddressableScene.Value);
            currentAddressableScene = null;
        }

        AsyncOperationHandle<SceneInstance> handle =
            Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single, false);

        while (!handle.IsDone)
        {
            SetLoadingUI(true, Mathf.Clamp01(handle.PercentComplete));
            await UniTask.Yield();
        }

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            SetLoadingUI(true, 1f);
            await UniTask.Delay(300);

            await handle.Result.ActivateAsync();
            currentAddressableScene = handle.Result;
        }
        else
        {
            Debug.LogError($"Addressable 씬 로드 실패: {sceneName}");
            Addressables.Release(handle);
        }
    }

    private async UniTask LoadNormalSceneAsync(string sceneName)
    {
        // 기존 Addressable 씬 언로드
        if (currentAddressableScene.HasValue)
        {
            await Addressables.UnloadSceneAsync(currentAddressableScene.Value);
            currentAddressableScene = null;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName); // 로드만
        op.allowSceneActivation = false; // 100% -> 0.9f

        while (op.progress < 0.9f)
        {
            SetLoadingUI(true, Mathf.Clamp01(op.progress / 0.9f));
            await UniTask.Yield();
        }

        SetLoadingUI(true, 1f);
        await UniTask.Delay(300);

        op.allowSceneActivation = true;
        await UniTask.WaitUntil(() => op.isDone);
    }

    private void SetLoadingUI(bool visible, float progress)
    {
        if (loadingPanel != null) loadingPanel.SetActive(visible);
        if (progressBar != null) progressBar.value = progress;
        if (progressText != null) progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
    }

    private async UniTask FadeAsync(float from, float to)
    {
        if (fadeCanvasGroup == null) return;

        float elapsed = 0f;
        fadeCanvasGroup.alpha = from;
        fadeCanvasGroup.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            await UniTask.Yield();
        }

        fadeCanvasGroup.alpha = to;
        fadeCanvasGroup.blocksRaycasts = to > 0f;
    }

    
}

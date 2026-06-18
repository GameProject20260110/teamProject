using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    public const string SceneTitle = "Title";
    public const string SceneBattle = "GameBoard";
    public const string SceneMap = "Map";
    public const string ShopScene = "Shop";

    [Header("로딩 패널(타이틀 -> 맵)")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("로딩 연출 속도 제어")]
    [SerializeField] private float loadingSmoothSpeed = 1.2f; // 숫자가 낮을 시 느림
    [SerializeField] private float finalPushSpeed = 0.3f; // 숫자가 낮을 시 더 지연


    [Header("스테이지 전환용(맵 -> 스테이지)")]
    [SerializeField] private Image wipeImage;
    [SerializeField] private float wipeDuration = 0.4f;

    private SceneInstance? currentAddressableScene;

    public bool IsTransitioning { get; private set; }

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);


        if(wipeImage != null)
        {
            wipeImage.fillAmount = 0f;
            wipeImage.gameObject.SetActive(false);
        }
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }
    
    private void OnValidate()
    {
        if (wipeImage == null)
            Debug.LogWarning("wipeImage가 비어있습니다.");
        if (loadingPanel == null)
            Debug.LogWarning("loadingPanel이 비어있습니다.");
    }
    public void ReloadCurrentScene() => LoadAsync(SceneManager.GetActiveScene().name, false).Forget();

    // 로딩창 (타이틀 -> 맵)
    public void LoadTitleScene() => LoadAsync(SceneTitle, true).Forget();
    public void LoadMapFromTitle() => LoadAsync(SceneMap, true).Forget();

    // 컷 인/아웃 (맵 -> 스테이지)
    public void LoadGameScene() => LoadAsync(SceneBattle, false).Forget(); 
    public void LoadMapScene() => LoadAsync(SceneMap, false).Forget();
    public void LoadShopScene() => LoadAsync(ShopScene, false).Forget();

    private async UniTask LoadAsync(string sceneName, bool showLoadingUI)
    {
        if (IsTransitioning) return;
        IsTransitioning = true;

        try
        {
            if(showLoadingUI)
            {
                SetLoadingUI(true, 0f);
            }
            else
            {
                await WipeAsync(isCovering: true);
            }

            await LoadNormalSceneAsync(sceneName, showLoadingUI);

            if(showLoadingUI)
            {
                SetLoadingUI(false, 0f);
            }
            else
            {
                await WipeAsync(isCovering: false);
            }
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

    private async UniTask LoadNormalSceneAsync(string sceneName, bool updateUI)
    {
        // 기존 Addressable 씬 언로드
        if (currentAddressableScene.HasValue)
        {
            await Addressables.UnloadSceneAsync(currentAddressableScene.Value);
            currentAddressableScene = null;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName); // 로드만
        op.allowSceneActivation = false; // 100% -> 0.9f

        float fakeProgress = 0f;

        while (op.progress < 0.9f || fakeProgress < 1f)
        {
            if(updateUI)
            {
                float targetProgress = Mathf.Clamp01(op.progress / 0.9f);

                fakeProgress = Mathf.Lerp(fakeProgress, targetProgress, Time.unscaledDeltaTime * loadingSmoothSpeed);

                if(op.progress >= 0.9f)
                {
                    fakeProgress = Mathf.MoveTowards(fakeProgress, 1f, Time.unscaledDeltaTime * finalPushSpeed);
                }
                SetLoadingUI(true, fakeProgress);
            }
            else
            {
                if (op.progress >= 0.9f) break;
            }
            await UniTask.Yield();
        }

        if (updateUI) SetLoadingUI(true, 1f);
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

    private async UniTask WipeAsync(bool isCovering)
    {
        if (wipeImage == null) return;

        wipeImage.gameObject.SetActive(true);

        if(isCovering)
        {
            wipeImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            wipeImage.fillAmount = 0f;

            await wipeImage.DOFillAmount(1f, wipeDuration)
                .SetUpdate(true)
                .SetEase(Ease.InOutCubic)
                .ToUniTask();
        }
        else
        {
            wipeImage.fillOrigin = (int)Image.OriginHorizontal.Right;
            wipeImage.fillAmount = 1f;

            await wipeImage.DOFillAmount(0f, wipeDuration)
                .SetUpdate(true)
                .SetEase(Ease.InOutCubic)
                .ToUniTask();

            wipeImage.gameObject.SetActive(false);
        }
    }
    
}

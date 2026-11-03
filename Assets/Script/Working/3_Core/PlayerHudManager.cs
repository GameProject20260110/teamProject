using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

public class PlayerHudManager : MonoBehaviour, IInitializable
{
    [Header("Visibility")]
    [SerializeField] private GameObject hudRoot;

    [Header("Rect")]
    [SerializeField] private Button inventory;
    [SerializeField] private TextMeshProUGUI goldText;

    public Button Inventory => inventory;
    public TextMeshProUGUI GoldText => goldText;

    private SceneController _sceneController;
    private ResourceManager _resourceManager;

    [Inject]
    public void Construct(SceneController sceneController, ResourceManager resourceManager)
    {
        _sceneController = sceneController;
        _resourceManager = resourceManager;
    }

    public void Initialize()
    {
        Debug.Log("[PlayerHudManager] Initialize È£ÃâµÊ!");
    }

    private void Awake()
    {
        inventory.onClick.AddListener(() =>
        { 
            Debug.Log($"[HUD] °¡¹æ Å¬¸¯ / IsOpen={DeckSceneLoader.IsOpen} / DeckEdit ·ÎµåµÊ={SceneManager.GetSceneByName("DeckEdit").isLoaded}");
            DeckSceneLoader.OpenAsync().Forget();
        });
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateVisibility();
        RefreshGold();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateVisibility();
        RefreshGold();
    }

    private void UpdateVisibility()
    {
        var target = hudRoot != null ? hudRoot : gameObject;
        target.SetActive(_sceneController.ShowHud);
    }

    private void RefreshGold()
    {
        if (goldText == null || _resourceManager == null) return;
        goldText.text = _resourceManager.gold.ToString();
    }
}

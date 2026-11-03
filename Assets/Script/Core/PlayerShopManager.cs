using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class PlayerShopManager : MonoBehaviour
{
    [SerializeField] private string ShopBGMKey;

    [Header("UI References")]
    [SerializeField] private GameObject shopCanvas;
    [SerializeField] private ShopPanelAnimator shopAnimator;

    private AudioManager _audioManager;
    private ShopSession _session;

    [Inject]
    public void Construct(AudioManager audioManager, ShopSession session)
    {
        _audioManager = audioManager;
        _session = session;
    }

    private void Start()
    {
        OpenWithAnimation().Forget();
        _audioManager.PlayBgm(ShopBGMKey);
    }

    public async UniTaskVoid OpenWithAnimation()
    {
        _session.Open();
        shopCanvas.SetActive(true);
        if (shopAnimator != null) await shopAnimator.Show();
    }

    public async UniTask CommitWithAnimation()
    {
        _session.Commit();
        if (shopAnimator != null) await shopAnimator.Hide();
    }

    public void Discard()
    {
        _session.Discard();
        if (shopAnimator != null) shopAnimator.Hide().Forget();
    }
}
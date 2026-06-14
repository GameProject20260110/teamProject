using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceRewardAnimator : MonoBehaviour
{
    [Header("파티클")]
    [SerializeField] private ParticleSystem diceRewardParticle;

    [Header("후광")]
    [SerializeField] private Image glowEffect;
    private Material _haloMaterial;

    [Header("주사위")]
    [SerializeField] private Image diceImage;
    [SerializeField] private TextMeshProUGUI diceNameText;
    private float _originalDiceY;

    [Header("버튼")]
    [SerializeField] private CanvasGroup buttonGroup;

    [Header("타이밍")]
    [SerializeField] private float particleDuration = 1f;
    [SerializeField] private float glowDuration = 1f;
    [SerializeField] private float diceAppearDuration = 0.5f;


    private void Awake()
    {
        if(glowEffect != null)
        {
            _haloMaterial = Instantiate(glowEffect.material);
            glowEffect.material = _haloMaterial;
        }

        if (diceImage != null)
            _originalDiceY = diceImage.transform.localPosition.y;
    }

    private void Initialized()
    {
        diceRewardParticle.Stop();
        diceRewardParticle.Clear();

        _haloMaterial.SetFloat("_Radius", 0f);
        glowEffect.gameObject.SetActive(false);

        diceImage.gameObject.SetActive(false);
        diceImage.transform.localScale = Vector3.zero;
        var diceColor = diceImage.color;
        diceColor.a = 0f;
        diceImage.color = diceColor;

        var pos = diceImage.transform.localPosition;
        pos.y = _originalDiceY;
        diceImage.transform.localPosition = pos;

        var nameColor = diceNameText.color;
        nameColor.a = 0f;
        diceNameText.color = nameColor;

        buttonGroup.alpha = 0f;
        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;
    }

    public async UniTask PlayAnimation()
    {
        Initialized();
        await PlayParticle();
        await UniTask.Delay(400, cancellationToken: this.GetCancellationTokenOnDestroy());
        PlayHalo().Forget();
        await PlayDiceAppear();
        await PlayUI();
    }

    private async UniTask PlayParticle()
    {
        diceRewardParticle.Play();
        await UniTask.Delay((int)(particleDuration * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());
        diceRewardParticle.Stop();
    }

    private async UniTask PlayHalo()
    {
        glowEffect.gameObject.SetActive(true);
        _haloMaterial.SetFloat("_Radius", 0f);

        await DOTween.To(
            () => _haloMaterial.GetFloat("_Radius"),
            x => _haloMaterial.SetFloat("_Radius", x),
            1f, glowDuration
        ).SetEase(Ease.OutQuart)
        .SetLink(gameObject)
        .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    private async UniTask PlayDiceAppear()
    {
        diceImage.gameObject.SetActive(true);

        var diceColor = diceImage.color;
        diceColor.a = 0.3f;
        diceImage.color = diceColor;
        diceImage.transform.localScale = Vector3.one * 0.3f;

        diceImage.DOFade(1f, diceAppearDuration).SetLink(gameObject);
        await diceImage.transform
            .DOScale(Vector3.one, diceAppearDuration)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject)
            .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

        diceImage.transform
            .DOLocalMoveY(_originalDiceY + 15f, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    private async UniTask PlayUI()
    {
        diceNameText.DOFade(1f, 0.5f).SetLink(gameObject);
        await UniTask.Delay(200, cancellationToken: this.GetCancellationTokenOnDestroy());

        buttonGroup.DOFade(1f, 0.5f).SetLink(gameObject);
        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;
    }

    public void StopFloating()
    {
        diceImage?.transform.DOKill();
    }

    private void OnDestroy()
    {
        if(_haloMaterial != null) 
            Destroy(_haloMaterial);
    }
}



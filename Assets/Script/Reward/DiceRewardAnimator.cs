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

    [Header("버튼")]
    [SerializeField] private CanvasGroup buttonGroup;

    [Header("타이밍")]
    [SerializeField] private float particleDuration = 1f;
    [SerializeField] private float glowDuration = 1f;
    [SerializeField] private float diceAppearDuration = 0.5f;

#if UNITY_EDITOR
    [ContextMenu("테스트 애니메이션")]
    private void TestAnimation()
    {
        PlayAnimation().Forget();
    }

#endif

    private void Awake()
    {
        if(glowEffect != null)
        {
            _haloMaterial = Instantiate(glowEffect.material);
            glowEffect.material = _haloMaterial;
        }
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
        await UniTask.Delay(400);
        await PlayHalo();
        await PlayDiceAppear();
        await PlayUI();
    }

    private async UniTask PlayParticle()
    {
        diceRewardParticle.Play();
        await UniTask.Delay((int)(particleDuration * 1000));
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
        ).SetEase(Ease.OutQuart).AsyncWaitForCompletion();
    }

    private async UniTask PlayDiceAppear()
    {
        diceImage.gameObject.SetActive(true);

        var diceColor = diceImage.color;
        diceColor.a = 0.3f;
        diceImage.color = diceColor;
        diceImage.transform.localScale = Vector3.one * 0.3f;

        diceImage.DOFade(1f, diceAppearDuration);
        await diceImage.transform
            .DOScale(Vector3.one, diceAppearDuration)
            .SetEase(Ease.OutBack)
            .AsyncWaitForCompletion();

        diceImage.transform
            .DOLocalMoveY(diceImage.transform.localPosition.y + 15f, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private async UniTask PlayUI()
    {
        diceNameText.DOFade(1f, 0.5f);
        await UniTask.Delay(200);

        buttonGroup.DOFade(1f, 0.5f);
        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;

        await UniTask.Delay(300);
    }

    public void StopFloating()
    {
        diceImage?.transform.DOKill();
    }

    private void OnDestroy()
    {
        diceImage?.transform.DOKill();
        if(_haloMaterial != null) 
            Destroy(_haloMaterial);
    }
}



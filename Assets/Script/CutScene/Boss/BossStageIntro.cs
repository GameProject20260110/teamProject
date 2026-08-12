using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Playables;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;

public class BossStageIntro : MonoBehaviour
{
    [SerializeField] private GameObject bossIntroCanvas;
    [SerializeField] private CanvasGroup fadePanel; // 임시
    
    [Header("배경")]
    [SerializeField] private GameObject baseBackground;
    [SerializeField] private GameObject bossBackground;

    [Header("카메라")]
    [SerializeField] private CinemachineCamera cutsceneCam;
    [SerializeField] private CinemachineCamera defaultCam;

    [Header("Bloom 제어")]
    [SerializeField] private Volume cutsceneVolume;

    [Header("핸더랜드 연출")]
    [SerializeField] private RectTransform trans1;
    [SerializeField] private RectTransform trans2;
    [SerializeField] private RectTransform trans3;
    [SerializeField] private Image bossEyeSprite;
    [SerializeField] private CanvasGroup bossEyeGroup;
    [SerializeField] private int eyeAppearCount = 3;
    [SerializeField] private float[] eyeSpriteSizes = { 300f, 500f, 800f };
    [SerializeField] private float[] eyeFadeInDuration = { 0.3f, 0.35f, 0.45f };
    [SerializeField] private float[] eyeHoldDuration = { 0.4f, 0.3f, 0.2f };
    [SerializeField] private float[] eyeFadeOutDuration = { 0.3f, 0.35f, 0.45f };
    [SerializeField] private float eyeShakeStrengh = 5f;
    [SerializeField] private float eyeImpulseForce = 0.5f;

    [Header("스포트라이트 + 백광")]
    [SerializeField] private SpriteRenderer spotLight;
    [SerializeField] private SpriteRenderer whiteFlashRenderer;
    [SerializeField] private float spotLightExpandDuration = 1f;
    [SerializeField] private float whiteFlashHoldDuration = 0.5f;
    [SerializeField] private float whiteFlashFadeOutDuration = 0.4f;
    [SerializeField] private float eyeToSpotlightDelaysMs = 300f;
    [SerializeField] private float bloomIntersityDuringlash = 5f;

    [Header("효과음")]
    [SerializeField] private string spotLightSfxKey;
    [SerializeField] private string laughSfxKey;
    [SerializeField] private float[] laughPitches = { 1f, 1.15f, 1.3f };

    private CinemachineImpulseSource _impulseSource;
    // 스킵
    private CancellationTokenSource _skipCts;
    private RectTransform[] EyeTrans => new[] { trans1, trans2, trans3 };
    private Bloom _bloom;


    private BattleDataManager _battleDataManager;
    private AudioManager _audioManager;

    [Inject]
    public void Construct(BattleDataManager battleDataManager, AudioManager audioManager)
    {
        _battleDataManager = battleDataManager;
        _audioManager = audioManager;
    }

    private void Awake()
    {
        _impulseSource = cutsceneCam?.GetComponent<CinemachineImpulseSource>();

        if (bossIntroCanvas != null) bossIntroCanvas.SetActive(false);
        if (bossBackground != null) bossBackground.SetActive(false);
        if (cutsceneCam != null) cutsceneCam.Priority = 0;
        if (defaultCam != null) defaultCam.Priority = 10;
        if (cutsceneVolume != null) cutsceneVolume.profile.TryGet(out _bloom);
    }


    public async UniTask Play(CancellationToken ct)
    {
        var bossData = _battleDataManager.currentEnemyData as BossDataSo;
        if (bossData == null) return;

        _skipCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        MainOption.instance?.SetSettingsButtonActive(false);
        bossIntroCanvas?.SetActive(true);
        if (cutsceneCam != null) cutsceneCam.Priority = 20;
        if (cutsceneVolume != null) cutsceneVolume.gameObject.SetActive(true);
        _audioManager.StopBgm();

        await PlayMoveEyes(_skipCts.Token);
        await UniTask.Delay(500, cancellationToken: ct);
        await PlaySpotlightReveal(_skipCts.Token);
        _audioManager?.PlayBgm("Boss");

        // 설정버튼 활성화 + 캔버스 비활성화
        MainOption.instance?.SetSettingsButtonActive(true);
        bossIntroCanvas.SetActive(false);

        if (cutsceneCam != null) cutsceneCam.Priority = 0;
        if (cutsceneVolume != null) cutsceneVolume.gameObject.SetActive(false);
    }

    private async UniTask PlayMoveEyes(CancellationToken ct)
    {
        bossEyeGroup.gameObject.SetActive(true);
        var eyeTrans = EyeTrans;
        for(int i = 0; i < eyeAppearCount; i++)
        {
            RectTransform targetPos = eyeTrans[i % eyeTrans.Length];

            bossEyeSprite.rectTransform.position = targetPos.position;
            bossEyeSprite.rectTransform.sizeDelta = Vector2.one * eyeSpriteSizes[i];
            bossEyeGroup.alpha = 0f;

            _impulseSource?.GenerateImpulse(eyeImpulseForce);
            _audioManager?.PlaySfx(laughSfxKey, laughPitches[i]);
            await bossEyeGroup.DOFade(1f, eyeFadeInDuration[i]).SetEase(Ease.OutBack).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: ct);

            bossEyeSprite.rectTransform.DOShakePosition(eyeHoldDuration[i], eyeShakeStrengh);

            await UniTask.Delay((int)(eyeHoldDuration[i] * 1000), cancellationToken: ct);

            await bossEyeGroup.DOFade(0f, eyeFadeOutDuration[i]).SetEase(Ease.InBack).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: ct);

            await UniTask.Delay(1000, cancellationToken: ct);
        }

        bossEyeGroup.gameObject.SetActive(false);
    }

    private async UniTask PlaySpotlightReveal(CancellationToken ct)
    {
        await UniTask.Delay((int)eyeToSpotlightDelaysMs, cancellationToken: ct);

        float halfWorldWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float taregtWidth = halfWorldWidth * 2f;

        float spotTargetScaleX = taregtWidth / spotLight.sprite.bounds.size.x;
        float flashTargetScaleX = taregtWidth / whiteFlashRenderer.sprite.bounds.size.x;

        spotLight.gameObject.SetActive(true);
        whiteFlashRenderer.gameObject.SetActive(true);
        _audioManager.PlaySfx(spotLightSfxKey);
        SetAlpha(whiteFlashRenderer, 0f);


        await UniTask.Delay(1000, cancellationToken: ct);
           
        Sequence seq = DOTween.Sequence();
        seq.Join(spotLight.transform.DOScaleX(spotTargetScaleX, spotLightExpandDuration).SetEase(Ease.OutQuad));
        seq.Join(whiteFlashRenderer.transform.DOScaleX(flashTargetScaleX, spotLightExpandDuration).SetEase(Ease.OutQuad));
        seq.Join(FadeSpriteRenderer(whiteFlashRenderer, 1f, spotLightExpandDuration));

        if (_bloom != null)
            seq.Join(DOTween.To(() => _bloom.intensity.value,
                v => _bloom.intensity.value = v,
                bloomIntersityDuringlash, spotLightExpandDuration));

        await seq.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: ct);

        await UniTask.Delay((int)(whiteFlashHoldDuration * 1000), cancellationToken: ct);

        if (bossBackground != null) bossBackground.gameObject.SetActive(true);

        Sequence fadeOutSeq = DOTween.Sequence();
        fadeOutSeq.Join(FadeSpriteRenderer(whiteFlashRenderer, 0f, whiteFlashFadeOutDuration));
        if(_bloom != null)
            fadeOutSeq.Join(DOTween.To(() => _bloom.intensity.value,
                v => _bloom.intensity.value = v,
                0f, whiteFlashFadeOutDuration
                ));

        await fadeOutSeq.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: ct);

        spotLight.gameObject.SetActive(false);
        whiteFlashRenderer.gameObject.SetActive(false);

    }

    private Tween FadeSpriteRenderer(SpriteRenderer sr, float targetAlpha, float duration)
    {
        return DOTween.To(() => sr.color.a,
            a => SetAlpha(sr, a),
            targetAlpha, duration);
    }

    private void SetAlpha(SpriteRenderer sr, float alpha)
    {
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }


    private void OnDestroy()
    {
        _skipCts?.Cancel();
       _skipCts?.Dispose();
    }

}

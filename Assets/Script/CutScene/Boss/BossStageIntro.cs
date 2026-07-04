using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Playables;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Runtime.InteropServices;

public class BossStageIntro : MonoBehaviour
{
    public static BossStageIntro instance;

    [SerializeField] private GameObject bossIntroCanvas;
    [SerializeField] private CanvasGroup fadePanel; // 임시
    

    [Header("배경")]
    [SerializeField] private GameObject normalBackground;
    [SerializeField] private GameObject normalCrackBackground;
    [SerializeField] private GameObject bossBackground;

    [Header("공")]
    [SerializeField] private GameObject ballObject;

    [Header("충돌 연출")]
    [SerializeField] private CanvasGroup whiteFlashPanel;
    [SerializeField] private ParticleSystem crackParticle;

    [Header("와이프")]
    [SerializeField] private Image wipePanel;

    [Header("충돌 설정")]
    [SerializeField] private float flashDuration = 0.05f;
    [SerializeField] private float flashOutDuration = 0.15f;
    [SerializeField] private float impulseForce = 3f;

    [Header("타임라인")]
    [SerializeField] private PlayableDirector director;
    [SerializeField] private PlayableAsset bossIntroTimeline;

    [Header("카메라")]
    [SerializeField] private CinemachineCamera cutsceneCam;
    [SerializeField] private CinemachineCamera defaultCam;

    [Header("Bloom 제어")]
    [SerializeField] private Volume cutsceneVolume;

    [Header("보스")]
    [SerializeField] private Transform bossTrans;
    [SerializeField] private GameObject boss;

    [Header("컷신 정리")]
    [SerializeField] private GameObject blackExpandPanel;
    [SerializeField] private GameObject spotLight;
    [SerializeField] private GameObject explosionParticle;
    [SerializeField] private GameObject letterBoxUI;

    private CinemachineImpulseSource _impulseSource;
    // 스킵
    private CancellationTokenSource _skipCts;
    private UniTaskCompletionSource _timelineTcs;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        _impulseSource = cutsceneCam?.GetComponent<CinemachineImpulseSource>();

        if (bossIntroCanvas != null) bossIntroCanvas.SetActive(false);
        if (bossBackground != null) bossBackground.SetActive(false);
        if (normalCrackBackground != null) normalCrackBackground.SetActive(false);
        if (ballObject != null) ballObject.SetActive(false);
        if (cutsceneCam != null) cutsceneCam.Priority = 0;
        if (defaultCam != null) defaultCam.Priority = 10;
        
    }


    public async UniTask Play(CancellationToken ct)
    {
        var bossData = BattleDataManager.instance.currentEnemyData as BossDataSo;
        if (bossData == null) return;

        _skipCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        MainOption.instance?.SetSettingsButtonActive(false);
        bossIntroCanvas?.SetActive(true);
        ballObject.SetActive(true);

        if (cutsceneCam != null) cutsceneCam.Priority = 20;
        if (cutsceneVolume != null) cutsceneVolume.gameObject.SetActive(true);


        // timeline 재생
        director.playableAsset = bossIntroTimeline;
        director.Play();
        await WaitForTimelineSignal(_skipCts.Token);

        if (bossData.appearDialogues != null && bossData.appearDialogues.Length > 0) 
            await BossDialogueUI.instance.ShowDialogues(bossData.appearDialogues, bossTrans, _skipCts.Token);

        
        director.Play();
        await WaitForTimelineSignal(_skipCts.Token);

        AudioManager.instance?.PlayBgm("Boss");
        await WipeIn(_skipCts.Token);

        OnBossStageReveal();

        await UniTask.Delay(1500, cancellationToken: _skipCts.Token);
        await WipeOut(_skipCts.Token);
        // 설정버튼 활성화 + 캔버스 비활성화
        MainOption.instance?.SetSettingsButtonActive(true);
        bossIntroCanvas.SetActive(false);

        if (cutsceneCam != null) cutsceneCam.Priority = 0;
        if (cutsceneVolume != null) cutsceneVolume.gameObject.SetActive(false);
    }

    public void OnImpactSignal()
    {
        _impulseSource?.GenerateImpulse(impulseForce);
        if (crackParticle != null) crackParticle.Play(); ;
    }

    public void SwapToBackground()
    {
        if (normalBackground != null) normalBackground.SetActive(false);
        if (normalCrackBackground != null) normalCrackBackground.SetActive(true);
        if (cutsceneCam != null) cutsceneCam.Target.TrackingTarget = null;
    }

    public void OnBossTransform()
    {
        if (ballObject != null) ballObject.SetActive(false);
        if (boss != null) boss.SetActive(true);
    }

    public void OnTimelineComplete()
    {
        _timelineTcs?.TrySetResult();
    }

    public void OnDialoguePause()
    {
        director.Pause();
        _timelineTcs?.TrySetResult();
    }


    public void OnBossStageReveal()
    {
        if (bossBackground != null) bossBackground.SetActive(true);
        if (normalCrackBackground != null) normalCrackBackground.SetActive(false);

        if (letterBoxUI != null) letterBoxUI.SetActive(false);
        if (blackExpandPanel != null) blackExpandPanel.SetActive(false);
        if (spotLight != null) spotLight.SetActive(false);
        if (boss != null) boss.SetActive(false);
        if (crackParticle != null) Destroy(crackParticle.gameObject);
        if (explosionParticle != null) Destroy(explosionParticle.gameObject);
    }

    private async UniTask WipeIn(CancellationToken ct)
    {
        wipePanel.fillAmount = 0f;
        wipePanel.gameObject.SetActive(true);
        await wipePanel.DOFillAmount(1f, 0.4f)
            .SetEase(Ease.OutQuad)
            .ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: ct);
    }

    private async UniTask WipeOut(CancellationToken ct)
    {
        await wipePanel.DOFillAmount(0f, 0.4f)
            .SetEase(Ease.InQuad)
            .ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: ct);
        wipePanel.gameObject.SetActive(false);
    }

    private UniTask WaitForTimelineSignal(CancellationToken ct)
    {
        _timelineTcs = new UniTaskCompletionSource();
        ct.Register(() =>
        {
            _timelineTcs.TrySetCanceled();
        });
        return _timelineTcs.Task;
    }

    private void OnDestroy()
    {
        _skipCts?.Cancel();
       _skipCts?.Dispose();
    }

}

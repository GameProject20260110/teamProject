using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Playables;
using Unity.VisualScripting;

public class BossStageIntro : MonoBehaviour
{
    public static BossStageIntro instance;

    [SerializeField] private GameObject bossIntroCanvas;
    [SerializeField] private Transform bossTarget;
    [SerializeField] private CanvasGroup fadePanel; // 임시
    [SerializeField] private Image wipePanel;

    [Header("배경")]
    [SerializeField] private GameObject normalBackground;
    [SerializeField] private GameObject bossBackground;

    [Header("타임라인")]
    [SerializeField] private PlayableDirector director;

    [Header("playableAsset")]
    [SerializeField] private PlayableAsset bossIntro01;


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (bossIntroCanvas != null)
            bossIntroCanvas.SetActive(false);

        bossBackground.SetActive(false);
    }


    public async UniTask Play(CancellationToken ct)
    {
        var bossData = BattleDataManager.instance.currentEnemyData as BossDataSo;
        if (bossData == null) return;

        // 1. 설정 버튼 비활성화 + 캔버스 활성화
        MainOption.instance?.SetSettingsButtonActive(false);
        bossIntroCanvas?.SetActive(true);

        // 2. BossIntro_01 재생
        director.playableAsset = bossIntro01;
        director.Play();
        await UniTask.WaitUntil(() => director.state == PlayState.Paused, cancellationToken: ct);


        // 박수 대기와 동시에 보스 등장
        await UniTask.Delay(500, cancellationToken: ct);

        // 모든 효과음/bgm off

        // 보스 대사
        if (bossData.appearDialogues != null && bossData.appearDialogues.Length > 0)
        {
            await BossDialogueUI.instance.ShowDialogues(bossData.appearDialogues, bossTarget, ct);
        }

        // 더치 앵글 + 줌 + 화면 흔들림(3회정도) / 마지막은 와이드되면서 원래 각도 복귀


        // 와이프 + bgm 시작
        AudioManager.instance?.PlayBgm("Boss");
        await WipeIn(ct);
        await UniTask.Delay(1500, cancellationToken: ct);
        await WipeOut(ct);

        // 설정버튼 활성화 + 캔버스 비활성화
        MainOption.instance?.SetSettingsButtonActive(true);
        bossIntroCanvas.SetActive(false);
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


}

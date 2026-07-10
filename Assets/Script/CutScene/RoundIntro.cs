using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using VContainer;

public class RoundIntro : MonoBehaviour
{
    [SerializeField] private CardAppearEffect RoundEffect;

    [Header("UI 요소")]
    [SerializeField] private GameObject visualsRoot;
    [SerializeField] private Image dimmer;
    [SerializeField] private RectTransform spear1Rect;
    [SerializeField] private RectTransform spear2Rect;
    [SerializeField] private Image spear1Image;
    [SerializeField] private Image spear2Image;
    [SerializeField] private RectTransform vsTextRect;
    [SerializeField] private TextMeshProUGUI vsText;
    [SerializeField] private Image blueBackground;
    [SerializeField] private Image redBackground;

    [Header("캐릭터 소환")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private GameObject playerIntroPrefab;

    [Header("VS 파티클")]
    [SerializeField] private GameObject energyBurst;
    [SerializeField] private ParticleSystem oraAura;

    [Header("Dimmer")]
    [SerializeField] private float dimmerTargetAlpha = 0.65f;
    [SerializeField] private float dimmerDuration = 0.4f;

    [Header("Spear 설정")]
    [SerializeField] private float spearOffscreenDistance = 1400f;
    [SerializeField] private float spear1Duration = 0.3f;
    [SerializeField] private float spear2Duration = 0.3f;
    [SerializeField] private float spearDelay = 0.3f;
    [SerializeField] private Ease spearEase = Ease.OutQuint;
    [SerializeField] private Vector2 crossPoint = Vector2.zero;

    [Header("충돌 효과")]
    [SerializeField] private float screenShakeStrength = 20f;
    [SerializeField] private float screenShakeDuration = 0.35f;

    [Header("VS 텍스트")]
    [SerializeField] private float vsAppearDuration = 0.25f;
    [SerializeField] private float vsStartScale = 5f;
    [SerializeField] private float bounceDuration = 0.1f;

    [Header("캐릭터 / 배경")]
    [SerializeField] private float characterBackFadeInDuration = 0.5f;
    [SerializeField] private float characterFadeInDuration = 0.5f;
    [SerializeField] private float characterSlideDistance = 3f;
    [SerializeField] private float characterFadeInDelay = 0.1f;

    [Header("아웃트로")]
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private float outroFadeDuration = 0.4f;

    private Sequence currentSequence;

    private Vector3 playerCharacterOriginPos;
    private Vector3 enemyCharacterOriginPos;

    private GameObject _spawnedPlayer;
    private GameObject _spawnedEnemy;

    private BattleDataManager _battleDataManager;
    private AudioManager _audioManager;

    [Inject]
    public void Construct(BattleDataManager battleDataManager, AudioManager audioManager)
    {
        _battleDataManager = battleDataManager;
        _audioManager = audioManager;
    }

    void Awake()
    {
        if (visualsRoot != null) visualsRoot.SetActive(false);
        ResetDimmer();

        if (playerSpawnPoint != null)
        {
            playerCharacterOriginPos = playerSpawnPoint.position;
            Debug.Log($"Player Origin: {playerCharacterOriginPos}");
        }

        if (enemySpawnPoint != null)
            enemyCharacterOriginPos = enemySpawnPoint.position;
    }

    public void Play() => Play(null);

    public void Play(Action onComplete)
    {
        KillCurrent();
        visualsRoot.SetActive(true);
        SpawnCharacters();

        SetInitialState();

        currentSequence = DOTween.Sequence();

        // 1. dimmer
        currentSequence.Append(
            dimmer.DOFade(dimmerTargetAlpha, dimmerDuration).SetEase(Ease.OutQuad)
        );

        // 2. 창1
        currentSequence.Append(
            spear1Rect.DOAnchorPos(crossPoint, spear1Duration).SetEase(spearEase)
        );
        currentSequence.Join(
                DOVirtual.DelayedCall(0f, () => _audioManager.PlaySfx("SlideIn"))
        );

        // 3. 창2
        currentSequence.AppendInterval(spearDelay);
        currentSequence.Append(
            spear2Rect.DOAnchorPos(crossPoint, spear2Duration).SetEase(spearEase)
        );
        currentSequence.Join(
                DOVirtual.DelayedCall(0f, () => _audioManager.PlaySfx("SlideIn"))
        );

        // 4. 창 충돌 연출
        currentSequence.AppendCallback(() => OnSpearImpact());

        // 5-1. VS 텍스트
        currentSequence.AppendInterval(0.1f);
        currentSequence.Append(
            vsText.DOFade(1f, 0.1f)
        );
        currentSequence.Join(
            vsTextRect.DOScale(Vector3.one, vsAppearDuration)
            .SetEase(Ease.OutBack, 1.0f)
        );

        // 5-2. VS 텍스트 연출
        currentSequence.Join(
            DOVirtual.DelayedCall(0.2f, () => OnTextLand())
        );

        // 5-3. VS 파티클
        currentSequence.AppendCallback(() =>
        {
            //if (energyBurst != null) energyBurst.Play();
            DOVirtual.DelayedCall(0.1f, () =>
            {
                if (oraAura != null) oraAura.Play();
            });
        });

        // 6. 캐릭터 슬라이드 인 + 배경
        currentSequence.AppendInterval(characterFadeInDelay);
        currentSequence.Append(
            blueBackground.DOFillAmount(1f, characterBackFadeInDuration).SetEase(Ease.OutQuad)
        );
        currentSequence.Join(
            redBackground.DOFillAmount(1f, characterBackFadeInDuration).SetEase(Ease.OutQuad)
        );

        if (_spawnedPlayer != null)
        {
            currentSequence.Join(
                _spawnedPlayer.transform
                    .DOMove(playerCharacterOriginPos, characterFadeInDuration)
                    .SetEase(Ease.OutQuint)
            );
            currentSequence.Join(
                FadeSpriteGroup(_spawnedPlayer, 1f, characterFadeInDuration * 0.5f)
            );
        }
        if (_spawnedEnemy != null)
        {
            currentSequence.Join(
                _spawnedEnemy.transform
                    .DOMove(enemyCharacterOriginPos, characterFadeInDuration)
                    .SetEase(Ease.OutQuint)
            );
            currentSequence.Join(
                FadeSpriteGroup(_spawnedEnemy, 1f, characterFadeInDuration * 0.5f)
            );
            currentSequence.Join(
                DOVirtual.DelayedCall(0f, () => _audioManager.PlaySfx("IntroCharacter"))
            );
        }


        // 7. 홀드
        currentSequence.AppendInterval(holdDuration);

        // 8. 아웃트로
        currentSequence.AppendCallback(() => PlayOutro());
        currentSequence.AppendInterval(outroFadeDuration);
        currentSequence.OnComplete(() =>
        {
            visualsRoot.SetActive(false);
            ResetDimmer();

            if (_spawnedPlayer != null) Destroy(_spawnedPlayer);
            if (_spawnedEnemy != null) Destroy(_spawnedEnemy);

            onComplete?.Invoke();
        });
    }

    private void SpawnCharacters()
    {
        if (_spawnedPlayer != null) Destroy(_spawnedPlayer);
        if (_spawnedEnemy != null) Destroy(_spawnedEnemy);

        if (playerIntroPrefab != null)
            _spawnedPlayer = Instantiate(playerIntroPrefab, playerSpawnPoint.position, playerIntroPrefab.transform.rotation);

        var enemyPrefab = _battleDataManager.GetEnemyIntroPrefab();
        if (enemyPrefab != null)
            _spawnedEnemy = Instantiate(enemyPrefab, enemySpawnPoint.position, enemyPrefab.transform.rotation);
    }

    private void OnSpearImpact()
    {
        spear1Rect.DOAnchorPos(crossPoint + new Vector2(5f, -5f), 0.05f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => spear1Rect.DOAnchorPos(crossPoint, 0.1f).SetEase(Ease.InQuad));

        spear2Rect.DOAnchorPos(crossPoint + new Vector2(-5f, -5f), 0.05f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => spear2Rect.DOAnchorPos(crossPoint, 0.1f).SetEase(Ease.InQuad));
    }

    private void OnTextLand()
    {
        if (energyBurst != null)
        {
            WorldPoolManager.instance.Get(energyBurst, Vector3.zero, Quaternion.identity);
        }

        RectTransform shakeTarget = visualsRoot.GetComponent<RectTransform>();
        if (shakeTarget != null)
        {
            Vector2 originalPos = shakeTarget.anchoredPosition;
            shakeTarget.DOShakeAnchorPos(
                screenShakeDuration, screenShakeStrength, 25, 90f, false, true
            ).OnComplete(() => shakeTarget.anchoredPosition = originalPos);
        }
    }

    private void PlayOutro()
    {
        float d = outroFadeDuration;
        dimmer.DOFade(0f, d).SetEase(Ease.InQuad);
        spear1Image.DOFade(0f, d).SetEase(Ease.InQuad);
        spear2Image.DOFade(0f, d).SetEase(Ease.InQuad);
        vsText.DOFade(0f, d).SetEase(Ease.InQuad);
        blueBackground.DOFade(0f, d).SetEase(Ease.InQuad);
        redBackground.DOFade(0f, d).SetEase(Ease.InQuad);

        if (_spawnedPlayer != null) FadeSpriteGroup(_spawnedPlayer, 0f, d);
        if (_spawnedEnemy != null) FadeSpriteGroup(_spawnedEnemy, 0f, d);

        if (oraAura != null)
            oraAura.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void SetInitialState()
    {
        spear1Rect.anchoredPosition = new Vector2(-spearOffscreenDistance, spearOffscreenDistance);
        SetAlpha(spear1Image, 1f);

        spear2Rect.anchoredPosition = new Vector2(spearOffscreenDistance, spearOffscreenDistance);
        SetAlpha(spear2Image, 1f);

        vsTextRect.anchoredPosition = Vector2.zero;
        vsTextRect.localScale = Vector3.one * vsStartScale;
        SetTMPAlpha(vsText, 0f);

        blueBackground.fillAmount = 0f;
        redBackground.fillAmount = 0f;
        SetAlpha(blueBackground, 1f);
        SetAlpha(redBackground, 1f);

        if (_spawnedPlayer != null)
        {
            _spawnedPlayer.transform.position =
                playerCharacterOriginPos + new Vector3(-characterSlideDistance, 0f, 0f);
            FadeSpriteGroup(_spawnedPlayer, 0f, 0f);
        }
        if (_spawnedEnemy != null)
        {
            _spawnedEnemy.transform.position =
                enemyCharacterOriginPos + new Vector3(characterSlideDistance, 0f, 0f);
            FadeSpriteGroup(_spawnedEnemy, 0f, 0f);
        }

        if (oraAura != null)
            oraAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void ResetDimmer()
    {
        if (dimmer != null) SetAlpha(dimmer, 0f);
    }

    private void KillCurrent()
    {
        currentSequence?.Kill();
        spear1Rect?.DOKill();
        spear2Rect?.DOKill();
        vsTextRect?.DOKill();
        dimmer?.DOKill();
        spear1Image?.DOKill();
        spear2Image?.DOKill();
        vsText?.DOKill();
        blueBackground?.DOKill();
        redBackground?.DOKill();

        if (_spawnedPlayer != null) _spawnedPlayer.transform.DOKill();
        if (_spawnedEnemy != null) _spawnedEnemy.transform.DOKill();

        if (oraAura != null)
            oraAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    private void SetTMPAlpha(TextMeshProUGUI tmp, float a)
    {
        if (tmp == null) return;
        Color c = tmp.color;
        c.a = a;
        tmp.color = c;
    }

    private Tween FadeSpriteGroup(GameObject character, float targetAlpha, float duration)
    {
        SpriteRenderer[] renderers = character.GetComponentsInChildren<SpriteRenderer>();

        Sequence seq = DOTween.Sequence();
        foreach (var sr in renderers)
        {
            if (duration <= 0f)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, targetAlpha);
            }
            else
            {
                seq.Join(sr.DOFade(targetAlpha, duration));
            }
        }
        return seq;
    }

    void OnDestroy()
    {
        KillCurrent();
        if (_spawnedPlayer != null) Destroy(_spawnedPlayer);
        if (_spawnedEnemy != null) Destroy(_spawnedEnemy);
    }

    [ContextMenu("Test Round 1")]
    private void TestRound1() => Play();
}
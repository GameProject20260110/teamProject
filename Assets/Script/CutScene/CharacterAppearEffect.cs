using UnityEngine;
using DG.Tweening;
using System;

public class CharacterAppearEffect : MonoBehaviour
{
    [Header("캐릭터 프리팹")]
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("줌 아웃 설정")]
    [SerializeField] private float startScale = 1.8f;
    [SerializeField] private float zoomDuration = 0.25f;

    [Header("흔들림 설정")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 0.1f;
    [SerializeField] private int shakeVibrato = 8;

    [Header("타입")]
    [SerializeField] private bool isPlayer = true;

    // 보스별 스폰 위치/크기 오버라이드
    private Vector3? _overridePosition;
    private Vector3? _overrideScale;

    private GameObject _spawnedCharacter;
    private Transform _targetTransform;
    private Sequence currentSequence;

    public GameObject SpawnCharacter => _spawnedCharacter;

    [ContextMenu("Test Play")]
    public void Play() => Play(null);

    public void SetSpawnOverride(Vector3 position, Vector3 scale)
    {
        _overridePosition = position;
        _overrideScale = scale;
    }

    public void Play(Action onComplete)
    {
        if (_spawnedCharacter != null)
            Destroy(_spawnedCharacter);

        Vector3 spawnPos = _overridePosition ?? (spawnPoint != null ? spawnPoint.position : Vector3.zero);

        _spawnedCharacter = Instantiate(
            characterPrefab,
            spawnPos,
            Quaternion.identity
        );
        _targetTransform = _spawnedCharacter.transform;

        SpriteRenderer[] renderers = isPlayer
        ? _spawnedCharacter.GetComponentInChildren<PlayerCharacter>().Renderers
        : _spawnedCharacter.GetComponentInChildren<EnemyCharacter>().Renderers;
        ParticleSystem particle = _spawnedCharacter.GetComponentInChildren<ParticleSystem>();

        currentSequence?.Kill();
        _targetTransform.DOKill();

        Vector3 finalScale = _overrideScale ?? Vector3.one;

        // 초기 상태
        _targetTransform.localScale = finalScale * startScale;
        foreach (var sr in renderers)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);

        if (particle != null)
        {
            particle.Stop();
            particle.Clear();
        }

        currentSequence = DOTween.Sequence();

        // 페이드 인 + 줌 아웃
        foreach (var sr in renderers)
            currentSequence.Join(sr.DOFade(1f, zoomDuration).SetEase(Ease.OutQuad));

        currentSequence.Join(
            _targetTransform.DOScale(finalScale, zoomDuration).SetEase(Ease.OutBack)
        );

        currentSequence.Join(
            DOVirtual.DelayedCall(0f, () => AudioManager.instance.PlaySfx("Character"))
        );

        // 파티클 재생
        currentSequence.AppendCallback(() =>
        {
            if (particle != null) particle.Play();
        });

        // 흔들림
        currentSequence.Append(
            _targetTransform.DOShakePosition(
                shakeDuration,
                new Vector3(shakeStrength, 0f, 0f),
                shakeVibrato
            )
        );

        currentSequence.OnComplete(() => {
            if (isPlayer)
                BattleInitalizer.instance.SetSpawnPlayer(_spawnedCharacter);
            else
                BattleInitalizer.instance.SetSpawnEnemy(_spawnedCharacter);
            onComplete?.Invoke();

            _overridePosition = null;
            _overrideScale = null;
        });
    }

    public void SetPrefab(GameObject prefab)
    {
        characterPrefab = prefab;
    }

    public void Clear()
    {
        currentSequence?.Kill();
        if (_spawnedCharacter != null)
            Destroy(_spawnedCharacter);
    }

    void OnDestroy()
    {
        currentSequence?.Kill();
        _targetTransform?.DOKill();
        if (_spawnedCharacter != null)
            Destroy(_spawnedCharacter);
    }
}

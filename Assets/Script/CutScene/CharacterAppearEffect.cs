using UnityEngine;
using DG.Tweening;
using System;
using VContainer;

public class CharacterAppearEffect : MonoBehaviour
{
    [Header("ƒ≥∏Ø≈Õ «¡∏Æ∆’")]
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private Transform spawnPoint;
    [Header("¡‹ æ∆øÙ º≥¡§")]
    [SerializeField] private float startScale = 1.8f;
    [SerializeField] private float zoomDuration = 0.25f;
    [Header("»ÁµÈ∏≤ º≥¡§")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 0.1f;
    [SerializeField] private int shakeVibrato = 8;
    [Header("≈∏¿‘")]
    [SerializeField] private bool isPlayer = true;

    private Vector3? _overridePosition;
    private Vector3? _overrideScale;
    private GameObject _spawnedCharacter;
    private Transform _targetTransform;
    private Sequence currentSequence;

    public GameObject SpawnCharacter => _spawnedCharacter;

    private AudioManager _audioManager;
    private BattleInitalizer _battleInitalizer;

    public void SetDependencies(AudioManager audioManager, BattleInitalizer battleInitalizer)
    {
        _audioManager = audioManager;
        _battleInitalizer = battleInitalizer;
    }

    [ContextMenu("Test Play")]
    public void Play() => Play(null);

    public void SetSpawnOverride(Vector3 position, Vector3 scale)
    {
        _overridePosition = position;
        _overrideScale = scale;
    }

    public void Play(Action onComplete)
    {
        Debug.Log($"[CharacterAppearEffect] Play Ω√¿€, isPlayer={isPlayer}, prefab={characterPrefab}");


        if (_spawnedCharacter != null)
            Destroy(_spawnedCharacter);

        Vector3 spawnPos = _overridePosition ?? (spawnPoint != null ? spawnPoint.position : Vector3.zero);
        _spawnedCharacter = Instantiate(characterPrefab, spawnPos, Quaternion.identity);
        _targetTransform = _spawnedCharacter.transform;
        Vector3 prefabScale = _targetTransform.localScale;

        SpriteRenderer[] renderers = isPlayer
            ? _spawnedCharacter.GetComponentInChildren<PlayerCharacter>().Renderers
            : _spawnedCharacter.GetComponentInChildren<EnemyCharacter>().Renderers;

        ParticleSystem particle = _spawnedCharacter.GetComponentInChildren<ParticleSystem>();

        currentSequence?.Kill();
        _targetTransform.DOKill();

        Vector3 finalScale = _overrideScale ?? prefabScale;

        _targetTransform.localScale = finalScale * startScale;
        foreach (var sr in renderers)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);

        if (particle != null)
        {
            particle.Stop();
            particle.Clear();
        }

        currentSequence = DOTween.Sequence();

        foreach (var sr in renderers)
            currentSequence.Join(sr.DOFade(1f, zoomDuration).SetEase(Ease.OutQuad));

        currentSequence.Join(_targetTransform.DOScale(finalScale, zoomDuration).SetEase(Ease.OutBack));
        currentSequence.Join(DOVirtual.DelayedCall(0f, () => _audioManager.PlaySfx("Character")));

        currentSequence.AppendCallback(() =>
        {
            if (particle != null) particle.Play();
        });

        currentSequence.Append(
            _targetTransform.DOShakePosition(shakeDuration, new Vector3(shakeStrength, 0f, 0f), shakeVibrato)
        );

        currentSequence.OnComplete(() =>
        {
            if (isPlayer)
                _battleInitalizer.SetSpawnPlayer(_spawnedCharacter);
            else
                _battleInitalizer.SetSpawnEnemy(_spawnedCharacter);
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

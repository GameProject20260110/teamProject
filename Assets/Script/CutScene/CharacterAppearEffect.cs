using UnityEngine;
using DG.Tweening;
using System;

public class CharacterAppearEffect : MonoBehaviour
{
    [Header("Ä³¸¯ÅÍ ÇÁ¸®ÆÕ")]
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("ÁÜ ¾Æ¿ô ¼³Á¤")]
    [SerializeField] private float startScale = 1.8f;
    [SerializeField] private float zoomDuration = 0.25f;

    [Header("Èçµé¸² ¼³Á¤")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 0.1f;
    [SerializeField] private int shakeVibrato = 8;

    [Header("Å¸ÀÔ")]
    [SerializeField] private bool isPlayer = true;

    private GameObject _spawnedCharacter;
    private Transform _targetTransform;
    private Sequence currentSequence;

    public GameObject SpawnCharacter => _spawnedCharacter;

    [ContextMenu("Test Play")]
    public void Play() => Play(null);

    public void Play(Action onComplete)
    {
        if (_spawnedCharacter != null)
            Destroy(_spawnedCharacter);

        _spawnedCharacter = Instantiate(
            characterPrefab,
            spawnPoint.position,
            Quaternion.identity
        );
        _targetTransform = _spawnedCharacter.transform;

        SpriteRenderer[] renderers = isPlayer
        ? _spawnedCharacter.GetComponentInChildren<PlayerCharacter>().Renderers
        : _spawnedCharacter.GetComponentInChildren<EnemyCharacter>().Renderers;
        ParticleSystem particle = _spawnedCharacter.GetComponentInChildren<ParticleSystem>();

        currentSequence?.Kill();
        _targetTransform.DOKill();

        // ÃÊ±â »óÅÂ
        _targetTransform.localScale = Vector3.one * startScale;
        foreach (var sr in renderers)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);

        if (particle != null)
        {
            particle.Stop();
            particle.Clear();
        }

        currentSequence = DOTween.Sequence();

        // ÆäÀÌµå ÀÎ + ÁÜ ¾Æ¿ô
        foreach (var sr in renderers)
            currentSequence.Join(sr.DOFade(1f, zoomDuration).SetEase(Ease.OutQuad));

        currentSequence.Join(
            _targetTransform.DOScale(Vector3.one, zoomDuration).SetEase(Ease.OutBack)
        );

        currentSequence.Join(
            DOVirtual.DelayedCall(0f, () => AudioManager.instance.PlaySfx("Character"))
        );

        // ÆÄÆ¼Å¬ Àç»ý
        currentSequence.AppendCallback(() =>
        {
            if (particle != null) particle.Play();
        });

        // Èçµé¸²
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

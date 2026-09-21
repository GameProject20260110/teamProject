using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class MapCharacterMover : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private RectTransform characterPrefab;

    [Header("Container")]
    [SerializeField] private RectTransform characterContainer;

    [Header("Spawn Effect")]
    [SerializeField] private SpawnEffect spawnEffectPrefab;
    [SerializeField] private RectTransform effectContainer;

    [Header("Movement")]
    [SerializeField] private float moveDurationPerNode = 0.3f;

    [Header("Sound")]
    [SerializeField] private string stepSoundKey = "walk_wood";
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private RectTransform instance;
    private CharacterAnimator animator;

    public Vector2 CurrentPosition => instance != null ? instance.anchoredPosition : Vector2.zero;

    public void Spawn(Vector2 position, bool playEffect = true)
    {
        if (instance != null)
        {
            instance.anchoredPosition = position;
            return;
        }

        instance = Instantiate(characterPrefab, characterContainer);
        animator = instance.GetComponent<CharacterAnimator>();
        instance.anchoredPosition = position;

        if (!playEffect || spawnEffectPrefab == null)
        {
            instance.localScale = Vector3.one;
            return;
        }

        PlaySpawnEffect(position).Forget();
    }

    private async UniTaskVoid PlaySpawnEffect(Vector2 position)
    {
        instance.localScale = Vector3.zero;

        var effect = Instantiate(spawnEffectPrefab, effectContainer);
        effect.GetComponent<RectTransform>().anchoredPosition = position;

        await effect.PlayAsync(onCharacterAppear: () =>
        {
            instance.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        });
    }

    public void Despawn()
    {
        if (instance == null) return;
        Destroy(instance.gameObject);
        instance = null;
        animator = null;
    }

    public void MoveAlongPath(List<Vector2> positions, Action<int> onStepArrived, Action onComplete)
    {
        animator?.SetMoving(true);

        var sequence = DOTween.Sequence();
        Vector2 previousPos = instance.anchoredPosition;

        for (int i = 0; i < positions.Count; i++)
        {
            Vector2 targetPos = positions[i];
            float deltaX = targetPos.x - previousPos.x;
            sequence.AppendCallback(() => UpdateFacing(deltaX));
            previousPos = targetPos;

            sequence.Append(instance.DOAnchorPos(targetPos, moveDurationPerNode).SetEase(Ease.Linear));

            int stepIndex = i; 
            sequence.AppendCallback(() =>
            {
                PlayStepSound();
                onStepArrived?.Invoke(stepIndex);
            });
        }

        sequence.OnComplete(() =>
        {
            animator?.SetMoving(false);
            onComplete?.Invoke();
        });
    }

    private void PlayStepSound()
    {
        if (AudioManager.Instance == null) return;

        float pitch = UnityEngine.Random.Range(pitchRange.x, pitchRange.y);
        AudioManager.Instance.PlaySfx(stepSoundKey, pitch);
    }

    private void UpdateFacing(float deltaX)
    {
        if (Mathf.Approximately(deltaX, 0f)) return;

        float sign = Mathf.Sign(deltaX);
        var scale = instance.localScale;
        instance.localScale = new Vector3(Mathf.Abs(scale.x) * sign, scale.y, scale.z);
    }
}
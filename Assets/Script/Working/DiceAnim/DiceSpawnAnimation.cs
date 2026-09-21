using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer;
using DG.Tweening;

public class DiceSpawnAnimation : MonoBehaviour
{
    [System.Serializable]
    public class DiceEntry
    {
        public GameObject diceObject;
        public ParticleSystem dustParticle;
    }

    [Header("주사위 목록")]
    private List<DiceEntry> diceList = new List<DiceEntry>();
    private List<DiceEntry> enemyDiceList = new List<DiceEntry>();

    [Header("연출 타이밍")]
    public float delayBetweenDice = 0.12f;
    public float fadeDuration = 0.15f;

    private AudioManager _audioManager;

    [Inject]
    public void Construct(AudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public void RegisterDice(GameObject diceObject, ParticleSystem particle = null)
    {
        var entry = new DiceEntry
        {
            diceObject = diceObject,
            dustParticle = particle
        };
        diceList.Add(entry);
        SetAlpha(diceObject, 0f);
    }

    public void RegisterEnemyDice(GameObject diceObject, ParticleSystem particle = null)
    {
        var entry = new DiceEntry
        {
            diceObject = diceObject,
            dustParticle = particle
        };
        enemyDiceList.Add(entry);
        SetAlpha(diceObject, 0f);
    }

    public void ClearList()
    {
        diceList.Clear();
    }

    public async UniTask PlayAsync(CancellationToken ct)
    {
        foreach (var entry in diceList)
        {
            if (entry.diceObject == null) continue;
            SpawnDice(entry).Forget();
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(delayBetweenDice),
                cancellationToken: ct
            );
        }
        await UniTask.Delay(
            System.TimeSpan.FromSeconds(fadeDuration),
            cancellationToken: ct
        );
    }

    public void ClearEnemyList()
    {
        enemyDiceList.Clear();
    }

    public async UniTask PlayEnemyAsync(CancellationToken ct)
    {
        foreach (var entry in enemyDiceList)
        {
            if (entry.diceObject == null) continue;
            SpawnDice(entry).Forget();
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(delayBetweenDice),
                cancellationToken: ct
            );
        }
        await UniTask.Delay(
            System.TimeSpan.FromSeconds(fadeDuration),
            cancellationToken: ct
        );
    }

    private async UniTaskVoid SpawnDice(DiceEntry entry)
    {
        if (entry.dustParticle != null)
        {
            entry.dustParticle.Play();
            _audioManager.PlaySfx("DiceSpawn");
        }

        await FadeInAsync(entry.diceObject);
    }

    private async UniTask FadeInAsync(GameObject dice)
    {
        SpriteGroupAlpha group = dice.GetComponent<SpriteGroupAlpha>();
        if (group == null)
            group = dice.AddComponent<SpriteGroupAlpha>();

        group.alpha = 0f;

        var completion = new UniTaskCompletionSource<bool>();
        DOVirtual.Float(0f, 1f, fadeDuration, v => group.alpha = v)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => completion.TrySetResult(true));

        await completion.Task;
    }

    void SetAlpha(GameObject dice, float alpha)
    {
        SpriteGroupAlpha group = dice.GetComponent<SpriteGroupAlpha>();
        if (group == null)
            group = dice.AddComponent<SpriteGroupAlpha>();
        group.alpha = alpha;
    }
}
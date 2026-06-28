using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class DiceSpawnAnimation : MonoBehaviour
{
    public static DiceSpawnAnimation instance;
    //public AudioClip particleSFX;

    [System.Serializable]
    public class DiceEntry
    {
        public GameObject diceObject;
        public ParticleSystem dustParticle;
    }

    [Header("주사위 목록")]
    public List<DiceEntry> diceList = new List<DiceEntry>();
    private List<DiceEntry> enemyDiceList = new List<DiceEntry>();

    [Header("연출 타이밍")]
    public float delayBetweenDice = 0.12f;
    public float fadeDuration = 0.15f;

    void Awake()
    {
        instance = this;
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
            AudioManager.instance.PlaySfx("particleSFX");
        }
        
        await FadeInAsync(entry.diceObject);
    }

    private async UniTask FadeInAsync(GameObject dice)
    {
        CanvasGroup cg = dice.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = dice.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);
            await UniTask.Yield();
        }

        cg.alpha = 1f;
    }

    void SetAlpha(GameObject dice, float alpha)
    {
        CanvasGroup cg = dice.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = dice.AddComponent<CanvasGroup>();
        cg.alpha = alpha;
    }
}
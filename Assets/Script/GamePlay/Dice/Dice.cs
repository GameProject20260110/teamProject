using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;

public class Dice : MonoBehaviour
{
    public SpriteRenderer diceImage;

    [Header("모듈")]
    public GameObject effectPrefab;
    public AudioClip rollSound;

    public TextMeshProUGUI diceScoreText;

    public DiceState MyState { get; private set; }
    public Transform OriginalSlot { get; private set; }

    // 캐싱
    public DiceEffectBase Effect { get; private set; }
    public DiceVFXBase VFX { get; private set; }
    public DiceGlow Glow { get; private set; }
    public MeshRenderer MeshRenderer { get; private set; }

    private int _currentDiceScore = 0;

    private void Awake()
    {
        Effect = GetComponent<DiceEffectBase>();
        VFX = GetComponent<DiceVFXBase>();
        Glow = GetComponentInChildren<DiceGlow>();
        MeshRenderer = GetComponentInChildren<MeshRenderer>(true);
        MeshRenderer.sortingLayerID = SortingLayer.NameToID("Dice");
        MeshRenderer.sortingOrder = 3;
    }

    public void Initialize(int index, DiceData data)
    {
        MyState = new DiceState(data, index, 1);
        OriginalSlot = transform.parent;
        UpdateDiceImage(1);
        UpdateDiceScoreUi(0, hide: true);
    }

    public void UpdateDiceImage(int value)
    {
        if (MyState != null && MyState.diceData != null && MyState.diceData.skin != null)
        {
            diceImage.sprite = MyState.diceData.skin.GetSprite(value);
        }
    }

    public void UpdateDiceScoreUi(int targetScore, bool anim = false, bool hide = false)
    {
        if (diceScoreText != null)
        {
            if (hide)
            {
                diceScoreText.alpha = 0f;
                _currentDiceScore = 0;
            }
            else
            {
                diceScoreText.alpha = 255f;
                if (anim && _currentDiceScore != targetScore)
                {
                    int from = _currentDiceScore;
                    _currentDiceScore = targetScore;
                    DOVirtual.Int(from, targetScore, 0.5f, (x) =>
                    {
                        diceScoreText.text = x.ToString();
                    });
                }
                else
                {
                    _currentDiceScore = targetScore;
                    diceScoreText.text = targetScore.ToString();
                }
            }
        }
    }

    public void StartRoll(float duration)
    {
        UpdateDiceScoreUi(0, hide: true);
        StartCoroutine(ChangeImageDuringRoll(duration));
    }

    IEnumerator ChangeImageDuringRoll(float duration)
    {
        float timer = 0f;
        float switchinterval = 0.1f;

        while (timer < duration)
        {
            int randomValue = Random.Range(1, 7);
            UpdateDiceImage(randomValue);

            yield return new WaitForSeconds(switchinterval);
            timer += switchinterval;
        }
    }

    public void SetResult(int resultValue)
    {
        StopAllCoroutines();

        MyState.originalValue = resultValue;
        MyState.modifiedValue = resultValue;

        UpdateDiceImage(resultValue);
    }

    public Sprite GetCurrentSprite()
    {
        return diceImage.sprite;
    }
}

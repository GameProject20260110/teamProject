using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;

public class Dice : MonoBehaviour
{
    public Image diceImage;

    [Header("사운드 및 이펙트")]
    public GameObject effectPrefab;
    public AudioClip rollSound;

    public TextMeshProUGUI diceScoreText;

    public DiceState MyState { get; private set; }

    private int _currentDiceScore = 0;

    public void Initialize(int index, DiceData data)
    {
        MyState = new DiceState(data, index, 1);
        UpdateDiceImage(1);
        UpdateDiceScoreUi(-1);
    }

    public void UpdateDiceImage(int value)
    {
        if (MyState != null && MyState.diceData != null && MyState.diceData.skin != null)
        {
            diceImage.sprite = MyState.diceData.skin.GetSprite(value);
        }
    }

    public void UpdateDiceScoreUi(int targetScore, bool anim = false)
    {
        if(diceScoreText != null)
        {
            if(targetScore < 0)
            {
                diceScoreText.gameObject.SetActive(false);
                _currentDiceScore = 0;
            }
            else
            {
                diceScoreText.gameObject.SetActive(true);
                if (anim && _currentDiceScore > 0)
                {
                    DOVirtual.Int(_currentDiceScore, targetScore, 0.5f, (x) =>
                    {
                        diceScoreText.text = x.ToString();
                    });
                }
                else
                {
                    diceScoreText.text = targetScore.ToString();
                }
            }
        }
    }

    public void StartRoll(float duration)
    {
        UpdateDiceScoreUi(-1);
        StartCoroutine(ChangeImageDuringRoll(duration));
    }

    IEnumerator ChangeImageDuringRoll(float duration)
    {
        float timer = 0f;
        float switchinterval = 0.1f;

        while(timer < duration)
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

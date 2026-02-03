using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class Dice : MonoBehaviour
{
    public Image diceImage;

    [Header("사운드 및 이펙트")]
    public GameObject effectPrefab;
    public AudioClip rollSound;

    public DiceState MyState { get; private set; }

    public void Initialize(int index, DiceData data)
    {
        if(data == null)
        {
            gameObject.SetActive(false);
            MyState = null;
        }       
        else
        {
            gameObject.SetActive(true);
            MyState = new DiceState(data, index, 1);
            UpdateDiceImage(1);
        }   
    }

    public void UpdateDiceImage(int value)
    {
        if (MyState != null && MyState.diceData != null && MyState.diceData.skin != null)
        {
            diceImage.sprite = MyState.diceData.skin.GetSprite(value);
        }
    }

    public void StartRoll(float duration)
    {
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

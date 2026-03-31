using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;

public class Dice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image diceImage;

    [Header("모듈")]
    public GameObject effectPrefab;
    public AudioClip rollSound;

    public TextMeshProUGUI diceScoreText;

    public DiceState MyState { get; private set; }

    private int _currentDiceScore = 0;

    public void Initialize(int index, DiceData data)
    {
        MyState = new DiceState(data, index, 1);
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (PopupManager.instance == null || MyState.diceData == null) return;
        PopupManager.instance.OpenPopup(MyState.diceData, GetComponent<RectTransform>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (PopupManager.instance == null) return;
        PopupManager.instance.ClosePopup();
    }
}

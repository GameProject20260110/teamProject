using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using NUnit.Framework.Constraints;


public class ScoreVisualizer : MonoBehaviour
{
    public static ScoreVisualizer instance;

    public TextMeshProUGUI finalScoreText;
    public GameObject floatingText;
    public Transform effectCanvas;

    public RectTransform goldUiTarget;
    public GameObject goldIconPrefab;

    public GameObject negateOverlayPrefab;

    private int _currentDisplayScore = 0;
    private string _lastEffectName;
    private List<GameObject> _negateOverlays = new List<GameObject>();

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public IEnumerator PlayScoreEventSequence(Dice[] uiDice, List<ScoreEventData> scoreEvent)
    {
        _lastEffectName = "";

        foreach(var evt in scoreEvent)
        {
            Dice targetDice = GetTargetDice(uiDice, evt.targetIndex);

            if(evt.type != ScoreEventData.Type.ItemEffect)
            {
                if (ShowEffectMessage(evt.effectName, evt.effectDesc)) yield return new WaitForSeconds(1.0f);
            }
            
            switch(evt.type)
            {
                case ScoreEventData.Type.AddScore:
                    yield return PlayAddScore(targetDice, evt);
                    break;
                case ScoreEventData.Type.Multiplier:
                    yield return PlayMultiplier(targetDice, evt);
                    break;
                case ScoreEventData.Type.TargetBuff:
                    yield return PlayTargetBuff(uiDice, evt);
                    break;
                case ScoreEventData.Type.ChangeFace:
                    yield return PlayChangeFace(uiDice, targetDice, evt);
                    break;
                case ScoreEventData.Type.GlobalBuff:
                    yield return PlayGlobalBuffs(uiDice, evt);
                    break;
                case ScoreEventData.Type.Negate:
                    yield return PlayNegate(targetDice, evt);
                    break;
                case ScoreEventData.Type.ItemEffect:
                    yield return PlayItemEffect(evt);
                    break;
                case ScoreEventData.Type.FinalScore:
                    yield return PlayFinalScore(uiDice, evt);
                    break;
                case ScoreEventData.Type.GainGold:
                    yield return new WaitForSeconds(0.3f);
                    break;
            }
        }
    }

    private IEnumerator PlayAddScore(Dice targetDice, ScoreEventData evt)
    {
        if(targetDice != null)
        {
            if(!string.IsNullOrEmpty(evt.effectName))
            {
                yield return PlayScale(targetDice);
            }
            PlayDotweenEffect(targetDice, "Punch");
            ShowFloatingText(targetDice.transform.position, evt.desc);
            if(evt.currentDiceScore != int.MinValue)
            {
                targetDice.UpdateDiceScoreUi(evt.currentDiceScore, true);
            }
        }
        UpdateScoreBoard(evt.value);
        yield return new WaitForSeconds(0.7f);
    }

    private IEnumerator PlayMultiplier(Dice targetDice, ScoreEventData evt)
    {
        if(targetDice != null)
        {
            yield return PlayScale(targetDice);
            PlayDotweenEffect(targetDice, "Punch");
            ShowFloatingText(targetDice.transform.position, evt.desc);
            if(evt.currentDiceScore != int.MinValue)
            {
                targetDice.UpdateDiceScoreUi(evt.currentDiceScore, true);
            }
        }
        UpdateScoreBoard(evt.value);
        yield return new WaitForSeconds(0.7f);
    }

    private IEnumerator PlayTargetBuff(Dice[] uiDice, ScoreEventData evt)
    {
        Dice triggerDice = GetTargetDice(uiDice, evt.targetIndex);
        if(triggerDice != null)
        {
            yield return PlayScale(triggerDice);
        }

        if(evt.targetIndices != null)
        {

            foreach(int idx in evt.targetIndices)
            {
                if (idx < 0 || idx >= uiDice.Length) continue;
                if (uiDice[idx] == null || !uiDice[idx].gameObject.activeSelf) continue;

                PlayDotweenEffect(uiDice[idx], "Bounce");
                if (evt.currentDiceScore != int.MinValue) uiDice[idx].UpdateDiceScoreUi(evt.currentDiceScore, true);
            }
        }
        UpdateScoreBoard(evt.value);
        yield return new WaitForSeconds(0.7f);
    }

    private IEnumerator PlayChangeFace(Dice[] uiDice, Dice targetDice, ScoreEventData evt)
    {
        if(evt.targetIndex == -1)
        {
            foreach (var dice in uiDice)
            {
                if (dice == null || !dice.gameObject.activeSelf) continue;
                dice.transform.DOShakeRotation(0.4f, 90f);
            }
            yield return new WaitForSeconds(0.3f);

            foreach(var dice in uiDice)
            {
                if (dice == null || !dice.gameObject.activeSelf) continue;
                dice.UpdateDiceImage(evt.value);
                ShowFloatingText(dice.transform.position, evt.desc);
            }
            yield return new WaitForSeconds(0.7f);
        }
        else if(targetDice != null)
        {
            targetDice.transform.DOShakeRotation(0.3f, 90f);
            yield return new WaitForSeconds(0.3f);

            targetDice.UpdateDiceImage(evt.value);
            ShowFloatingText(targetDice.transform.position, evt.desc);
            yield return new WaitForSeconds(0.7f);
        }
    }

    private IEnumerator PlayGlobalBuffs(Dice[] uiDice, ScoreEventData evt)
    {
        Dice triggerDice = GetTargetDice(uiDice, evt.targetIndex);
        if(triggerDice != null)
        {
            yield return PlayScale(triggerDice);
        }

        Tween lastTween = null;
        foreach(var dice in uiDice)
        {
            if (dice == null || !dice.gameObject.activeSelf) continue;
            lastTween = PlayDotweenEffect(dice, "Jump");
            ShowFloatingText(dice.transform.position, evt.desc);
        }
        UpdateScoreBoard(evt.value);

        if(lastTween != null)
        {
            yield return lastTween.WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(0.7f);
        }
    }

    private IEnumerator PlayNegate(Dice targetDice, ScoreEventData evt)
    {
        if(targetDice != null)
        {
            Image diceImage = targetDice.GetComponent<Image>();
            if(diceImage != null)
            {
                diceImage.DOColor(Color.gray, 0.5f);
            }

            targetDice.transform.DOScale(Vector3.one * 0.8f, 0.7f);
            ShowFloatingText(targetDice.transform.position, evt.desc);

            if(negateOverlayPrefab != null)
            {
                GameObject overlay = Instantiate(negateOverlayPrefab, targetDice.transform);
                overlay.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                _negateOverlays.Add(overlay);
            }
        }
        else if(evt.targetIndex == -1)
        {
            UiController.instance?.NegateItemCard(evt.effectName, negateOverlayPrefab);
            ShowFloatingText(effectCanvas.position, evt.desc);
        }
        yield return new WaitForSeconds(0.7f);
    }
    private IEnumerator PlayItemEffect(ScoreEventData evt)
    {
        ShowEffectMessage(evt.effectName, evt.effectDesc);
        var card = UiController.instance?.inventoryUI?.FindCardByName(evt.effectName);
        if (card != null)
        {
            Vector3 originalScale = card.transform.localScale;
            card.transform.DOScale(originalScale * 1.3f, 0.3f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.7f);

            card.transform.DOScale(originalScale, 0.3f);
        }
        else
        {
            ShowFloatingText(effectCanvas.position, evt.desc);
        }
        UpdateScoreBoard(evt.value);
        yield return new WaitForSeconds(0.7f);
    }

    private IEnumerator PlayFinalScore(Dice[] uiDice, ScoreEventData evt)
    {
        UpdateScoreBoard(evt.value);
        finalScoreText.transform.DOPunchScale(Vector3.one * 0.35f, 0.3f);
        yield return new WaitForSeconds(0.35f);
    }

    public void ClearNegateOverlays()
    {
        foreach (var overlay in _negateOverlays)
        {
            if (overlay != null) Destroy(overlay);
        }
        _negateOverlays.Clear();
    }

    public void ResetDiceColors(Dice[] uiDice)
    {
        foreach(var dice in uiDice)
        {
            if (dice == null || !dice.gameObject.activeSelf) continue;
            Image img = dice.GetComponent<Image>();
            if (img != null) 
            {
                img.DOColor(Color.white, 0.3f);
                dice.transform.DOScale(Vector3.one, 0.3f);
            }
        }
    }
    private Dice GetTargetDice(Dice[] uiDice, int index)
    {
        if (index >= 0 && index < uiDice.Length) return uiDice[index];
        return null;
    }

    private bool ShowEffectMessage(string effectName, string effectDesc)
    {
        if (string.IsNullOrEmpty(effectName) || effectName == _lastEffectName) return false;
        _lastEffectName = effectName;
        
        string message = string.IsNullOrEmpty(effectDesc) ? effectName : $"{effectName}\n{effectDesc}";
        UiController.instance.notificationUI.Show(message, 0.7f);
        return true;
    }

    public void UpdateScoreBoard(int targetValue)
    {
        int originalValue = _currentDisplayScore;
        _currentDisplayScore = targetValue;

        DOVirtual.Int(originalValue, targetValue, 0.3f, (x) =>
        {
            finalScoreText.text = x.ToString();
        });
        finalScoreText.transform.DOShakePosition(0.3f, 2f);
    }

    public Tween PlayDotweenEffect(Dice dice, string type)
    {
        Transform t = dice.transform;

        switch(type)
        {
            case "Punch":
                return t.DOPunchScale(Vector3.one * 0.3f, 0.3f, 10, 1);
            case "Jump":
                DG.Tweening.Sequence jumpSeq = DOTween.Sequence();
                jumpSeq.Append(t.DOLocalJump(t.localPosition, 30f, 1, 1.5f));
                jumpSeq.Join(t.DORotate(new Vector3(0, 0, 360), 1.5f, RotateMode.FastBeyond360));
                return jumpSeq;
            case "Shake":
                return t.DOShakePosition(0.35f, 7f, 25, 100, false, true);
            case "Bounce":
                DG.Tweening.Sequence bounceSeq = DOTween.Sequence();
                bounceSeq.Append(t.DOLocalMoveY(t.localPosition.y + 20f, 0.15f).SetEase(Ease.OutQuad));
                bounceSeq.Append(t.DOLocalMoveY(t.localPosition.y, 0.15f).SetEase(Ease.InQuad));
                return bounceSeq;
            default:
                return null;
        }
    }

    public void ShowFloatingText(Vector3 wordPos, string text)
    {
        if (floatingText == null) return;
        if(AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Score);
        GameObject obj = Instantiate(floatingText, effectCanvas);
        obj.transform.position = wordPos + Vector3.up * 70f;

        TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = text;

        obj.transform.DOMoveY(obj.transform.position.y + 100f, 1.5f);
        tmp.DOFade(0, 1f).OnComplete(() => Destroy(obj));
    }
    private float GetBaseScale(Dice dice)
    {
        if (dice.MyState != null && dice.MyState.isIgnored) return 0.8f;
        return 1.0f;
    }
    private IEnumerator PlayScale(Dice dice)
    {
        float baseScale = GetBaseScale(dice);
        dice.transform.DOScale(Vector3.one * baseScale * 1.3f, 0.3f);
        yield return new WaitForSeconds(0.3f);
        dice.transform.DOScale(Vector3.one * baseScale, 0.3f);
        yield return new WaitForSeconds(0.7f);
    }
}

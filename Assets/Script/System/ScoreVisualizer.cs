using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

public class ScoreVisualizer : MonoBehaviour
{
    public static ScoreVisualizer instance;

    public TextMeshProUGUI finalScoreText;
    public GameObject floatingText;
    public Transform effectCanvas;
    public RectTransform goldUITarget;
    public GameObject goldIconPrefab;
    public GameObject negateOverlayPrefab;
    public ParticleSystem diceEffect;

    private int _currentDisplayScore = 0;
    private string _lastEffectName;
    private List<GameObject> _negateOverlays = new List<GameObject>();

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public async UniTask PlayScoreEventSequence(Dice[] allDice, List<ScoreEventData> scoreEvent)
    {
        _lastEffectName = "";
   
        foreach(var evt in scoreEvent)
        {
            Dice targetDice = GetTargetDice(allDice, evt.targetIndex);

            if(evt.type != ScoreEventData.Type.ItemEffect)
            {
                if (ShowEffectMessage(evt.effectName, evt.effectDesc)) await UniTask.Delay(1000);
            }
            
            switch(evt.type)
            {
                case ScoreEventData.Type.TriggerDice:
                    Dice triggerDice = GetTargetDice(allDice, evt.triggerIndex);
                    if (triggerDice != null)
                        await PlayScale(triggerDice);
                    break;
                case ScoreEventData.Type.AddScore:
                    await PlayAddScore(allDice, targetDice, evt);
                    break;
                case ScoreEventData.Type.TargetBuff:
                    await PlayTargetBuff(allDice, evt);
                    break;
                case ScoreEventData.Type.ChangeFace:
                    await PlayChangeFace(allDice, targetDice, evt);
                    break;
                case ScoreEventData.Type.GlobalBuff:
                    await PlayGlobalBuffs(allDice, evt);
                    break;
                case ScoreEventData.Type.Negate:
                    await PlayNegate(targetDice, evt);
                    break;
                case ScoreEventData.Type.ItemEffect:
                    await PlayItemEffect(evt);
                    break;
                case ScoreEventData.Type.FinalScore:
                    await PlayFinalScore(allDice, evt);
                    break;
                case ScoreEventData.Type.GainGold:
                    await UniTask.Delay(300);
                    break;
                case ScoreEventData.Type.GainReroll:
                    await PlayGainReroll(evt);
                    break;
                case ScoreEventData.Type.Notice:
                    await PlayNotice(evt);
                    break;
            }
        }
    }

    private async UniTask PlayAddScore(Dice[] allDice, Dice targetDice, ScoreEventData evt)
    {
        if(targetDice != null)
        {
            PlayDotweenEffect(targetDice, "Punch");
            ShowFloatingText(targetDice.transform.position, evt.desc);
            if(evt.currentDiceScore != int.MinValue)
            {
                targetDice.UpdateDiceScoreUi(evt.currentDiceScore, true);
            }
        }
        if(evt.value != 0)
        {
            UpdateScoreBoard(evt.value);
        }
        await UniTask.Delay(700);
    }

    private async UniTask PlayTargetBuff(Dice[] allDice, ScoreEventData evt)
    {
        if(evt.targetIndices != null)
        {

            foreach(int idx in evt.targetIndices)
            {
                if (idx < 0 || idx >= allDice.Length) continue;
                if (allDice[idx] == null || !allDice[idx].gameObject.activeSelf) continue;

                PlayDotweenEffect(allDice[idx], "Bounce");
                if (evt.currentDiceScore != int.MinValue) allDice[idx].UpdateDiceScoreUi(evt.currentDiceScore, true);
            }
        }
        UpdateScoreBoard(evt.value);
        await UniTask.Delay(700);
    }

    private async UniTask PlayChangeFace(Dice[] allDice, Dice targetDice, ScoreEventData evt)
    {
        if(evt.targetIndex == -1)
        {
            foreach (var dice in allDice)
            {
                if (dice == null || !dice.gameObject.activeSelf) continue;
                dice.transform.DOShakeRotation(0.4f, 90f);
            }
            await UniTask.Delay(300);

            foreach(var dice in allDice)
            {
                if (dice == null || !dice.gameObject.activeSelf) continue;
                dice.UpdateDiceImage(evt.value);
                ShowFloatingText(dice.transform.position, evt.desc);
            }
            await UniTask.Delay(700);
        }
        else if(targetDice != null)
        {
            targetDice.transform.DOShakeRotation(0.3f, 90f);
            await UniTask.Delay(300);

            targetDice.UpdateDiceImage(evt.currentDiceScore);
            if (evt.currentDiceScore != int.MinValue)
                targetDice.UpdateDiceScoreUi(evt.currentDiceScore, true);
            ShowFloatingText(targetDice.transform.position, evt.desc);
            UpdateScoreBoard(evt.value);
            await UniTask.Delay(700);
        }
    }

    private async UniTask PlayGlobalBuffs(Dice[] allDice, ScoreEventData evt)
    {
        Tween lastTween = null;
        foreach(var dice in allDice)
        {
            if (dice == null || !dice.gameObject.activeSelf) continue;
            lastTween = PlayDotweenEffect(dice, "Jump");
            ShowFloatingText(dice.transform.position, evt.desc);
        }
        UpdateScoreBoard(evt.value);

        if(lastTween != null)
            await lastTween.AsyncWaitForCompletion();
        else
            await UniTask.Delay(700);
    }

    private async UniTask PlayNegate(Dice targetDice, ScoreEventData evt)
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
        await UniTask.Delay(700);
    }
    private async UniTask PlayItemEffect(ScoreEventData evt)
    {
        ShowEffectMessage(evt.effectName, evt.effectDesc);
        var card = evt.targetIndex >= 0 ? UiController.instance.inventoryUI.FindCardByIndex(evt.targetIndex) : UiController.instance.inventoryUI.FindCardByName(evt.effectName);

        if (card != null)
        {
            Vector3 originalScale = card.transform.localScale;
            card.transform.DOScale(originalScale * 1.3f, 0.3f).SetEase(Ease.OutBack);
            await UniTask.Delay(700);
            card.transform.DOScale(originalScale, 0.3f);
            await UniTask.Delay(700);
        }
        else
        {
            ShowFloatingText(effectCanvas.position, evt.desc);
        }
        UpdateScoreBoard(evt.value);
        await UniTask.Delay(700);
    }

    private async UniTask PlayFinalScore(Dice[] allDice, ScoreEventData evt)
    {
        UpdateScoreBoard(evt.value);
        finalScoreText.transform.DOPunchScale(Vector3.one * 0.35f, 0.3f);
        await UniTask.Delay(350);
    }

    private async UniTask PlayNotice(ScoreEventData evt)
    {
        await UniTask.Delay(700);
    }

    public void ClearNegateOverlays()
    {
        foreach (var overlay in _negateOverlays)
        {
            if (overlay != null) Destroy(overlay);
        }
        _negateOverlays.Clear();
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

    public void ResetDiceColors(Dice[] allDice)
    {
        foreach(var overlay in _negateOverlays)
        {
            if (overlay != null) Destroy(overlay);
        }
        _negateOverlays.Clear();
    }

    private void PlayDiceParticle(Dice dice) 
    {
        if (diceEffect == null || dice == null) return;
        diceEffect.transform.position = dice.transform.position;
        diceEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        diceEffect.Play();
        
    }
    private float GetBaseScale(Dice dice)
    {
        if (dice.MyState != null && dice.MyState.isIgnored) return 0.8f;
        return 1.0f;
    }
    private async UniTask PlayScale(Dice dice)
    {
        PlayDiceParticle(dice);
        float baseScale = GetBaseScale(dice);
        dice.transform.DOScale(Vector3.one * baseScale * 1.3f, 0.3f);
        await UniTask.Delay(300);
        dice.transform.DOScale(Vector3.one * baseScale, 0.3f);
        await UniTask.Delay(750);
    }

    private async UniTask PlayGainReroll(ScoreEventData evt)
    {
        GameManager.instance.CurrentRerollCount++;
        Vector3 pos = UiController.instance.rerollText.transform.position;
        ShowFloatingText(pos, evt.desc);
        await UniTask.Delay(400);
    }
}

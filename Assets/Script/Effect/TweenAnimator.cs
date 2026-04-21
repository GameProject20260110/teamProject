using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;

public class TweenAnimator : MonoBehaviour
{
    public GameObject negateOverlayPrefab;
    private List<GameObject> _negateOverlays = new List<GameObject>();

    public async UniTask PlayScale(Dice dice, VFXEffectHandler vfx)
    {
        vfx.PlayDiceParticle(dice);
        float baseScale = GetBaseScale(dice);
        dice.transform.DOScale(Vector3.one * baseScale * 1.5f, 0.2f);
        await UniTask.Delay(200);
        dice.transform.DOScale(Vector3.one * baseScale, 0.2f);
        await UniTask.Delay(500);
    }

    public async UniTask PlayAddScore(Dice targetDice, ScoreEventData evt, UIEffectHandler ui)
    {
        if(targetDice != null)
        {
            PlayDotweenEffect(targetDice, "Punch");
            ui.ShowFloatingText(targetDice.transform.position, evt.desc);
            if(evt.currentDiceScore != int.MinValue)
                targetDice.UpdateDiceScoreUi(evt.currentDiceScore, true);
        }
        if (evt.value != 0)
            ui.UpdateScoreBoard(evt.value);
        await UniTask.Delay(600);
    }

    public async UniTask PlayTargetBuff(Dice[] allDice, ScoreEventData evt, UIEffectHandler ui)
    {
        if(evt.targetIndices != null)
        {
            foreach (var idx in evt.targetIndices)
            {
                if (idx < 0 || idx >= allDice.Length) continue;
                if (allDice[idx] == null || !allDice[idx].gameObject.activeSelf) continue;
                PlayDotweenEffect(allDice[idx], "Bounce");
                if (evt.currentDiceScore != int.MinValue)
                    allDice[idx].UpdateDiceScoreUi(evt.currentDiceScore, true);
            }
        }
        ui.UpdateScoreBoard(evt.value);
        await UniTask.Delay(600);
    }

    public async UniTask PlayGlobalBuffs(Dice[] allDice, ScoreEventData evt, UIEffectHandler ui)
    {
        Tween lastTween = null;
        foreach(var dice in allDice)
        {
            if (dice == null || !dice.gameObject.activeSelf) continue;
            lastTween = PlayDotweenEffect(dice, "Jump");
            ui.ShowFloatingText(dice.transform.position, evt.desc);
        }
        ui.UpdateScoreBoard(evt.value);
        if (lastTween != null)
            await lastTween.AsyncWaitForCompletion();
        else
            await UniTask.Delay(600);
    }

    public async UniTask PlayChangeFace(Dice[] allDice, Dice targetDice, ScoreEventData evt, UIEffectHandler ui)
    {
        if (evt.targetIndex == -1)
        {
            foreach(var dice in allDice)
            {
                if (dice == null || !dice.gameObject.activeSelf) continue;
                dice.transform.DOShakeRotation(0.35f, 90f);
                dice.UpdateDiceImage(evt.value);
                ui.ShowFloatingText(dice.transform.position, evt.desc);
            }
            await UniTask.Delay(300);            
        }
        else if(targetDice != null)
        {
            targetDice.transform.DOShakeRotation(0.35f, 90f);
            await UniTask.Delay(300);
            targetDice.UpdateDiceImage(evt.currentDiceScore);
            if (evt.currentDiceScore != int.MinValue)
                targetDice.UpdateDiceScoreUi(evt.currentDiceScore, true);
            ui.ShowFloatingText(targetDice.transform.position, evt.desc);
            ui.UpdateScoreBoard(evt.value);
            await UniTask.Delay(300);
        }
    }

    public async UniTask PlayNegate(Dice targetDice, ScoreEventData evt, UIEffectHandler ui)
    {
        if(targetDice != null)
        {
            Image diceImage = targetDice.GetComponent<Image>();
            if (diceImage != null)
                diceImage.DOColor(Color.gray, 0.3f);

            targetDice.transform.DOScale(Vector3.one * 0.8f, 0.3f);
            ui.ShowFloatingText(targetDice.transform.position, evt.desc);

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
            ui.ShowFloatingText(ui.effectCanvas.position, evt.desc);

        }
        await UniTask.Delay(400);
    }

    public async UniTask PlayItemCard(ScoreEventData evt, UIEffectHandler ui)
    {
        ui.ShowMessageBox(evt.effectName, evt.effectDesc);
        var card = evt.targetIndex >= 0 ? UiController.instance?.inventoryUI.FindCardByIndex(evt.targetIndex) : UiController.instance?.inventoryUI.FindCardByName(evt.effectName);

        if (card != null)
        {
            Vector3 originalScale = card.transform.localScale;
            card.transform.DOScale(originalScale * 1.5f, 0.3f).SetEase(Ease.OutBack);
            await UniTask.Delay(500);
            card.transform.DOScale(originalScale, 0.3f).SetEase(Ease.InBack);
            await UniTask.Delay(500);
        }
        ui.UpdateScoreBoard(evt.value);
        await UniTask.Delay(300);
    }

    public async UniTask PlayFinalScore(int value, UIEffectHandler ui)
    {
        ui.UpdateScoreBoard(value);
        ui.finalScoreText.transform.DOPunchScale(Vector3.one * 0.5f, 0.35f);
        await UniTask.Delay(300);
    }

    private Tween PlayDotweenEffect(Dice dice, string type)
    {
        Transform t = dice.transform;
        switch(type)
        {
            case "Punch":
                return t.DOPunchScale(Vector3.one * 0.3f, 0.3f, 10, 1);
            case "Jump":
                Sequence jumpSeq = DOTween.Sequence();
                jumpSeq.Append(t.DOJump(t.localPosition, 30f, 1, 1f));
                jumpSeq.Join(t.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360));
                return jumpSeq;
            case "Shake":
                return t.DOShakePosition(0.35f, 7f, 20, 100, false, true);
            case "Bounce":
                Sequence bounceSeq = DOTween.Sequence();
                bounceSeq.Append(t.DOLocalMoveY(t.localPosition.y + 20f, 0.15f).SetEase(Ease.OutQuad));
                bounceSeq.Append(t.DOLocalMoveY(t.localPosition.y, 0.15f).SetEase(Ease.InQuad));
                return bounceSeq;
            default:
                return null;
        }
    }

    public void ResetDiceColor(Dice[] allDice)
    {
        foreach(var overlay in _negateOverlays)
        {
            if (overlay != null)
                Destroy(overlay);
        }
        _negateOverlays.Clear();
    }

    private float GetBaseScale(Dice dice)
    {
        if (dice.MyState != null && dice.MyState.isIgnored) return 0.8f;
        return 1.0f;
    }
}

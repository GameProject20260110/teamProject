using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class VisualManager : MonoBehaviour
{
    public static VisualManager instance;

    public static event System.Action OnFloatingTextSound;
    public UIEffectHandler uiEffect;
    public VFXEffectHandler vfxEffect;
    public TweenAnimator tweenAnimator;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public async UniTask PlayScoreEventSequence(Dice[] allDice, List<ScoreEventData> events)
    {
        uiEffect.ResetLastDiceEffectName();

        foreach(var evt in events)
        {
            Dice targetDice = GetTargetDice(allDice, evt.targetIndex);

            if(evt.type != ScoreEventData.Type.ItemEffect &&
               evt.type != ScoreEventData.Type.GainReroll)
            {
                if (uiEffect.ShowMessageBox(evt.effectName, evt.effectDesc))
                    await UniTask.Delay(500);
            }
            
            switch(evt.type)
            {
                case ScoreEventData.Type.TriggerDice:
                    Dice triggerDice = GetTargetDice(allDice, evt.triggerIndex);
                    if (triggerDice != null)
                        await tweenAnimator.PlayScale(triggerDice, vfxEffect);
                    break;
                case ScoreEventData.Type.AddScore:
                    await tweenAnimator.PlayAddScore(targetDice, evt, uiEffect);
                    break;
                case ScoreEventData.Type.TargetBuff:
                    await tweenAnimator.PlayTargetBuff(allDice, evt, uiEffect);
                    break;
                case ScoreEventData.Type.ChangeFace:
                    await tweenAnimator.PlayChangeFace(allDice, targetDice, evt, uiEffect);
                    break;
                case ScoreEventData.Type.GlobalBuff:
                    await tweenAnimator.PlayGlobalBuffs(allDice, evt, uiEffect);
                    break;
                case ScoreEventData.Type.Negate:
                    await tweenAnimator.PlayNegate(targetDice, evt, uiEffect);
                    break;
                case ScoreEventData.Type.ItemEffect:
                    await tweenAnimator.PlayItemCard(evt, uiEffect);
                    break;
                case ScoreEventData.Type.GainReroll:
                    await uiEffect.PlayGainReroll(evt);
                    break;
                case ScoreEventData.Type.Notice:
                    await uiEffect.PlayNotice(evt);
                    break;
            }
        }
    }

    public void ResetDiceColors(Dice[] allDice) => tweenAnimator.ResetDiceColor(allDice);

    public void ShowFloatingText(Vector3 pos, string text)
    {
        OnFloatingTextSound?.Invoke();
        uiEffect.ShowFloatingText(pos, text);
    }
    private Dice GetTargetDice(Dice[] allDice, int index)
    {
        if (index >= 0 && index < allDice.Length)
            return allDice[index];
        return null;
    }
    
    
}   

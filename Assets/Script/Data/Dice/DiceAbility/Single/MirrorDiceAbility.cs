//using JetBrains.Annotations;
//using System.Collections.Generic;
//using UnityEngine;

//[CreateAssetMenu(fileName = "Ability", menuName = "DiceAbility/mirror")]
//public class MirrorDiceAbility : DiceData
//{
//    private void AddMirrorNotify(DiceState myState, DiceState targetDice, List<ScoreEventData> events)
//    {
//        events.Add(new ScoreEventData(ScoreEventData.Type.Notice, myState.diceIndex, 0, "")
//        {
//            effectName = abilityName,
//            effectDesc = $"{targetDice.diceData.abilityName}의 효과 복사"
//        });
//    }

//    public override void OnRuleEffect(DiceState myState, List<DiceState> allDice, List<ScoreEventData> events)
//    {
//        if(myState.diceIndex > 0)
//        {
//            var targetDice = allDice[myState.diceIndex - 1];
//            if (targetDice.diceData.timing != DiceTiming.Rule) return;
//            AddMirrorNotify(myState, targetDice, events);
//            targetDice.diceData.OnRuleEffect(targetDice, allDice, events);
//        }
//    }

//    public override void OnRollEffect(DiceState myState, List<DiceState> allDice, ref int totalScore, List<ScoreEventData> events)
//    {
//        if(myState.diceIndex > 0)
//        {
//            var targetDice = allDice[myState.diceIndex - 1];
//            if (targetDice.diceData.timing != DiceTiming.Roll) return;
//            AddMirrorNotify(myState, targetDice, events);
//            targetDice.diceData.OnRollEffect(targetDice, allDice, ref totalScore, events);
//        }
//    }

//    public override void CalculateEffect(DiceState myState, List<DiceState> allDice, ref int score, List<ScoreEventData> events)
//    {
//        if(myState.diceIndex > 0)
//        {
//            var targetDice = allDice[myState.diceIndex - 1];
//            if (targetDice.diceData.timing != DiceTiming.Calculate) return;
//            AddMirrorNotify(myState, targetDice, events);
//            targetDice.diceData.CalculateEffect(targetDice, allDice, ref score, events);
//        }
//    }

//    public override void AfterCalculateEffect(DiceState myState, List<DiceState> allDice, ref int score, List<ScoreEventData> events)
//    {
//        if(myState.diceIndex > 0)
//        {
//            var targetDice = allDice[myState.diceIndex - 1];
//            if (targetDice.diceData.timing != DiceTiming.After) return;
//            AddMirrorNotify(myState, targetDice, events);
//            targetDice.diceData.AfterCalculateEffect(targetDice, allDice, ref score, events);
//        }
//    }
//}

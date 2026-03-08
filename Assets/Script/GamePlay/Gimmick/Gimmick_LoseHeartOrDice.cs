using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Gimmick_LoseHeartOrDice", menuName = "Gimmick/LoseHeartOrDice")]
public class Gimmick_LoseHeartOrDice : GimmickSo
{
    public override void ExecuteGimmick()
    {
        if (GameManager.instance == null || PlayerManager.instance == null) return;

        if (GameManager.instance.CurrentHearts > 1)
        {
            GameManager.instance.ModifyHearts(-1);
            Debug.Log("하트 1개 감소");
        }
        else
        {
            var abilityDices = new List<int>();
            for(int i = 0; i < PlayerManager.instance.dices.Count; i++)
            {
                DiceData d = PlayerManager.instance.dices[i];
                if(d != null && d != PlayerManager.instance.defaultDice)
                {
                    abilityDices.Add(i);
                }
            }
            if(abilityDices.Count > 0)
            {
                int pick = abilityDices[Random.Range(0, abilityDices.Count)];
                PlayerManager.instance.dices[pick] = PlayerManager.instance.defaultDice;
            }
            else
            {
                GameManager.instance.ModifyHearts(-1);
                Debug.Log("효과 주사위 없음 -> 하트 1개 감소");
            }
        }
        GameManager.instance.NotifyAllUI();
    }
}

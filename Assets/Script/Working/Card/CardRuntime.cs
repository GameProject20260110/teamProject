using System;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

[Serializable]
public class CardRuntime
{
    public readonly string instanceID;
    public CardData data;

    public int rolledPower;
    public int finalPower;

    // 카드마다 고유한 ID를 부여, 같은 카드라도 다른 인스턴스로 취급
    public CardRuntime(CardData cardData)
    {
        data = cardData ?? throw new ArgumentNullException(nameof(cardData));
        instanceID = Guid.NewGuid().ToString("N");
    }

    // 멀리건 후 호출
    public void Roll()
    {
        rolledPower = data.RollPower();
        finalPower = rolledPower;
    }

    // 효과로 공격력을 바꿀 때 사용
    public void ModifyPower(int delta)
    {
        finalPower = Mathf.Max(0, finalPower + delta);
    }
}
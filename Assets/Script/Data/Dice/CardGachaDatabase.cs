using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardGacha/CardGachaDataBase")]
public class CardGachaDatabase : ScriptableObject
{
    public List<CardGachaTable> CardGachaList;


    public CardGachaTable GetTable(int index)
    {
        return CardGachaList[index];
    }
}


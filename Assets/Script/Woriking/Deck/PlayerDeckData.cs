using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Deck/PlayerDeckData")]
public class PlayerDeckData : ScriptableObject
{
    public List<DiceData> defultDeck;
}
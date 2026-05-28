using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager instance;

    [SerializeField] private int drawCount = 6;

    public List<DiceData> drawPile = new List<DiceData>();
    public List<DiceData> hand = new List<DiceData>();
    public List<DiceData> discardPile = new List<DiceData>();

    void Awake()
    {
        instance = this;
    }

    public void InitializeDeck()
    {
        drawPile = new List<DiceData>(PlayerDeck.instance.inventory);
        Shuffle(drawPile);
        hand.Clear();
        discardPile.Clear();
    }

    public void DrawDice()
    {
        DiscardHand();

        for (int i = 0; i < drawCount; i++)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0) break;
                RefillFromDiscard();
            }

            DiceData data = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(data);

            DiceManager.instance.PlaceDice(i, data);
        }
    }

    public void DiscardHand()
    {
        DiceManager.instance.ClearAllSlots();
        discardPile.AddRange(hand);
        hand.Clear();
    }

    private void RefillFromDiscard()
    {
        drawPile = new List<DiceData>(discardPile);
        discardPile.Clear();
        Shuffle(drawPile);
    }

    private void Shuffle(List<DiceData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
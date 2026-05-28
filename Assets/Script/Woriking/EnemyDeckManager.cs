using System.Collections.Generic;
using UnityEngine;

public class EnemyDeckManager : MonoBehaviour
{
    public static EnemyDeckManager instance;

    [SerializeField] private int drawCount = 6;

    public List<DiceData> drawPile = new List<DiceData>();
    public List<DiceData> enemyHand = new List<DiceData>();
    public List<DiceData> discardPile = new List<DiceData>();

    [Header("юс╫ц")]
    public PlayerDeckData enemyDeckData;

    void Awake() => instance = this;

    public void InitializeDeck()
    {
        drawPile = new List<DiceData>(enemyDeckData.defultDeck);
        Shuffle(drawPile);
        enemyHand.Clear();
        discardPile.Clear();
    }

    public void DrawEnemyDice()
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
            enemyHand.Add(data);
            DiceManager.instance.EnemyPlaceDice(i, data);
        }
    }

    public void DiscardHand()
    {
        DiceManager.instance.ClearEnemyAllSlots();
        discardPile.AddRange(enemyHand);
        enemyHand.Clear();
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
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}

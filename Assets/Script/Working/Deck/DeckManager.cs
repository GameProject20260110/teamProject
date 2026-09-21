using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private int drawCount = 6;
    public List<DiceData> drawPile = new List<DiceData>();
    public List<DiceData> hand = new List<DiceData>();
    public List<DiceData> discardPile = new List<DiceData>();

    private PlayerDeck _playerDeck;
    private DiceManager _diceManager;
    private DiceSpawnAnimation _diceSpawnAnimation;

    [Inject]
    public void Construct(PlayerDeck playerDeck, DiceManager diceManager, DiceSpawnAnimation diceSpawnAnimation)
    {
        _playerDeck = playerDeck;
        _diceManager = diceManager;
        _diceSpawnAnimation = diceSpawnAnimation;
    }

    public void InitializeDeck()
    {
        drawPile = new List<DiceData>(_playerDeck.inventory);
        Shuffle(drawPile);
        hand.Clear();
        discardPile.Clear();
    }

    public void DrawDice()
    {
        DiscardHand();
        _diceSpawnAnimation.ClearList();
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
            _diceManager.PlaceDice(i, data);
            var dice = _diceManager.panelDiceScript[i];
            if (dice != null)
            {
                var particle = dice.GetComponentInChildren<ParticleSystem>();
                _diceSpawnAnimation.RegisterDice(dice.gameObject, particle);
            }
        }
    }

    public void DiscardHand()
    {
        _diceManager.ClearAllSlots();
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
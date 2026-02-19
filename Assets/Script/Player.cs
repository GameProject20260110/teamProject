using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    public DiceData defaultDice;
    public PlayerSo player;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PushPlayerDices(DiceData Dice)
    {
        for (int i = 0; i < player.DiceSo.Length; i++)
        {
            if (player.DiceSo[i] == null)
            {
                player.DiceSo[i] = Dice;
                return;
            }

        }

    }

    public void PushPlayerDices(DiceData Dice, int index)
    {
        //if (player.DiceSo[index] == null)
        //{
            player.DiceSo[index] = Dice;
        //}

    }

    public void PullPlayerDices(DiceData Dice, int index)
    {
        if (player.DiceSo[index] == Dice)
        {
            player.DiceSo[index] = null;
        }
    }

    public void PushPlayerItems(ItemSo item)
    {
        for (int i = 0; i < player.itemSo.Count; i++)
        {
            if (player.itemSo[i] == null)
            {
                player.itemSo[i] = item;
                return;
            }

        }

    }

    public void PullPlayerDices(DiceData Dice)
    {
        for (int i = 0; i < player.DiceSo.Length; i++)
        {
            if (player.DiceSo[i] == Dice)
            {
                player.DiceSo[i] = null;
                return;
            }

        }
    }

    public void PullPlayerItems(ItemSo item)
    {
        for (int i = 0; i < player.itemSo.Count; i++)
        {
            if (player.itemSo[i] == item)
            {
                player.itemSo[i] = null;
                return;
            }

        }
    }

    public void ResetDices(DiceData[] Dices)
    {
        for(int i = 0;i < Dices.Length; i++)
        {
            player.DiceSo[i] = Dices[i];
        }
    }

}

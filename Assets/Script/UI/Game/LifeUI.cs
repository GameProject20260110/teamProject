using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class LifeUI : MonoBehaviour
{
    public Transform lifeContainer;
    public Image lifePrefab;

    private List<Image> _heart = new List<Image>();

    public void UpdateHearts(int lives) 
    {
        while(_heart.Count > 0)
        {
            Image newHeart = Instantiate(lifePrefab, lifeContainer);
            _heart.Add(newHeart);
        }

        for(int i = 0; i < _heart.Count; i++)
        {
            _heart[i].gameObject.SetActive(i < lives);
        }
    }
}

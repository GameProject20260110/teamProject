using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailPanelController : MonoBehaviour
{
    [Header("표시 영역")]
    [SerializeField] private GameObject placeholder;
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private CardVisualView visual;

    private void Awake()
    {
        Clear();
    }

    public void ShowCard(CardData data)
    {
        if (data == null)
        {
            Clear();
            return;
        }

        placeholder?.SetActive(false);
        contentRoot?.SetActive(true);
        visual?.SetCard(data);

    }

    public void Clear()
    {
        placeholder?.SetActive(true);
        contentRoot?.SetActive(false);
        visual?.Clear();
    }
}

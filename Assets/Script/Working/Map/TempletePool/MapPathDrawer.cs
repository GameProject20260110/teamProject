using UnityEngine;
using UnityEngine.UI;

public class MapPathDrawer : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private Image linePrefab;

    [Header("Container")]
    [SerializeField] private RectTransform lineContainer;

    [Header("Style")]
    [SerializeField] private float lineThickness = 5f;

    public void DrawLine(Vector2 pointA, Vector2 pointB)
    {
        var line = Instantiate(linePrefab, lineContainer);
        var rect = line.rectTransform;

        Vector2 direction = pointB - pointA;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = pointA;
        rect.sizeDelta = new Vector2(distance, lineThickness);
        rect.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void ClearLines()
    {
        foreach (Transform child in lineContainer)
            Destroy(child.gameObject);
    }
}
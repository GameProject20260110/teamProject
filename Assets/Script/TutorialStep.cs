using UnityEngine;

[System.Serializable]
public class TutorialStep
{
    public string stepName;

    [TextArea(3, 5)]
    public string message;

    public RectTransform targetUI;

    public float autoNextDelay = 0f;
}

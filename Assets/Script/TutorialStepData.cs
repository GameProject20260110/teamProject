using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "TutorialStepData", menuName = "Tutorial/Step Data")]
public class TutorialStepData : ScriptableObject
{
    [System.Serializable]
    public class Step
    {
        public string stepName;

        [TextArea(3, 5)]
        public string message;

        public string targetUIName;
    }

    public List<Step> steps = new List<Step>();

}

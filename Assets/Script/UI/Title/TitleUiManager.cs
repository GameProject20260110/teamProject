using UnityEngine;
using UnityEngine.UIElements;

public class TitleUiManager : MonoBehaviour
{
    [Header("UI ÆÐ³Î")]
    public GameObject exitPanel;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void InitUI()
    {
        if (exitPanel != null)
        {
            exitPanel.SetActive(false);
        }
    }

    public void OpenExitPanel() => exitPanel.SetActive(true);
    public void CloseExitPanel() => exitPanel.SetActive(false);
   
}

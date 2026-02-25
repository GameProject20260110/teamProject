using System;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
    public TitleUiManager uiManager;
    public Button StartBtn;

    void Start()
    {
        uiManager.InitUI();
        StartBtn.onClick.AddListener(() => SceneController.instance.LoadHomeScreen());

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm(AudioManager.Bgm.title,true);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(uiManager.exitPanel.activeSelf)
            {
                OnClickCancelExit();
            }
            else
            {
                OnClickExit();
            }
        }
    }
    
    public void OnClickExit()
    {
        uiManager.OpenExitPanel();
    }

    public void OnClickConfirmExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClickCancelExit()
    {
        uiManager.CloseExitPanel();
    }

}

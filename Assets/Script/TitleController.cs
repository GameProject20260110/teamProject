using System;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    public TitleUiManager uiManager;

    void Start()
    {
        uiManager.InitUI();


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

    public void OnClickStart()
    {
        SceneManager.LoadScene("HomeScreen");
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

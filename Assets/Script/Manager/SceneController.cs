using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadGameScene()
    {
        PlayerManager.instance.Save();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
        SceneManager.LoadScene("GameBoard");
    }

    public void LoadDiceSelect()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
        SceneManager.LoadScene("DiceSelect");
    }

    public void LoadHomeScreen()
    {
        PlayerManager.instance.Save();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
        SceneManager.LoadScene("HomeScreen");
    }

    public void LoadShop()
    {
        PlayerManager.instance.Save();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
        SceneManager.LoadScene("Shop");
    }
}

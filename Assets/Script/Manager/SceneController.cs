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
        SceneManager.LoadScene("GameBoard");
    }

    public void LoadDiceSelect()
    {
        SceneManager.LoadScene("DiceSelect");
    }

    public void LoadHomeScreen()
    {
        PlayerManager.instance.Save();
        SceneManager.LoadScene("HomeScreen");
    }

    public void LoadShop()
    {
        PlayerManager.instance.Save();
        SceneManager.LoadScene("Shop");
    }
}

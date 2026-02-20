using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LoadGameScene()
    {
        PlayerManager.instance.Save();
        SceneManager.LoadScene("GameBoard");
    }
}

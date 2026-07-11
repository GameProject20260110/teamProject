using UnityEngine;

public class DontDestroyBootStrapper : MonoBehaviour
{
    private static DontDestroyBootStrapper instance;

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
}

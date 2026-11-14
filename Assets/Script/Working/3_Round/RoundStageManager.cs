using UnityEngine;
using VContainer;

public class RoundStageManager : MonoBehaviour
{
    private AudioManager audioManager;

    [Inject]
    public void Construct(AudioManager audioManger)
    {
        this.audioManager = audioManger;
    }

    private void Awake()
    {
        audioManager.PlayBgm("Round");
    }
}

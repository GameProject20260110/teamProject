using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public int ResolutionIndex;
    public bool IsFullScreen;

    public float MasterVolume = 1f;
    public float MusicVolume = 0.6f;
    public float SfxVolume = 0.75f;

    [Header("UI")]
    [SerializeField] private GameObject SettingsContent;
    [SerializeField] private Button homeBtn;
    [SerializeField] private GameObject Panel;

    [Inject]
    public void Construct()
    {
        Instance = this;
        LoadSettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", ResolutionIndex);
        PlayerPrefs.SetInt("IsFullscreen", IsFullScreen ? 1 : 0);
        PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        PlayerPrefs.SetFloat("SfxVolume", SfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        ResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 2);
        IsFullScreen = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 0f);
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
        SfxVolume = PlayerPrefs.GetFloat("SfxVolume", 0.75f);

        ApplySettings();
    }

    public void ApplySettings()
    {
        // 해상도 적용
        FullScreenMode mode = IsFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        switch (ResolutionIndex)
        {
            case 0: Screen.SetResolution(1920, 1080, mode); break;
            case 1: Screen.SetResolution(1600, 900, mode); break;
            case 2: Screen.SetResolution(1280, 720, mode); break;
        }

    }
}

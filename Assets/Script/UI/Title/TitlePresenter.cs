using VContainer.Unity;
using DG.Tweening;
using System;

public class TitlePresenter : IInitializable, IDisposable
{
    private readonly TitleView _view;
    private readonly AudioManager _audioManager;
    private readonly SceneController _sceneController;

    public TitlePresenter(TitleView view, AudioManager audioManager, SceneController sceneController)
    {
        _view = view;
        _audioManager = audioManager;
        _sceneController = sceneController;
    }

    public void Initialize()
    {
        _view.OnStartClicked += HandleStartClicked;
        _view.OnSettingsClicked += HandleSettingsClicked;
        _view.OnQuitClicked += HandleQuitClicked;

        MainOption.instance?.SetSettingsButtonActive(false);
        _audioManager?.PlayBgm("Title");
    }

    private void HandleStartClicked()
    {
        _view.SetButtonsInteractable(false);
        _audioManager.PlaySfx("Click");
        _audioManager.StopBgm();

        _view.PlayTransitionSequence().OnComplete(() => _sceneController.LoadMapFromTitle());
    }

    private void HandleSettingsClicked()
    {
        _audioManager.PlaySfx("Click");
        MainOption.instance.ToggleSettingsPanel();
    }

    private void HandleQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Dispose()
    {
        _view.OnStartClicked -= HandleStartClicked;
        _view.OnSettingsClicked -= HandleSettingsClicked;
        _view.OnQuitClicked -= HandleQuitClicked;
    }
}

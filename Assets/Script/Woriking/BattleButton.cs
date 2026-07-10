using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class BattleButton : MonoBehaviour
{
    public enum State { EnemyTurn, Roll, PlaceComplete, InBattle }
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ButtonGlowController glowController;
    private State currentState;
    private float _currentAngle = 0f; // 누적 각도
    private UniTaskCompletionSource _waitForClick;
    private readonly string[] labels = { "상대 턴", "굴리기", "배치 완료", "전투 중" };

    private void Awake()
    {
        GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        button.onClick.AddListener(OnClick);
    }

    public async UniTask SetState(State state)
    {
        _currentAngle += 90f;
        await transform.DORotate(new Vector3(0, 0, _currentAngle - 45f), 0.3f)
            .SetEase(Ease.InQuart)
            .AsyncWaitForCompletion();
        currentState = state;
        label.text = labels[(int)state];
        bool interactable = state == State.Roll || state == State.PlaceComplete;
        button.interactable = interactable;
        canvasGroup.alpha = interactable ? 1f : 0.5f;
        switch (state)
        {
            case State.EnemyTurn:
                glowController.HideGlow();
                break;
            case State.Roll:
                glowController.ShowImageGlow();
                break;
            case State.PlaceComplete:
                break;
            case State.InBattle:
                glowController.HideImageGlow();
                glowController.ShowShaderGlow();
                break;
        }
        // 나머지 45도
        await transform.DORotate(new Vector3(0, 0, _currentAngle), 0.3f)
            .SetEase(Ease.OutQuart)
            .AsyncWaitForCompletion();
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
        canvasGroup.alpha = interactable ? 1f : 0.5f;
    }

    public UniTask WaitForClick()
    {
        _waitForClick = new UniTaskCompletionSource();
        return _waitForClick.Task;
    }

    private void OnClick()
    {
        switch (currentState)
        {
            case State.Roll:
                glowController.HideImageGlow();
                GameManager.Instance.OnClickRollBtn();
                break;
            case State.PlaceComplete:
                glowController.HideImageGlow();
                GameManager.Instance.OnClickScoreConfirmButton();
                break;
        }
    }
}
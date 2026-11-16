using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using System.Threading;

// 패널에 나오는 주사위 하나를 나타냄(주사위 이름 + 주사위 이미지 + 기본 공격력)
public class DiceRollItemView : MonoBehaviour, IPoolCallbackReceiver
{
    [SerializeField] private SpriteRenderer diceImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text powerText;
    [SerializeField] private SortingGroup sortingGroup;
    [SerializeField] private Vector2 _diceTargetSize;

    private CardRuntime _runtime;
    private int _currentFace = 1;
    private Vector3 _baseScale = Vector3.one;
    private Vector3 _diceBaseScale = Vector3.one;
    private Vector3 _powerBaseScale = Vector3.one;
    private float _alpha = 1f;
    private bool _powerVisible;

    private Tween _alphaTween;
    private DOGetter<float> _alphaGetter;
    private DOSetter<float> _alphaSetter;

    private int ResultFace => _runtime != null ? _runtime.basePower : _currentFace;
    private int PowerValue => _runtime != null ? _runtime.basePower : 0;

    private void Awake()
    {
        _baseScale = transform.localScale;
        if (diceImage != null)
        {
            _diceBaseScale = diceImage.transform.localScale;
            Vector2 spriteSize = diceImage.sprite != null ? (Vector2)diceImage.sprite.bounds.size : Vector2.one;
            _diceTargetSize = new Vector2(Mathf.Abs(spriteSize.x * _diceBaseScale.x), Mathf.Abs(spriteSize.y * _diceBaseScale.y));
        }
            if (powerText != null) _powerBaseScale = powerText.transform.localScale;

        
        if (sortingGroup == null && !TryGetComponent(out sortingGroup))
            sortingGroup = gameObject.AddComponent<SortingGroup>();

        _alphaGetter = () => _alpha;
        _alphaSetter = v => { _alpha = v; ApplyAlpha(); };
    }

    public void Setup(CardRuntime runtime, Vector3 localPos, float scale, int sortingLayerId, int sortingOrder)
    {
        _runtime = runtime;

        KillTweens();
        transform.localPosition = localPos;
        transform.localRotation = Quaternion.identity;
        transform.localScale = _baseScale * scale;

        if (sortingGroup != null)
        {
            sortingGroup.sortingLayerID = sortingLayerId;
            sortingGroup.sortingOrder = sortingOrder;
        }

        if (nameText != null) nameText.text = runtime != null && runtime.data != null ? runtime.data.cardName : string.Empty;

        // 시작 눈 = 직전 기본 공격력(첫 턴처럼 아직 없으면 1) - 굴림(로직)보다 먼저 호출됨
        _currentFace = runtime != null && runtime.basePower > 0 ? runtime.basePower : 1;
        SetFace(_currentFace);
        FitDiceToSize();

        HidePowerInstant();
        SetAlphaInstant(0f);
    }

    #region 페이드
    public void SetAlphaInstant(float alpha)
    {
        _alphaTween?.Kill();
        _alpha = alpha;
        ApplyAlpha();
    }

    public async UniTask FadeToAsync(float target, float duration, CancellationToken ct)
    {
        _alphaTween?.Kill();
        if (duration <= 0f) { SetAlphaInstant(target); return; }

        _alphaTween = DOTween.To(_alphaGetter, _alphaSetter, target, duration).SetEase(Ease.OutQuad);
        await _alphaTween.WithCancellation(ct);
    }

    private void ApplyAlpha()
    {
        SpriteRendererUtility.SetAlpha(diceImage, _alpha);
        if (nameText != null) nameText.alpha = _alpha;
        if (powerText != null) powerText.alpha = _powerVisible ? _alpha : 0f; 
    }
    #endregion

    #region 연출
    public async UniTask PlayRollAsync(float stopTime, DiceRollConfig config, CancellationToken ct)
    {
        if (config == null) { ShowResultInstant(); return; }

        int sides = _runtime != null && _runtime.data != null ? Mathf.Max(1, _runtime.data.DiceSides) : 1;

        // 프레임 단위로 경과 시간을 재서 일정 간격마다 눈 교체(Delay 객체 없이 진행)
        float interval = config.faceSwapInterval > 0f ? config.faceSwapInterval : Mathf.Max(stopTime, 0.01f);
        float elapsed = 0f;
        float nextSwap = 0f;
        while (elapsed < stopTime)
        {
            if (elapsed >= nextSwap)
            {
                _currentFace = PickOtherFace(sides, _currentFace);
                SetFace(_currentFace);
                nextSwap += interval;
            }
            await UniTask.NextFrame(cancellationToken: ct);
            elapsed += Time.deltaTime;
        }

        // 정지 - 결과 눈 + 튕김 + 숫자 공개를 같은 순간에 시작
        _currentFace = ResultFace;
        SetFace(_currentFace);
        await UniTask.WhenAll(PunchDiceAsync(config.stopPunchScale, config.stopPunchDuration, ct),
                              RevealPowerAsync(config.revealDuration, config.revealStartScale, config.revealOvershoot, ct));
    }

    private async UniTask PunchDiceAsync(float punchScale, float duration, CancellationToken ct)
    {
        if (diceImage == null) return;
        Transform t = diceImage.transform;
        t.DOKill();
        t.localScale = _diceBaseScale;
        if (duration <= 0f) return;

        await t.DOPunchScale(_diceBaseScale * (punchScale - 1f), duration, 1, 0f).WithCancellation(ct);
    }

    private async UniTask RevealPowerAsync(float duration, float startScale, float overshoot, CancellationToken ct)
    {
        if (powerText == null) return;

        powerText.text = PowerValue.ToString();
        _powerVisible = true;
        ApplyAlpha();

        Transform t = powerText.transform;
        t.DOKill();
        if (duration <= 0f) { t.localScale = _powerBaseScale; return; }

        t.localScale = _powerBaseScale * startScale;
        await t.DOScale(_powerBaseScale, duration).SetEase(Ease.OutBack, overshoot).WithCancellation(ct);
    }

    private void HidePowerInstant()
    {
        _powerVisible = false;
        if (powerText != null) powerText.transform.localScale = _powerBaseScale; 
        ApplyAlpha();
    }
    #endregion

    #region 스킵
    public void ShowResultInstant()
    {
        KillTweens();
        _currentFace = ResultFace;
        SetFace(_currentFace);
        if (diceImage != null) diceImage.transform.localScale = _diceBaseScale;

        if (powerText != null)
        {
            powerText.text = PowerValue.ToString();
            powerText.transform.localScale = _powerBaseScale;
        }
        _powerVisible = true;
        SetAlphaInstant(1f);
    }
    #endregion

    #region 풀링
    public void OnRent() { }

    public void OnReturn()
    {
        KillTweens();
        transform.localScale = _baseScale;
        if (diceImage != null) diceImage.transform.localScale = _diceBaseScale;
        HidePowerInstant();
        _runtime = null;
    }
    #endregion

    private void KillTweens()
    {
        _alphaTween?.Kill();
        transform.DOKill();
        if (diceImage != null) diceImage.transform.DOKill();
        if (powerText != null) powerText.transform.DOKill();
    }

    private void SetFace(int value)
    {
        if (diceImage == null) return;
        Sprite sprite = ResolveFaceSprite(value);
        if (sprite != null) diceImage.sprite = sprite;
    }

    // 스킨 스프라이트가 면 수보다 적어도 크래시 없이 마지막 스프라이트로 대체
    private Sprite ResolveFaceSprite(int value)
    {
        if (_runtime == null || _runtime.data == null) return null;
        DiceSkin skin = _runtime.data.diceSkin;
        if (skin == null || skin.sprites == null || skin.sprites.Length == 0) return null;

        return skin.GetSprite(Mathf.Clamp(value, 1, skin.sprites.Length));
    }

    // 굴리는 동안 같은 눈이 연속으로 나오지 않게 - 연출 전용 랜덤
    private static int PickOtherFace(int sides, int current)
    {
        int face = Random.Range(1, sides + 1);
        if (sides > 1 && face == current)
            face = face % sides + 1; 
        return face;
    }

    private void FitDiceToSize()
    {
        if (diceImage == null || diceImage.sprite == null) return;
        Vector2 size = diceImage.sprite.bounds.size;
        if (size.x <= 0f || size.y <= 0f || _diceTargetSize.x <= 0f || _diceTargetSize.y <= 0f) return;

        float k = Mathf.Min(_diceTargetSize.x / size.x, _diceTargetSize.y / size.y);
        _diceBaseScale = new Vector3(k, k, 1f);
        diceImage.transform.localScale = _diceBaseScale;
    }
}
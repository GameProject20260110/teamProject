using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening.Core;
using DG.Tweening;
using System.Threading;
using System;

// 주사위 굴림을 위한 팝업 패널
public class DiceRollPanelView : MonoBehaviour
{
    [SerializeField] private PopupOverlayView overlay;
    [SerializeField] private PopupInputBlockView inputBlocker;   // 최상단 캔버스의 화면 전체 투명 Image - 팝업 중 뒤쪽 입력 차단 + 스킵 클릭 보고
    [SerializeField] private SortingGroup panelGroup;
    [SerializeField] private SpriteRenderer panelBackground;
    [SerializeField] private TextMeshPro titleText;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject dicePrefab;
    [SerializeField] private DiceRollConfig config;

    private readonly List<DiceRollItemView> _items = new List<DiceRollItemView>();
    private readonly List<UniTask> _tasks = new List<UniTask>();
    private int _sortingLayerId;
    private float _panelAlpha;
    private Tween _panelTween;
    private DOGetter<float> _panelAlphaGetter;
    private DOSetter<float> _panelAlphaSetter;

    public event Action OnSkipRequested; // 화면 클릭시

    private void Awake()
    {
        _sortingLayerId = SortingLayerUtility.ResolveId(config != null ? config.sortingLayer : null, nameof(DiceRollPanelView));
        if (panelGroup == null && !TryGetComponent(out panelGroup))
            panelGroup = gameObject.AddComponent<SortingGroup>();

        _panelAlphaGetter = () => _panelAlpha;
        _panelAlphaSetter = SetPanelAlpha;
        HideImmediate();
    }

    private void OnDestroy()
    {
        if (inputBlocker != null) inputBlocker.OnClicked -= HandleSkipClicked;
    }

    // 열기 준비(동기)
    public bool Prepare(IReadOnlyList<CardRuntime> hand)
    {
        if (config == null || dicePrefab == null || itemContainer == null || panelBackground == null || hand == null)
        {
            Debug.LogWarning("[DiceRollPanelView] config / dicePrefab / itemContainer / panelBackground 연결을 확인하세요.");
            return false;
        }
        if (WorldPoolManager.instance == null)
        {
            Debug.LogWarning("[DiceRollPanelView] Hierarchy에 WorldPoolManager 인스턴스가 없습니다.");
            return false;
        }

        HideImmediate();
        if (overlay != null) overlay.Prepare();
        if (inputBlocker != null) inputBlocker.SetBlocking(true); // 닫힐 때(HideImmediate)까지 뒤쪽 입력 차단
        ApplySorting();
        if (titleText != null) titleText.text = config.title;

        if (!TryGetItemArea(out Vector3 areaCenter, out Vector2 areaSize)) return false;
        itemContainer.position = areaCenter;

        int total = hand.Count;
        ComputeLayout(total, areaSize, out int perRow, out int rows, out float scale);
        for (int i = 0; i < total; i++)
        {
            // 손패와 같은 좌우 순서(목록→화면 변환 규칙은 HandLayoutView와 공유)
            int visualIndex = HandLayoutView.ToVisualIndex(i, total);
            Vector3 localPos = GetItemPosition(visualIndex, total, perRow, rows, scale);

            GameObject obj = WorldPoolManager.instance.Get(dicePrefab, itemContainer.TransformPoint(localPos), itemContainer.rotation, itemContainer);
            DiceRollItemView item = obj != null ? obj.GetComponent<DiceRollItemView>() : null;
            if (item == null)
            {
                Debug.LogWarning("[DiceRollPanelView] dicePrefab에 DiceRollItemView가 없습니다.");
                if (obj != null) WorldPoolManager.instance.Return(obj);
                _items.Add(null); // 인덱스 정렬 유지
                continue;
            }

            item.Setup(hand[i], localPos, scale, _sortingLayerId, config.itemBaseOrder + visualIndex);
            _items.Add(item);
        }
        return true;
    }

    // 스킵 입력 켜기/끄기 - InputBlocker 클릭(화면 전체)을 받음. 켤 때 skipInputDelay 동안은 클릭 무시(이월 클릭 방지).
    public void SetSkipEnabled(bool enabled)
    {
        if (inputBlocker == null) return; // 예외 방지
        inputBlocker.OnClicked -= HandleSkipClicked; // 중복 구독 방지
        if (enabled) inputBlocker.OnClicked += HandleSkipClicked;
        inputBlocker.SetClickReport(enabled, enabled && config != null ? config.skipInputDelay : 0f);
    }

    // 열기: 오버레이·패널 페이드인(동시) → 항목이 왼쪽부터 순차 등장.
    public async UniTask PlayOpenAsync(CancellationToken ct)
    {
        if (config == null) return;

        UniTask overlayTask = overlay != null
            ? overlay.FadeToAsync(config.overlayAlpha, config.overlayFadeDuration, ct)
            : UniTask.CompletedTask;
        await FadePanelAsync(config.panelAlpha, config.panelFadeDuration, ct);

        int total = _items.Count;
        _tasks.Clear();
        _tasks.Add(overlayTask);
        for (int i = 0; i < total; i++)
        {
            if (_items[i] == null) continue;
            float delay = HandLayoutView.ToVisualIndex(i, total) * config.itemAppearStagger; // 왼쪽부터 차례로
            _tasks.Add(FadeItemDelayedAsync(_items[i], 1f, delay, config.itemAppearDuration, ct));
        }
        await UniTask.WhenAll(_tasks);
    }

    // 전체가 동시에 눈 교체 → 왼쪽부터 하나씩 정지(앞 주사위가 멈추면 바로 다음), 멈춘 항목은 즉시 숫자 공개.
    public async UniTask PlayRollAsync(CancellationToken ct)
    {
        if (config == null) return;

        int total = _items.Count;
        float interval = total <= 1 ? 0f : Mathf.Min(config.stopInterval, config.maxStopSpread / (total - 1));

        _tasks.Clear();
        for (int i = 0; i < total; i++)
        {
            DiceRollItemView item = _items[i];
            if (item == null) continue;

            float stopTime = config.rollDuration + HandLayoutView.ToVisualIndex(i, total) * interval;
            _tasks.Add(item.PlayRollAsync(stopTime, config, ct));
        }
        await UniTask.WhenAll(_tasks);
    }

    // 결과 확인 시간(스킵해도 동일하게 적용).
    public async UniTask WaitResultHoldAsync(CancellationToken ct)
    {
        if (config == null || config.closeHoldDelay <= 0f) return;
        await UniTask.Delay(TimeSpan.FromSeconds(config.closeHoldDelay), cancellationToken: ct);
    }

    // 스킵 - 진행 중인 연출을 끊고 최종 상태로 즉시 맞춤.
    public void ShowResultInstant()
    {
        if (config == null) return;

        _panelTween?.Kill();
        SetPanelAlpha(config.panelAlpha);
        if (overlay != null) overlay.FadeToAsync(config.overlayAlpha, 0f, CancellationToken.None).Forget(); // duration 0 = 즉시 적용
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null) _items[i].ShowResultInstant();
        }
    }

    public async UniTask CloseAsync(CancellationToken ct)
    {
        if (config == null) { HideImmediate(); return; }

        float duration = config.closeFadeDuration;
        _tasks.Clear();
        _tasks.Add(FadePanelAsync(0f, duration, ct));
        if (overlay != null) _tasks.Add(overlay.FadeToAsync(0f, duration, ct));
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null) _tasks.Add(_items[i].FadeToAsync(0f, duration, ct));
        }
        await UniTask.WhenAll(_tasks);
        HideImmediate();
    }

    // 즉시 정리 - 스킵 입력 해제 + 입력 차단 해제 + 항목 풀 반납 + 패널·오버레이 숨김.
    public void HideImmediate()
    {
        SetSkipEnabled(false);
        if (inputBlocker != null) inputBlocker.SetBlocking(false);
        ReleaseItems();
        _panelTween?.Kill();
        SetPanelAlpha(0f);
        if (overlay != null) overlay.HideImmediate();
    }

    private void HandleSkipClicked()
    {
        OnSkipRequested?.Invoke();
    }

    private async UniTask FadePanelAsync(float target, float duration, CancellationToken ct)
    {
        _panelTween?.Kill();
        if (duration <= 0f) { SetPanelAlpha(target); return; }

        _panelTween = DOTween.To(_panelAlphaGetter, _panelAlphaSetter, target, duration).SetEase(Ease.OutQuad);
        await _panelTween.WithCancellation(ct);
    }

    private static async UniTask FadeItemDelayedAsync(DiceRollItemView item, float target, float delay, float duration, CancellationToken ct)
    {
        if (delay > 0f) await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct);
        await item.FadeToAsync(target, duration, ct);
    }

    private void SetPanelAlpha(float alpha)
    {
        _panelAlpha = alpha;
        SpriteRendererUtility.SetAlpha(panelBackground, alpha);
        if (titleText != null)
        {
            titleText.alpha = alpha;
            titleText.enabled = alpha > 0f;
        }
    }

    private void ApplySorting()
    {
        if (panelGroup != null)
        {
            panelGroup.sortingLayerID = _sortingLayerId;
            panelGroup.sortingOrder = config.panelOrder;
        }
        if (panelBackground != null)
        {
            panelBackground.sortingLayerID = _sortingLayerId;
            panelBackground.sortingOrder = config.backgroundOrder;
        }
        if (titleText != null)
        {
            titleText.sortingLayerID = _sortingLayerId;
            titleText.sortingOrder = config.titleOrder;
        }
    }

    private bool TryGetItemArea(out Vector3 centerWorld, out Vector2 sizeLocal)
    {
        centerWorld = itemContainer.position;
        sizeLocal = Vector2.zero;

        Sprite sprite = panelBackground.sprite;
        if (sprite == null)
        {
            Debug.LogWarning("[DiceRollPanelView] panelBackground에 스프라이트가 없습니다.");
            return false;
        }

        bool isSimple = panelBackground.drawMode == SpriteDrawMode.Simple;
        Vector2 raw = isSimple ? (Vector2)sprite.bounds.size : panelBackground.size;
        Transform bgTransform = panelBackground.transform;
        Vector3 bgScale = bgTransform.lossyScale;
        Vector2 worldSize = new Vector2(Mathf.Abs(raw.x * bgScale.x), Mathf.Abs(raw.y * bgScale.y));
        Vector3 bgCenter = isSimple ? bgTransform.TransformPoint(sprite.bounds.center) : bgTransform.position;

        float left = bgCenter.x - worldSize.x * 0.5f + config.panelPadding.x;
        float right = bgCenter.x + worldSize.x * 0.5f - config.panelPadding.x;
        float titleTop = bgCenter.y + worldSize.y * 0.5f - config.panelPadding.y;
        float top = titleTop - config.titleAreaHeight;
        float bottom = bgCenter.y - worldSize.y * 0.5f + config.panelPadding.y;
        if (right <= left || top <= bottom)
        {
            Debug.LogWarning("[DiceRollPanelView] 패널이 여백/제목 영역보다 작습니다. 패널의 크기를 확인해주세요");
            return false;
        }

        if (titleText != null)
        {
            Vector3 titlePos = titleText.transform.position;
            titleText.transform.position = new Vector3(bgCenter.x, top + config.titleAreaHeight * 0.5f, titlePos.z);
        }

        centerWorld = new Vector3((left + right) * 0.5f, (top + bottom) * 0.5f, itemContainer.position.z);
        Vector3 containerScale = itemContainer.lossyScale;
        sizeLocal = new Vector2(Ratio(right - left, Mathf.Abs(containerScale.x)), Ratio(top - bottom, Mathf.Abs(containerScale.y)));
        return true;
    }

    // 줄 수/줄당 개수/배율 결정 - 1줄부터 maxRows까지 시도해서 처음으로 minItemScale 이상이 되는 배치, 없으면 가장 큰 배율.
    private void ComputeLayout(int total, Vector2 area, out int perRow, out int rows, out float scale)
    {
        perRow = Mathf.Max(1, total);
        rows = 1;
        scale = 1f;
        if (total <= 0) return;

        Vector2 item = config.itemSize;
        int maxRows = Mathf.Max(1, config.maxRows);
        float bestScale = -1f;

        for (int r = 1; r <= maxRows; r++) // 수정: < → <= (maxRows 줄까지 시도)
        {
            int candidatePerRow = Mathf.CeilToInt(total / (float)r);
            int candidateRows = Mathf.CeilToInt(total / (float)candidatePerRow);
            float width = candidatePerRow * item.x + (candidatePerRow - 1) * config.itemGap;
            float height = candidateRows * item.y + (candidateRows - 1) * config.rowGap;
            float s = Mathf.Min(1f, Mathf.Min(Ratio(area.x, width), Ratio(area.y, height)));

            if (s > bestScale)
            {
                bestScale = s;
                perRow = candidatePerRow;
                rows = candidateRows;
                scale = s;
            }
            if (s >= config.minItemScale) break;
        }
    }

    // 화면 순서(0 = 왼쪽 위) -> 항목 컨테이너 로컬 위치, 각 줄은 가운데 정렬
    private Vector3 GetItemPosition(int visualIndex, int total, int perRow, int rows, float scale)
    {
        int row = visualIndex / perRow;
        int col = visualIndex % perRow;
        int countInRow = row < rows - 1 ? perRow : total - perRow * (rows - 1);

        float pitchX = (config.itemSize.x + config.itemGap) * scale;
        float pitchY = (config.itemSize.y + config.rowGap) * scale;
        float x = (col - (countInRow - 1) * 0.5f) * pitchX;
        float y = ((rows - 1) * 0.5f - row) * pitchY;
        return new Vector3(x, y, 0f);
    }

    // 항목 전부 풀 반납
    private void ReleaseItems()
    {
        if (WorldPoolManager.instance != null)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                DiceRollItemView item = _items[i];
                if (item != null) WorldPoolManager.instance.Return(item.gameObject);
            }
        }
        _items.Clear(); 
    }

    // 0으로 나누기 방지(분모 0이면 1 - MathUtility.SafeDivide와 규칙이 달라 따로 둠)
    private static float Ratio(float a, float b) => b > 0.0001f ? a / b : 1f;
}
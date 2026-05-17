using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ButtonGlowController : MonoBehaviour
{
    [Header("페이드 설정")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float maxGlow = 3f;
    [SerializeField] private float maxSpeed = 0.7f;

    [Header("박스 크기")]
    [SerializeField] private float boxWidth = 0.85f;
    [SerializeField] private float boxHeight = 0.85f;

    [Header("초기 상태")]
    [SerializeField] private bool startEnabled = false;

    [Header("레퍼런스")]
    [SerializeField] private Image glow;
    [SerializeField] private float maxAlpha = 1f;

    private Material materialInstance;
    private float currentGlow = 0f;
    private float targetGlow = 0f;

    private static readonly int GlowID = Shader.PropertyToID("_GlowIntensity");
    private static readonly int SpeedID = Shader.PropertyToID("_RotationSpeed");
    private static readonly int BoxWidthID = Shader.PropertyToID("_BoxWidth");
    private static readonly int BoxHeightID = Shader.PropertyToID("_BoxHeight");

    void Awake()
    {
        Image img = GetComponent<Image>();
        materialInstance = Instantiate(img.material);
        img.material = materialInstance;
        materialInstance.SetFloat(BoxWidthID, boxWidth);
        materialInstance.SetFloat(BoxHeightID, boxHeight);
        currentGlow = startEnabled ? maxGlow : 0f;
        targetGlow = currentGlow;
        ApplyValues();
    }

    void Update()
    {
        if (Mathf.Approximately(currentGlow, targetGlow)) return;

        currentGlow = Mathf.MoveTowards(currentGlow, targetGlow, fadeSpeed * Time.deltaTime);
        ApplyValues();
    }

    void ApplyValues()
    {
        materialInstance.SetFloat(GlowID, currentGlow);
        materialInstance.SetFloat(SpeedID, currentGlow > 0.01f ? maxSpeed : 0f);

        if (glow != null)
        {
            float ratio = currentGlow / maxGlow;  // 0 ~ 1
            Color c = glow.color;
            c.a = ratio * maxAlpha;
            glow.color = c;
        }
    }

    public void ShowGlow() => targetGlow = maxGlow;
    public void HideGlow() => targetGlow = 0f;

    void OnDestroy() // 메모리 누수 방지
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}
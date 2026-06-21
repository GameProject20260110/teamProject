using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ButtonGlowController : MonoBehaviour
{
    [Header("페이드 설정")]
    [SerializeField] private float shaderFadeSpeed = 5f;
    [SerializeField] private float glowFadeSpeed = 5f;

    [Header("셰이더 설정")]
    [SerializeField] private float maxGlow = 3f;
    [SerializeField] private float maxSpeed = 0.7f;

    [Header("박스 크기")]
    [SerializeField] private float boxWidth = 0.85f;
    [SerializeField] private float boxHeight = 0.85f;

    [Header("초기 상태")]
    [SerializeField] private bool startShaderEnabled = false;
    [SerializeField] private bool startGlowEnabled = false;

    [Header("레퍼런스")]
    [SerializeField] private Image glow;
    [SerializeField] private float maxAlpha = 1f;

    private Material materialInstance;

    private float currentGlow = 0f;
    private float targetGlow = 0f;

    private float currentGlowAlpha = 0f;
    private float targetGlowAlpha = 0f;

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

        currentGlow = startShaderEnabled ? maxGlow : 0f;
        targetGlow = currentGlow;

        currentGlowAlpha = startGlowEnabled ? 1f : 0f;
        targetGlowAlpha = currentGlowAlpha;

        ApplyShaderValues();
        ApplyGlowValues();
    }

    void Update()
    {
        if (!Mathf.Approximately(currentGlow, targetGlow))
        {
            currentGlow = Mathf.MoveTowards(currentGlow, targetGlow, shaderFadeSpeed * Time.deltaTime);
            ApplyShaderValues();
        }

        if (!Mathf.Approximately(currentGlowAlpha, targetGlowAlpha))
        {
            currentGlowAlpha = Mathf.MoveTowards(currentGlowAlpha, targetGlowAlpha, glowFadeSpeed * Time.deltaTime);
            ApplyGlowValues();
        }
    }

    private void ApplyShaderValues()
    {
        materialInstance.SetFloat(GlowID, currentGlow);
        materialInstance.SetFloat(SpeedID, currentGlow > 0.01f ? maxSpeed : 0f);
    }

    private void ApplyGlowValues()
    {
        if (glow == null) return;
        Color c = glow.color;
        c.a = currentGlowAlpha * maxAlpha;
        glow.color = c;
    }

    // 셰이더만
    public void ShowShaderGlow() => targetGlow = maxGlow;
    public void HideShaderGlow() => targetGlow = 0f;

    // 이미지만
    public void ShowImageGlow() => targetGlowAlpha = 1f;
    public void HideImageGlow() => targetGlowAlpha = 0f;

    // 둘 다
    public void ShowGlow() { ShowShaderGlow(); ShowImageGlow(); }
    public void HideGlow() { HideShaderGlow(); HideImageGlow(); }

    void OnDestroy()
    {
        if (materialInstance != null)
            Destroy(materialInstance);
    }
}
using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    [Header("떠 있는 효과")]
    [SerializeField] private float floatAmplitude = 10f;
    [SerializeField] private float floatSpeed = 1.5f;

    [Header("숨 쉬기 효과")]
    [SerializeField] private bool useBreathing = true;
    [SerializeField] private float breathAmplitude = 0.03f;
    [SerializeField] private float breathSpeed = 1.5f;

    [Header("기울기 효과")]
    [SerializeField] private float tiltAmount = 5f;

    [Header("타이밍(각 주사위마다 다르게)")]
    [SerializeField] private float timeOffset = 0f;

    private Vector3 _startScale;

    private void Awake()
    {
        _startScale = transform.localScale;
        enabled = false;
    }

    private void Update()
    {
        float t = Time.time + timeOffset;

        float y = Mathf.Sin(t * floatSpeed) * floatAmplitude;
        transform.localPosition = new Vector3(0, y, 0);

        float tilt = Mathf.Sin(t * floatSpeed) * tiltAmount;
        transform.localRotation = Quaternion.Euler(0, 0, tilt);
        if(useBreathing)
        {
            float scale = 1f + Mathf.Sin(t * breathSpeed) * breathAmplitude;
            transform.localScale = _startScale * scale;
        }
    }

    public void StopFloating()
    {
        enabled = false;
        Vector3 pos = transform.localPosition;
        pos.y = 0;
        transform.localPosition = pos;
        transform.localRotation = Quaternion.identity;
        transform.localScale = _startScale;
    }

}

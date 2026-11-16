using UnityEngine;

// 수치 계산 공용 보조
public static class MathUtility
{
    public static float SafeDivide(float numerator, float denominator)
    {
        return Mathf.Approximately(denominator, 0f) ? numerator : numerator / denominator;
    }
}

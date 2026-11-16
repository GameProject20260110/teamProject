using UnityEngine;

public static class SpriteRendererUtility
{
    public static void SetAlpha(SpriteRenderer sr, float alpha, bool disableWhenZero = true)
    {
        if (sr == null) return;
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
        if (disableWhenZero) sr.enabled = alpha > 0f;
    }
}

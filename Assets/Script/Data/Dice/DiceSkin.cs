using UnityEngine;

[CreateAssetMenu(fileName = "DiceSkin", menuName = "Scriptable Objects/DiceSkin")]
public class DiceSkin : ScriptableObject
{
    public Sprite[] sprites;

    public Sprite GetSprite(int value)
    {
        int index = value - 1;
        if( sprites != null && index >= 0 && index < sprites.Length)
        {
            return sprites[index];
        }
        return null;
    }
}

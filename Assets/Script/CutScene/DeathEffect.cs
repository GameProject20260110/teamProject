using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DeathEffect : MonoBehaviour
{
    [SerializeField] private Image enemyImage;

    public Sequence PlayGrayScale()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(enemyImage.DOColor(Color.gray, 1.0f));

        seq.Join(enemyImage.transform.DOScale(0.9f, 1.0f));

        return seq;
    }

    public void RestoreGrayImage()
    {
        enemyImage.color = Color.white;
        enemyImage.transform.localScale = Vector3.one;
    }
}

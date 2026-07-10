using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class DiceVFXBase : MonoBehaviour
{
    [SerializeField] protected GameObject burstPrefab;
    [SerializeField] protected TextMeshProUGUI buffText;

    private Vector3 originScale;
    private int totalBonus;

    protected virtual void Awake()
    {
        originScale = transform.localScale;
        totalBonus = 0;
    }

    public virtual async UniTask PlayBuff(int bonusAmount, CancellationToken ct)
    {
        if (buffText != null)
        {
            totalBonus += bonusAmount;
            buffText.transform.parent.gameObject.SetActive(true);
            buffText.text = $"+{totalBonus}";
        }
        await ScalePulse(ct);
        PlayBurst();
    }

    public virtual void ResetBuff()
    {
        totalBonus = 0;
        if (buffText != null)
            buffText.transform.parent.gameObject.SetActive(false);
    }

    public virtual UniTask PlayAttack(DiceContext ctx, int damage) => UniTask.CompletedTask;
    public virtual UniTask PlayDefense(DiceContext ctx, int damage) => UniTask.CompletedTask;

    protected async UniTask ScalePulse(CancellationToken ct)
    {
        await transform.DOScale(originScale * 1.3f, 0.18f)
            .SetEase(Ease.OutQuad)
            .AsyncWaitForCompletion();

        if (ct.IsCancellationRequested || this == null) return;

        await transform.DOScale(originScale, 0.17f)
            .SetEase(Ease.InQuad)
            .AsyncWaitForCompletion();
    }

    protected void PlayBurst()
    {
        if (burstPrefab == null) return;

        GameObject burst = UIPoolManager.instance.Get(burstPrefab, transform.parent, Vector2.zero);
        //burst.transform.position = transform.position; // Get이 anchoredPosition=zero로 넣은 뒤, 정확한 위치로 재보정
        burst.transform.localScale = transform.localScale;

        Image img = burst.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.8f);

        Sequence seq = DOTween.Sequence();
        seq.Join(burst.transform.DOScale(transform.localScale * 2f, 0.4f).SetEase(Ease.OutQuad));
        seq.Join(img.DOFade(0f, 0.4f).SetEase(Ease.OutQuad));
        seq.OnComplete(() => UIPoolManager.instance.Return(burst));
    }
}

using DG.Tweening;
using UnityEngine;

public class PanelEffect : MonoBehaviour
{
    [Header("패널")]
    public RectTransform attackPanel;
    public RectTransform defensePanel;

    [Header("스케일 설정")]
    public float scaleUpValue = 1.05f;
    public float scaleDuration = 0.15f;

    private bool _attackPanelScaled = false;
    private bool _defensePanelScaled = false;

    [Header("궤도 이펙트")]
    public ButtonGlowController attackGlow;
    public ButtonGlowController defenseGlow;

    public void CheckHover(Vector2 screenPos, Camera cam)
    {
        bool overAttack = attackPanel != null && RectTransformUtility.RectangleContainsScreenPoint(attackPanel, screenPos, cam);
        bool overDefense = attackPanel != null && RectTransformUtility.RectangleContainsScreenPoint(defensePanel, screenPos, cam);

        if (overAttack && !_attackPanelScaled)
        {
            _attackPanelScaled = true;
            attackPanel.DOScale(scaleUpValue, scaleDuration).SetEase(Ease.OutQuad);
        } 
        else if(!overAttack && _attackPanelScaled)
        {
            _attackPanelScaled = false;
            attackPanel.DOScale(Vector3.one, scaleDuration).SetEase(Ease.InQuad);
        }

        if(overDefense && !_defensePanelScaled)
        {
            _defensePanelScaled = true;
            defensePanel.DOScale(scaleUpValue, scaleDuration).SetEase(Ease.OutQuad);
        }
        else if(!overDefense && _defensePanelScaled)
        {
            _defensePanelScaled = false;
            defensePanel.DOScale(Vector3.one, scaleDuration).SetEase(Ease.InQuad);
        }
    }

    public void ResetPanelScale()
    {
        if(_attackPanelScaled)
        {
            _attackPanelScaled = false;
            attackPanel.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutQuad);
        }
        if(_defensePanelScaled)
        {
            _defensePanelScaled = false;
            attackPanel.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutQuad);
        }
    }

    public void ShowGlow()
    {
        attackGlow?.ShowGlow();
        defenseGlow?.ShowGlow();
    }

    public void HideGlow()
    {
        attackGlow?.HideGlow();
        defenseGlow?.HideGlow();
    }
}

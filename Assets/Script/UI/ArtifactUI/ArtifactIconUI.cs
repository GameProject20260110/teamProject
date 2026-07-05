using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class ArtifactIconUI : MonoBehaviour
{
    [SerializeField] private Image artifactIcon;
    [SerializeField] private ParticleSystem ps;

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    public void SetUp(BattleItemSo artifact)
    {
        if (artifact.itemIcon != null)
            artifactIcon.sprite = artifact.itemIcon;
    }

    public void PlayTriggerEffect()
    {
        transform.DOKill();
        transform.localScale = _originalScale;

        transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.4f, 5, 1f).SetLink(gameObject);
    }

    public void SetParticleActive(bool active)
    {
        if (ps == null) return;

        if (active) ps.Play();
        else ps.Stop();
    }
}

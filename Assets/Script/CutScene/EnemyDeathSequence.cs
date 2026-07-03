using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class EnemyDeathSequence : MonoBehaviour
{
    public static EnemyDeathSequence instance;

    [SerializeField] private MagicCircleEffect magicCircle;
    [SerializeField] private ParticleSystem burst;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup battlecanvasGroup;
    private GameObject enemyObject;

    private SpriteRenderer[] enemyRenderers;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void SetupEnemy(GameObject enemyObject)
    {
        this.enemyObject = enemyObject;
        var enemyCharacter = enemyObject.GetComponentInChildren<EnemyCharacter>();
        enemyRenderers = enemyCharacter.Renderers;
    }

    public async UniTask PlayDeathSequence(Vector2 position)
    {
        canvasGroup.alpha = 1;
        battlecanvasGroup.alpha = 0;

        magicCircle.PlaySealEffect(position);
        PlayGrayScale();

        await UniTask.Delay(1000);

        PlayBurst(position);
        if (enemyObject != null)
            enemyObject.SetActive(false);

        await UniTask.Delay(1000);

        RestoreGrayImage();
        canvasGroup.alpha = 0;
    }

    private void PlayGrayScale()
    {
        if (enemyRenderers == null || enemyRenderers.Length == 0) return;

        Sequence seq = DOTween.Sequence();
        foreach (var sr in enemyRenderers)
            seq.Join(sr.DOColor(Color.gray, 1.0f));
    }

    private void RestoreGrayImage()
    {
        if (enemyRenderers == null) return;

        foreach (var sr in enemyRenderers)
            sr.color = Color.white;
    }

    public void PlayBurst(Vector2 position)
    {
        burst.transform.position = position;
        burst.gameObject.SetActive(true);
        burst.Play();
    }
}

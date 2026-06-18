using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyDeathSequence : MonoBehaviour
{
    [SerializeField] private MagicCircleEffect magicCircle;
    [SerializeField] private DeathEffect enemyEffect;
    [SerializeField] private ParticleSystem burst;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup battlecanvasGroup;
    [SerializeField] private CanvasGroup enemyImageGroup;

    public async UniTask PlayDeathSequence(Vector2 position)
    {
        canvasGroup.alpha = 1;
        battlecanvasGroup.alpha = 0;

        magicCircle.PlaySealEffect(position);
        enemyEffect.PlayGrayScale();

        await UniTask.Delay(1000);

        PlayBurst(position);
        enemyImageGroup.alpha = 0;

        await UniTask.Delay(1000);
        enemyEffect.RestoreGrayImage();
        canvasGroup.alpha = 0;
    }

    public void PlayBurst(Vector2 position)
    {
        burst.transform.position = position;
        burst.gameObject.SetActive(true);
        burst.Play();
    }
}

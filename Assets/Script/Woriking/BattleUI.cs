using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class BattleUI : MonoBehaviour
{
    [Header("HP Bars")]
    [SerializeField] private Image playerHPFill;
    [SerializeField] private Image enemyHPFill;

    [Header("HP Texts")]
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private TextMeshProUGUI enemyHPText;
    [SerializeField] private TextMeshProUGUI playerShieldText;
    [SerializeField] private TextMeshProUGUI enemyShieldText;
    [SerializeField] private TextMeshProUGUI currentTurn;

    [Header("Damage Text (Optional)")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Transform playerDamageSpawn;
    [SerializeField] private Transform enemyDamageSpawn;
    [SerializeField] private RoundController turnUI;
    [SerializeField] private float floatHeight = 100f;
    [SerializeField] private float floatDuration = 0.7f;

    [Header("Shield")]
    [SerializeField] private GameObject shieldObject;
    [SerializeField] private TextMeshProUGUI shieldText;

    public void UpdatePlayerHP(int current, int max)
    {
        if (playerHPFill != null)
            playerHPFill.fillAmount = (float)current / max;

        if (playerHPText != null)
            playerHPText.text = $"{current}/{max}";
    }

    public void UpdatePlayerShield(int current)
    {
        current = current < 0 ? 0 : current;
        playerShieldText.text = current.ToString();
    }

    public void UpdateEnemyShield(int current)
    {
        Debug.Log(1);
        current = current < 0 ? 0 : current;
        enemyShieldText.text = current.ToString();
    }

    public void UpdateEnemyHP(int current, int max)
    {
        if (enemyHPFill != null)
            enemyHPFill.fillAmount = (float)current / max;

        if (enemyHPText != null)
            enemyHPText.text = $"{current}/{max}";
    }

    public async UniTask UpdateCurrentTurn(int currentTurn)
    {
        await turnUI.NextTurn(currentTurn);
    }

    // 데미지 텍스트
    public void ShowDamageText(int damage, bool isPlayer)
    {
        ShowFloatingText(damage, isPlayer, Color.red, "-");
    }

    // 추가 데미지 텍스트
    public void ShowBonusDamageText(int damage)
    {
        Debug.Log($"ShowBonusDamage 호출 : {damage}");
        ShowFloatingText(damage, false, Color.yellow, "-");
    }

    // 힐 텍스트
    public void ShowHealText(int amount)
    {
        Debug.Log($"ShowHealText 호출 : {amount}");
        ShowFloatingText(amount, true, Color.green, "+");
    }

    public void ShowFloatingText(int damage, bool isPlayer, Color color, string prefix)
    {
        if (damageTextPrefab == null) return;

        Transform spawnPos = isPlayer ? playerDamageSpawn : enemyDamageSpawn;
        if (spawnPos == null) return;

        GameObject textObj = Instantiate(damageTextPrefab, spawnPos.position, Quaternion.identity, spawnPos);
        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();

        if (tmpText != null)
        {
            tmpText.text = $"{prefix}{damage}";
            tmpText.color = color;

            tmpText.transform.DOMoveY(floatHeight, floatDuration)
                .SetRelative()
                .SetEase(Ease.OutQuart)
                .SetLink(textObj);

            tmpText.DOFade(0f, floatDuration)
                .SetEase(Ease.InQuad)
                .SetLink(textObj)
                .OnComplete(() => Destroy(textObj));
        }
        else
        {
            Destroy(textObj, 1f);
        }   
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

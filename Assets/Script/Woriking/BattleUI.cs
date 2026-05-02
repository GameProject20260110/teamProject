using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    [Header("HP Bars")]
    [SerializeField] private Image playerHPFill;
    [SerializeField] private Image enemyHPFill;

    [Header("HP Texts")]
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private TextMeshProUGUI enemyHPText;
    [SerializeField] private TextMeshProUGUI playerShieldText;
    [SerializeField] private TextMeshProUGUI currentTurn;

    [Header("Damage Text (Optional)")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Transform playerDamageSpawn;
    [SerializeField] private Transform enemyDamageSpawn;
    [SerializeField] private TextMeshProUGUI enemyAttackAmount;
    [SerializeField] private RoundController turnUI;

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

    public void UpdateEnemyAttackAmount(int current)
    {
        enemyAttackAmount.text = current.ToString();
    }

    public void UpdatePlayerShield(int current)
    {
        current = current < 0 ? 0 : current;
        playerShieldText.text = current.ToString();
    }

    public void UpdateEnemyHP(int current, int max)
    {
        if (enemyHPFill != null)
            enemyHPFill.fillAmount = (float)current / max;

        if (enemyHPText != null)
            enemyHPText.text = $"{current}/{max}";
    }

    public void UpdateCurrentTurn(int currentTurn)
    {
        turnUI.NextTurn(currentTurn);
    }

    public void ShowDamageText(int damage, bool isPlayer)
    {
        if (damageTextPrefab == null) return;

        Transform spawnPos = isPlayer ? playerDamageSpawn : enemyDamageSpawn;
        if (spawnPos == null) return;

        GameObject damageObj = Instantiate(damageTextPrefab, spawnPos.position, Quaternion.identity, spawnPos);
        TextMeshProUGUI damageText = damageObj.GetComponent<TextMeshProUGUI>();

        if (damageText != null)
        {
            damageText.text = $"-{damage}";
            damageText.color = Color.red;
        }

        Destroy(damageObj, 1f);
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

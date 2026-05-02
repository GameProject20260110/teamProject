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
    [SerializeField] private TextMeshProUGUI currentTurn;

    [Header("Damage Text (Optional)")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Transform playerDamageSpawn;
    [SerializeField] private Transform enemyDamageSpawn;

    [Header("Shield")]
    [SerializeField] private GameObject shieldObject;
    [SerializeField] private TextMeshProUGUI shieldText;

    public void UpdatePlayerHP(int current, int max)
    {
        if (playerHPFill != null)
            playerHPFill.fillAmount = (float)current / max;

        if (playerHPText != null)
            playerHPText.text = $"{current}/{max}";

        UpdateHPColor(playerHPFill, (float)current / max);
    }

    public void UpdateEnemyHP(int current, int max)
    {
        if (enemyHPFill != null)
            enemyHPFill.fillAmount = (float)current / max;

        if (enemyHPText != null)
            enemyHPText.text = $"{current}/{max}";

        UpdateHPColor(enemyHPFill, (float)current / max);
    }

    public void UpdateShield(int defensePower)
    {
        if (shieldObject == null) return;
        
        if(defensePower <= 0)
        {
            shieldObject.SetActive(false);
            return;
        }

        shieldObject.SetActive(true);
        if (shieldText != null)
            shieldText.text = defensePower.ToString();
    }

    private void UpdateHPColor(Image hpBar, float ratio)
    {
        if (hpBar == null) return;

        if (ratio < 0.3f)
            hpBar.color = Color.red;
        else if (ratio < 0.6f)
            hpBar.color = Color.yellow;
        else
            hpBar.color = Color.green;
    }

    public void UpdateCurrentTurn(int currentTurn)
    {
        this.currentTurn.text = currentTurn.ToString();
    }

    public void ShowDamageText(int damage, bool isPlayer)
    {
        if (damageTextPrefab == null) return;

        Transform spawnPos = isPlayer ? playerDamageSpawn : enemyDamageSpawn;
        if (spawnPos == null) return;

        GameObject damageObj = Instantiate(damageTextPrefab, spawnPos.position, Quaternion.identity, spawnPos);
        Text damageText = damageObj.GetComponent<Text>();

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

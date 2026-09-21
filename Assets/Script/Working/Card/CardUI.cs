using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CardUI : MonoBehaviour, IPoolCallbackReceiver
{
    [Header("UI 참조")]
    [SerializeField] private RollAnimator rollAnimator;
    [SerializeField] private TextMeshPro cardNameText;
    [SerializeField] private TextMeshPro manaCostText;
    [SerializeField] private TextMeshPro attackText;
    [SerializeField] private TextMeshPro descText;
    

    public CardRuntime Runtime { get; private set; }

    // 카드 배정할 때 호출(WorldPoolManager에서 SetUp 호출)
    public void SetUp(CardRuntime runtime)
    {
        Runtime = runtime;
        if(runtime?.data == null)
        {
            Debug.LogWarning("[CardUI] CardRuntime 또는 CardData가 없습니다.");
            return;
        }

        if (cardNameText != null) cardNameText.text = runtime.data.cardName;
        if (manaCostText != null) manaCostText.text = runtime.data.cost.ToString();
        if (descText != null) descText.text = runtime.data.description;
        rollAnimator?.SetFace(1, runtime.data.diceSkin);

        RefreshPower();

    }

    // 멀리건 직후 호출
    public async UniTask PlayRollAsync()
    {
        if (Runtime?.data == null || rollAnimator == null) return;

        Runtime?.Roll();
        await rollAnimator.PlayAsync(Runtime.data.diceSkin, Runtime.data.DiceSides, Runtime.rolledPower);
        RefreshPower();
    }

    // 카드 효과로 공격력이 바뀔 때마다 호출
    public void RefreshPower()
    {
        if (attackText != null && Runtime != null)
            attackText.text = Runtime.finalPower.ToString();
    }

    public void OnRent() { }

    public void OnReturn()
    {
        Runtime = null;
    }
}

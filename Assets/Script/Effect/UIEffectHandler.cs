using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIEffectHandler : MonoBehaviour
{
    public GameObject floatingText;
    public Transform effectCanvas;

    private string _lastDiceEffectName;
    private int _currentDisplayScore = 0;

    public void ResetLastDiceEffectName()
    {
        _lastDiceEffectName = "";
    }

    public bool ShowMessageBox(string effectName, string effectDesc)
    {
        if (string.IsNullOrEmpty(effectName) || effectName == _lastDiceEffectName) return false;
        _lastDiceEffectName = effectName;

        string message = string.IsNullOrEmpty(effectDesc) ? effectName : $"{effectName}\n{effectDesc}";
        UiController.instance?.notificationUI.Show(message, 0.5f);
        return true;
    }

    public void ShowFloatingText(Vector3 wordPos, string text)
    {
        if (floatingText == null) return;
        GameObject obj = Instantiate(floatingText, effectCanvas);
        obj.transform.position = wordPos + Vector3.up * 70f;

        TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = text;

        obj.transform.DOMoveY(obj.transform.position.y + 50f, 0.5f);
        tmp.DOFade(0, 1f).OnComplete(() => Destroy(obj));
    }

    public async UniTask PlayGainReroll(ScoreEventData evt)
    {
        ShowMessageBox(evt.effectName, evt.effectDesc);
        await UniTask.Delay(500);
        GameManager.instance.CurrentRerollCount++;
        Vector3 rerollPos = UiController.instance.rerollText.transform.position;
        ShowFloatingText(rerollPos, evt.desc);
        await UniTask.Delay(500);
    }

    public async UniTask PlayNotice(ScoreEventData evt)
    {
        await UniTask.Delay(500);
    }
}

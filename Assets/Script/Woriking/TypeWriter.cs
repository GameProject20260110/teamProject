using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;

public class TypeWriter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float delay = 0.05f;

    public async UniTask Play(string content, CancellationToken ct = default)
    {
        text.text = "";
        foreach (char c in content)
        {
            ct.ThrowIfCancellationRequested();
            text.text += c;
            await UniTask.Delay((int)(delay * 1000), cancellationToken: ct);
        }
    }
}

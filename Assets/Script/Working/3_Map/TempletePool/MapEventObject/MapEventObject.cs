using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class MapEventObject : MonoBehaviour
{
    public abstract UniTask PlayAsync();
}

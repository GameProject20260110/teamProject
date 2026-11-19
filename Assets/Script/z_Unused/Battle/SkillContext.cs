using System.Threading;
using UnityEngine;

public struct SkillContext
{
    public bool isPlayer;
    public int damage;
    public Vector3 startPos;
    public Vector3 targetPos;
    public System.Action onHit;
    public System.Action onEnd;
    public CancellationToken ct;
}

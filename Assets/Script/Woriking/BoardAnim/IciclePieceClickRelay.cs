using UnityEngine;

public class IciclePieceClickRelay : MonoBehaviour
{
    private IcicleSlot _owner;

    public void Init(IcicleSlot owner) => _owner = owner;

    private void OnMouseDown() => _owner?.OnPieceClicked();
}
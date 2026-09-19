public class BoardDecorationController
{
    private readonly BattleEventBus _eventBus;
    private readonly IcicleSlot[] _icicleSlots;
    //private readonly SnowTile[] _snowTiles;

    public BoardDecorationController(BattleEventBus eventBus, IcicleSlot[] icicleSlots, SnowTile[] snowTiles)
    {
        _eventBus = eventBus;
        _icicleSlots = icicleSlots ?? new IcicleSlot[0];
        //_snowTiles = snowTiles ?? new SnowTile[0];

        _eventBus.OnTurnEnd += HandleTurnEnd;
    }

    public void Dispose()
    {
        _eventBus.OnTurnEnd -= HandleTurnEnd;
    }

    private void HandleTurnEnd(BattleContext ctx)
    {
        foreach (var slot in _icicleSlots)
        {
            if (slot == null) continue;
            slot.AdvanceGrowth();
        }

        //foreach (var tile in _snowTiles)
        //{
        //    if (tile == null) continue;
        //    tile.AdvanceSnow();
        //}
    }
}

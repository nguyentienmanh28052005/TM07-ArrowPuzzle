public sealed class BoardState
{
    public CellOccupancy Occupancy { get; }
    public bool IsValid => Occupancy != null && Occupancy.IsValid;

    public BoardState(CellOccupancy occupancy)
    {
        Occupancy = occupancy;
    }

    public static BoardState FromGridManager(GridManager manager)
    {
        return manager == null ? null : new BoardState(new CellOccupancy(manager));
    }

    public static BoardState Active
    {
        get { return FromGridManager(GridManager.Instance); }
    }
}

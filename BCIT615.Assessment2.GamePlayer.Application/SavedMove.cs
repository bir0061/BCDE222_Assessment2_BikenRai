namespace BCIT615.Assessment2.GamePlayer.Application;

public class SavedMove
{
    public int SequenceNumber { get; set; }

    public int FromRow { get; set; }

    public int FromColumn { get; set; }

    public int ToRow { get; set; }

    public int ToColumn { get; set; }

    public string MovementPiece { get; set; } = string.Empty;
}

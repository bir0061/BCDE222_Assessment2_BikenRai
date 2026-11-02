namespace BCIT615.Assessment2.GamePlayer.Persistence;

public class SavedGame
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string BoardId { get; set; } = "ReferenceBoard";

    public int CurrentRow { get; set; }

    public int CurrentColumn { get; set; }

    public bool IsComplete { get; set; }

    public string MoveHistoryJson { get; set; } = "[]";

    public DateTime SavedAt { get; set; }
}
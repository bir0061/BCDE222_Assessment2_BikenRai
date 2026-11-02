namespace BCIT615.Assessment2.GamePlayer.Persistence;

public interface IGameSaveRepository
{
    Task<int> SaveAsync(SavedGame game, int? existingSaveId);

    Task<List<SavedGame>> GetAllAsync();

    Task<SavedGame?> GetByIdAsync(int id);

    Task<bool> DeleteAsync(int id);
}
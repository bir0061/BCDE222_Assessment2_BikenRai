using Microsoft.EntityFrameworkCore;

namespace BCIT615.Assessment2.GamePlayer.Persistence;

public class GameSaveRepository : IGameSaveRepository
{
    private readonly GamePlayerDbContext _context;

    public GameSaveRepository(GamePlayerDbContext context)
    {
        _context = context;
    }


    public async Task<int> SaveAsync(SavedGame game, int? existingSaveId)
    {
        string name = game.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Please enter a game name.");
        }

        // Check whether another saved game already uses this name.
        bool duplicateName = await _context.SavedGames
            .AnyAsync(g =>
                g.Name.ToLower() == name.ToLower() &&
                g.Id != existingSaveId);

        if (duplicateName)
        {
            throw new InvalidOperationException(
                $"A saved game named '{name}' already exists. Please choose a different name.");
        }

        // If this is an existing saved game, update it.
        if (existingSaveId.HasValue)
        {
            var existingGame = await _context.SavedGames
                .FirstOrDefaultAsync(g => g.Id == existingSaveId.Value);

            if (existingGame == null)
            {
                throw new InvalidOperationException(
                    "The saved game no longer exists. Please save it as a new game.");
            }

            existingGame.Name = name;
            existingGame.BoardId = game.BoardId;
            existingGame.CurrentRow = game.CurrentRow;
            existingGame.CurrentColumn = game.CurrentColumn;
            existingGame.IsComplete = game.IsComplete;
            existingGame.MoveHistoryJson = game.MoveHistoryJson;
            existingGame.SavedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existingGame.Id;
        }

        // Otherwise, create a new saved game.
        game.Name = name;
        game.SavedAt = DateTime.UtcNow;

        _context.SavedGames.Add(game);

        await _context.SaveChangesAsync();

        return game.Id;
    }


    public async Task<List<SavedGame>> GetAllAsync()
    {
        return await _context.SavedGames
            .AsNoTracking()
            .OrderByDescending(game => game.SavedAt)
            .ToListAsync();
    }

    public async Task<SavedGame?> GetByIdAsync(int id)
    {
        return await _context.SavedGames
            .AsNoTracking()
            .FirstOrDefaultAsync(game => game.Id == id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var game = await _context.SavedGames.FindAsync(id);

        if (game == null)
        {
            return false;
        }

        _context.SavedGames.Remove(game);

        await _context.SaveChangesAsync();

        return true;
    }
}
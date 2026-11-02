using Microsoft.EntityFrameworkCore;

namespace BCIT615.Assessment2.GamePlayer.Persistence;

public class GameSaveRepository : IGameSaveRepository
{
    private readonly GamePlayerDbContext _context;

    public GameSaveRepository(GamePlayerDbContext context)
    {
        _context = context;
    }


    public async Task<int> SaveAsync(SavedGame game)
    {
        bool nameExists = await _context.SavedGames
            .AnyAsync(g => g.Name.ToLower() == game.Name.Trim().ToLower());

        if (nameExists)
        {
            throw new InvalidOperationException(
                $"A saved game named '{game.Name}' already exists. Please choose a different name.");
        }

        game.Name = game.Name.Trim();
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
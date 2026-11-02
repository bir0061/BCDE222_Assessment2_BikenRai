using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BCIT615.Assessment2.GamePlayer.Persistence;

public class GamePlayerDbContextFactory
    : IDesignTimeDbContextFactory<GamePlayerDbContext>
{
    public GamePlayerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<GamePlayerDbContext>();

        optionsBuilder.UseSqlite("Data Source=gameplayer.db");

        return new GamePlayerDbContext(optionsBuilder.Options);
    }
}
using Microsoft.EntityFrameworkCore;

namespace BCIT615.Assessment2.GamePlayer.Persistence;

public class GamePlayerDbContext : DbContext
{
    public GamePlayerDbContext(
        DbContextOptions<GamePlayerDbContext> options)
        : base(options)
    {
    }

    public DbSet<SavedGame> SavedGames => Set<SavedGame>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SavedGame>(entity =>
        {
            entity.HasKey(game => game.Id);

            entity.Property(game => game.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(game => game.BoardId)
                .IsRequired();

            entity.Property(game => game.MoveHistoryJson)
                .IsRequired();
        });
    }
}

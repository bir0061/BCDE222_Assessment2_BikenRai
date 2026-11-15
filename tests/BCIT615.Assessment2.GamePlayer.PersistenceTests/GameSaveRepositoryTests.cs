using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BCIT615.Assessment2.GamePlayer.Persistence;

namespace BCIT615.Assessment2.GamePlayer.PersistenceTests;

[TestClass]
public class GameSaveRepositoryTests
{
    private SqliteConnection _connection = null!;
    private GamePlayerDbContext _context = null!;
    private GameSaveRepository _repository = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<GamePlayerDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new GamePlayerDbContext(options);

        await _context.Database.EnsureCreatedAsync();

        _repository = new GameSaveRepository(_context);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [TestMethod]
    public async Task SaveAsync_NewGame_CreatesSavedRecord()
    {
        var game = new SavedGame
        {
            Name = "Test Game",
            BoardId = "ReferenceBoard",
            CurrentRow = 5,
            CurrentColumn = 3,
            IsComplete = false,
            MoveHistoryJson = "[]"
        };

        int id = await _repository.SaveAsync(game, null);

        var savedGame = await _repository.GetByIdAsync(id);

        Assert.IsTrue(id > 0);
        Assert.IsNotNull(savedGame);
        Assert.AreEqual("Test Game", savedGame.Name);
        Assert.AreEqual(5, savedGame.CurrentRow);
        Assert.AreEqual(3, savedGame.CurrentColumn);
        Assert.IsFalse(savedGame.IsComplete);
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsAllSavedGames()
    {
        await _repository.SaveAsync(
            new SavedGame { Name = "Game One" }, null);

        await _repository.SaveAsync(
            new SavedGame { Name = "Game Two" }, null);

        var games = await _repository.GetAllAsync();

        Assert.AreEqual(2, games.Count);
    }

    [TestMethod]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectGame()
    {
        int id = await _repository.SaveAsync(
            new SavedGame
            {
                Name = "Load Test",
                CurrentRow = 3,
                CurrentColumn = 5
            }, null);

        var result = await _repository.GetByIdAsync(id);

        Assert.IsNotNull(result);
        Assert.AreEqual(id, result.Id);
        Assert.AreEqual("Load Test", result.Name);
        Assert.AreEqual(3, result.CurrentRow);
        Assert.AreEqual(5, result.CurrentColumn);
    }

    [TestMethod]
    public async Task SaveAsync_ExistingGame_UpdatesSameRecord()
    {
        int id = await _repository.SaveAsync(
            new SavedGame { Name = "Original Game" }, null);

        var updatedGame = new SavedGame
        {
            Name = "Updated Game",
            BoardId = "ReferenceBoard",
            CurrentRow = 1,
            CurrentColumn = 4,
            IsComplete = true,
            MoveHistoryJson = "[{\"SequenceNumber\":1}]"
        };

        int updatedId = await _repository.SaveAsync(updatedGame, id);

        var result = await _repository.GetByIdAsync(id);
        var allGames = await _repository.GetAllAsync();

        Assert.AreEqual(id, updatedId);
        Assert.AreEqual(1, allGames.Count);
        Assert.IsNotNull(result);
        Assert.AreEqual("Updated Game", result.Name);
        Assert.AreEqual(1, result.CurrentRow);
        Assert.AreEqual(4, result.CurrentColumn);
        Assert.IsTrue(result.IsComplete);
        Assert.AreEqual(
            updatedGame.MoveHistoryJson,
            result.MoveHistoryJson);
    }

    [TestMethod]
    public async Task SaveAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        await _repository.SaveAsync(
            new SavedGame { Name = "Duplicate Game" }, null);

        var duplicate = new SavedGame
        {
            Name = "Duplicate Game"
        };

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => _repository.SaveAsync(duplicate, null));
    }

    [TestMethod]
    public async Task SaveAsync_MissingExistingId_ThrowsInvalidOperationException()
    {
        var game = new SavedGame
        {
            Name = "Missing Game"
        };

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => _repository.SaveAsync(game, 999));
    }

    [TestMethod]
    public async Task DeleteAsync_ExistingGame_DeletesRecord()
    {
        int id = await _repository.SaveAsync(
            new SavedGame { Name = "Delete Test" }, null);

        bool deleted = await _repository.DeleteAsync(id);

        var result = await _repository.GetByIdAsync(id);

        Assert.IsTrue(deleted);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task DeleteAsync_MissingGame_ReturnsFalse()
    {
        bool deleted = await _repository.DeleteAsync(999);

        Assert.IsFalse(deleted);
    }
}

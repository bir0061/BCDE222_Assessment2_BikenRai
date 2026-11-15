using System.IO;
using BCIT615.Assessment2.GamePlayer.Application;
using BCIT615.Assessment2.GamePlayer.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCIT615.Assessment2.GamePlayer.PersistenceTests;

[TestClass]
public class GameRestorationServiceTests
{
    private readonly GameRestorationService _service = new();

    private static SavedGame CreateSavedGame(
        string moveHistoryJson,
        int currentRow,
        int currentColumn,
        bool isComplete,
        string boardId = "ReferenceBoard")
    {
        return new SavedGame
        {
            Name = "Restoration Test",
            BoardId = boardId,
            CurrentRow = currentRow,
            CurrentColumn = currentColumn,
            IsComplete = isComplete,
            MoveHistoryJson = moveHistoryJson
        };
    }

    [TestMethod]
    public void Restore_IncompleteGame_RestoresPositionAndHistory()
    {
        // Arrange
        string history =
            """
            [
                {
                    "SequenceNumber": 1,
                    "FromRow": 5,
                    "FromColumn": 0,
                    "ToRow": 5,
                    "ToColumn": 3,
                    "MovementPiece": "Rook"
                }
            ]
            """;

        SavedGame saved = CreateSavedGame(
            history, 5, 3, false);

        // Act
        var restoredGame = _service.Restore(saved);

        // Assert
        Assert.AreEqual(5, restoredGame.CurrentPosition.Row);
        Assert.AreEqual(3, restoredGame.CurrentPosition.Column);
        Assert.AreEqual(1, restoredGame.MoveHistory.Count);
        Assert.IsFalse(restoredGame.IsComplete);
    }

    [TestMethod]
    public void Restore_CompletedGame_RestoresCompletedState()
    {
        // Arrange
        string history =
            """
            [
                {
                    "SequenceNumber": 1,
                    "FromRow": 5,
                    "FromColumn": 0,
                    "ToRow": 5,
                    "ToColumn": 3,
                    "MovementPiece": "Rook"
                },
                {
                    "SequenceNumber": 2,
                    "FromRow": 5,
                    "FromColumn": 3,
                    "ToRow": 3,
                    "ToColumn": 5,
                    "MovementPiece": "Bishop"
                },
                {
                    "SequenceNumber": 3,
                    "FromRow": 3,
                    "FromColumn": 5,
                    "ToRow": 1,
                    "ToColumn": 4,
                    "MovementPiece": "Knight"
                },
                {
                    "SequenceNumber": 4,
                    "FromRow": 1,
                    "FromColumn": 4,
                    "ToRow": 0,
                    "ToColumn": 5,
                    "MovementPiece": "King"
                }
            ]
            """;

        SavedGame saved = CreateSavedGame(
            history, 0, 5, true);

        // Act
        var restoredGame = _service.Restore(saved);

        // Assert
        Assert.AreEqual(0, restoredGame.CurrentPosition.Row);
        Assert.AreEqual(5, restoredGame.CurrentPosition.Column);
        Assert.AreEqual(4, restoredGame.MoveHistory.Count);
        Assert.IsTrue(restoredGame.IsComplete);
    }

    [TestMethod]
    public void Restore_InvalidMoveSequence_ThrowsInvalidDataException()
    {
        // Arrange
        string history =
            """
            [
                {
                    "SequenceNumber": 2,
                    "FromRow": 5,
                    "FromColumn": 0,
                    "ToRow": 5,
                    "ToColumn": 3,
                    "MovementPiece": "Rook"
                }
            ]
            """;

        SavedGame saved = CreateSavedGame(
            history, 5, 3, false);

        // Act and Assert
        Assert.ThrowsException<InvalidDataException>(
            () => _service.Restore(saved));
    }

    [TestMethod]
    public void Restore_PositionDoesNotMatchHistory_ThrowsInvalidDataException()
    {
        // Arrange
        string history =
            """
            [
                {
                    "SequenceNumber": 1,
                    "FromRow": 5,
                    "FromColumn": 0,
                    "ToRow": 5,
                    "ToColumn": 3,
                    "MovementPiece": "Rook"
                }
            ]
            """;

        SavedGame saved = CreateSavedGame(
            history, 2, 2, false);

        // Act and Assert
        Assert.ThrowsException<InvalidDataException>(
            () => _service.Restore(saved));
    }

    [TestMethod]
    public void Restore_UnsupportedBoard_ThrowsInvalidDataException()
    {
        // Arrange
        SavedGame saved = CreateSavedGame(
            "[]", 5, 0, false, "UnknownBoard");

        // Act and Assert
        Assert.ThrowsException<InvalidDataException>(
            () => _service.Restore(saved));
    }

    [TestMethod]
    public void Restore_CorruptedJson_ThrowsInvalidDataException()
    {
        // Arrange
        SavedGame saved = CreateSavedGame(
            "{ invalid json", 5, 0, false);

        // Act and Assert
        Assert.ThrowsException<InvalidDataException>(
            () => _service.Restore(saved));
    }
}

using BCIT615.Assessment1.GamePlayer;
using GamePlayerClass = BCIT615.Assessment1.GamePlayer.Model.GamePlayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCIT615.Assessment1.GamePlayerTests;

[TestClass]
public sealed class GamePlayerTests
{
    [TestMethod]
    public void NewGame_HasCorrectInitialState()
    {
        var game = new GamePlayerClass();

        Assert.AreEqual(new Position(5, 0), game.StartPosition);
        Assert.AreEqual(new Position(0, 5), game.TargetPosition);
        Assert.AreEqual(new Position(5, 0), game.CurrentPosition);
        Assert.IsFalse(game.IsComplete);
        Assert.AreEqual(0, game.MoveHistory.Count);
    }

    [TestMethod]
    public void TryMove_RookCanMoveToBishopPosition()
    {
        var game = new GamePlayerClass();

        var result = game.TryMove(new Position(5, 3));

        Assert.AreEqual(MoveResult.Success, result);
        Assert.AreEqual(new Position(5, 3), game.CurrentPosition);
        Assert.AreEqual(PieceType.Bishop,
            game.GetPieceAt(new Position(5, 3)));
    }

    [TestMethod]
    public void TryMove_ToEmptyNonTargetPosition_ReturnsInvalidDestination()
    {
        var game = new GamePlayerClass();

        var result = game.TryMove(new Position(5, 2));

        Assert.AreEqual(MoveResult.InvalidDestination, result);
        Assert.AreEqual(new Position(5, 0), game.CurrentPosition);
        Assert.AreEqual(0, game.MoveHistory.Count);
    }

    [TestMethod]
    public void TryMove_RookCannotMoveDiagonally()
    {
        var game = new GamePlayerClass();

        var result = game.TryMove(new Position(4, 1));

        Assert.AreEqual(MoveResult.InvalidMovement, result);
        Assert.AreEqual(new Position(5, 0), game.CurrentPosition);
    }

    [TestMethod]
    public void TryMove_RookCannotMoveThroughBlockingPiece()
    {
        var game = new GamePlayerClass();

        var result = game.TryMove(new Position(5, 5));

        Assert.AreEqual(MoveResult.PathBlocked, result);
        Assert.AreEqual(new Position(5, 0), game.CurrentPosition);
    }

    [TestMethod]
    public void TryMove_OutOfBounds_ReturnsOutOfBounds()
    {
        var game = new GamePlayerClass();

        var result = game.TryMove(new Position(6, 0));

        Assert.AreEqual(MoveResult.OutOfBounds, result);
        Assert.AreEqual(new Position(5, 0), game.CurrentPosition);
    }

    [TestMethod]
    public void TryMove_ValidMove_IsAddedToMoveHistory()
    {
        var game = new GamePlayerClass();

        var result = game.TryMove(new Position(5, 3));

        Assert.AreEqual(MoveResult.Success, result);
        Assert.AreEqual(1, game.MoveHistory.Count);

        var move = game.MoveHistory[0];

        Assert.AreEqual(1, move.SequenceNumber);
        Assert.AreEqual(new Position(5, 0), move.From);
        Assert.AreEqual(new Position(5, 3), move.To);
        Assert.AreEqual(PieceType.Rook, move.MovementPiece);
    }

    [TestMethod]
    public void TryMove_CompleteReferenceRoute_CompletesGame()
    {
        var game = new GamePlayerClass();

        Assert.AreEqual(MoveResult.Success,
            game.TryMove(new Position(5, 3)));

        Assert.AreEqual(MoveResult.Success,
            game.TryMove(new Position(3, 5)));

        Assert.AreEqual(MoveResult.Success,
            game.TryMove(new Position(1, 4)));

        var result = game.TryMove(new Position(0, 5));

        Assert.AreEqual(MoveResult.GameCompleted, result);
        Assert.AreEqual(new Position(0, 5), game.CurrentPosition);
        Assert.IsTrue(game.IsComplete);
        Assert.AreEqual(4, game.MoveHistory.Count);
    }

    [TestMethod]
    public void TryMove_AfterCompletion_IsRejected()
    {
        var game = new GamePlayerClass();

        game.TryMove(new Position(5, 3));
        game.TryMove(new Position(3, 5));
        game.TryMove(new Position(1, 4));
        game.TryMove(new Position(0, 5));

        var result = game.TryMove(new Position(1, 5));

        Assert.AreEqual(MoveResult.GameAlreadyCompleted, result);
        Assert.AreEqual(new Position(0, 5), game.CurrentPosition);
        Assert.AreEqual(4, game.MoveHistory.Count);
    }

    [TestMethod]
    public void Restart_RestoresInitialStateAndClearsHistory()
    {
        var game = new GamePlayerClass();

        game.TryMove(new Position(5, 3));
        game.TryMove(new Position(3, 5));

        game.Restart();

        Assert.AreEqual(new Position(5, 0), game.CurrentPosition);
        Assert.IsFalse(game.IsComplete);
        Assert.AreEqual(0, game.MoveHistory.Count);
        Assert.AreEqual(PieceType.Rook,
            game.GetPieceAt(new Position(5, 0)));
        Assert.AreEqual(PieceType.Bishop,
            game.GetPieceAt(new Position(5, 3)));
    }
}

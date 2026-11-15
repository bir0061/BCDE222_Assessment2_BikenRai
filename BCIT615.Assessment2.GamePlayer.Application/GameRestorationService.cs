using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BCIT615.Assessment1.GamePlayer;
using BCIT615.Assessment1.GamePlayer.Model;
using BCIT615.Assessment2.GamePlayer.Persistence;

using GamePlayerModel =
    BCIT615.Assessment1.GamePlayer.Model.GamePlayer;

namespace BCIT615.Assessment2.GamePlayer.Application;

public class GameRestorationService
{
    public GamePlayerModel Restore(SavedGame saved)
    {
        ArgumentNullException.ThrowIfNull(saved);

        if (saved.BoardId != "ReferenceBoard")
        {
            throw new InvalidDataException(
                "This save uses an unsupported board.");
        }

        List<SavedMove>? moves;

        try
        {
            moves = JsonSerializer.Deserialize<List<SavedMove>>(
                saved.MoveHistoryJson);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(
                "The saved move history is invalid.", ex);
        }

        if (moves == null)
        {
            throw new InvalidDataException(
                "The saved move history is invalid.");
        }

        GamePlayerModel restoredGame = new GamePlayerModel();

        foreach (SavedMove move in moves)
        {
            if (move.SequenceNumber !=
                restoredGame.MoveHistory.Count + 1)
            {
                throw new InvalidDataException(
                    "The saved move sequence is invalid.");
            }

            Position from = new Position(
                move.FromRow, move.FromColumn);

            Position to = new Position(
                move.ToRow, move.ToColumn);

            if (!from.Equals(restoredGame.CurrentPosition))
            {
                throw new InvalidDataException(
                    "The saved move starting position is invalid.");
            }

            PieceType? piece = restoredGame.GetPieceAt(from);

            if (piece == null ||
                piece.Value.ToString() != move.MovementPiece)
            {
                throw new InvalidDataException(
                    "The saved movement piece is invalid.");
            }

            MoveResult result = restoredGame.TryMove(to);

            if (result != MoveResult.Success &&
                result != MoveResult.GameCompleted)
            {
                throw new InvalidDataException(
                    $"A saved move could not be restored: {result}.");
            }
        }

        if (restoredGame.CurrentPosition.Row != saved.CurrentRow ||
            restoredGame.CurrentPosition.Column != saved.CurrentColumn ||
            restoredGame.IsComplete != saved.IsComplete)
        {
            throw new InvalidDataException(
                "The saved game state does not match its move history.");
        }

        return restoredGame;
    }
}

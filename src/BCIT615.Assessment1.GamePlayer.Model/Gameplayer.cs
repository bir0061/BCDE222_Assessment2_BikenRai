using System.Collections.Generic;
using BCIT615.Assessment1.GamePlayer;

namespace BCIT615.Assessment1.GamePlayer.Model;

/// <summary>
/// Implements the Game Player Model for Assessment 1.
/// Maintains the current position, target position,
/// board state and history of successful moves.
/// </summary>
public class GamePlayer : IGamePlayer
{
    private readonly Dictionary<Position, PieceType> _pieces;
    private readonly List<MoveRecord> _moveHistory;

    public Position StartPosition { get; }

    public Position TargetPosition { get; }

    public Position CurrentPosition { get; private set; }

    public bool IsComplete { get; private set; }

    public IReadOnlyList<MoveRecord> MoveHistory =>
        _moveHistory.AsReadOnly();

    /// <summary>
    /// Creates a new Game Player using the supplied reference board.
    /// </summary>
    public GamePlayer()
    {
        StartPosition = ReferenceBoardData.Start;
        TargetPosition = ReferenceBoardData.Target;

        _pieces = new Dictionary<Position, PieceType>(
            ReferenceBoardData.Pieces);

        _moveHistory = new List<MoveRecord>();

        CurrentPosition = StartPosition;
        IsComplete = false;
    }

    /// <summary>
    /// Returns the piece located at the specified board position.
    /// Returns null when the position is empty.
    /// </summary>
    public PieceType? GetPieceAt(Position position)
    {
        if (!IsOnBoard(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (_pieces.TryGetValue(position, out PieceType piece))
        {
            return piece;
        }

        return null;
    }

    /// <summary>
    /// Attempts to move the current piece to the destination.
    /// </summary>
    public MoveResult TryMove(Position destination)
    {
        // The game cannot be played after completion.
        if (IsComplete)
        {
            return MoveResult.GameAlreadyCompleted;
        }

        // Check board boundaries.
        if (!IsOnBoard(destination))
        {
            return MoveResult.OutOfBounds;
        }

        // Cannot move to the same position.
        if (destination.Equals(CurrentPosition))
        {
            return MoveResult.InvalidMovement;
        }

        // Get the piece at the player's current position.
        if (!_pieces.TryGetValue(CurrentPosition, out PieceType piece))
        {
            return MoveResult.InvalidMovement;
        }

        // Validate the movement shape.
        if (!IsValidMove(piece, CurrentPosition, destination))
        {
            return MoveResult.InvalidMovement;
        }

        // Check whether another piece blocks the path.
        if (IsPathBlocked(piece, CurrentPosition, destination))
        {
            return MoveResult.PathBlocked;
        }

        // A valid destination must contain a fixed piece
        // or be the target position.
        if (!_pieces.ContainsKey(destination) &&
            !destination.Equals(TargetPosition))
        {
            return MoveResult.InvalidDestination;
        }

        // Store the previous position.
        Position previousPosition = CurrentPosition;

        // Update only the player's position.
        // Do not move or remove the fixed board pieces.
        CurrentPosition = destination;

        // Record the successful move.
        _moveHistory.Add(
            new MoveRecord(
                _moveHistory.Count + 1,
                previousPosition,
                destination,
                piece));

        // Check whether the target has been reached.
        if (CurrentPosition.Equals(TargetPosition))
        {
            IsComplete = true;
            return MoveResult.GameCompleted;
        }

        return MoveResult.Success;
    }

    /// <summary>
    /// Restarts the game using the original reference board.
    /// </summary>
    public void Restart()
    {
        _pieces.Clear();

        foreach (var piece in ReferenceBoardData.Pieces)
        {
            _pieces[piece.Key] = piece.Value;
        }

        _moveHistory.Clear();

        CurrentPosition = StartPosition;
        IsComplete = false;
    }

    /// <summary>
    /// Checks whether a position is inside the 6 x 6 board.
    /// </summary>
    private static bool IsOnBoard(Position position)
    {
        return position.Row >= 0 &&
               position.Row < ReferenceBoardData.Rows &&
               position.Column >= 0 &&
               position.Column < ReferenceBoardData.Columns;
    }

    /// <summary>
    /// Validates movement according to the piece type.
    /// </summary>
    private static bool IsValidMove(
        PieceType piece,
        Position from,
        Position to)
    {
        int rowDifference = System.Math.Abs(to.Row - from.Row);
        int columnDifference = System.Math.Abs(to.Column - from.Column);

        return piece switch
        {
            // Disallow zero-length moves for Rook
            PieceType.Rook =>
                (rowDifference == 0 || columnDifference == 0) &&
                (rowDifference + columnDifference > 0),

            // Disallow zero-length moves for Bishop
            PieceType.Bishop =>
                rowDifference == columnDifference &&
                (rowDifference + columnDifference > 0),

            PieceType.Knight =>
                (rowDifference == 2 && columnDifference == 1) ||
                (rowDifference == 1 && columnDifference == 2),

            PieceType.King =>
                rowDifference <= 1 &&
                columnDifference <= 1 &&
                (rowDifference + columnDifference > 0),

            _ => false
        };
    }

    /// <summary>
    /// Checks whether a Rook or Bishop has another piece
    /// blocking the path to the destination.
    /// </summary>
    private bool IsPathBlocked(
        PieceType piece,
        Position from,
        Position to)
    {
        // Knights and Kings do not have intermediate squares.
        if (piece == PieceType.Knight ||
            piece == PieceType.King)
        {
            return false;
        }

        int rowDirection = System.Math.Sign(to.Row - from.Row);
        int columnDirection = System.Math.Sign(to.Column - from.Column);

        int currentRow = from.Row + rowDirection;
        int currentColumn = from.Column + columnDirection;

        // Check each square between the starting position and the destination.
        while (currentRow != to.Row ||
               currentColumn != to.Column)
        {
            Position position =
                new Position(currentRow, currentColumn);

            if (_pieces.ContainsKey(position))
            {
                return true;
            }

            currentRow += rowDirection;
            currentColumn += columnDirection;
        }

        return false;
    }
}


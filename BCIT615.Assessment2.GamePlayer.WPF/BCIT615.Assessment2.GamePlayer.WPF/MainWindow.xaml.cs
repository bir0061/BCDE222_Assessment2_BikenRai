using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using BCIT615.Assessment1.GamePlayer;
using BCIT615.Assessment1.GamePlayer.Model;
using BCIT615.Assessment2.GamePlayer.Persistence;

using GamePlayerModel =
    BCIT615.Assessment1.GamePlayer.Model.GamePlayer;

namespace BCIT615.Assessment2.GamePlayer.WPF;

public partial class MainWindow : Window
{
    private GamePlayerModel _game;
    private readonly GamePlayerDbContext _dbContext;
    private readonly GameSaveRepository _repository;

    // Tracks which saved record belongs to the current game.
    private int? _currentSavedGameId = null;

    public MainWindow()
    {
        InitializeComponent();

        _game = new GamePlayerModel();

        // Store the database in the user's local application data folder.
        string databaseFolder = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "ChessMazeGamePlayer");

        Directory.CreateDirectory(databaseFolder);

        string databasePath = Path.Combine(
            databaseFolder, "gameplayer.db");

        var options = new DbContextOptionsBuilder<GamePlayerDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        _dbContext = new GamePlayerDbContext(options);
        _repository = new GameSaveRepository(_dbContext);

        // Apply any pending database migrations.
        _dbContext.Database.Migrate();

        TargetPositionText.Text = FormatPosition(_game.TargetPosition);

        CreateBoard();
        UpdateInterface();

        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;
    }

    private async void MainWindow_Loaded(
        object sender, RoutedEventArgs e)
    {
        await RefreshSavedGamesAsync();
    }

    private void MainWindow_Closed(
        object? sender, EventArgs e)
    {
        _dbContext.Dispose();
    }

    // Create all 36 board squares.
    private void CreateBoard()
    {
        BoardGrid.Children.Clear();

        for (int row = 0; row < ReferenceBoardData.Rows; row++)
        {
            for (int column = 0;
                 column < ReferenceBoardData.Columns;
                 column++)
            {
                Border square = new Border
                {
                    Background = (row + column) % 2 == 0
                        ? Brushes.White
                        : new SolidColorBrush(
                            Color.FromRgb(220, 228, 239)),
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1)
                };

                BoardGrid.Children.Add(square);
            }
        }
    }

    // Refresh the board and game information.
    private void UpdateInterface()
    {
        CurrentPositionText.Text =
            FormatPosition(_game.CurrentPosition);

        if (_game.IsComplete)
        {
            StatusText.Text =
                "Congratulations! You completed the game.";

            StatusText.Foreground = Brushes.Green;
        }

        UpdateBoard();
        UpdateMoveHistory();
    }

    // Display fixed pieces, target and current player position.
    private void UpdateBoard()
    {
        for (int row = 0; row < ReferenceBoardData.Rows; row++)
        {
            for (int column = 0;
                 column < ReferenceBoardData.Columns;
                 column++)
            {
                Position position = new Position(row, column);

                int index =
                    row * ReferenceBoardData.Columns + column;

                Border square = (Border)BoardGrid.Children[index];

                square.Background = (row + column) % 2 == 0
                    ? Brushes.White
                    : new SolidColorBrush(
                        Color.FromRgb(220, 228, 239));

                StackPanel contents = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                PieceType? piece = _game.GetPieceAt(position);

                if (piece.HasValue)
                {
                    TextBlock pieceText = new TextBlock
                    {
                        Text = GetPieceSymbol(piece.Value),
                        FontSize = 35,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    contents.Children.Add(pieceText);

                    TextBlock nameText = new TextBlock
                    {
                        Text = piece.Value.ToString(),
                        FontSize = 11,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    contents.Children.Add(nameText);
                }

                if (position.Equals(_game.TargetPosition))
                {
                    TextBlock targetText = new TextBlock
                    {
                        Text = "GOAL",
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.DarkGreen,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    contents.Children.Add(targetText);
                }

                if (position.Equals(_game.CurrentPosition))
                {
                    square.Background =
                        new SolidColorBrush(
                            Color.FromRgb(147, 197, 253));

                    TextBlock playerText = new TextBlock
                    {
                        Text = "YOU",
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.DarkBlue,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    contents.Children.Add(playerText);
                }

                square.Child = contents;
            }
        }
    }

    // Return a chess symbol for each fixed piece.
    private static string GetPieceSymbol(PieceType piece)
    {
        return piece switch
        {
            PieceType.Rook => "♜",
            PieceType.Bishop => "♝",
            PieceType.Knight => "♞",
            PieceType.King => "♚",
            _ => "?"
        };
    }

    // Handle the Move button.
    private void MoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(RowInput.Text, out int row) ||
            !int.TryParse(ColumnInput.Text, out int column))
        {
            StatusText.Text =
                "Please enter valid row and column numbers.";

            StatusText.Foreground = Brushes.Red;
            return;
        }

        Position destination = new Position(row, column);
        MoveResult result = _game.TryMove(destination);

        UpdateInterface();

        switch (result)
        {
            case MoveResult.Success:
                StatusText.Text = "Move successful!";
                StatusText.Foreground = Brushes.Green;
                break;

            case MoveResult.InvalidDestination:
                StatusText.Text =
                    "Invalid Destination: You can only move to an occupied square or the target.";
                StatusText.Foreground = Brushes.Red;
                break;

            case MoveResult.InvalidMovement:
                StatusText.Text =
                    "Invalid Movement: The movement is not allowed.";
                StatusText.Foreground = Brushes.Red;
                break;

            case MoveResult.PathBlocked:
                StatusText.Text =
                    "Path Blocked: Another piece is blocking the route.";
                StatusText.Foreground = Brushes.Red;
                break;

            case MoveResult.OutOfBounds:
                StatusText.Text =
                    "Out of Bounds: Choose a position between 0 and 5.";
                StatusText.Foreground = Brushes.Red;
                break;

            case MoveResult.GameCompleted:
                StatusText.Text =
                    "Congratulations! You completed the game.";
                StatusText.Foreground = Brushes.Green;
                break;

            case MoveResult.GameAlreadyCompleted:
                StatusText.Text =
                    "The game is already completed. Restart to play again.";
                StatusText.Foreground = Brushes.Red;
                break;
        }

        if (result == MoveResult.Success ||
            result == MoveResult.GameCompleted)
        {
            RowInput.Clear();
            ColumnInput.Clear();
        }
    }

    // Handle the Restart button.
    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        _game.Restart();

        _currentSavedGameId = null;

        //Clear the row and column input fields.
        RowInput.Clear();
        ColumnInput.Clear();

        // Clear the saved game name.
        SaveNameInput.Clear();

        // Clear the selected saved game.
        SavedGamesComboBox.SelectedItem = null;

        // Refresh the board and game information.
        UpdateInterface();

        StatusText.Text = "Game restarted. Ready to play.";
        StatusText.Foreground = Brushes.Green;
    }

    // Display successful moves.
    private void UpdateMoveHistory()
    {
        MoveHistoryList.Items.Clear();

        foreach (MoveRecord move in _game.MoveHistory)
        {
            MoveHistoryList.Items.Add(
                $"{move.SequenceNumber}. " +
                $"{move.MovementPiece}: " +
                $"{FormatPosition(move.From)} -> " +
                $"{FormatPosition(move.To)}");
        }
    }

    // Save the current game.
    private async void SaveGameButton_Click(
    object sender, RoutedEventArgs e)
    {
        string name = SaveNameInput.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            StatusText.Text = "Please enter a name for your saved game.";
            StatusText.Foreground = Brushes.Red;
            return;
        }

        try
        {
            List<SavedMove> moves = _game.MoveHistory
                .Select(move => new SavedMove
                {
                    SequenceNumber = move.SequenceNumber,
                    FromRow = move.From.Row,
                    FromColumn = move.From.Column,
                    ToRow = move.To.Row,
                    ToColumn = move.To.Column,
                    MovementPiece = move.MovementPiece.ToString()
                })
                .ToList();

            SavedGame saved = new SavedGame
            {
                Name = name,
                BoardId = "ReferenceBoard",
                CurrentRow = _game.CurrentPosition.Row,
                CurrentColumn = _game.CurrentPosition.Column,
                IsComplete = _game.IsComplete,
                MoveHistoryJson = JsonSerializer.Serialize(moves)
            };

            // Create a new record or update the current saved record.
            int savedId = await _repository.SaveAsync(
                saved, _currentSavedGameId);

            // Remember the record ID for future overwrites.
            _currentSavedGameId = savedId;

            StatusText.Text = $"Game '{name}' saved successfully.";
            StatusText.Foreground = Brushes.Green;

            await RefreshSavedGamesAsync();
        }
        catch (InvalidOperationException ex)
        {
            StatusText.Text = ex.Message;
            StatusText.Foreground = Brushes.Red;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Unable to save game: {ex.Message}";
            StatusText.Foreground = Brushes.Red;
        }
    }

    // Refresh the saved-games list.
    private async void RefreshSavedGamesButton_Click(
        object sender, RoutedEventArgs e)
    {
        await RefreshSavedGamesAsync();
    }

    private async System.Threading.Tasks.Task RefreshSavedGamesAsync()
    {
        try
        {
            var games = await _repository.GetAllAsync();

            SavedGamesComboBox.ItemsSource = games;
            SavedGamesComboBox.DisplayMemberPath = "Name";
        }
        catch (Exception ex)
        {
            StatusText.Text =
                $"Unable to retrieve saved games: {ex.Message}";

            StatusText.Foreground = Brushes.Red;
        }
    }

    // Load and validate a selected saved game.
    private async void LoadGameButton_Click(
        object sender, RoutedEventArgs e)
    {
        if (SavedGamesComboBox.SelectedItem is not SavedGame selected)
        {
            StatusText.Text = "Please select a saved game first.";
            StatusText.Foreground = Brushes.Red;
            return;
        }

        try
        {
            SavedGame? saved =
                await _repository.GetByIdAsync(selected.Id);

            if (saved == null)
            {
                StatusText.Text = "The selected saved game no longer exists.";
                StatusText.Foreground = Brushes.Red;
                return;
            }

            if (saved.BoardId != "ReferenceBoard")
            {
                StatusText.Text = "This save uses an unsupported board.";
                StatusText.Foreground = Brushes.Red;
                return;
            }

            List<SavedMove>? moves =
                JsonSerializer.Deserialize<List<SavedMove>>(
                    saved.MoveHistoryJson);

            if (moves == null)
            {
                throw new InvalidDataException(
                    "The saved move history is invalid.");
            }

            // Restore into a temporary game first.
            // The current game remains unchanged unless validation succeeds.
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

            // Verify the restored state matches the database record.
            if (restoredGame.CurrentPosition.Row != saved.CurrentRow ||
                restoredGame.CurrentPosition.Column != saved.CurrentColumn ||
                restoredGame.IsComplete != saved.IsComplete)
            {
                throw new InvalidDataException(
                    "The saved game state does not match its move history.");
            }

            // Replace the current game only after all checks succeed.
            _game = restoredGame;

            _currentSavedGameId = saved.Id;

            // Restore the selected saved game name in the input field.
            SaveNameInput.Text = saved.Name;

            // Update the UI to reflect the loaded game.
            RowInput.Clear();
            ColumnInput.Clear();

            UpdateInterface();

            StatusText.Text = $"Game '{saved.Name}' loaded successfully.";
            StatusText.Foreground = Brushes.Green;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Unable to load game: {ex.Message}";
            StatusText.Foreground = Brushes.Red;
        }
    }

    private async void DeleteGameButton_Click(
    object sender, RoutedEventArgs e)
    {
        if (SavedGamesComboBox.SelectedItem is not SavedGame selected)
        {
            StatusText.Text = "Please select a saved game to delete.";
            StatusText.Foreground = Brushes.Red;
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            $"Are you sure you want to delete '{selected.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            bool deleted = await _repository.DeleteAsync(selected.Id);

            if (deleted)
            {
                StatusText.Text = $"Game '{selected.Name}' deleted successfully.";
                StatusText.Foreground = Brushes.Green;

                await RefreshSavedGamesAsync();
            }
            else
            {
                StatusText.Text = "The selected saved game could not be found.";
                StatusText.Foreground = Brushes.Red;
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Unable to delete game: {ex.Message}";
            StatusText.Foreground = Brushes.Red;
        }
    }

    private static string FormatPosition(Position position)
    {
        return $"({position.Row}, {position.Column})";
    }
}

// Data stored in the saved move-history JSON.
public class SavedMove
{
    public int SequenceNumber { get; set; }
    public int FromRow { get; set; }
    public int FromColumn { get; set; }
    public int ToRow { get; set; }
    public int ToColumn { get; set; }
    public string MovementPiece { get; set; } = string.Empty;
}

# BCIT615 Assessment 2 – GamePlayer

## Overview

This project is a GamePlayer application developed using C#, .NET 9 and WPF for BCIT615 Assessment 2.

The application uses a 6 × 6 game board containing four chess movement pieces: Rook, Bishop, Knight and King.

The player starts at position `(5, 0)` and must reach the target position `(0, 5)` by following the required movement route.

## Technologies Used

- C#
- .NET 9
- WPF and XAML
- Entity Framework Core
- SQLite
- MSTest
- Visual Studio 2026
- Git and GitHub

## Main Features

The application provides the following functionality:

- Displays the 6 × 6 game board.
- Displays the player, target and four movement pieces.
- Validates player movement according to the game rules.
- Prevents invalid movements and provides feedback.
- Maintains an ordered move history.
- Detects successful game completion.
- Allows the player to restart the game.
- Saves named game records to an SQLite database.
- Lists, loads and deletes saved games.
- Restores the player's position, move history and completion state.
- Validates saved game data before restoring it.

## Reference Board

| Item | Position |
|---|---|
| Starting position | (5, 0) |
| Rook | (5, 0) |
| Bishop | (5, 3) |
| Knight | (3, 5) |
| King | (1, 4) |
| Target | (0, 5) |

Required movement route:

`(5,0) → (5,3) → (3,5) → (1,4) → (0,5)`

The game is completed when the player successfully reaches the target.

## Database and Persistence

The application uses Entity Framework Core with SQLite to store saved games.

Each saved game includes:

- Saved game name and identifier.
- Board identity.
- Current player position.
- Ordered move history.
- Game completion status.

The database is automatically migrated when the WPF application starts.

The database is stored locally at:

`%LOCALAPPDATA%\ChessMazeGamePlayer\gameplayer.db`

## Project Structure

The solution contains the following main projects:

- `BCIT615.Assessment1.GamePlayer` – Shared game contracts.
- `BCIT615.Assessment1.GamePlayer.Model` – Game rules and game state.
- `BCIT615.Assessment2.GamePlayer.Application` – Application services and game restoration.
- `BCIT615.Assessment2.GamePlayer.Persistence` – Database entities, context and repository.
- `BCIT615.Assessment2.GamePlayer.WPF` – WPF user interface.

The solution also contains separate automated test projects for contracts, game model and persistence/application functionality.

## Requirements

Before running the project, ensure the following are installed:

- Visual Studio 2026.
- .NET 9 SDK.
- Windows operating system.
- Required NuGet packages restored through the solution.

The repository includes a `global.json` file specifying the .NET SDK version.

## How to Restore, Build and Test

Open PowerShell in the root project folder, where the solution file is located.

### Restore dependencies

```powershell
dotnet restore
```

### Build the solution

```powershell
dotnet build
```

### Run automated tests

```powershell
dotnet test
```

### Run the WPF application

```powershell
dotnet run --project ".\BCIT615.Assessment2.GamePlayer.WPF\BCIT615.Assessment2.GamePlayer.WPF\BCIT615.Assessment2.GamePlayer.WPF.csproj"
```

## Git Repository

Git is used for source control and tracking project development.

The project repository is hosted on GitHub:

https://github.com/bir0061/BCDE222_Assessment2_BikenRai

## Author

Biken Rai

BCDE222 / BCIT615 – Assessment 2
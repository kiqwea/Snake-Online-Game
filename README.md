# Multiplayer Snake Game

A classic Snake game with multiplayer functionality, built using C# and TCP networking.

## Features

- Real-time multiplayer gameplay
- Client-server architecture
- Score tracking and leaderboard
- Database integration for persistent scores
- Console-based UI
- Configurable game settings

## Technical Details

### Server
- TCP-based server implementation
- Handles multiple client connections
- Manages game state synchronization
- Implements lobby system for player matching

### Client
- TCP client implementation
- Real-time game state updates
- Local game logic
- Console-based rendering

### Database
- MySQL integration
- Persistent score storage
- Leaderboard functionality
- Player statistics tracking

## Requirements

- .NET 6.0 - 7
- MySQL Server
- Visual Studio 2022

## Installation

1. Clone the repository
2. Set up MySQL database:
   - Create database named "SnakeGame"
   - Import database schema
3. Update database connection string in `DataBase.cs`
4. Build and run the solution

## How to Play

1. Start the server application
2. Launch two client instances
3. Enter player nicknames
4. Use WASD keys to control the snake
5. Collect food to grow and increase score
6. Avoid collisions with walls and other snakes

## Controls

- W: Move Up
- A: Move Left
- S: Move Down
- D: Move Right
- X: Switch to online mode
- Q: View leaderboard


**2025 created**

# Overview

The game is called tiny chess, it plays almost identically to chess, except much smaller. Rooks and bishops can only move one square, horizontally or diagonally respectively, and knights cannot jump over pieces. Starting with white, click and a piece and a desired square and the piece will move there. White and black alternate turns.
{Describe your purpose for writing this software.}

# Development Environment

I used Godot 4, an open source game engine to make it, and I wrote it all in C# with the Godot libraries to handle inputs and object management.

# Future Work

- Make pawn promotion allow promotion of any piece, not just a rook.
- Turn it into tinyhouse chess, not just tiny chess, which is where you can spend a turn to place down a piece you captured, e.g. if you take your opponents rook you can spend a turn to place a rook of your own anywhere on the board.
- Add a game over screen that announces who won or if it was a draw.

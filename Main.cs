using Godot;
using System;

public partial class Main : Node2D
{
	public override void _Ready()
	{
		Board board = GetNode<Board>("Board");
		
		Vector2 viewportSize = GetViewportRect().Size;
		
		float boardSize = MathF.Min(viewportSize.X, viewportSize.Y) * 0.8f;
		
		Vector2 boardPosition = (viewportSize - new Vector2(boardSize, boardSize)) / 2;
		
		board.BoardSize = boardSize;
		board.BoardPosition = boardPosition;
		board.TileSize = boardSize / 4;
		
		board.QueueRedraw();
		board.DrawPieces();
	}

	public override void _Process(double delta)
	{
	}
}

using Godot;
using System;

public enum PieceColor
{
	White,
	Black
}

public enum Type
{
	King,
	Rook,
	Bishop,
	Knight,
	Pawn
}

public partial class Piece : Sprite2D
{
	public Vector2I Cell;
	
	public PieceColor Color;
	public Type Type;
}

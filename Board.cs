using Godot;
using System;

public partial class Board : Node2D
{
	private Node2D _pieces;
	
	public float BoardSize { get; set; } = 0;
	
	public Vector2 BoardPosition { get; set; } = new Vector2(0, 0);
	
	public float TileSize;
	
	private Piece _selectedPiece = null;
	
	private PieceColor _currentTurn = PieceColor.White;
	
	private AudioStreamPlayer2D _moveSound;
	
	public void DrawPieces()
	{
		_pieces = GetNode<Node2D>("Pieces");
		_moveSound = GetNode<AudioStreamPlayer2D>("MoveSound");
		_moveSound.Stream = GD.Load<AudioStream>("res://sounds/move.wav");
		
		string[] pieces = {
			"res://pieces/black-bishop.png",
			"res://pieces/black-knight.png",
			"res://pieces/black-rook.png",
			"res://pieces/black-king.png",
			"res://pieces/black-pawn.png",
			"res://pieces/white-pawn.png",
			"res://pieces/white-king.png",
			"res://pieces/white-rook.png",
			"res://pieces/white-knight.png",
			"res://pieces/white-bishop.png"
		};
		
		for (int i = 0; i < pieces.Length; i++)
		{
			var piece = new Piece();
			piece.Texture = GD.Load<Texture2D>(pieces[i]);
			piece.Scale = new Vector2(9, 9);
			
			if (pieces[i].Contains("white"))
				piece.Color = PieceColor.White;
			else
				piece.Color = PieceColor.Black;

			if (pieces[i].Contains("king"))
				piece.Type = Type.King;
			else if (pieces[i].Contains("rook"))
				piece.Type = Type.Rook;
			else if (pieces[i].Contains("bishop"))
				piece.Type = Type.Bishop;
			else if (pieces[i].Contains("knight"))
				piece.Type = Type.Knight;
			else
				piece.Type = Type.Pawn;
			
			int index = i;
			
			if (index > 3) { index += 3; }
			if (index > 8) { index += 3; }
			
			int col = index % 4;
			int row = index / 4;
			
			piece.Cell = new Vector2I(col, row);
			piece.Position = CellToWorld(piece.Cell);
			
			_pieces.AddChild(piece);
		}
	}
	
	public override void _Draw()
	{
		for (int row = 0; row < 4; row++)
		{
			for (int col = 0; col < 4; col++)
			{
				Vector2 tilePosition = BoardPosition + new Vector2(col * TileSize, row * TileSize);
				
				Color color = ((row + col) % 2 == 0)
					? new Color(0.519f, 0.539f, 0.59f, 1.0f) : new Color(0.147f, 0.165f, 0.21f, 1.0f);
				
				DrawRect(new Rect2(tilePosition, new Vector2(TileSize, TileSize)), color);
			}
		}
	}
	
	private Vector2 CellToWorld(Vector2I cell)
	{
		float x = BoardPosition.X + cell.X * TileSize + TileSize / 2;
		float y = BoardPosition.Y + cell.Y * TileSize + TileSize / 2;
		
		return new Vector2(x, y);
	}
	
	private Vector2I WorldToCell(Vector2 worldPos)
	{
		int col = (int)((worldPos.X - BoardPosition.X) / TileSize);
		int row = (int)((worldPos.Y - BoardPosition.Y) / TileSize);
		
		return new Vector2I(col, row);
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
		{
			HandleClick(mouse.Position);
		}
	}
	
	private void HandleClick(Vector2 mousePos)
	{
		foreach (Node child in _pieces.GetChildren())
		{
			if (child is Piece piece)
			{
				if (piece.GetRect().HasPoint(piece.ToLocal(mousePos)))
				{
					if (piece.Color == _currentTurn)
					{
						_selectedPiece = piece;
						return;
					}
				}
			}
		}
		
		if (_selectedPiece != null)
		{
			Vector2I targetCell = WorldToCell(mousePos);

			if (targetCell.X >= 0 && targetCell.X < 4 && targetCell.Y >= 0 && targetCell.Y < 4)
			{
				if (!IsValidMove(targetCell)) return;
				if (LeavesKingInCheck(targetCell)) return;

				Piece targetPiece = GetPieceAtCell(targetCell);
				if (targetPiece != null && targetPiece.Color != _currentTurn)
					targetPiece.QueueFree();

				_selectedPiece.Cell = targetCell;
				_selectedPiece.Position = CellToWorld(targetCell);

				if (_selectedPiece.Type == Type.Pawn)
				{
					if ((_selectedPiece.Color == PieceColor.White && targetCell.Y == 0) ||
						(_selectedPiece.Color == PieceColor.Black && targetCell.Y == 3))
					{
						_selectedPiece.Type = Type.Rook;

						string rookTexture = _selectedPiece.Color == PieceColor.White
							? "res://pieces/white-rook.png"
							: "res://pieces/black-rook.png";

						_selectedPiece.Texture = GD.Load<Texture2D>(rookTexture);
					}
				}
				
				_currentTurn = _currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
				_selectedPiece = null;
				_moveSound.Play();
			}
		}
	}
	
	private Piece GetPieceAtCell(Vector2I cell)
	{
		foreach (Node child in _pieces.GetChildren())
		{
			if (child is Piece piece)
			{
				if (piece.Cell == cell)
					return piece;
			}
		}
		return null;
	}
	
	private bool IsValidMove(Vector2I targetCell)
	{
		if (targetCell == _selectedPiece.Cell) return false;
		
		Vector2I delta = targetCell - _selectedPiece.Cell;
		
		switch (_selectedPiece.Type)
		{
			case Type.Pawn:
				return IsValidPawnMove(delta, targetCell);
				
			case Type.Rook:
				return (delta.X == 0 && Math.Abs(delta.Y) == 1) || (delta.Y == 0 && Math.Abs(delta.X) == 1);
				
			case Type.Bishop:
				return Math.Abs(delta.X) == 1 && Math.Abs(delta.Y) == 1;
				
			case Type.Knight:
				if ((Math.Abs(delta.X) == 2 && Math.Abs(delta.Y) == 1) ||
					(Math.Abs(delta.Y) == 2 && Math.Abs(delta.X) == 1))
				{
					return !IsKnightBlocked(targetCell, delta);
				}
				return false;
				
			case Type.King:
				return Math.Abs(delta.X) <= 1 && Math.Abs(delta.Y) <= 1;
			
			default:
				return false;
		}
	}
	
	// Knights can't jump over pieces in this chess variant
	private bool IsKnightBlocked(Vector2I targetCell, Vector2I delta)
	{
		Vector2I leg;
		
		if (Math.Abs(delta.X) == 2 && Math.Abs(delta.Y) == 1)
		{
			leg = new Vector2I(_selectedPiece.Cell.X + delta.X / 2, _selectedPiece.Cell.Y);
		}
		else
		{
			leg = new Vector2I(_selectedPiece.Cell.X, _selectedPiece.Cell.Y + delta.Y / 2);
		}
		
		return GetPieceAtCell(leg) != null;
	}
	
	private bool IsValidPawnMove(Vector2I delta, Vector2I targetCell)
	{
		int dir = _selectedPiece.Color == PieceColor.White ? -1 : 1;
		
		if (delta.X == 0 && delta.Y == dir)
		{
			return GetPieceAtCell(targetCell) == null;
		}
		if (Math.Abs(delta.X) == 1 && delta.Y == dir)
		{
			Piece targetPiece = GetPieceAtCell(targetCell);
			return targetPiece != null && targetPiece.Color != _selectedPiece.Color;
		}
		return false;
	}
	
	private Piece GetKing(PieceColor color)
	{
		foreach(Node child in _pieces.GetChildren())
		{
			if (child is Piece piece && piece.Type == Type.King && piece.Color == color)
			{
				return piece;
			}
		}
		return null;
	}
	
	private bool IsCellAttacked(Vector2I cell, PieceColor byColor)
	{
		foreach (Node child in _pieces.GetChildren())
		{
			if (child is Piece piece && piece.Color == byColor)
			{
				bool attack = false;
				Piece originalPiece = _selectedPiece;
				_selectedPiece = piece;
				if (IsValidMove(cell)) attack = true;
				_selectedPiece = originalPiece;
				if (attack) return true;
			}
		}
		return false;
	}
	
	private bool LeavesKingInCheck(Vector2I targetCell)
	{
		Vector2I originalCell = _selectedPiece.Cell;
		Piece captured = GetPieceAtCell(targetCell);
		
		_selectedPiece.Cell = targetCell;
		if (captured != null) captured.Visible = false;
		
		Piece king = GetKing(_selectedPiece.Color);
		
		bool inCheck = IsCellAttacked(king.Cell, _selectedPiece.Color == PieceColor.White ? PieceColor.Black : PieceColor.White);
		
		_selectedPiece.Cell = originalCell;
		if (captured != null) captured.Visible = true;
		
		return inCheck;
	}
}

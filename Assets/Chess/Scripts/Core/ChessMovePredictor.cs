using Chess.Scripts.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ChessMovePredictor : MonoBehaviour
{
    internal static ChessMovePredictor Instance;

    private void Awake()
    {
        Instance = this;
    }

    public enum ChessPieceType
    {
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }

    // Call this method to predict and highlight the moves for a piece at a given position
    public void PredictMoves(int row, int column, string pieceName)
    {
        if (!Enum.TryParse(pieceName, true, out ChessPieceType pieceType))
        {
            Debug.LogError("Invalid piece name: " + pieceName);
            return;
        }

        ChessBoardPlacementHandler.Instance.ClearHighlights();

        List<Vector2Int> possibleMoves = GetPossibleMoves(row, column, pieceType);

        foreach (var move in possibleMoves)
        {
            ChessBoardPlacementHandler.Instance.Highlight(move.x, move.y);
        }
    }

    // Determine the possible moves based on the piece type and position
    private List<Vector2Int> GetPossibleMoves(int row, int column, ChessPieceType pieceType)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        switch (pieceType)
        {
            case ChessPieceType.Pawn:
                // Pawn moves
                int forwardDir = 1; // Adjust this based on your board orientation
                int startRow = 1;   // Adjust this based on your board setup

                if (IsValidMove(row + forwardDir, column))
                    moves.Add(new Vector2Int(row + forwardDir, column));

                if (row == startRow && IsValidMove(row + 2 * forwardDir, column))
                    moves.Add(new Vector2Int(row + 2 * forwardDir, column));

                // Check diagonal attacks for pawn
                CheckDiagonalAttack(row + forwardDir, column - 1, moves);
                CheckDiagonalAttack(row + forwardDir, column + 1, moves);

                break;

            case ChessPieceType.Rook:
                // Rook moves
                AddStraightLineMoves(row, column, 1, 0, moves); // Up
                AddStraightLineMoves(row, column, -1, 0, moves); // Down
                AddStraightLineMoves(row, column, 0, 1, moves); // Right
                AddStraightLineMoves(row, column, 0, -1, moves); // Left
                break;

            case ChessPieceType.Knight:
                // Knight moves
                Vector2Int[] knightMoves = {
                    new Vector2Int(2, 1), new Vector2Int(1, 2), new Vector2Int(-1, 2), new Vector2Int(-2, 1),
                    new Vector2Int(-2, -1), new Vector2Int(-1, -2), new Vector2Int(1, -2), new Vector2Int(2, -1)
                };
                foreach (var move in knightMoves)
                {
                    int newRow = row + move.x;
                    int newCol = column + move.y;
                    if (IsValidMove(newRow, newCol) && !Blocked(newRow, newCol)) moves.Add(new Vector2Int(newRow, newCol));
                }
                break;

            case ChessPieceType.Bishop:
                // Bishop moves
                AddDiagonalMoves(row, column, 1, 1, moves); // Top-Right
                AddDiagonalMoves(row, column, -1, 1, moves); // Bottom-Right
                AddDiagonalMoves(row, column, 1, -1, moves); // Top-Left
                AddDiagonalMoves(row, column, -1, -1, moves); // Bottom-Left
                break;

            case ChessPieceType.Queen:
                // Queen moves (combination of Rook and Bishop moves)
                AddStraightLineMoves(row, column, 1, 0, moves); // Up
                AddStraightLineMoves(row, column, -1, 0, moves); // Down
                AddStraightLineMoves(row, column, 0, 1, moves); // Right
                AddStraightLineMoves(row, column, 0, -1, moves); // Left
                AddDiagonalMoves(row, column, 1, 1, moves); // Top-Right
                AddDiagonalMoves(row, column, -1, 1, moves); // Bottom-Right
                AddDiagonalMoves(row, column, 1, -1, moves); // Top-Left
                AddDiagonalMoves(row, column, -1, -1, moves); // Bottom-Left
                break;

            case ChessPieceType.King:
                // King moves
                Vector2Int[] kingMoves = {
                    new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1), new Vector2Int(-1, 1),
                    new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1)
                };
                foreach (var move in kingMoves)
                {
                    int newRow = row + move.x;
                    int newCol = column + move.y;
                    if (IsValidMove(newRow, newCol) && !Blocked(newRow, newCol)) moves.Add(new Vector2Int(newRow, newCol));
                }
                break;
        }

        return moves;
    }

    private bool Blocked(int row, int column)
    {
        Debug.Log(FindPieceAtPosition(row, column));
        return FindPieceAtPosition(row, column);
    }

    // Helper method to check if a move is within the bounds of the board
    private bool IsValidMove(int row, int column)
    {
        return row >= 0 && row < 8 && column >= 0 && column < 8;
    }

    // Helper method to add straight line moves (vertical and horizontal)
    private void AddStraightLineMoves(int row, int column, int deltaRow, int deltaCol, List<Vector2Int> moves)
    {
        for (int i = 1; i < 8; i++)
        {
            int newRow = row + i * deltaRow;
            int newCol = column + i * deltaCol;
            if (IsValidMove(newRow, newCol) && !Blocked(newRow, newCol))
            {
                moves.Add(new Vector2Int(newRow, newCol));
                if (!IsValidMove(newRow + deltaRow, newCol + deltaCol))
                    break; // Stop if path is blocked
            }
            else
            {
                break; // Stop if out of bounds
            }
        }
    }

    // Helper method to add diagonal moves
    private void AddDiagonalMoves(int row, int column, int deltaRow, int deltaCol, List<Vector2Int> moves)
    {
        for (int i = 1; i < 8; i++)
        {
            int newRow = row + i * deltaRow;
            int newCol = column + i * deltaCol;
            if (IsValidMove(newRow, newCol) && !Blocked(newRow, newCol))
            {
                moves.Add(new Vector2Int(newRow, newCol));
                if (!IsValidMove(newRow + deltaRow, newCol + deltaCol))
                    break; // Stop if path is blocked
            }
            else
            {
                break; // Stop if out of bounds
            }
        }
    }

    // Helper method to check and add diagonal attacks for pawn
    private void CheckDiagonalAttack(int row, int column, List<Vector2Int> moves)
    {
        if (IsValidMove(row, column))
        {
            // Add diagonal move only if there is an opponent's piece
            // For simplicity, you might need to add additional logic to check for opponent pieces
            moves.Add(new Vector2Int(row, column));
        }
    }

    private bool FindPieceAtPosition(int row, int column)
    {
        GameObject chessPieces = GameObject.Find("Player Positions");


        foreach (Transform child in chessPieces.transform)
        {
            GameObject piece = child.gameObject;
            ChessPlayerPlacementHandler playerPlacementHandler = piece.GetComponent<ChessPlayerPlacementHandler>();
            if (playerPlacementHandler.row == row && playerPlacementHandler.column == column)
            {
                return true;
            }
        }

        return false;
    }
}

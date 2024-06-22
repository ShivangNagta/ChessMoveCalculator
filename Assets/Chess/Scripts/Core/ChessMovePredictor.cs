using Chess.Scripts.Core;
using System;
using System.Collections.Generic;
using System.Data;
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
        Pawn1,
        Pawn2,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }

    // Call this method to predict and highlight the moves for a piece at a given position
    public void PredictMoves(int row, int column, string pieceName, string teamName)
    {
        if (!Enum.TryParse(pieceName, true, out ChessPieceType pieceType))
        {
            Debug.LogError("Invalid piece name: " + pieceName);
            return;
        }

        ChessBoardPlacementHandler.Instance.ClearHighlights();
        List<Vector2Int> possibleMoves = GetPossibleMoves(row, column, pieceType, teamName);
        foreach (var move in possibleMoves)
        {
            ChessBoardPlacementHandler.Instance.Highlight(move.x, move.y);
        }
    }

    // Determine the possible moves based on the piece type and position
    private List<Vector2Int> GetPossibleMoves(int row, int column, ChessPieceType pieceType, string teamName)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        switch (pieceType)
        {
            case ChessPieceType.Pawn1:
                // Pawn moves
                int forwardDir = 1; // Adjust this based on your board orientation
                int startRow = 1;   // Adjust this based on your board setup

                if (IsValidMove(row + forwardDir, column) && NotBlocked(row + forwardDir, column))
                    moves.Add(new Vector2Int(row + forwardDir, column));
                if (row == startRow && IsValidMove(row + 2 * forwardDir, column) && NotBlocked(row + 2* forwardDir, column) && NotBlocked(row + forwardDir, column))
                    moves.Add(new Vector2Int(row + 2 * forwardDir, column));
                // Check diagonal attacks for pawn
                CheckDiagonalAttack1(row, column , moves, teamName);

                break;

            case ChessPieceType.Pawn2:
                // Pawn moves
                int backwardDir = 1; // Adjust this based on your board orientation
                int startingRow = 6;   // Adjust this based on your board setup

                if (IsValidMove(row - backwardDir, column) && NotBlocked(row - backwardDir, column))
                    moves.Add(new Vector2Int(row - backwardDir, column));
                if (row == startingRow && IsValidMove(row - 2 * backwardDir, column) && NotBlocked(row - 2 * backwardDir, column) && NotBlocked(row - backwardDir, column))
                    moves.Add(new Vector2Int(row - 2 * backwardDir, column));

                // Check diagonal attacks for pawn
                CheckDiagonalAttack2(row, column, moves, teamName);

                break;

            case ChessPieceType.Rook:
                // Rook moves
                AddStraightLineMoves(row, column, 1, 0, moves, teamName); // Up
                AddStraightLineMoves(row, column, -1, 0, moves, teamName); // Down
                AddStraightLineMoves(row, column, 0, 1, moves, teamName); // Right
                AddStraightLineMoves(row, column, 0, -1, moves, teamName); // Left
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
                    if (IsValidMove(newRow, newCol))
                    {
                        if (NotBlocked(newRow, newCol))
                        {
                            moves.Add(new Vector2Int(newRow, newCol));
                        }
                        else if (TeamAtGivenPosition(newRow, newCol) != teamName)
                        {
                            ChessBoardPlacementHandler.Instance.EnemyHighlight(newRow, newCol);
                        }
                    }
                }
                break;


            case ChessPieceType.Bishop:
                // Bishop moves
                AddDiagonalMoves(row, column, 1, 1, moves, teamName); // Top-Right
                AddDiagonalMoves(row, column, -1, 1, moves, teamName); // Bottom-Right
                AddDiagonalMoves(row, column, 1, -1, moves, teamName); // Top-Left
                AddDiagonalMoves(row, column, -1, -1, moves, teamName); // Bottom-Left
                break;

            case ChessPieceType.Queen:
                // Queen moves (combination of Rook and Bishop moves)
                AddStraightLineMoves(row, column, 1, 0, moves, teamName); // Up
                AddStraightLineMoves(row, column, -1, 0, moves, teamName); // Down
                AddStraightLineMoves(row, column, 0, 1, moves, teamName); // Right
                AddStraightLineMoves(row, column, 0, -1, moves, teamName); // Left
                AddDiagonalMoves(row, column, 1, 1, moves, teamName); // Top-Right
                AddDiagonalMoves(row, column, -1, 1, moves, teamName); // Bottom-Right
                AddDiagonalMoves(row, column, 1, -1, moves, teamName); // Top-Left
                AddDiagonalMoves(row, column, -1, -1, moves, teamName); // Bottom-Left
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
                    if (IsValidMove(newRow, newCol))
                    {
                        if (NotBlocked(newRow, newCol))
                        {
                            moves.Add(new Vector2Int(newRow, newCol));
                        }
                        else if (TeamAtGivenPosition(newRow, newCol) != teamName)
                        {
                            ChessBoardPlacementHandler.Instance.EnemyHighlight(newRow, newCol);
                        }
                    }
                }
                break;

        }

        return moves;
    }

    private bool NotBlocked(int row, int column)
    {
        return TeamAtGivenPosition(row, column) == "Empty";
    }

    // Helper method to check if a move is within the bounds of the board
    private bool IsValidMove(int row, int column)
    {
        return row >= 0 && row < 8 && column >= 0 && column < 8;
    }

    // Helper method to add straight line moves (vertical and horizontal)
    private void AddStraightLineMoves(int row, int column, int deltaRow, int deltaCol, List<Vector2Int> moves, string teamName)
    {
        for (int i = 1; i < 8; i++)
        {
            int newRow = row + i * deltaRow;
            int newCol = column + i * deltaCol;

            if (!IsValidMove(newRow, newCol)) break;
            if (NotBlocked(newRow, newCol))
                moves.Add(new Vector2Int(newRow, newCol));
            else
            {
                if (TeamAtGivenPosition(newRow, newCol) != teamName)            
                    ChessBoardPlacementHandler.Instance.EnemyHighlight(newRow, newCol);
                break;
            }
        }
    }


    // Helper method to add diagonal moves
    private void AddDiagonalMoves(int row, int column, int deltaRow, int deltaCol, List<Vector2Int> moves, string teamName)
    {
        for (int i = 1; i < 8; i++)
        {
            int newRow = row + i * deltaRow;
            int newCol = column + i * deltaCol;

            if (!IsValidMove(newRow, newCol)) break;
            if (NotBlocked(newRow, newCol))
                moves.Add(new Vector2Int(newRow, newCol));
            else
            {
                if (TeamAtGivenPosition(newRow, newCol) != teamName)
                    ChessBoardPlacementHandler.Instance.EnemyHighlight(newRow, newCol);
                break;
            }
        }
    }


    // Helper method to check and add diagonal attacks for pawn
    private void CheckDiagonalAttack1(int row, int column, List<Vector2Int> moves, string teamName)
    {
        if (IsValidMove(row, column))
        {
            if (TeamAtGivenPosition(row + 1, column) != "Empty" && TeamAtGivenPosition(row + 1, column) != teamName){
                moves.Add(new Vector2Int(row + 1, column - 1));
                moves.Add(new Vector2Int(row + 1, column + 1));
                //moves.Remove(new Vector2Int(row + 1, column));
                ChessBoardPlacementHandler.Instance.EnemyHighlight(row + 1, column);
            }
            
        }
    }

    private void CheckDiagonalAttack2(int row, int column, List<Vector2Int> moves, string teamName)
    {
        if (IsValidMove(row, column))
        {
            if (TeamAtGivenPosition(row -1, column) != "Empty" && TeamAtGivenPosition(row - 1, column) != teamName)
            {
                moves.Add(new Vector2Int(row - 1, column - 1));
                moves.Add(new Vector2Int(row - 1, column + 1));
                ChessBoardPlacementHandler.Instance.EnemyHighlight(row - 1, column);
            }

        }
    }

    private string TeamAtGivenPosition(int row, int column)
    {
        GameObject chessPieces = GameObject.Find("Player Positions");
        foreach (Transform child in chessPieces.transform)
        {
            GameObject piece = child.gameObject;
            ChessPlayerPlacementHandler playerPlacementHandler = piece.GetComponent<ChessPlayerPlacementHandler>();
            string teamName = playerPlacementHandler.teamOption.ToString();
            if (playerPlacementHandler.row == row && playerPlacementHandler.column == column)
            {
                return teamName;
            }
        }
        return "Empty";
    }
}

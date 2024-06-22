using Chess.Scripts.Core;
using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

// Script has been attached to the ChessBoard gameObject
public class ChessMovePredictor : MonoBehaviour
{
    // Singleton Pattern - Instance being used in InputHandler
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

    // Highlight the possible friendly and enemy moves for a piece at a given position
    // Being called from InputHandler
    internal void PredictMoves(int row, int column, string pieceName, string teamName)
    {
        // Parse the string pieceName to the corresponding value of the ChessPieceType enumeration
        // There is a chance of error if the name in enum 'ChessPieceType' doesn't match the piece name given to the GameObject in Hierarchy
        if (!Enum.TryParse(pieceName, false, out ChessPieceType pieceType))
        {
            Debug.LogError("Invalid piece name: " + pieceName);
            return;
        }
        ChessBoardPlacementHandler.Instance.ClearHighlights();
        List<Vector2Int> possibleMoves = GetPossibleMoves(row, column, pieceType, teamName);

        // Only friendly moves, enemy detection has been done when the GetPossibleMoves method is called
        foreach (var move in possibleMoves)
        {
            ChessBoardPlacementHandler.Instance.Highlight(move.x, move.y);
        }
    }

    // Determine the possible moves based on the piece type and position
    // Enemy highlighting also included
    private List<Vector2Int> GetPossibleMoves(int row, int column, ChessPieceType pieceType, string teamName)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        switch (pieceType)
        {
            case ChessPieceType.Pawn1:
                int forwardDir = 1;
                int startRow = 1;
                // Single Move Check
                if (WithinBounds(row + forwardDir, column) && NotBlocked(row + forwardDir, column))
                    moves.Add(new Vector2Int(row + forwardDir, column));
                // Double Move Check initially
                if (row == startRow && WithinBounds(row + 2 * forwardDir, column) && NotBlocked(row + 2* forwardDir, column) && NotBlocked(row + forwardDir, column))
                    moves.Add(new Vector2Int(row + 2 * forwardDir, column));
                // Diagonal Attack Check
                CheckDiagonalAttackForPawn1(row, column , moves, teamName);
                break;

            case ChessPieceType.Pawn2:
                int backwardDir = 1;
                int startingRow = 6;
                // Single Move Check
                if (WithinBounds(row - backwardDir, column) && NotBlocked(row - backwardDir, column))
                    moves.Add(new Vector2Int(row - backwardDir, column));
                // Double Move Check initially
                if (row == startingRow && WithinBounds(row - 2 * backwardDir, column) && NotBlocked(row - 2 * backwardDir, column) && NotBlocked(row - backwardDir, column))
                    moves.Add(new Vector2Int(row - 2 * backwardDir, column));
                // Diagonal Attack Check
                CheckDiagonalAttackForPawn2(row, column, moves, teamName);
                break;

            case ChessPieceType.Rook:
                AddStraightLineMoves(row, column, 1, 0, moves, teamName); // Up
                AddStraightLineMoves(row, column, -1, 0, moves, teamName); // Down
                AddStraightLineMoves(row, column, 0, 1, moves, teamName); // Right
                AddStraightLineMoves(row, column, 0, -1, moves, teamName); // Left
                break;

            case ChessPieceType.Knight:
                Vector2Int[] knightMoves = {
                    new Vector2Int(2, 1), new Vector2Int(1, 2), new Vector2Int(-1, 2), new Vector2Int(-2, 1),
                    new Vector2Int(-2, -1), new Vector2Int(-1, -2), new Vector2Int(1, -2), new Vector2Int(2, -1)
                };
                foreach (var move in knightMoves)
                {
                    int newRow = row + move.x;
                    int newCol = column + move.y;
                    if (WithinBounds(newRow, newCol))
                    {
                        // Tile is empty
                        if (NotBlocked(newRow, newCol))
                            moves.Add(new Vector2Int(newRow, newCol));
                        // Tile contains enemy 
                        else if (TeamAtGivenPosition(newRow, newCol) != teamName)
                            ChessBoardPlacementHandler.Instance.EnemyHighlight(newRow, newCol);
                    }
                }
                break;

            case ChessPieceType.Bishop:
                AddDiagonalMoves(row, column, 1, 1, moves, teamName); // Top-Right
                AddDiagonalMoves(row, column, -1, 1, moves, teamName); // Bottom-Right
                AddDiagonalMoves(row, column, 1, -1, moves, teamName); // Top-Left
                AddDiagonalMoves(row, column, -1, -1, moves, teamName); // Bottom-Left
                break;

            case ChessPieceType.Queen:
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
                Vector2Int[] kingMoves = {
                    new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1), new Vector2Int(-1, 1),
                    new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1)
                    };
                foreach (var move in kingMoves)
                {
                    int newRow = row + move.x;
                    int newCol = column + move.y;
                    if (WithinBounds(newRow, newCol))
                    {
                        // Tile is empty
                        if (NotBlocked(newRow, newCol))
                            moves.Add(new Vector2Int(newRow, newCol));
                        // Tile contains Enemy
                        else if (TeamAtGivenPosition(newRow, newCol) != teamName)
                            ChessBoardPlacementHandler.Instance.EnemyHighlight(newRow, newCol);
                    }
                }
                break;
        }
        return moves;
    }


    // Add straight line moves (vertical and horizontal)
    // Call Enemy Highlighter if enemy in range
    private void AddStraightLineMoves(int row, int column, int deltaRow, int deltaCol, List<Vector2Int> moves, string teamName)
    {
        for (int i = 1; i < 8; i++)
        {
            int newRow = row + i * deltaRow;
            int newCol = column + i * deltaCol;

            if (!WithinBounds(newRow, newCol)) break;
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


    // Add diagonal moves
    // Call Enemy Highlighter if enemy in range
    private void AddDiagonalMoves(int row, int column, int deltaRow, int deltaCol, List<Vector2Int> moves, string teamName)
    {
        for (int i = 1; i < 8; i++)
        {
            int newRow = row + i * deltaRow;
            int newCol = column + i * deltaCol;

            if (!WithinBounds(newRow, newCol)) break;
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


    // Check and add diagonal attacks for pawn1
    // Call Enemy Highlighter if enemy in range
    private void CheckDiagonalAttackForPawn1(int row, int column, List<Vector2Int> moves, string teamName)
    {
        if (WithinBounds(row, column))
        {
            if (TeamAtGivenPosition(row + 1, column) != "Empty" && TeamAtGivenPosition(row + 1, column) != teamName){
                moves.Add(new Vector2Int(row + 1, column - 1));
                moves.Add(new Vector2Int(row + 1, column + 1));
                //moves.Remove(new Vector2Int(row + 1, column));
                ChessBoardPlacementHandler.Instance.EnemyHighlight(row + 1, column);
            }
        }
    }

    // Check and add diagonal attacks for pawn2
    // Call Enemy Highlighter if enemy in range
    private void CheckDiagonalAttackForPawn2(int row, int column, List<Vector2Int> moves, string teamName)
    {
        if (WithinBounds(row, column))
        {
            if (TeamAtGivenPosition(row -1, column) != "Empty" && TeamAtGivenPosition(row - 1, column) != teamName)
            {
                moves.Add(new Vector2Int(row - 1, column - 1));
                moves.Add(new Vector2Int(row - 1, column + 1));
                ChessBoardPlacementHandler.Instance.EnemyHighlight(row - 1, column);
            }

        }
    }

    // Check if a move is within the bounds of the board
    private bool WithinBounds(int row, int column)
    {
        return row >= 0 && row < 8 && column >= 0 && column < 8;
    }

    // Check if there is no Piece at the given position
    private bool NotBlocked(int row, int column)
    {
        return TeamAtGivenPosition(row, column) == "Empty";
    }

    // Check which team is present at the given position
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

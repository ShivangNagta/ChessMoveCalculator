using Chess.Scripts.Core;
using UnityEngine;

// Script has been attached to all the individual tiles
public class InputHandler : MonoBehaviour
{
    // Calculate the possible moves when clicked on a piece
    private void OnMouseUp()
    {
        Vector2 clickedTilePosition = transform.position;
        // Find the piece corresponding to the clicked tile position
        GameObject pieceObject = FindPieceAtPosition(clickedTilePosition);
        if (pieceObject != null)
        {
            ChessPlayerPlacementHandler playerPlacementHandler = pieceObject.GetComponent<ChessPlayerPlacementHandler>();
            string teamName = playerPlacementHandler.teamOption.ToString();
            int row = playerPlacementHandler.row;
            int column = playerPlacementHandler.column;
            ChessMovePredictor.Instance.PredictMoves(row, column, pieceObject.name, teamName);
        }
    }

    // Check if there is a piece at a given tile
    private GameObject FindPieceAtPosition(Vector2 tilePosition)
    {
        GameObject chessPieces = GameObject.Find("Player Positions");
        foreach (Transform child in chessPieces.transform)
        {
            GameObject piece = child.gameObject;
            if (Vector2.Distance(piece.transform.position, tilePosition) < 0.01f) // This could be 0, but added for extra check
                return piece;
        }
        return null;
    }


}

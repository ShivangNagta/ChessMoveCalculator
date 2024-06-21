using Chess.Scripts.Core;
using UnityEngine;


public class InputHandler : MonoBehaviour
{

    private void OnMouseUp()
    {
        Vector2 clickedTilePosition = transform.position;

        // Find the piece corresponding to the clicked tile position
        GameObject pieceObject = FindPieceAtPosition(clickedTilePosition);

        if (pieceObject != null)
        {

            ChessPlayerPlacementHandler playerPlacementHandler = pieceObject.GetComponent<ChessPlayerPlacementHandler>();

            int row = playerPlacementHandler.row;
            int column = playerPlacementHandler.column;

            ChessMovePredictor.Instance.PredictMoves(row, column, pieceObject.name);

        }
    }


    private GameObject FindPieceAtPosition(Vector2 tilePosition)
    {
        GameObject chessPieces = GameObject.Find("Player Positions");

        foreach (Transform child in chessPieces.transform)
        {
            GameObject piece = child.gameObject;

            // Compare positions with some tolerance
            if (Vector2.Distance(piece.transform.position, tilePosition) < 0.1f)
            {
                return piece;
            }
        }

        return null;
    }


}
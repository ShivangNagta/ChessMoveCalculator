using System;
using Unity.Mathematics;
using UnityEngine;

namespace Chess.Scripts.Core {

    public class ChessPlayerPlacementHandler : MonoBehaviour {
        public enum Team
        {
            Team1,
            Team2
        }

        [SerializeField] public int row, column;
        [SerializeField] public Team teamOption;

        private void Start() {
            transform.position = ChessBoardPlacementHandler.Instance.GetTile(row, column).transform.position;
        }
    }
}
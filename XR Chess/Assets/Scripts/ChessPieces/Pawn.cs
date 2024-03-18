using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> returnMoves = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        // Advance one tile
        if (board[currentX, currentY + direction] == null)
        {
            returnMoves.Add(new Vector2Int(currentX, currentY + direction));
        }

        // Advance two tiles (starting move)
        if (board[currentX, currentY + direction] == null)
        {
            // white team
            if (team == 0 && currentY == 1 && board[currentX, currentY + (direction * 2)] == null)
            {
                returnMoves.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }

            // black team
            if (team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
            {
                returnMoves.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }
        }

        // Attack move
        // right diagonal
        if (currentX != tileCountX - 1)
        {
            if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
            {
                returnMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
            }
        }

        // left diagonal
        if (currentX != 0)
        {
            if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
            {
                returnMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
            }
        }

        return returnMoves;
    }
}

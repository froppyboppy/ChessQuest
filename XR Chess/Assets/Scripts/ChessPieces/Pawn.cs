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

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == 0) ? 1 : -1;

        // Promotion
        if ((team == 0 && currentY == 6) || (team == 1 && currentY == 1))
        {
            return SpecialMove.Promotion;
        }

        // En Passant
        // check if there is a previous move
        if (moveList.Count > 0)
        {
            var lastMove = moveList[moveList.Count - 1];
            if (board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn)
            {
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2)
                {
                    if (board[lastMove[1].x, lastMove[1].y].team != team)
                    {
                        if (lastMove[1].y == currentY)
                        {
                            // land left
                            if (lastMove[1].x == currentX - 1)
                            {
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }

                            // land right
                            if (lastMove[1].x == currentX + 1)
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }
        }

        return SpecialMove.None;
    }
}

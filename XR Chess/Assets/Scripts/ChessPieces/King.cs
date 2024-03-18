using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        // valid moves list
        List<Vector2Int> returnMoves = new List<Vector2Int>();

        // Right
        if (currentX + 1 < tileCountX)
        {
            // Right side
            // check for available space
            if (board[currentX + 1, currentY] == null)
            {
                returnMoves.Add(new Vector2Int(currentX + 1, currentY));
            }

            // attack
            else if (board[currentX + 1, currentY].team != team)
            {
                returnMoves.Add(new Vector2Int(currentX + 1, currentY));
            }

            // top right
            if (currentY + 1 < tileCountY)
            {
                if (board[currentX + 1, currentY + 1] == null)
                {
                    returnMoves.Add(new Vector2Int(currentX + 1, currentY + 1));
                }

                // attack
                else if (board[currentX + 1, currentY + 1].team != team)
                {
                    returnMoves.Add(new Vector2Int(currentX + 1, currentY + 1));
                }
            }

            // bottom right
            if (currentY - 1 >= 0)
            {
                if (board[currentX + 1, currentY - 1] == null)
                {
                    returnMoves.Add(new Vector2Int(currentX + 1, currentY - 1));
                }

                // attack
                else if (board[currentX + 1, currentY - 1].team != team)
                {
                    returnMoves.Add(new Vector2Int(currentX + 1, currentY + 1));
                }
            }
        }

        // Left
        if (currentX - 1 >= 0)
        {
            // Left side
            // check for available space
            if (board[currentX - 1, currentY] == null)
            {
                returnMoves.Add(new Vector2Int(currentX - 1, currentY));
            }

            // attack
            else if (board[currentX - 1, currentY].team != team)
            {
                returnMoves.Add(new Vector2Int(currentX - 1, currentY));
            }

            // top left
            if (currentY + 1 < tileCountY)
            {
                if (board[currentX - 1, currentY + 1] == null)
                {
                    returnMoves.Add(new Vector2Int(currentX - 1, currentY + 1));
                }

                // attack
                else if (board[currentX - 1, currentY + 1].team != team)
                {
                    returnMoves.Add(new Vector2Int(currentX - 1, currentY + 1));
                }
            }

            // bottom right
            if (currentY - 1 >= 0)
            {
                if (board[currentX - 1, currentY - 1] == null)
                {
                    returnMoves.Add(new Vector2Int(currentX - 1, currentY - 1));
                }

                // attack
                else if (board[currentX - 1, currentY - 1].team != team)
                {
                    returnMoves.Add(new Vector2Int(currentX - 1, currentY + 1));
                }
            }
        }

        // Up
        if (currentY + 1 < tileCountY)
        {
            if (board[currentX, currentY + 1] == null || board[currentX, currentY + 1].team != team)
            {
                returnMoves.Add(new Vector2Int(currentX, currentY + 1));
            }
        }

        // Down
        if (currentY - 1 >= 0)
        {
            if (board[currentX, currentY - 1] == null || board[currentX, currentY - 1].team != team)
            {
                returnMoves.Add(new Vector2Int(currentX, currentY - 1));
            }
        }

        return returnMoves;
    }
}

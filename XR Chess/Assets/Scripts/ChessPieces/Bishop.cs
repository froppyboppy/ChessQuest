using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        // valid moves list
        List<Vector2Int> returnMoves = new List<Vector2Int>();

        // Top right
        for (int x = currentX + 1, y = currentY + 1; x < tileCountX && y < tileCountY; x++, y++)
        {
            if (board[x, y] == null)
            {
                returnMoves.Add(new Vector2Int(x, y));
            }

            else
            {
                if (board[x, y].team != team)
                {
                    returnMoves.Add(new Vector2Int(x, y));
                }

                break;
            }
        }

        // Top left
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < tileCountY; x--, y++)
        {
            if (board[x, y] == null)
            {
                returnMoves.Add(new Vector2Int(x, y));
            }

            else
            {
                if (board[x, y].team != team)
                {
                    returnMoves.Add(new Vector2Int(x, y));
                }

                break;
            }
        }

        // Bottom right
        for (int x = currentX + 1, y = currentY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            if (board[x, y] == null)
            {
                returnMoves.Add(new Vector2Int(x, y));
            }

            else
            {
                if (board[x, y].team != team)
                {
                    returnMoves.Add(new Vector2Int(x, y));
                }

                break;
            }
        }

        // Bottom left
        for (int x = currentX - 1, y = currentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (board[x, y] == null)
            {
                returnMoves.Add(new Vector2Int(x, y));
            }

            else
            {
                if (board[x, y].team != team)
                {
                    returnMoves.Add(new Vector2Int(x, y));
                }

                break;
            }
        }

        return returnMoves;
    }
}
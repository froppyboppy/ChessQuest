using System.Collections.Generic;
using UnityEngine;

public class Qeen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        // valid moves list
        List<Vector2Int> returnMoves = new List<Vector2Int>();

        // Down
        for (int i = currentY - 1; i >= 0; i--)
        {
            // valid move
            if (board[currentX, i] == null)
            {
                returnMoves.Add(new Vector2Int(currentX, i));
            }

            // attack move
            if (board[currentX, i] != null)
            {
                if (board[currentX, i].team != team)
                {
                    returnMoves.Add(new Vector2Int(currentX, i));
                }

                // stop when theres another piece
                break;
            }
        }

        // Up
        for (int i = currentY + 1; i < tileCountY; i++)
        {
            // valid move
            if (board[currentX, i] == null)
            {
                returnMoves.Add(new Vector2Int(currentX, i));
            }

            // attack move
            if (board[currentX, i] != null)
            {
                if (board[currentX, i].team != team)
                {
                    returnMoves.Add(new Vector2Int(currentX, i));
                }

                // stop when theres another piece
                break;
            }
        }

        // Left
        for (int i = currentX - 1; i >= 0; i--)
        {
            // valid move
            if (board[i, currentY] == null)
            {
                returnMoves.Add(new Vector2Int(i, currentY));
            }

            // attack move
            if (board[i, currentY] != null)
            {
                if (board[i, currentY].team != team)
                {
                    returnMoves.Add(new Vector2Int(i, currentY));
                }

                // stop when theres another piece
                break;
            }
        }

        // Right
        for (int i = currentX + 1; i < tileCountX; i++)
        {
            // valid move
            if (board[i, currentY] == null)
            {
                returnMoves.Add(new Vector2Int(i, currentY));
            }

            // attack move
            if (board[i, currentY] != null)
            {
                if (board[i, currentY].team != team)
                {
                    returnMoves.Add(new Vector2Int(i, currentY));
                }

                // stop when theres another piece
                break;
            }
        }

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
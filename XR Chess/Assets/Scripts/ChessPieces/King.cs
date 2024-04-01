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

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        SpecialMove r = SpecialMove.None;

        // check if king and rooks have moved
        var kingMove = moveList.Find(move => move[0].x == 4 && move[0].y == ((team == 0) ? 0 : 7));
        var leftRook = moveList.Find(move => move[0].x == 0 && move[0].y == ((team == 0) ? 0 : 7));
        var rightRook = moveList.Find(move => move[0].x == 7 && move[0].y == ((team == 0) ? 0 : 7));

        if (kingMove == null && currentX == 4)
        {
            // white team
            if (team == 0)
            {
                // left rook
                if (leftRook == null)
                {
                    if (board[0, 0].type == ChessPieceType.Rook)
                    {
                        if (board[0, 0].team == 0)
                        {
                            if (board[3, 0] == null)
                            {
                                if (board[2, 0] == null)
                                {
                                    if (board[1, 0] == null)
                                    {
                                        // no obstruction
                                        availableMoves.Add(new Vector2Int(2, 0));

                                        r = SpecialMove.Castling;
                                    }
                                }
                            }
                        }
                    }
                }

                // right rook
                if (rightRook == null)
                {
                    if (board[7, 0].type == ChessPieceType.Rook)
                    {
                        if (board[7, 0].team == 0)
                        {
                            if (board[5, 0] == null)
                            {
                                if (board[6, 0] == null)
                                {
                                    // no obstruction
                                    availableMoves.Add(new Vector2Int(6, 0));

                                    r = SpecialMove.Castling;
                                }
                            }
                        }
                    }
                }
            }

            // black team
            else
            {
                // left rook
                if (leftRook == null)
                {
                    if (board[0, 7].type == ChessPieceType.Rook)
                    {
                        if (board[0, 7].team == 1)
                        {
                            if (board[3, 7] == null)
                            {
                                if (board[2, 7] == null)
                                {
                                    if (board[1, 7] == null)
                                    {
                                        // no obstruction
                                        availableMoves.Add(new Vector2Int(2, 7));

                                        r = SpecialMove.Castling;
                                    }
                                }
                            }
                        }
                    }
                }

                // right rook
                if (rightRook == null)
                {
                    if (board[7, 7].type == ChessPieceType.Rook)
                    {
                        if (board[7, 7].team == 1)
                        {
                            if (board[5, 7] == null)
                            {
                                if (board[6, 7] == null)
                                {
                                    // no obstruction
                                    availableMoves.Add(new Vector2Int(6, 7));

                                    r = SpecialMove.Castling;
                                }
                            }
                        }
                    }
                }
            }
        }

        return r;
    }
}

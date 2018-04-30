using UnityEngine;
using System.Collections;

public static class Constants {

    public static readonly string[,] DefaultBoard = { { "Rook", "Knight", "Bishop", "Queen", "King", "Bishop", "Knight", "Rook" },
                                             { "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn" },
                                             { "", "", "", "", "", "", "", "" },
                                             { "", "", "", "", "", "", "", "" },
                                             { "", "", "", "", "", "", "", "" },
                                             { "", "", "", "", "", "", "", "" },
                                             { "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn" },
                                             { "Rook", "Knight", "Bishop", "King", "Queen", "Bishop", "Knight", "Rook" } };

    public readonly static Vector3 oneRight = new Vector3(1.41f, 0f, 0f);
    public readonly static Vector3 oneForward = new Vector3(0f, 1.34f, 0f);
    public static Material Black, White, Selected, Target, Check;
    public static Texture2D blackTurn, whiteTurn;
}

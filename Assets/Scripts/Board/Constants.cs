/// <summary>
/// 用于存放常量的类
/// </summary>

/*
将/帅: King(K)
士/仕: Advisor(A)
象/相: Bishop(B)
车: Rook(R)
马: Knight(N)
炮: Cannon(C)
兵/卒: Pawn(P)
*/

namespace Xiangqi
{
    public static class SIDE
    {
        public const byte Red = 0;
        public const byte Black = 1;
    }

    public static class PIECE_TYPE
    {
        public const byte Empty = 0;
        public const byte King = 1;
        public const byte Advisor = 2;
        public const byte Bishop = 3;
        public const byte Rook = 4;
        public const byte Knight = 5;
        public const byte Cannon = 6;
        public const byte Pawn = 7;
    }

    public static class PIECE
    {
        public const byte Empty = 0;
        public const byte RedKing = 0x01;
        public const byte RedAdvisor = 0x02;
        public const byte RedBishop = 0x03;
        public const byte RedRook = 0x04;
        public const byte RedKnight = 0x05;
        public const byte RedCannon = 0x06;
        public const byte RedPawn = 0x07;
        public const byte BlackKing = 0x81;
        public const byte BlackAdvisor = 0x82;
        public const byte BlackBishop = 0x83;
        public const byte BlackRook = 0x84;
        public const byte BlackKnight = 0x85;
        public const byte BlackCannon = 0x86;
        public const byte BlackPawn = 0x87;
    }

    /// <summary>
    /// 9x10的棋盘用一维数组表示位置
    /// </summary>
    public static class PiecePosition
    {
        public static readonly byte[] King = {
            0,0,0,1,1,1,0,0,0,
            0,0,0,1,1,1,0,0,0,
            0,0,0,1,1,1,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,1,1,1,0,0,0,
            0,0,0,1,1,1,0,0,0,
            0,0,0,1,1,1,0,0,0
        };
        public static readonly byte[] Advisor = {
            0,0,0,1,0,1,0,0,0,
            0,0,0,0,1,0,0,0,0,
            0,0,0,1,0,1,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,1,0,1,0,0,0,
            0,0,0,0,1,0,0,0,0,
            0,0,0,1,0,1,0,0,0
        };
        public static readonly byte[] Bishop = {
            0,0,1,0,0,0,1,0,0,
            0,0,0,0,0,0,0,0,0,
            1,0,0,0,1,0,0,0,1,
            0,0,0,0,0,0,0,0,0,
            0,0,1,0,0,0,1,0,0,
            0,0,1,0,0,0,1,0,0,
            0,0,0,0,0,0,0,0,0,
            1,0,0,0,1,0,0,0,1,
            0,0,0,0,0,0,0,0,0,
            0,0,1,0,0,0,1,0,0
        };
    }

    public static class PieceMove
    {
        public static readonly byte[] ExpandedBoard = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 ,0,
            0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 ,0 ,0,
            0, 0, 1, 1, 1, 1, 1, 1, 1, 1 ,1 ,0 ,0,
            0, 0, 1, 1, 1, 1, 1, 1, 1, 1 ,1 ,0 ,0,
            0, 0, 1, 1, 1, 1, 1, 1, 1, 1 ,1 ,0 ,0,
            0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 ,0,
            0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 ,0 ,0,
            0, 0, 1, 1, 1, 1, 1, 1, 1, 1 ,1 ,0 ,0,
            0, 0, 1, 1, 1, 1, 1, 1, 1, 1 ,1 ,0 ,0,
            0, 0, 1, 1, 1, 1, 1, 1, 1, 1 ,1 ,0 ,0,
            0, 0, 0, 0, 0, 0, 0, 0, 0 ,0 ,0 ,0 ,0,
            0, 0, 0, 0, 0, 0, 0, 0, 0 ,0 ,0 ,0 ,0
        };
        public static readonly int[,] Direction = {
            // King
            { 1, -9, -1, 9, 0, 0, 0, 0 },
            // Advisor
            { -8, -10, 8, 10, 0, 0, 0, 0 },
            // Bishop
            { -16, -20, 16, 20, 0, 0, 0, 0 },
            // Rook
            { 1, -9, -1, 9, 0, 0, 0, 0 },
            // Knight
            { -7, -17, -19, -11, 7, 17, 19, 11 },
            // Cannon
            { 1, -9, -1, 9, 0, 0, 0, 0 },
            // Pawn
            { -1, 9, 1, 0, 0, 0, 0, 0 }
        };

        public static readonly int[,] ExpandedDirection = {
            // King
            { 1, -13, -1, 13, 0, 0, 0, 0 },
            // Advisor
            { -12, -14, 12, 14, 0, 0, 0, 0 },
            // Bishop
            { -24, -28, 24, 28, 0, 0, 0, 0 },
            // Rook
            { 1, -13, -1, 13, 0, 0, 0, 0 },
            // Knight
            { -11, -25, -27, -15, 11, 25, 27, 15 },
            // Cannon
            { 1, -13, -1, 13, 0, 0, 0, 0 },
            // Pawn
            { -1, -13, 1, 13, 0, 0, 0, 0 }
        };

        public static readonly int[] MaxStep = {
            1, 1, 1, 9, 1, 9, 1
        };
    }

}

/// <summary>
/// MyUtil工具类
/// </summary>

namespace Xiangqi
{

    public static class PieceUtil
    {
        public static byte LetterToPiece(char letter)
        {
            switch (letter)
            {
                case 'K': return PIECE.RedKing;
                case 'A': return PIECE.RedAdvisor;
                case 'B': return PIECE.RedBishop;
                case 'R': return PIECE.RedRook;
                case 'N': return PIECE.RedKnight;
                case 'C': return PIECE.RedCannon;
                case 'P': return PIECE.RedPawn;
                case 'k': return PIECE.BlackKing;
                case 'a': return PIECE.BlackAdvisor;
                case 'b': return PIECE.BlackBishop;
                case 'r': return PIECE.BlackRook;
                case 'n': return PIECE.BlackKnight;
                case 'c': return PIECE.BlackCannon;
                case 'p': return PIECE.BlackPawn;
                default:
                    return PIECE.Empty;
            }
        }

        public static char PieceToLetter(byte piece)
        {
            switch (piece)
            {
                case PIECE.RedKing:         return 'K';
                case PIECE.RedAdvisor:      return 'A';
                case PIECE.RedBishop:       return 'B';
                case PIECE.RedRook:         return 'R';
                case PIECE.RedKnight:       return 'N';
                case PIECE.RedCannon:       return 'C';
                case PIECE.RedPawn:         return 'P';
                case PIECE.BlackKing:       return 'k';
                case PIECE.BlackAdvisor:    return 'a';
                case PIECE.BlackBishop:     return 'b';
                case PIECE.BlackRook:       return 'r';
                case PIECE.BlackKnight:     return 'n';
                case PIECE.BlackCannon:     return 'c';
                case PIECE.BlackPawn:       return 'p';
                default:
                    return ' ';
            }
        }

        public static byte ChineseCharacterToPiece(char letter, byte side = 0)
        {
            switch (letter)
            {
                case '帅':
                case '帥':
                    return PIECE.RedKing;
                case '仕': return PIECE.RedAdvisor;
                case '相': return PIECE.RedBishop;
                case '兵': return PIECE.RedPawn;
                case '将':
                case '將':
                    return PIECE.BlackKing;
                case '士': return PIECE.BlackAdvisor;
                case '象': return PIECE.BlackBishop;
                case '卒': return PIECE.BlackPawn;
                case '车':
                case '車':
                    return side == SIDE.Red ? PIECE.RedRook : PIECE.BlackRook;
                case '马':
                case '馬':
                    return side == SIDE.Red ? PIECE.RedKnight : PIECE.BlackKnight;
                case '炮': return side == SIDE.Red ? PIECE.RedCannon : PIECE.BlackCannon;
                default:
                    return PIECE.Empty;
            }
        }

        public static char PieceToChineseCharacter(byte piece)
        {
            switch (piece)
            {
                case PIECE.RedKing:         return '帅';
                case PIECE.RedAdvisor:      return '仕';
                case PIECE.RedBishop:       return '相';
                case PIECE.RedPawn:         return '兵';
                case PIECE.BlackKing:       return '将';
                case PIECE.BlackAdvisor:    return '士';
                case PIECE.BlackBishop:     return '象';
                case PIECE.BlackPawn:       return '卒';
                case PIECE.RedRook:         return '车';
                case PIECE.RedKnight:       return '马';
                case PIECE.RedCannon:       return '炮';
                case PIECE.BlackRook:       return '车';
                case PIECE.BlackKnight:     return '马';
                case PIECE.BlackCannon:     return '炮';
                default:
                    return '\0';
            }
        }

        public static int GetCollumn(byte piece) => piece % 9;

        public static int GetRow(byte piece) => piece / 9;

        public static byte GetPieceSide(byte piece)
        {
            return (piece & 0x80) == 0 ? SIDE.Red : SIDE.Black;
        }

        public static byte GetPieceType(byte piece)
        {
            return (byte)(piece & 0x7F);
        }

        public static bool IsSameRow(byte piece1, byte piece2) => GetRow(piece1) == GetRow(piece2);

        public static bool IsSameCollumn(byte piece1, byte piece2) => GetCollumn(piece1) == GetCollumn(piece2);

        public static bool IsCrossHalf(byte piece, byte side) => ((side == SIDE.Red) ^ (GetRow(piece) > 4));

        public static bool IsPositionValid(byte pieceType, byte position)
        {
            if (position < 0 || position > 89)
            {
                return false;
            }
            switch (pieceType)
            {
                case PIECE_TYPE.King:
                    return PiecePosition.King[position] == 1;
                case PIECE_TYPE.Advisor:
                    return PiecePosition.Advisor[position] == 1;
                case PIECE_TYPE.Bishop:
                    return PiecePosition.Bishop[position] == 1;
                default:
                    return true;
            }
        }
        
    }

}

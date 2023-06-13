/// <summary>
/// 棋谱工具类
/// </summary>

using System;
using System.Collections.Generic;
namespace Xiangqi
{
    public static class ChessNotationUtil
    {
        /// <summary>
        /// 将移动转换为WXF(World XiangQi Federation)记谱方式
        /// 炮二平五(C2.5), 马8进7(n8+7)
        /// 前、中、后分别用(+), (-), (.)表示，且放在棋子名称之后，如：前炮平五(C+.5)
        /// 进、退、平分别用(+), (-), (.)表示
        /// 在有两条纵线，每条纵线上都有一个以上的兵：按照“先从右到左，再从前到后”编号
        /// 同列超过三个棋子，或多列有同种棋子，用字母(a,b,c,...)表示，如：三兵进一(Pc+1)
        /// 详见: https://www.xqbase.com/protocol/cchess_move.htm
        /// </summary>
        /// <param name="move">移动</param>
        /// <param name="pieces">棋子数组</param>
        /// <returns></returns>
        public static string MoveToWXFNotation(short move, byte[] pieces)
        {
            byte start = (byte)(move >> 8);
            byte end = (byte)(move & 0xFF);
            byte piece = pieces[start];
            char pieceName = PieceUtil.PieceToLetter(piece);
            if (piece == PIECE.Empty || pieceName == '\0')
            {
                return "";
            }
            char [] notation = new char[4] { pieceName, '\0', '\0', '\0' };
            // step 1: 判断红黑
            int t1 = PieceUtil.GetPieceSide(piece) == SIDE.Red ? -1 : 1;
            // step 2: 判断移动方式(平, 进, 退), WXF中表示为(.)、(+)、(-)
            int row1 = PieceUtil.GetRow(start), row2 = PieceUtil.GetRow(end);
            int col1 = PieceUtil.GetCollumn(start), col2 = PieceUtil.GetCollumn(end);
            if (row1 == row2)
            {
                notation[2] = '.';
                notation[3] = (char)(5 + t1 * (col2 - 4) + '0');
            }
            else
            {
                notation[2] = t1 * (row2 - row1) > 0 ? '+' : '-';
                // 直线行走的棋子进退看移动的距离, 其他棋子进退看最终位置所在的列
                switch (PieceUtil.GetPieceType(piece))
                {
                    case PIECE_TYPE.King:
                    case PIECE_TYPE.Rook:
                    case PIECE_TYPE.Cannon:
                    case PIECE_TYPE.Pawn:
                        notation[3] = Math.Abs(row2 - row1).ToString()[0];
                        break;
                    default:
                        notation[3] = (char)(5 + t1 * (col2 - 4) + '0');
                        break;
                }
            }
            // step 3: 判断起点处是否有多个同种棋子处于同一列
            int sameCountOfFront = 0, sameCountOfBack = 0;
            for (int i = start + t1 * 9; i >= 0 && i < 90; i += t1 * 9)
            {
                if (pieces[i] == piece) { sameCountOfFront++; }
            }
            for (int i = start - t1 * 9; i >= 0 && i < 90; i -= t1 * 9)
            {
                if (pieces[i] == piece) { sameCountOfBack++; }
            }
            char startCollumnName = (char)(5 + t1 * (col1 - 4) + '0');
            if (sameCountOfFront == 0 && sameCountOfBack == 0)
            { // 没有同种棋子处于同一列, 例如: 炮二平五(C2.5)
                notation[1] = startCollumnName;
            }
            else
            {
                // 判断其他列是否有同种棋子同列, 如果有则编号
                bool flag = false;
                for (int k = 0; k < 9; k++)
                {
                    if (k == col1) { continue; }
                    if (isMultiplePieceInSameCollumn(pieces, piece, k))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    notation[1] = (char)('a' + markPieceNumber(piece, pieces).IndexOf(start));
                }
                else
                {
                    if (sameCountOfFront == 1 && sameCountOfBack == 1)
                    { // 三颗棋子处于同一列, 且移动的棋子处于中间, 例如: 中兵进一(P.+1)
                        notation[1] = '.';
                    }
                    else if (sameCountOfFront + sameCountOfBack > 2)
                    { // 三颗及以上棋子处于同一列，例如：三兵进一(Pc+1)
                        notation[1] = (char)('a' + sameCountOfFront);
                    }
                    else
                    { // 棋子处于最前面或最后面, 例如: 前炮平五(C+.5)
                        notation[1] = sameCountOfFront == 0 ? '+' : '-';
                    }
                }
            }
            return new string(notation).Replace("\0", "");
        }

        /// <summary>
        /// 将WXF(World XiangQi Federation)记谱方式转换为移动
        /// </summary>
        /// <param name="notation">棋谱</param>
        /// <param name="pieces">棋子数组</param>
        public static short WXFNotationToMove(string notation, byte[] pieces)
        {
            if (notation.Length != 4) { return 0; }
            byte start = 0, end = 0;
            // STEP 1: 判断棋子种类
            byte piece = PieceUtil.LetterToPiece(notation[0]);
            if (piece == PIECE.Empty)
            {
                return 0;
            }
            // STEP 2: 确定起始位置
            int t1 = PieceUtil.GetPieceSide(piece) == SIDE.Red ? -1 : 1;
            if (char.IsDigit(notation[1])) // 类似于"炮二平五"(C2.5)
            {
                int col = t1 * (notation[1] - '0' - 5) + 4;
                start = getFirstPiecePositionOfColumn(pieces, piece, col);
            }
            else 
            {
                List<byte> list = markPieceNumber(piece, pieces);
                switch (notation[1])
                {
                    case '+': start = list[0]; break;
                    case '-': start = list[list.Count - 1]; break;
                    case '.': start = list[1]; break;
                    default: start = list[notation[1] - 'a']; break;
                }
            }
            // STEP 3: 确定目标位置
            char moveType = notation[notation.Length - 2];
            int endNumber = notation[notation.Length - 1] - '0';
            if (moveType == '.')
            {
                end = (byte)(start / 9 * 9 + 4 + t1 * (endNumber - 5));
            }
            else
            {
                int t2 = moveType == '+' ? 1 : -1;
                byte ptype = PieceUtil.GetPieceType(piece);
                switch (ptype)
                {
                    case PIECE_TYPE.King:
                    case PIECE_TYPE.Rook:
                    case PIECE_TYPE.Cannon:
                    case PIECE_TYPE.Pawn:
                        end = (byte)(start + t1 * 9 * t2 * endNumber);
                        break;
                    default:
                        int endColumn = t1 * (endNumber - 5) + 4;
                        bool b = ptype == PIECE_TYPE.Knight && Math.Abs(endColumn - start % 9) == 1 || ptype == PIECE_TYPE.Bishop;
                        int t3 = b ? 2 : 1;
                        end = (byte)((start / 9 + t1 * t2 * t3) * 9 + endColumn);
                        break;
                }
            }
            return (short)(start << 8 | end);
        }

        private static bool isMultiplePieceInSameCollumn(byte[] pieces, byte piece, int column)
        {
            int num = 0;
            for (int i = 0; i < 10; i++)
            {
                if (pieces[i * 9 + column] == piece) { num++; }
            }
            return num > 1;
        }

        private static List<byte> markPieceNumber(byte piece, byte[] pieces)
        {
            List<byte> list = new List<byte>();
            int t = PieceUtil.GetPieceSide(piece) == SIDE.Red ? -1 : 1;
            // 从右到左, 从上到下
            for (int i = t == -1 ? 8 : 0; i >= 0 && i < 9; i += t)
            {
                if (!isMultiplePieceInSameCollumn(pieces, piece, i)) { continue; }
                for (int j = t == -1 ? 0 : 9; j >= 0 && j < 10; j -= t)
                {
                    byte position = (byte)(j * 9 + i);
                    if (pieces[position] == piece)
                    {
                        list.Add(position);
                    }
                }
            }
            return list;
        }

        private static byte getFirstPiecePositionOfColumn(byte[] pieces, byte piece, int column)
        {
            int t = PieceUtil.GetPieceSide(piece) == SIDE.Red ? -1 : 1;
            for (int i = (int)(4.5 * (1 + t)); i >= 0 && i < 10; i -= t)
            {
                if (pieces[i * 9 + column] == piece)
                {
                    return (byte)(i * 9 + column);
                }
            }
            return 0;
        }

        public static string ChineseCharset = "一二三四五六七八九进平退一二三四五六七八九前中后";

        public static string AsciiCharset = "123456789+.-abcdefghi+.-";

        private static char chineseToAsciiCharset(char chinese, bool reverse = false)
        {
            int index = reverse ? ChineseCharset.LastIndexOf(chinese) : ChineseCharset.IndexOf(chinese);
            return index == -1 ? chinese : AsciiCharset[index];
        }

        private static char asciiToChineseCharset(char ascii, bool reverse = false)
        {
            int index = reverse ? AsciiCharset.LastIndexOf(ascii) : AsciiCharset.IndexOf(ascii);
            return index == -1 ? ascii : ChineseCharset[index];
        }

        private static bool IsDigit(char c)
        {
            return (c >= '0' && c <= '9') || (c >= '０' && c <= '９'); // 半角和全角数字
        }

        private static char ToFullWidthDigit(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (char)(c - '0' + '０');
            }
            return c;
        }

        private static char ToHalfWidthDigit(char c)
        {
            if (c >= '０' && c <= '９')
            {
                return (char)(c - '０' + '0');
            }
            return c;
        }

        private static string traditionalChineseCharset = "進後";
        private static string simplifiedChineseCharset = "进后";

        // 繁体转简体
        private static char toSinsimplifiedChinese(char c)
        {
            int index = traditionalChineseCharset.IndexOf(c);
            return index == -1 ? c : simplifiedChineseCharset[index];
        }

        /// <summary>
        /// WXF记谱方式转换为中文记谱方式
        /// </summary>
        /// <param name="wxfNotation">WXF记谱方式</param>
        /// <returns></returns>
        public static string WXFToChineseNotation(string wxfNotation)
        {
            if (wxfNotation.Length != 4) { return ""; }
            char[] chineseNotation = new char[4];
            byte piece = PieceUtil.LetterToPiece(wxfNotation[0]);
            byte side = PieceUtil.GetPieceSide(piece);
            if (char.IsDigit(wxfNotation[1]))
            {
                chineseNotation[0] = PieceUtil.PieceToChineseCharacter(piece);
                chineseNotation[1] = side == SIDE.Red ? asciiToChineseCharset(wxfNotation[1]) : ToFullWidthDigit(wxfNotation[1]);
            }
            else
            {
                chineseNotation[0] = asciiToChineseCharset(wxfNotation[1], true);
                chineseNotation[1] = PieceUtil.PieceToChineseCharacter(piece);
            }
            chineseNotation[2] = asciiToChineseCharset(wxfNotation[2]);
            chineseNotation[3] = side == SIDE.Red ? asciiToChineseCharset(wxfNotation[3]) : ToFullWidthDigit(wxfNotation[3]);
            return new string(chineseNotation);
        }

        /// <summary>
        /// 中文记谱方式转换为WXF记谱方式
        /// </summary>
        /// <param name="chineseNotation">中文记谱方式</param>
        /// <returns></returns>
        public static string ChineseToWXFNotation(string chineseNotation)
        {
            if (chineseNotation.Length != 4) { return ""; }
            char[] wxfNotation = new char[4];
            byte side = IsDigit(chineseNotation[3]) ? SIDE.Black : SIDE.Red;
            // 全部转化为半角数字
            // string ?notation = new String (chineseNotation.Select(c => ToHalfWidthDigit(c)).ToArray());
            char[] notation = new char[4];
            for (int i = 0; i < 4; i++)
            {
                notation[i] = ToHalfWidthDigit(chineseNotation[i]);
            }
            if (ChineseCharset.Contains(notation[0])) // 第一个字符不是棋子名称
            {
                wxfNotation[0] = PieceUtil.PieceToLetter(PieceUtil.ChineseCharacterToPiece(notation[1], side));
                wxfNotation[1] = chineseToAsciiCharset(toSinsimplifiedChinese(notation[0]), true);
            }
            else
            {
                wxfNotation[0] = PieceUtil.PieceToLetter(PieceUtil.ChineseCharacterToPiece(notation[0], side));
                wxfNotation[1] = chineseToAsciiCharset(notation[1]);
            }
            wxfNotation[2] = chineseToAsciiCharset(toSinsimplifiedChinese(notation[2]));
            wxfNotation[3] = chineseToAsciiCharset(notation[3]);
            return new string(wxfNotation);
        }

        /// <summary>
        /// 将移动转换为棋谱记谱方式
        /// </summary>
        /// <param name="move">移动</param>
        /// <param name="pieces">棋子数组</param>
        /// <returns></returns>
        public static string MoveToChineseNotation(short move, byte[] pieces)
        {
            return WXFToChineseNotation(MoveToWXFNotation(move, pieces));
        }

        /// <summary>
        /// 将中文棋谱记谱方式转换为移动
        /// </summary>
        /// <param name="notation">棋谱</param>
        /// <param name="pieces">棋子数组</param>
        /// <returns></returns>
        public static short ChineseNotationToMove(string notation, byte[] pieces)
        {
            return WXFNotationToMove(ChineseToWXFNotation(notation), pieces);
        }

        /// <summary>
        /// 将ICCS记谱方式转换为移动
        /// </summary>
        /// <param name="notation">棋谱</param>
        /// <returns></returns>
        #nullable enable
        public static short ICCSNotationToMove(string notation, byte[]? pieces = null)
        {
            return MoveUtil.StringToMove(notation);
        }

        /// <summary>
        /// 将移动转换为ICCS记谱方式
        /// </summary>
        /// <param name="move">移动</param>
        /// <returns></returns>
        public static string MoveToICCSNotation(short move, byte[]? pieces = null)
        {
            return MoveUtil.MoveToString(move);
        }

    }
}
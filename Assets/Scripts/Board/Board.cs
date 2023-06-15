/// <summary>
/// Board棋盘类
/// </summary>

using System;
using System.Collections.Generic;

namespace Xiangqi
{
    public partial class Board
    {
        // 走棋方, 0: Red, 1: Black
        public byte Side { get; set; }
        // 回合数
        public int Round { get; set; }
        // 未吃子回合数
        public int NoCapRound { get; set; }
        // 棋盘
        public byte[] Pieces { get; set; }
        // 评论
        public string Comment = "";

        public Board()
        {
            Side = 0;
            Round = 0;
            NoCapRound = 0;
            Pieces = new byte[90];
        }

        public Board(Board board)
        {
            Pieces = new byte[90];
            _copyBoard(board);
        }

        public Board(string fen)
        {
            Pieces = new byte[90];
            _copyBoard(GetBoardFromFenString(fen));
        }

        private void _copyBoard(Board board)
        {
            Side = board.Side;
            Round = board.Round;
            NoCapRound = board.NoCapRound;
            for (int i = 0; i < 90; i++)
            {
                Pieces[i] = board.Pieces[i];
            }
        }

        public void SetComment(string comment) => Comment = comment;

        public void LoadFen(string fen)
        {
            _copyBoard(GetBoardFromFenString(fen));
        }

        public void PrintBoard()
        {
            Console.WriteLine("Side: " + Side);
            Console.WriteLine("Round: " + Round);
            Console.WriteLine("NoCapRound: " + NoCapRound);
            Console.WriteLine("Pieces:");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("+---+---+---+---+---+---+---+---+---+");
                for (int j = 0; j < 9; j++)
                {
                    Console.Write("| " + PieceUtil.PieceToLetter(Pieces[i * 9 + j]) + " ");
                }
                Console.WriteLine($"| {9 - i}");
            }
            Console.WriteLine("+---+---+---+---+---+---+---+---+---+");
            Console.WriteLine("  a   b   c   d   e   f   g   h   i");
        }

        public bool CanMovePiece(byte start, byte end, bool checkPosition = true)
        {
            if (start > 89 || end > 89)
            {
                return false;
            }
            byte p1 = Pieces[start];
            byte p2 = Pieces[end];
            if (p1 == PIECE.Empty || start == end
                || PieceUtil.GetPieceSide(p1) != Side
                || (p2 != PIECE.Empty && PieceUtil.GetPieceSide(p2) == Side))
            {
                return false;
            }
            byte type = PieceUtil.GetPieceType(p1);
            if (checkPosition && !PieceUtil.IsPositionValid(type, end))
            {
                return false;
            }
            bool flag = MoveUtil.CanMovePiece(start, end, Pieces);
            if (flag)
            {
                Board b = new Board(this);
                b.MovePiece(start, end);
                flag = !b.IsChecked(Side);
            }
            return flag;
        }

        public bool CanMovePiece(short move, bool checkPosition = true)
        {
            return CanMovePiece((byte)(move >> 8), (byte)(move & 0xff), checkPosition);
        }

        public bool CanMovePiece(string move, bool checkPosition = true)
        {
            return CanMovePiece(MoveUtil.StringToMove(move), checkPosition);
        }

        public void MovePiece(byte start, byte end)
        {
            byte p1 = Pieces[start];
            byte p2 = Pieces[end];
            Pieces[start] = PIECE.Empty;
            Pieces[end] = p1;
            Side = (byte)(1 - Side);
            if (Side == SIDE.Red)
            {
                Round++;
            }
            if (p2 != PIECE.Empty)
            {
                NoCapRound = 0;
            }
            else
            {
                NoCapRound++;
            }
        }

        public void MovePiece(short move)
        {
            MovePiece((byte)(move >> 8), (byte)(move & 0xff));
        }

        public void MovePiece(string move)
        {
            MovePiece(MoveUtil.StringToMove(move));
        }

        /// <summary>
        /// 是否将军
        /// </summary>
        /// <param name="side">哪一方被将军</param>
        /// <returns></returns>
        public bool IsChecked(int side = -1)
        {
            side = side == -1 ? Side : side;
            byte kingPos = 0;
            for (; kingPos < 90; kingPos++)
            {
                byte piece = Pieces[kingPos];
                if (PieceUtil.GetPieceType(piece) == PIECE_TYPE.King && PieceUtil.GetPieceSide(piece) == side)
                {
                    break;
                }
            }
            for (byte i = 0; i < 90; i++)
            {
                if (Pieces[i] != PIECE.Empty && PieceUtil.GetPieceSide(Pieces[i]) != side)
                {
                    if (MoveUtil.CanMovePiece(i, kingPos, Pieces))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 获取所有棋子位置
        /// </summary>
        /// <param name="side">哪一方</param>
        /// <returns></returns>
        public List<byte> GetAllPiecePosition(int side = -1)
        {
            List<byte> pieces = new List<byte>();
            for (byte i = 0; i < 90; i++)
            {
                if (Pieces[i] != PIECE.Empty && (side == -1 || PieceUtil.GetPieceSide(Pieces[i]) == side))
                {
                    pieces.Add(i);
                }
            }
            return pieces;
        }

        
        /// <summary>
        /// 获取所有合法移动
        /// </summary>
        /// <returns></returns>
        public List<short> GetAllMoves()
        {
            List<short> moves = new List<short>();
            List<byte> pieces = GetAllPiecePosition(Side);
            foreach (byte position in pieces)
            {
                byte p = Pieces[position];
                byte type = PieceUtil.GetPieceType(p);
                for (int i = 0; i < 8; i++)
                {
                    // 使用扩展棋盘(13x14)便于判断出界
                    int offset = PieceMove.ExpandedDirection[type - 1, i];
                    if (offset == 0) { break; }
                    int expandedPosition = position / 9 * 13 + position % 9 + 28;
                    int end = expandedPosition;
                    for (int j = 0; j < PieceMove.MaxStep[type - 1]; j++)
                    {
                        end += offset;
                        if (PieceMove.ExpandedBoard[end] == 0) { break; }
                        short move = (short)((position << 8) | (end / 13 * 9 + end % 13 - 20));
                        if (CanMovePiece(move))
                        {
                            moves.Add(move);
                        }
                    }
                }
            }
            return moves;
        }

        /// <summary>
        /// 生成所有合法走法(考虑将军)
        /// </summary>
        /// <returns></returns>
        public List<short> GetAllLegalMoves()
        {
            List<short> moves = GetAllMoves();
            List<short> legalMoves = new List<short>();
            foreach (short move in moves)
            {
                Board b = new Board(this);
                b.MovePiece(move);
                if (!b.IsChecked(Side))
                {
                    legalMoves.Add(move);
                }
            }
            return legalMoves;
        }

        public bool IsCurrentSideLose()
        {
            return GetAllLegalMoves().Count == 0;
        }

        public bool IsDraw()
        {
            // (1)双方都没有可进攻子力; (2)60回合未吃子或300回合
            bool flag = true;
            for (byte i = 0; i < 90; i++)
            {
                if (PieceUtil.GetPieceType(Pieces[i]) >= PIECE_TYPE.Rook)
                {
                    flag = false;
                    break;
                }
            }
            flag = flag && !IsChecked(1 - Side);
            return flag || NoCapRound >= MAX_NO_CAP_ROUND || Round >= MAX_ROUND;
        }

    }
}

/// <summary>
/// Board棋盘类的静态方法
/// </summary>

namespace Xiangqi
{
    public partial class Board
    {
        
        // 最大回合数
        public static int MAX_ROUND = 300;
        // 最大未吃子回合数
        public static int MAX_NO_CAP_ROUND = 60;
        // 初始局面FEN
        public static readonly string INIT_FEN = "rnbakabnr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNBAKABNR w - - 0 1";
        public static readonly string[] INIT_MOVES = new string[] { "h2d2", "h2e2", "h2f2", "b2d2", "b2e2", "b2f2", "g0e2", "c0e2", "c3c4", "g3g4", "b0c2", "h0g2" };

        public static Board GetBoardFromFenString(string fen)
        {
            // https://www.xqbase.com/protocol/cchess_fen.htm
            // rnbakabnr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNBAKABNR w - - 0 1
            Board board = new Board();
            string[] fenArray = fen.Split(' ');
            if (fenArray.Length != 6)
            {
                return board;
            }
            string[] fenPieces = fenArray[0].Split('/');
            if (fenPieces.Length != 10)
            {
                return board;
            }
            int index = 0;
            foreach (string fenPiece in fenPieces)
            {
                int oldIndex = index;
                foreach (char letter in fenPiece)
                {
                    if (letter >= '1' && letter <= '9')
                    {
                        index += letter - '0';
                    }
                    else
                    {
                        board.Pieces[index] = PieceUtil.LetterToPiece(letter);
                        index++;
                    }
                }
                if (index - oldIndex != 9)
                {
                    return board;
                }
            }
            board.Side = fenArray[1] == "b" ? SIDE.Black : SIDE.Red;
            try
            {
                // fen[2]和fen[3]是王车易位和吃过路兵, 在中国象棋中不会出现, 所以不处理
                board.NoCapRound = int.Parse(fenArray[4]);
                board.Round = int.Parse(fenArray[5]);
            }
            catch
            {
                return board;
            }
            return board;
        }

        public static string GetFenStringFromBoard(Board board)
        {
            string fen = "";
            int emptyCount = 0;
            for (int i = 0; i < 90; i++)
            {
                if (i % 9 == 0 && i != 0)
                {
                    if (emptyCount != 0)
                    {
                        fen += emptyCount.ToString();
                        emptyCount = 0;
                    }
                    fen += "/";
                }
                if (board.Pieces[i] == PIECE.Empty)
                {
                    emptyCount++;
                }
                else
                {
                    if (emptyCount != 0)
                    {
                        fen += emptyCount.ToString();
                        emptyCount = 0;
                    }
                    fen += PieceUtil.PieceToLetter(board.Pieces[i]);
                }
            }
            fen += board.Side == SIDE.Red ? " w " : " b ";
            fen += "- - ";
            fen += board.NoCapRound.ToString() + " ";
            fen += board.Round.ToString();
            return fen;
        }
    }
}
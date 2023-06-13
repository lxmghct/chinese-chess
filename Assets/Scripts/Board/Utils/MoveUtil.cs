
namespace Xiangqi
{
    public static class MoveUtil
    {
        
        private static bool canMoveKing(byte start, byte end, byte[] pieces)
        {
            if (PieceUtil.GetPieceType(pieces[end]) == PIECE_TYPE.King && pieces[start] != pieces[end])
            {
                // 将帅照面
                return canMoveStraight(start, end, 0, pieces);
            }
            switch (end - start)
            {
                case 1:
                case -1:
                case 9:
                case -9:
                    return true;
                default:
                    return false;
            }
        }

        private static bool canMoveAdvisor(byte start, byte end)
        {
            switch (end - start)
            {
                case 8:
                case -8:
                case 10:
                case -10:
                    return true;
                default:
                    return false;
            }
        }

        private static bool canMoveBishop(byte start, byte end, byte[] pieces)
        {
            switch (end - start)
            {
                case 20:
                case -20:
                case 16:
                case -16:
                    return pieces[(start + end) >> 1] == PIECE.Empty;
                default:
                    return false;
            }
        }

        private static bool canMoveKnight(byte start, byte end, byte[] pieces)
        {
            int offset;
            switch (end - start)
            {
                case 19:
                case 17:
                    offset = 9; break;
                case -19:
                case -17:
                    offset = -9; break;
                case 11:
                case -7:
                    offset = 1; break;
                case -11:
                case 7:
                    offset = -1; break;
                default:
                    return false;
            }
            return pieces[start + offset] == PIECE.Empty;
        }

        private static bool canMoveStraight(byte start, byte end, byte moveType, byte[] pieces)
        {
            int dire;
            if (PieceUtil.IsSameRow(start, end))
            {
                dire = end > start ? 1 : -1;
            }
            else if (PieceUtil.IsSameCollumn(start, end))
            {
                dire = end > start ? 9 : -9;
            }
            else
            {
                return false;
            }
            int tempPos1 = start + dire;
            for (; pieces[tempPos1] == PIECE.Empty && tempPos1 != end; tempPos1 += dire);
            // 车
            if (moveType == 0)
            {
                return tempPos1 == end;
            }
            // 炮
            if (pieces[tempPos1] == PIECE.Empty) // 正常直走
            {
                return true;
            }
            else // 吃子
            {
                if (pieces[end] == PIECE.Empty)
                {
                    return false;
                }
                int tempPos2 = end - dire;
                for (; pieces[tempPos2] == PIECE.Empty && tempPos2 != start; tempPos2 -= dire);
                return tempPos1 == tempPos2;
            }
        }

        private static bool canMovePawn(byte start, byte end, byte piece)
        {
            byte side = PieceUtil.GetPieceSide(piece);
            switch (end - start)
            {
                case 9:
                    return side == SIDE.Black;
                case -9:
                    return side == SIDE.Red;
                case 1:
                case -1:
                    return PieceUtil.IsCrossHalf(start, side);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 判断是否可以移动棋子(在不考虑位置合法性的情况下)
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">目标位置</param>
        /// <param name="pieces">棋盘</param>
        /// <returns></returns>
        public static bool CanMovePiece(byte start, byte end, byte[] pieces)
        {
            byte piece = pieces[start];
            switch (PieceUtil.GetPieceType(piece))
            {
                case PIECE_TYPE.King:
                    return MoveUtil.canMoveKing(start, end, pieces);
                case PIECE_TYPE.Advisor:
                    return MoveUtil.canMoveAdvisor(start, end);
                case PIECE_TYPE.Bishop:
                    return MoveUtil.canMoveBishop(start, end, pieces);
                case PIECE_TYPE.Knight:
                    return MoveUtil.canMoveKnight(start, end, pieces);
                case PIECE_TYPE.Rook:
                    return MoveUtil.canMoveStraight(start, end, 0, pieces);
                case PIECE_TYPE.Cannon:
                    return MoveUtil.canMoveStraight(start, end, 1, pieces);
                case PIECE_TYPE.Pawn:
                    return MoveUtil.canMovePawn(start, end, piece);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 将位置转换为字符串
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns></returns>
        public static string PositionToString(byte position)
        {
            // 从左到右: a-i; 从上到下: 9-0
            return $"{(char)(position % 9 + 'a')}{9 - position / 9}";
        }

        /// <summary>
        /// 将字符串转换为位置
        /// </summary>
        /// <param name="position">字符串</param>
        /// <returns></returns>
        public static byte StringToPosition(string position)
        {
            if (position.Length != 2 || position[0] < 'a' || position[0] > 'i' || position[1] < '0' || position[1] > '9')
            {
                return 0;
            }
            return (byte)((9 - (position[1] - '0')) * 9 + (position[0] - 'a'));
        }
        

        /// <summary>
        /// 将移动转换为字符串
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="end">目标位置</param>
        /// <returns></returns>
        public static string MoveToString(byte start, byte end)
        {
            return $"{PositionToString(start)}{PositionToString(end)}";
        }

        public static string MoveToString(short move)
        {
            return MoveToString((byte)(move >> 8), (byte)(move & 0xFF));
        }

        /// <summary>
        /// 将字符串转换为移动
        /// </summary>
        /// <param name="move">字符串</param>
        /// <returns></returns>
        public static short StringToMove(string move)
        {
            if (move.Length != 4)
            {
                return 0;
            }
            return (short)((StringToPosition(move.Substring(0, 2)) << 8) | StringToPosition(move.Substring(2, 2)));
        }

        // <summary>
        /// 转化为左右镜像移动
        /// </summary>
        /// <param name="move">移动</param>
        /// <returns></returns>
        public static short MirrorMove(short move)
        {
            if (move == 0)
            {
                return 0;
            }
            int start = move >> 8;
            int end = move & 0xFF;
            start = start / 9 * 9 + 8 - start % 9;
            end = end / 9 * 9 + 8 - end % 9;
            return (short)((start << 8) | end);
        }
        
    }
}

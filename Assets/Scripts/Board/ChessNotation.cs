/// <summary>
/// 棋谱类
/// </summary>

using System;
using System.IO;
using System.Collections.Generic;
namespace Xiangqi
{
    public class NotationNode
    {
        #nullable enable
        public NotationNode? Pre = null;
        public List<NotationNode> Next = new List<NotationNode>();
        public List<short> Moves = new List<short>();
        public Board Board;
        public int Choice = 0;

        public NotationNode()
        {
            Board = new Board();
        }

        public NotationNode(Board board)
        {
            this.Board = new Board(board);
        }

        public int GetMoveCount() => Moves.Count;
    }

    public partial class ChessNotation
    {
        public NotationNode Root;
        public NotationNode Current;
        public PgnInfo PgnInfo = new PgnInfo();

        public ChessNotation(NotationNode root)
        {
            this.Root = root;
            Current = Root;
        }

        public ChessNotation(string fen = "")
        {
            if (fen == "") { fen = Board.INIT_FEN; }
            Root = new NotationNode(new Board(fen));
            Current = Root;
        }

        public void InitBoard(Board board)
        {
            Root = new NotationNode(board);
            Current = Root;
        }

        private void _addMove(short move, string comment = "")
        {
            Board b = new Board(Current.Board);
            b.SetComment(comment);
            b.MovePiece(move);
            NotationNode node = new NotationNode(b);
            node.Pre = Current;
            Current.Next.Add(node);
            Current.Moves.Add(move);
            Current.Choice = Current.Next.Count - 1;
        }

        public void AddMove(short move, string comment = "")
        {
            for (int i = 0; i < Current.Moves.Count; i++)
            {
                if (Current.Moves[i] == move)
                {
                    Current.Choice = i;
                    return;
                }
            }
            _addMove(move);
        }

        public void MovePiece(short move)
        {
            AddMove(move);
            GoNext();
        }

        public void SetCurrentComment(string comment)
        {
            Current.Board.SetComment(comment);
        }

        public void PopCurrentNode()
        {
            if (Current.Pre != null)
            {
                NotationNode old = Current;
                Current = Current.Pre;
                for (int i = 0; i < Current.Next.Count; i++)
                {
                    if (Current.Next[i] == old)
                    {
                        Current.Next.RemoveAt(i);
                        Current.Moves.RemoveAt(i);
                        Current.Choice = 0;
                        break;
                    }
                }
            }
        }

        public NotationNode? GetByIndex(int index)
        {
            if (index < 0) { return null; }
            NotationNode node = Root;
            for (int i = 0; i < index; i++)
            {
                if (node.Next.Count == 0) { return null; }
                node = node.Next[node.Choice];
            }
            return node;
        }

        public int GetNodeIndex(NotationNode node)
        {
            int index = 0;
            NotationNode n = Root;
            while (n != node)
            {
                if (n.Next.Count == 0) { return -1; }
                n = n.Next[n.Choice];
                index++;
            }
            return index;
        }

        public int GetCurrentIndex() => GetNodeIndex(Current);

        public int GetNotationNodeCount()
        {
            int count = 1;
            NotationNode node = Root;
            while (node.Next.Count != 0)
            {
                node = node.Next[node.Choice];
                count++;
            }
            return count;
        }

        public void ChangeChoice(int choice)
        {
            if (choice < 0 || choice >= Current.Next.Count) { return; }
            Current.Choice = choice;
        }

        public void GoPre()
        {
            if (Current.Pre != null)
            {
                Current = Current.Pre;
            }
        }

        public void GoNext(int choice = -1)
        {
            choice = choice < 0 ? Current.Choice : choice;
            if (Current.Next.Count > choice)
            {
                Current = Current.Next[choice];
            }
        }

        public void GoTo(int index)
        {
            if (index < 0 || index >= GetNotationNodeCount()) { return; }
            Current = GetByIndex(index)!;
        }

        public void PrintNotation()
        {
            NotationNode node = Root;
            while (node.Next.Count != 0)
            {
                Console.Write(ChessNotationUtil.MoveToChineseNotation(node.Moves[node.Choice], node.Board.Pieces));
                Console.Write(node.Board.Side == 0 ? " " : "\n");
                node = node.Next[node.Choice];
            }
            Console.WriteLine();
        }

        public void LoadPgnFile(string path)
        {
            PgnUtil.LoadPgnFile(path, this);
        }

        public void SavePgnFile(string path, bool saveVariations = true)
        {
            string pgn = "";
            PgnUtil.NotationToPgnData(this, ref pgn, saveVariations);
            File.WriteAllText(path, pgn);
        }

        public short GetLastMove()
        {
            if (Current.Pre == null) { return 0; }
            return Current.Pre.Moves[Current.Pre.Choice];
        }

    }
}

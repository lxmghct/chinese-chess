
/// <summary>
/// Pgn格式棋谱处理工具
/// 参考: https://www.xqbase.com/protocol/cchess_pgn.htm
/// </summary>

/* PGN格式棋谱示例

[Game "Chinese Chess"]
[Event "许银川让九子对聂棋圣"]
[Site "广州"]
[Date "1999.12.09"]
[Red "许银川"]
[Black "聂卫平"]
[Result "1-0"]
[FEN "rnbakabnr/9/1c5c1/p1p1p1p1p/9/9/9/1C5C1/9/RN2K2NR r - - 0 1"]
{　　评注：许银川
　　象棋让九子原属茶余饭后的娱乐，不意今日却被摆上赛桌，更为离奇的是：我的对手竟是在围棋棋坛上叱咤风云的聂大帅。赛前我并不了解对手的实力，但相信以聂棋圣在围棋上所体现出来的过人智慧，必能在棋理上触类旁通。因此我在赛前也作了一些准备，在对局中更是小心翼翼，不敢掉以轻心。
　　许银川让去５只兵和双士双相，执红先行。棋盘如右图所示。当然，PGN文件里是无法嵌入图片的。}

1. 炮八平五 炮８平５
{　　红方首着架中炮必走之着，聂棋圣还架中炮拼兑子力，战术对头。}
2. 炮五进五 象７进５ 3. 炮二平五
{　　再架中炮也属正着，如改走马八进七，则象５退７，红方帅府受攻，当然若红方仍再架中炮拼兑，那么失去双炮就难有作用了。}
马８进７ 4. 马二进三 车９平８ 5. 马八进七 马２进１ 6. 车九平六 车１平２
{　　聂棋圣仍按常规战法出动主力，却忽略了红方车塞象眼的凶着，应走车１进１。}
7. 车六进八
{　　红车疾点象眼，局势霎时有剑拔弩张之感。这种对弈不能以常理揣度，红方只能像程咬金的三板斧一般猛攻一轮，若黑方防守得法则胜负立判。}
炮２进７
{　　却说聂棋圣见我来势汹汹，神色顿时颇为凝重，一番思索之后沉下底炮以攻为守，果是身手不凡。此着如改走炮２平３，则帅五平六，炮３进５，车六进一，将５进１，炮五退二，黑方不易驾驭局面。}
8. 车一进四 炮２平１ 9. 马七进八 炮１退４ 10. 马八退七 炮１进４ 11. 马七进八 车２进２
{　　其实黑方仍可走炮１退４，红方若续走马八退七，则仍炮１进４不变作和，因黑右车叫将红可车六退九，故不算犯规。}
12. 炮五平八 炮１退４
{　　劣着，导致失子，应走车２平３，红方如马八进六，则车３退１，红方无从着手。但有一点必须注意，黑车躲进暗道似与棋理相悖，故聂棋圣弃子以求局势缓和情有可原。}
13. 炮八进五 炮１平９ 14. 炮八平三 车８进２ 15. 炮三进一 车８进２ 16. 马八进六 炮９平５
17. 炮三平一 士６进５ 18. 马六进四 车８平５ 19. 帅五平六
{　　可直接走马四进三叫将再踩中象。}
车５平６ 20. 马四进三 将５平６ 21. 车六退四 卒５进１ 22. 车六进二 炮５平７
23. 前马退二 象５进７ 24. 马二退三 卒５进１ 25. 车六平三 卒５平６ 26. 车三进三 将６进１
27. 后马进二 士５进６ 28. 马二进三 将６平５ 29. 前马进二
{　　红方有些拖沓，应直接走车三平六立成绝杀。}
将５进１ 30. 车三平六 士６退５ 31. 马二退三 车６退１ 32. 车六退三
{　　再擒一车，以下着法仅是聊尽人事而已。}
车６平７ 33. 车六平三 卒６平７ 34. 车三平五 将５平６ 35. 帅六平五 将６退１
36. 车五进二 将６退１ 37. 车五进一 将６进１ 38. 车五平七
{　　至此，聂棋圣认负。与此同时，另一盘围棋对弈我被屠去一条大龙，已无力再战，遂平分秋色，皆大欢喜。}
1-0
*/
using System.Text;
using System;
using System.IO;
using System.Collections.Generic;

namespace Xiangqi
{
    public class PgnInfo
    {
        public Dictionary<string, string> Info = new Dictionary<string, string>()
        {
            {"Game", "Chinese Chess"},      // 游戏类型，国际象棋没有这个标签，中国象棋的PGN文件中这个标签必须放在第一位，其值必须为“Chinese Chess”
            {"Event", ""},                  // 比赛名
            {"Site", ""},                   // 比赛地点
            {"Date", ""},                   // 比赛日期, yyyy.mm.dd
            {"Round", ""},                  // 比赛轮次
            {"RedTeam", ""},                // 红方代表队
            {"Red", ""},                    // 红方棋手
            {"BlackTeam", ""},              // 黑方代表队
            {"Black", ""},                  // 黑方棋手
            {"Result", ""},                 // 比赛结果, 1-0: 红胜, 0-1: 黑胜, 1/2-1/2: 和棋, *: 未知
            {"FEN", ""},                    // Fen 开始局面
            {"Opening", ""},                // 开局名称
            {"Variation", ""},              // 变例
            {"ECCO", ""},                   // ECCO编号
            {"Format", "Chinese"}           // 记谱方式, Chinese(中文纵线格式)、WXF(WXF纵线格式)、ICCS(ICCS坐标格式)
        };

        public string GetPgnInfoString()
        {
            string pgnInfo = "";
            foreach (KeyValuePair<string, string> kv in Info)
            {
                if (kv.Value == "") { continue; }
                pgnInfo += $"[{kv.Key} \"{kv.Value}\"]\n";
            }
            return pgnInfo;
        }
    }

    public class PgnUtil
    {
        public static string SplitChar = " \t\r\n";

        public static Dictionary<string, string> ValidCharset = new Dictionary<string, string>()
        {
            {"Chinese", "帅将士仕相象车马炮兵卒进退平前中后.一二三四五六七八九１２３４５６７８９123456789帥將車馬進後"},
            {"WXF", "KARBCNPkarnbcp123456789+-."},
            {"ICCS", "abcdefghi0123456789."}
        };

        public static string PgnWrongFormatMessage = "PGN格式错误";

        public static void LoadPgnFile(string path, ChessNotation notation)
        {
            if (!File.Exists(path))
            {
                throw new Exception("文件不存在");
            }
            string pgn = File.ReadAllText(path);
            notation.InitBoard(new Board("rnbakabnr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNBAKABNR w - - 0 1"));
            LoadPgn(pgn, notation);
        }

        public static void LoadPgn(string pgn, ChessNotation notation)
        {
            int i = 0;
            pgn += "\n#";
            int len = pgn.Length;
            string moveFormat = notation.PgnInfo.Info["Format"];
            while (i < len)
            {
                char c = pgn[i];
                if (SplitChar.Contains(c)) { }
                else if (c == '[')
                {
                    i = _loadPgnInfo(ref pgn, i, notation, ref moveFormat);
                }
                else
                {
                    _loadPgnMove(ref pgn, i, len, notation, ref moveFormat);
                    break;
                }
                i++;
            }
        }

        public static void NotationToPgnData(ChessNotation notation, ref string pgn, bool variation = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(notation.PgnInfo.GetPgnInfoString());
            NotationNode node = notation.Root;
            if (node.Board.Comment != "")
            {
                sb.Append($"{{{node.Board.Comment}}}\n");
            }
            _notationToPgnData(node, sb, 0, "", variation);
            pgn = sb.ToString();
        }

        private static int _loadPgnInfo(ref string pgn, int i, ChessNotation notation, ref string moveFormat)
        {
            while (SplitChar.Contains(pgn[++i])) ;
            int start = i;
            while (!SplitChar.Contains(pgn[++i])) ;
            string key = pgn.Substring(start, i - start);
            while (SplitChar.Contains(pgn[++i])) ;
            if (pgn[i] != '"')
            {
                throw new Exception(PgnWrongFormatMessage);
            }
            start = ++i;
            while (pgn[++i] != '"') ;
            if (i >= pgn.Length)
            {
                throw new Exception(PgnWrongFormatMessage);
            }
            string value = pgn.Substring(start, i - start);
            if (key == "Format")
            {
                if (value != "Chinese" && value != "WXF" && value != "ICCS")
                {
                    value = "Chinese";
                }
                moveFormat = value;
            }
            else if (key == "FEN")
            {
                notation.InitBoard(new Board(value));
            }
            notation.PgnInfo.Info[key] = value;
            while (SplitChar.Contains(pgn[++i])) ;
            if (pgn[i] != ']')
            {
                throw new Exception(PgnWrongFormatMessage);
            }
            return i;
        }

        private static int _loadPgnComment(ref string pgn, int i, ChessNotation notation)
        {
            int start = i;
            while (pgn[++i] != '}') ;
            if (i >= pgn.Length)
            {
                throw new Exception(PgnWrongFormatMessage);
            }
            string comment = pgn.Substring(start, i - start);
            notation.Current.Board.SetComment(comment);
            return i;
        }

        private static void _loadPgnMove(ref string pgn, int start, int end, ChessNotation notation, ref string moveFormat)
        {
            int i = start;
            while (i < end)
            {
                char c = pgn[i];
                if (SplitChar.Contains(c)) { }
                else if (char.IsDigit(c))
                {
                    string temp = pgn.Substring(i, Math.Min(7, end - i));
                    if (temp.StartsWith("1-0") || temp.StartsWith("0-1") || temp.StartsWith("1/2-1/2"))
                    {
                        notation.PgnInfo.Info["Result"] = temp[1] == '-' ? temp.Substring(0, 3) : temp.Substring(0, 7);
                        break;
                    }
                    while (char.IsDigit(pgn[++i])) ;
                    if (pgn[i] != '.' || !SplitChar.Contains(pgn[i + 1]))
                    {
                        throw new Exception(PgnWrongFormatMessage);
                    }
                }
                else if (c == '{')
                {
                    int j = i + 1;
                    while (pgn[++i] != '}') ;
                    notation.SetCurrentComment(pgn.Substring(j, i - j));
                }
                else if (c == '(')
                {
                    int leftCount = 1, j = i + 1;
                    while (leftCount > 0)
                    {
                        c = pgn[++i];
                        if (c == '(') { leftCount++; }
                        else if (c == ')') { leftCount--; }
                    }
                    if (leftCount > 0)
                    {
                        throw new Exception(PgnWrongFormatMessage);
                    }
                    NotationNode temp = notation.Current;
                    notation.GoPre();
                    if (pgn.Substring(j, i - j).TrimStart().StartsWith("..."))
                    {
                        notation.GoPre();
                    }
                    _loadPgnMove(ref pgn, j, i - 1, notation, ref moveFormat);
                    notation.Current = temp;
                    temp.Pre!.Choice = 0;
                }
                else if (ValidCharset[moveFormat].Contains(c))
                {
                    i = _loadPgnSingleMove(ref pgn, i, notation, ref moveFormat);
                    if (i == -1) { break; }
                }
                else if (c == '#') { break; }
                else
                {
                    throw new Exception(PgnWrongFormatMessage);
                }
                i++;
            }
        }

        private static int _loadPgnSingleMove(ref string pgn, int i, ChessNotation notation, ref string moveFormat)
        {
            int start = i;
            while (ValidCharset[moveFormat].Contains(pgn[++i])) ;
            string moveString = pgn.Substring(start, i - start);
            if (!SplitChar.Contains(pgn[i]))
            {
                throw new Exception(PgnWrongFormatMessage);
            }
            if (moveString == "...") { notation.GoNext(); }
            else if (moveString == "1-0" || moveString == "0-1" || moveString == "1/2-1/2")
            {
                notation.PgnInfo.Info["Result"] = moveString;
                return -1;
            }
            else if (moveString.Length != 4)
            {
                throw new Exception(PgnWrongFormatMessage);
            }

            short move = 0;
            if (moveFormat == "WXF")
            {
                move = ChessNotationUtil.WXFNotationToMove(moveString, notation.Current.Board.Pieces);
            }
            else if (moveFormat == "ICCS")
            {
                move = ChessNotationUtil.ICCSNotationToMove(moveString, notation.Current.Board.Pieces);
            }
            else
            {
                move = ChessNotationUtil.ChineseNotationToMove(moveString, notation.Current.Board.Pieces);
            }
            if (!notation.Current.Board.CanMovePiece(move))
            {
                return -1;
            }
            notation.AddMove(move);
            notation.GoNext();
            return i;
        }

        private static void _notationToPgnData(NotationNode node, StringBuilder sb, int choice = 0, string indentation = "", bool variation = false)
        {
            while (node.Next.Count != 0)
            {
                sb.Append(node.Board.Side == SIDE.Red ? $"{indentation}{node.Board.Round}. " : " ");
                bool flag = !variation || node.Next.Count == 1;
                choice = flag ? node.Choice : choice;
                sb.Append(ChessNotationUtil.MoveToChineseNotation(node.Moves[choice], node.Board.Pieces));
                string comment = node.Next[choice].Board.Comment;
                if (comment != "") { sb.Append($" {{{comment}}}"); }
                if (flag || choice > 0)
                {
                    node = node.Next[choice];
                    sb.Append(node.Board.Side == SIDE.Red ? "\n" : " ");
                }
                else
                {
                    for (int i = 1; i < node.Next.Count; i++)
                    {
                        sb.Append($"\n{indentation}(\n");
                        if (node.Board.Side == SIDE.Black) { sb.Append($"{indentation}\t{node.Board.Round}. ... "); }
                        _notationToPgnData(node, sb, i, indentation + "\t", variation);
                        // 如果前一个不是换行符，就加一个换行符
                        if (sb[sb.Length - 1] != '\n') { sb.Append("\n"); }
                        sb.Append($"{indentation})");
                    }
                    node = node.Next[0];
                    sb.Append("\n");
                }
                choice = node.Choice;
            }
        }
    }
}
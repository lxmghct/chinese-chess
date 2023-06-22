/// <summary>
/// Computer move
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using Xiangqi;
using System.Runtime.InteropServices;

public class ComputerMove : MonoBehaviour
{
    private bool waitingForMove = false;
    private static bool engineLoaded = false;

    private BoardUI boardObject;
    private short bestmove = 0;
    private string boardScore = "";
    private string engineInfo = "";
    private Color engineInfoColor = Color.black;
    private Text engineScoreText;
    private Text engineInfoText;
    private static string engineOutput = "";

    private int hashSize = 16;
    private int threadCount = 1;
    void Start()
    {
        startEngine();
    }

    void Update()
    {
        if (engineOutput != "")
        {
            processEngineOutput();
            engineOutput = "";
        }
        if (bestmove != 0)
        {
            boardObject.MovePiece(bestmove);
            bestmove = 0;
            engineInfo = " ";
        }
        if (boardScore != "")
        {
            if (engineScoreText == null)
            {
                engineScoreText = GameObject.Find("Text-EngineScore").GetComponent<Text>();
            }
            if (GlobalConfig.Configs["ShowScore"] == "false")
            {
                engineScoreText.text = "";
                return;
            }
            try
            {
                float score = float.Parse(boardScore);
                if (score >= 0)
                {
                    engineScoreText.text = "红优: " + score + "分";
                    engineScoreText.color = Color.red;
                }
                else
                {
                    engineScoreText.text = "黑优: " + (-score) + "分";
                    engineScoreText.color = Color.black;
                }
            }
            catch (Exception)
            {
                Debug.Log("Invalid score: " + boardScore);
            }
            boardScore = "";
        }
        if (engineInfo != "")
        {
            if (engineInfoText == null)
            {
                engineInfoText = GameObject.Find("Text-EngineInfo").GetComponent<Text>();
            }
            engineInfoText.text = engineInfo;
            engineInfoText.color = engineInfoColor;
            engineInfo = "";
        }
    }

    void OnDestroy()
    {
        try
        {
            CloseEngine();
        }
        catch (Exception)
        {
            Debug.Log("Engine process has been closed");
        }
    }

    private void startEngine()
    {
        runEngineThread();
        writeToEngine("uci");
    }

    public delegate void ReadOutputDelegate(string output);
    
    #if UNITY_EDITOR
    [DllImport("D:/projects/unity/chinese-chess/Assets/Plugins/Engine/pikafish.dll")]
    #elif UNITY_STANDALONE_WIN
    [DllImport("Engine/pikafish")]
    #elif UNITY_ANDROID
    [DllImport("Engine/libpikafish")]
    #else
    [DllImport("Engine/pikafish")]
    #endif
    public static extern void RunEngine(string evalFile, ReadOutputDelegate callback);

    #if UNITY_EDITOR
    [DllImport("D:/projects/unity/chinese-chess/Assets/Plugins/Engine/pikafish.dll")]
    #elif UNITY_STANDALONE_WIN
    [DllImport("Engine/pikafish")]
    #elif UNITY_ANDROID
    [DllImport("Engine/libpikafish")]
    #else
    [DllImport("Engine/pikafish")]
    #endif
    public static extern void WriteCommand(string command);

    private void writeToEngine(string command)
    {
        if (!engineLoaded) { return; }
        Debug.Log(command);
        WriteCommand(command);
    }

    private static void runEngineThread()
    {
        RunEngine("D:/projects/unity/chinese-chess/Assets/Plugins/Engine/pikafish.nnue", readOutputFromEngine);
        engineLoaded = true;
    }

    private static void readOutputFromEngine(string output)
    {
        Debug.Log(output);
        engineOutput = output;
    }

    private void processEngineOutput()
    {
        string[] lines = engineOutput.Split('\n');
        foreach (string line in lines)
        {
            if (line.StartsWith("Final evaluation"))
            {
                // 局面评分, 格式: Final evaluation       +1.21 (white side) [with scaled NNUE, optimism, ...]
                int index = "Final evaluation".Length;
                while (line[++index] == ' ') { }
                int start = index;
                while (line[++index] != ' ') { }
                boardScore = line.Substring(start, index - start);
            }
            else if (line.StartsWith("bestmove"))
            {
                if (!waitingForMove) { return; }
                waitingForMove = false;
                // 最佳着法, 格式: bestmove e2e4 ponder e7e5
                int index = "bestmove".Length;
                while (line[++index] == ' ') { }
                string move = line.Substring(index, 4);
                bestmove = MoveUtil.StringToMove(move);
            }
        }
    }

    public void UpdateEngineConfig()
    {
        int oldHashSize = hashSize, oldThreadCount = threadCount;
        hashSize = int.Parse(GlobalConfig.Configs["HashSize"]);
        threadCount = int.Parse(GlobalConfig.Configs["ThreadCount"]);
        if (hashSize != oldHashSize)
        {
            writeToEngine($"setoption name Hash value {hashSize}");
        }
        if (threadCount != oldThreadCount)
        {
            writeToEngine($"setoption name Threads value {threadCount}");
        }
    }

    public void Think()
    {
        UpdateEngineConfig();
        if (boardObject == null)
        {
            boardObject = GameObject.Find("Img-Board").GetComponent<BoardUI>();
        }
        ChessNotation notation = boardObject.GetNotation();
        NotationNode node = notation.Current;
        // 向前搜索找到最近一个吃子的着法
        while (node.Pre != null && node.Pre.Board.NoCapRound != 0)
        {
            node = node.Pre;
        }
        StringBuilder command = new StringBuilder();
        string fen = Board.GetFenStringFromBoard(node.Board);
        if (node == notation.Current && fen == Board.INIT_FEN)
        {
            // 初始局面随机走法
            int index = UnityEngine.Random.Range(0, Board.INIT_MOVES.Length);
            bestmove = MoveUtil.StringToMove(Board.INIT_MOVES[index]);
            return;
        }
        command.Append("position fen ").Append(fen).Append(" moves");
        while (node != notation.Current)
        {
            if (node.Next.Count == 0) { break; }
            short move = node.Moves[node.Choice];
            command.Append(" ").Append(MoveUtil.MoveToString(move));
            node = node.Next[node.Choice];
        }
        int maxDepth = int.Parse(GlobalConfig.Configs["MaxDepth"]);
        int maxTime = int.Parse(GlobalConfig.Configs["MaxTime"]);
        try
        {
            writeToEngine(command.ToString());
            waitingForMove = true;
            writeToEngine($"go depth {maxDepth} movetime {maxTime}");
            engineInfo = "电脑思考中...";
            engineInfoColor = node.Board.Side == SIDE.Red ? Color.red : Color.black;
        }
        catch (Exception)
        {
            engineInfo = "引擎加载失败";
            engineInfoColor = Color.red;
            startEngine();
        }
    }

    public void Evaluate()
    {
        UpdateEngineConfig();
        if (boardObject == null)
        {
            boardObject = GameObject.Find("Img-Board").GetComponent<BoardUI>();
        }
        Board b = boardObject.GetNotation().Current.Board;
        writeToEngine($"position fen {Board.GetFenStringFromBoard(b)}");
        writeToEngine("eval");
    }

    public void StopEngineThinking()
    {
        writeToEngine("stop");
    }

    public void CloseEngine()
    {
        writeToEngine("quit");
    }

}

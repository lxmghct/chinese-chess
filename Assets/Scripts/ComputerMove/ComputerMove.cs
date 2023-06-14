/// <summary>
/// Computer move
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Diagnostics;
using System.Text;
using Xiangqi;

public class ComputerMove : MonoBehaviour
{
    private static string enginePath = "Assets/Resources/Engine/Pikafish";

    private Process engineProcess;
    private Thread outputThread;

    private bool waitingForMove = false;

    private BoardUI boardObject;
    private short bestmove = 0;
    private string boardScore = "";
    private string engineInfo = "";
    private Color engineInfoColor = Color.black;
    private Text engineScoreText;
    private Text engineInfoText;

    private int hashSize = 16;
    private int threadCount = 1;

    void Start()
    {
        runEngine();
    }

    void Update()
    {
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
                UnityEngine.Debug.Log("Invalid score: " + boardScore);
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
            UnityEngine.Debug.Log("Engine process has been closed");
        }
    }

    private void runEngine()
    {
        if (engineProcess != null)
        {
            engineProcess.Close();
        }
        // 创建一个进程对象
        engineProcess = new Process();
        // 设置进程信息
        engineProcess.StartInfo.FileName = enginePath;
        engineProcess.StartInfo.UseShellExecute = false;
        engineProcess.StartInfo.RedirectStandardInput = true;
        engineProcess.StartInfo.RedirectStandardOutput = true;
        engineProcess.StartInfo.CreateNoWindow = true;
        // 启动进程
        engineProcess.Start();
        // 启动线程接收引擎输出
        if (outputThread != null)
        {
            outputThread.Abort();
        }
        outputThread = new Thread(receiveEngineOutput);
        outputThread.Start(engineProcess);
    }

    #nullable enable
    private void receiveEngineOutput(object? obj)
    {
        if (obj == null)
        {
            return;
        }
        Process process = (Process)obj;
        string? output;

        while (!process.StandardOutput.EndOfStream)
        {
            output = process.StandardOutput.ReadLine();
            if (output.StartsWith("Final evaluation"))
            {
                // 局面评分, 格式: Final evaluation       +1.21 (white side) [with scaled NNUE, optimism, ...]
                int index = "Final evaluation".Length;
                while (output[++index] == ' ') { }
                int start = index;
                while (output[++index] != ' ') { }
                boardScore = output.Substring(start, index - start);
            }
            else if (output.StartsWith("bestmove"))
            {
                if (!waitingForMove) { continue; }
                waitingForMove = false;
                // 最佳着法, 格式: bestmove e2e4 ponder e7e5
                int index = "bestmove".Length;
                while (output[++index] == ' ') { }
                string move = output.Substring(index, 4);
                bestmove = MoveUtil.StringToMove(move);
            }
        }
    }

    private void writeToEngine(string command)
    {
        if (engineProcess == null)
        {
            return;
        }
        engineProcess.StandardInput.WriteLine(command);
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
            UnityEngine.Debug.Log("Engine process has been closed");
            engineInfo = "引擎加载失败";
            engineInfoColor = Color.red;
            runEngine();
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
        engineProcess.Close();
    }
    
}

/// <summary>
/// Computer move
/// </summary>

using UnityEngine;
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
                string score = output.Substring(start, index - start);
                UnityEngine.Debug.Log(score);
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
        int hashSize = EngineConfig.HashSize;
        int threadCount = EngineConfig.ThreadCount;
        writeToEngine($"setoption name Hash value {hashSize}");
        writeToEngine($"setoption name Threads value {threadCount}");
    }

    public void Think()
    {
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
        command.Append("position fen ").Append(Board.GetFenStringFromBoard(node.Board)).Append(" moves");
        while (node != notation.Current)
        {
            if (node.Next.Count == 0) { break; }
            short move = node.Moves[node.Choice];
            command.Append(" ").Append(MoveUtil.MoveToString(move));
            node = node.Next[node.Choice];
        }
        int maxDepth = EngineConfig.MaxDepth;
        int maxTime = EngineConfig.MaxTime;
        try
        {
            writeToEngine(command.ToString());
            waitingForMove = true;
            writeToEngine($"go depth {maxDepth} movetime {maxTime}");
        }
        catch (Exception)
        {
            UnityEngine.Debug.Log("Engine process has been closed");
            runEngine();
        }
    }

    public void Evaluate()
    {
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

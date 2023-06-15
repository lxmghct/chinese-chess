using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using Xiangqi;

public partial class BoardUI : MonoBehaviour, IPointerClickHandler
{
    // 静态变量
    public static readonly int rows = 10;
    public static readonly int columns = 9;
    private static readonly byte EMPTY_POSITION = 255;

    // 非静态变量
    public bool isReverseBoard = false; // 是否翻转棋盘
    private ChessNotation notation;
    private byte clickPosition = EMPTY_POSITION;
    private bool canOperate = true;
    private CommentUI commentUI;
    private ComputerMove engine;

    public ChessNotation GetNotation() => notation;
    void Start()
    {
        notation = new ChessNotation();
        LoadResources();
        isReverseBoard = GlobalConfig.Configs["IsBoardReverse"] == "true";
        DrawPieces();
        commentUI = GameObject.Find("Img-Comment").GetComponent<CommentUI>();
        updateChoiceAndComment();
    }

    public void DrawPieces()
    {
        byte[] pieces = notation.Current.Board.Pieces;
        for (int i = 0; i < 90; i++)
        {
            DrawPiece((byte)i, pieces[i]);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canOperate)
        {
            return;
        }
        // 先获取当前组件的左上角坐标
        // 将点击位置从屏幕坐标转换为父元素的本地坐标
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPosition);
        // 处理棋盘点击事件的逻辑
        byte oldPosition = clickPosition;
        clickPosition = coordinateToPosition(localPosition.x, localPosition.y);
        
        if (clickPosition > 89)
        {
            updateChoice(0, EMPTY_POSITION);
            return;
        }
        if (isReverseBoard)
        {
            clickPosition = (byte)(89 - clickPosition);
        }
        Board b = notation.Current.Board;
        if (oldPosition > 89 || b.Pieces[oldPosition] == PIECE.Empty)
        {
            byte piece2 = b.Pieces[clickPosition];
            if (piece2 != PIECE.Empty && PieceUtil.GetPieceSide(piece2) == b.Side)
            {
                updateChoice(0, clickPosition);
                return;
            }
        }
        else if (b.CanMovePiece(oldPosition, clickPosition))
        {
            movePiece(oldPosition, clickPosition);
        }
        updateChoice(0, EMPTY_POSITION);
    }

    private void movePiece(byte start, byte end)
    {
        if (!canOperate) { return; }
        notation.MovePiece((short)(start << 8 | end));
        startMovePieceAnimation(start, end);
        canOperate = false;
        clickPosition = EMPTY_POSITION;
        updateChoiceAndComment();
    }

    private void openMessageBox(string title, string message)
    {
        MessageBox messageBox = GameObject.Find("Canvas").transform.Find("Img-MessageBox").GetComponent<MessageBox>();
        if (messageBox != null)
        {
            messageBox.ShowMessage(title, message); 
        }
    }

    private void judgeResult()
    {
        if (notation.Current.Board.IsCurrentSideLose())
        {
            string result = notation.Current.Board.Side == SIDE.Red ? "黑胜" : "红胜";
            openMessageBox("游戏结束", result);
        }
        else if (notation.Current.Board.IsDraw())
        {
            openMessageBox("游戏结束", "和棋");
        }
    }

    public void MovePiece(short move)
    {
        movePiece((byte)(move >> 8), (byte)(move & 0xff));
    }

    public void GotoNotationIndex(int index)
    {
        if (index < 0 || index >= notation.GetNotationNodeCount()) { return; }
        notation.GoTo(index);
        DrawPieces();
        updateChoiceAndComment();
    }

    public void Withdraw()
    {
        if (!notationGoPre()) { return; }
        notation.GoNext();
        notation.PopCurrentNode();
    }

    public void NotationGoPre()
    {
        notationGoPre();
    }

    public void NotationGoNext(int choice = -1)
    {
        if (notation.Current.Next.Count > 1)
        {
            if (choice == -1)
            {
                // ScrollViewAlter scrollViewAlter = GameObject.Find("ScrollView-Alter").GetComponent<ScrollViewAlter>();
                // scrollView未激活，不能直接获取组件，需要先获取父组件
                ScrollViewAlter scrollViewAlter = GameObject.Find("Canvas").transform.Find("ScrollView-Alter").GetComponent<ScrollViewAlter>();
                scrollViewAlter.SetAlterList(notation.Current);
                return;
            }
            else
            {
                notation.ChangeChoice(choice);
            }
        }
        notationGoNext();
    }

    private bool notationGoPre()
    {
        if (!canOperate || notation.Current.Pre == null) { return false; }
        short currentMove = notation.GetLastMove();
        notation.GoPre();
        canOperate = false;
        byte start = (byte)(currentMove >> 8), end = (byte)(currentMove & 0xff);
        startMovePieceAnimation(end, start);
        updateChoiceAndComment();
        return true;
    }

    private bool notationGoNext()
    {
        if (!canOperate || notation.Current.Next.Count == 0) { return false; }
        notation.GoNext();
        canOperate = false;
        short currentMove = notation.GetLastMove();
        startMovePieceAnimation((byte)(currentMove >> 8), (byte)(currentMove & 0xff));
        updateChoiceAndComment();
        return true;
    }

    private void updateChoiceAndComment()
    {
        short currentMove = notation.GetLastMove();
        if (currentMove == 0)
        {
            updateChoice(1, EMPTY_POSITION);
            updateChoice(2, EMPTY_POSITION);
        }
        else
        {
            updateChoice(1, (byte)(currentMove >> 8));
            updateChoice(2, (byte)(currentMove & 0xff));
        }
        updateChoice(0, EMPTY_POSITION);
        updateCommentInput();
        if (engine == null)
        {
            engine = GameObject.Find("Btn-Computer").GetComponent<ComputerMove>();
        }
        if (GlobalConfig.Configs["ShowScore"] != "false")
        {
            engine.Evaluate();
        }
    }

    public void SaveNotation()
    {
        // 打开保存对话框
        
        string savePath = EditorUtility.SaveFilePanel("Save File", "", "", "pgn");

        if (!string.IsNullOrEmpty(savePath))
        {
            Debug.Log("Selected save path: " + savePath);
            try 
            {
                notation.SavePgnFile(savePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                openMessageBox("Error", "保存棋谱失败");
            }
        }

        GameObject.Find("Img-Menu").SetActive(false);

    }

    public void LoadNotation()
    {
        // 打开打开对话框
        string loadPath = EditorUtility.OpenFilePanel("Open File", "", "pgn");

        if (!string.IsNullOrEmpty(loadPath))
        {
            Debug.Log("Selected load path: " + loadPath);
            try 
            {
                notation.LoadPgnFile(loadPath);
                notation.GoTo(0);
                DrawPieces();
                updateChoiceAndComment();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                openMessageBox("Error", "导入棋谱失败");
            }
        }
        GameObject.Find("Img-Menu").SetActive(false);
    }

    public void SetComment(string comment)
    {
        notation.Current.Board.Comment = comment;
    }

    #nullable enable
    private void updateCommentInput(string? comment = null)
    {
        if (comment == null)
        {
            comment = notation.Current.Board.Comment;
        }
        commentUI.SetComment(comment);
    }

    public void NotationGoStart()
    {
        GotoNotationIndex(0);
    }

    public void NotationGoEnd()
    {
        GotoNotationIndex(notation.GetNotationNodeCount() - 1);
    }

    public void ChangeBoardReverse()
    {
        isReverseBoard = !isReverseBoard;
        DrawPieces();
        updateChoiceAndComment();
        GlobalConfig.Configs["IsBoardReverse"] = isReverseBoard ? "true" : "false";
        GlobalConfig.SaveConfig();
    }
    
}

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
    private ChessNotation notation;
    private byte clickPosition = EMPTY_POSITION;
    private bool canOperate = true;
    private CommentUI commentUI;
    void Start()
    {
        notation = new ChessNotation();
        LoadResources();
        DrawPieces();
        commentUI = GameObject.Find("Img-Comment").GetComponent<CommentUI>();
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
            notation.MovePiece((short)(oldPosition << 8 | clickPosition));
            movePiece(oldPosition, clickPosition);
            canOperate = false;
            updateChoice(1, oldPosition);
            updateChoice(2, clickPosition);
            clickPosition = EMPTY_POSITION;
            updateCommentInput();
        }
        updateChoice(0, EMPTY_POSITION);
    }

    public void GotoNotationIndex(int index)
    {
        if (index < 0 || index >= notation.GetNotationNodeCount()) { return; }
        notation.GoTo(index);
        short move = notation.GetLastMove();
        DrawPieces();
        if (move == 0)
        {
            updateChoice(1, EMPTY_POSITION);
            updateChoice(2, EMPTY_POSITION);
        }
        else
        {
            updateChoice(1, (byte)(move >> 8));
            updateChoice(2, (byte)(move & 0xff));
        }
        updateCommentInput();
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
        movePiece(end, start);
        currentMove = notation.GetLastMove();
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
        updateCommentInput();
        return true;
    }

    private bool notationGoNext()
    {
        if (!canOperate || notation.Current.Next.Count == 0) { return false; }
        notation.GoNext();
        short currentMove = notation.GetLastMove();
        canOperate = false;
        byte start = (byte)(currentMove >> 8), end = (byte)(currentMove & 0xff);
        movePiece(start, end);
        updateChoice(1, start);
        updateChoice(2, end);
        updateCommentInput();
        return true;
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
                UnityEditor.EditorUtility.DisplayDialog("Error", "保存失败", "OK");
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
                for (int i = 0; i < 3; i++)
                {
                    updateChoice(i, EMPTY_POSITION);
                }
                updateCommentInput();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                UnityEditor.EditorUtility.DisplayDialog("Error", "导入棋谱失败", "OK");
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
    
}

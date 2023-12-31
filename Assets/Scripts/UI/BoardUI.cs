using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using Xiangqi;
using Keiwando.NFSO;

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
    public bool IsEditBoard = false;
    public PieceChooseWindow pieceChooseWindow;
    public Board BoardForEdit;
    private SupportedFileType pgnFileType = new SupportedFileType() {
		Name = "Portable Game Notation",
		Extension = "pgn",
		Owner = false,
		AppleUTI = "public.data|public.content",
		MimeType = "*/*"
    };

    public ChessNotation GetNotation() => notation;
    void Start()
    {
        notation = new ChessNotation();
        LoadResources();
        isReverseBoard = GlobalConfig.Configs["IsBoardReverse"] == "true";
        DrawPieces();
        commentUI = GameObject.Find("Img-Comment").GetComponent<CommentUI>();
        UpdateChoiceAndComment();
    }

    public void DrawPieces(byte[] pieces = null)
    {
        if (pieces == null)
        {
            pieces = notation.Current.Board.Pieces;
        }
        for (int i = 0; i < 90; i++)
        {
            DrawPiece((byte)i, pieces[i]);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canOperate) { return; }
        // 将点击位置从屏幕坐标转换为父元素的本地坐标
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPosition);
        localPosition *= scaleFactor;
        // 处理棋盘点击事件的逻辑
        byte oldPosition = clickPosition;
        clickPosition = coordinateToPosition(localPosition.x, localPosition.y);
        
        if (IsEditBoard)
        {
            handleEditBoardClick(localPosition, oldPosition);
        }
        else
        {
            handleMovePieceClick(oldPosition);
        }
    }

    private void handleMovePieceClick(byte oldPosition)
    {
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
        if (oldPosition > 89 || b.Pieces[oldPosition] == PIECE.Empty || PieceUtil.GetPieceSide(b.Pieces[oldPosition]) != b.Side)
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
        else
        {
            clickPosition = EMPTY_POSITION;
        }
        updateChoice(0, EMPTY_POSITION);
    }

    private void handleEditBoardClick(Vector2 coordinate, byte oldPosition)
    {
        if (clickPosition > 89)
        {
            updateChoice(0, EMPTY_POSITION);
            return;
        }
        if (isReverseBoard)
        {
            clickPosition = (byte)(89 - clickPosition);
        }
        Board b = BoardForEdit;
        if (oldPosition > 89 || b.Pieces[oldPosition] == PIECE.Empty)
        {
            updateChoice(0, clickPosition);
            if (b.Pieces[clickPosition] == PIECE.Empty)
            {
                if (pieceChooseWindow == null) { return; }
                pieceChooseWindow.OpenWindow(BoardForEdit.Pieces, coordinate, pieceSize);
            }
        }
        else
        {
            b.Pieces[clickPosition] = b.Pieces[oldPosition];
            b.Pieces[oldPosition] = PIECE.Empty;
            DrawPiece(clickPosition, b.Pieces[clickPosition]);
            DrawPiece(oldPosition, PIECE.Empty);
            clickPosition = EMPTY_POSITION;
            updateChoice(0, EMPTY_POSITION);
        }
    }

    private void movePiece(byte start, byte end)
    {
        if (!canOperate) { return; }
        if (getResult() != "") {
            judgeResult();
            return;
        }
        notation.MovePiece((short)(start << 8 | end));
        startMovePieceAnimation(start, end);
        canOperate = false;
        clickPosition = EMPTY_POSITION;
        UpdateChoiceAndComment();
    }

    private void judgeResult()
    {
        string result = getResult();
        if (result != "")
        {
            UIUtil.OpenMessageBox("游戏结束", result);
        }
    }

    private string getResult()
    {
        if (notation.Current.Board.IsCurrentSideLose())
        {
            return notation.Current.Board.Side == SIDE.Red ? "黑胜" : "红胜";
        }
        else if (notation.Current.Board.IsDraw())
        {
            return "和棋";
        }
        else
        {
            return "";
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
        UpdateChoiceAndComment();
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
                ScrollViewAlter scrollViewAlter = GameObject.Find("Canvas").transform.Find("AlterWindow").GetComponent<ScrollViewAlter>();
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
        UpdateChoiceAndComment();
        return true;
    }

    private bool notationGoNext()
    {
        if (!canOperate || notation.Current.Next.Count == 0) { return false; }
        notation.GoNext();
        canOperate = false;
        short currentMove = notation.GetLastMove();
        startMovePieceAnimation((byte)(currentMove >> 8), (byte)(currentMove & 0xff));
        UpdateChoiceAndComment();
        return true;
    }

    public void UpdateChoiceAndComment()
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
        string path = "fileToSave.pgn";
        try 
        {
            notation.SavePgnFile(path);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            UIUtil.OpenMessageBox("Error", "保存棋谱失败");
        }
        // 打开保存对话框
        
        string newFilename = "新建棋谱.pgn";

        FileToSave file = new FileToSave(path, newFilename, pgnFileType);

        // Allows the user to choose a save location and saves the 
        // file to that location
        NativeFileSO.shared.SaveFile(file);
        
        File.Delete(path);
        
        GameObject.Find("MenuWindow").SetActive(false);

    }

    public void LoadNotation()
    {
        // 打开打开对话框

        NativeFileSO.shared.OpenFile(new SupportedFileType[] { pgnFileType },
            delegate (bool fileWasOpened, OpenedFile file) {
                if (fileWasOpened) {
                    // Process the loaded contents of "file"
                    try
                    {
                        notation.LoadPgnDataString(file.ToUTF8String());
                        notation.GoTo(0);
                        DrawPieces();
                        UpdateChoiceAndComment();
                    }
                    catch
                    {
                        UIUtil.OpenMessageBox("Error", "导入棋谱失败");
                    }
                } else {
                    // The file selection was cancelled.	
                }
            }
        );
        GameObject.Find("MenuWindow").SetActive(false);
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
        if (clickPosition != EMPTY_POSITION)
        {
            clickPosition = (byte)(89 - clickPosition);
        }
        DrawPieces();
        UpdateChoiceAndComment();
        GlobalConfig.Configs["IsBoardReverse"] = isReverseBoard ? "true" : "false";
        GlobalConfig.SaveConfig();
    }

    public void ReloadNotation(ChessNotation newNotation)
    {
        notation.CopyFrom(newNotation);
        DrawPieces();
        UpdateChoiceAndComment();
    }

    public void StartEditBoard()
    {
        IsEditBoard = true;
        updateChoice(0, EMPTY_POSITION);
        updateChoice(1, EMPTY_POSITION);
        updateChoice(2, EMPTY_POSITION);
        commentUI.SetComment("");
        BoardForEdit = new Board(notation.Current.Board);
        GameObject.Find("Btn-EditSide").GetComponentInChildren<Text>().text = BoardForEdit.Side == SIDE.Red ? "先手:红方" : "先手:黑方";
    }

    public void CancelEditBoard()
    {
        IsEditBoard = false;
        DrawPieces();
        UpdateChoiceAndComment();
        UIUtil.ChangeEditBoardButtonStatus(false);
    }

    public void PlacePiece(byte piece)
    {
        if (clickPosition > 89 || !PieceUtil.IsPositionValid(PieceUtil.GetPieceType(piece), clickPosition, PieceUtil.GetPieceSide(piece)))
        {
            return;
        }
        BoardForEdit.Pieces[clickPosition] = piece;
        DrawPiece(clickPosition, piece);
        clickPosition = EMPTY_POSITION;
    }

    public void SaveEditBoard()
    {
        if (BoardForEdit.IsChecked(1 - BoardForEdit.Side))
        {
            UIUtil.OpenMessageBox("Error", BoardForEdit.Side == SIDE.Red ? "红方被将军" : "黑方被将军");
            return;
        }
        notation = new ChessNotation(BoardForEdit);
        CancelEditBoard();
    }

    public void ChangeEditBoardSide()
    {
        BoardForEdit.Side = (byte)(1 - BoardForEdit.Side);
        GameObject.Find("Btn-EditSide").GetComponentInChildren<Text>().text = BoardForEdit.Side == SIDE.Red ? "先手:红方" : "先手:黑方";
    }

    public void InitEditBoard()
    {
        BoardForEdit = new Board(Board.INIT_FEN);
        DrawPieces(BoardForEdit.Pieces);
        updateChoice(0, EMPTY_POSITION);
    }

    public void ClearEditBoard()
    {
        BoardForEdit = new Board(Board.EMPTY_FEN);
        DrawPieces(BoardForEdit.Pieces);
        updateChoice(0, EMPTY_POSITION);
    }

}

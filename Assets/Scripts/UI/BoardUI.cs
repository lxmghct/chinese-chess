using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    void Start()
    {
        notation = new ChessNotation();
        LoadResources();
        DrawPieces();
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
        }
        updateChoice(0, EMPTY_POSITION);
    }

    public void GotoNotationIndex(int index)
    {
        if (index < 0 || index >= notation.Current.GetMoveCount()) { return; }
        notation.GoTo(index);
        short move = notation.GetLastMove();
        byte start = (byte)(move >> 8), end = (byte)(move & 0xff);
        DrawPieces();
        updateChoice(1, start);
        updateChoice(2, end);
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

    public void NotationGoNext()
    {
        notationGoNext();
    }

    public bool notationGoPre()
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
        return true;
    }

    public bool notationGoNext()
    {
        if (!canOperate || notation.Current.Next.Count == 0) { return false; }
        notation.GoNext();
        short currentMove = notation.GetLastMove();
        canOperate = false;
        byte start = (byte)(currentMove >> 8), end = (byte)(currentMove & 0xff);
        movePiece(start, end);
        updateChoice(1, start);
        updateChoice(2, end);
        return true;
    }
}

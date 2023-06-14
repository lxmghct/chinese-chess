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
            DrawPieces();
            updateChoice(1, oldPosition);
            updateChoice(2, clickPosition);
            clickPosition = EMPTY_POSITION;
        }
        updateChoice(0, EMPTY_POSITION);
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Xiangqi;

public class BoardUI : MonoBehaviour, IPointerClickHandler
{
    // 静态变量
    public static readonly int rows = 10;
    public static readonly int columns = 9;
    public static string imageRootPath = "Assets/Resources/Images/Board/";
    private static readonly float boardImageWidth = 554;
    private static readonly float boardImageHeight = 694;
    private static readonly Vector2 boardImageLeftTop = new Vector2(41, 85);
    private static readonly Vector2 boardImageRightBottom = new Vector2(512, 612);
    
    // 非静态变量
    private GameObject boardObject;
    private float cellWidth;
    private float cellHeight;
    private float pieceSize;
    private Vector2 leftBottom;
    private Vector2 rightTop;
    private Sprite[] pieceSprites;
    private Sprite[] pieceBorders;
    private ChessNotation notation;
    void Start()
    {
        notation = new ChessNotation();
        DrawBoard();
        LoadPieceSprites();
        LoadPieceBorder();
        DrawPieces();
    }

    public void DrawBoard()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        boardObject = new GameObject("Borad", typeof(Image));
        boardObject.transform.SetParent(transform);
        Image boardImage = boardObject.GetComponent<Image>();
        boardImage.sprite = LoadSprite(imageRootPath + "board.png");
        boardImage.type = Image.Type.Sliced;
        // 根据BoardUI的RectTransform的大小，按比例缩放背景图片
        float originWidth = rectTransform.rect.width;
        float originHeight = rectTransform.rect.height;
        RectTransform boardRect = boardObject.GetComponent<RectTransform>();
        if (originWidth / originHeight > boardImageWidth / boardImageHeight)
        {
            boardRect.sizeDelta = new Vector2(originHeight / boardImageHeight * boardImageWidth, originHeight);
        }
        else
        {
            boardRect.sizeDelta = new Vector2(originWidth, originWidth / boardImageWidth * boardImageHeight);
        }
        boardRect.anchoredPosition = new Vector2(0, 0);
        // 调整格子大小
        float newWidth = boardRect.rect.width;
        float newHeight = boardRect.rect.height;
        cellWidth = (boardImageRightBottom.x - boardImageLeftTop.x) / boardImageWidth * newWidth / (columns - 1);
        cellHeight = (boardImageRightBottom.y - boardImageLeftTop.y) / boardImageHeight * newHeight / (rows - 1);
        pieceSize = cellWidth * 0.9f;
        leftBottom = new Vector2(-newWidth / 2 + boardImageLeftTop.x / boardImageWidth * newWidth, -newHeight / 2 + (boardImageHeight - boardImageRightBottom.y) / boardImageHeight * newHeight);
        rightTop = new Vector2(leftBottom.x + (columns - 1) * cellWidth, leftBottom.y + (rows - 1) * cellHeight);
    }

    public void DrawPieces()
    {
        byte[] pieces = notation.Current.Board.Pieces;
        for (int i = 0; i < 90; i++)
        {
            if (pieces[i] != PIECE.Empty)
            {
                DrawPiece(i / 9, i % 9, pieces[i]);
            }
        }
    }

    private Sprite LoadSprite(string path)
    {
        Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null)
        {
            Debug.LogError("Failed to load sprite at path: " + path);
            return null;
        }

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        return sprite;
    }

    private void LoadPieceSprites()
    {
        Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(imageRootPath + "pieces.png");
        if (texture == null)
        {
            Debug.LogError("Failed to load texture at path: " + imageRootPath + "pieces.png");
            return;
        }

        int pieceCount = 14;
        int pieceWidth = texture.width / 7;
        int pieceHeight = texture.height / 2;

        // 从texture中切割出棋子图片
        pieceSprites = new Sprite[pieceCount];
        for (int i = 0; i < pieceCount; i++)
        {
            Rect rect = new Rect(i % 7 * pieceWidth, (1 - i / 7) * pieceHeight, pieceWidth, pieceHeight);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            pieceSprites[i] = sprite;
        }
    }

    private void LoadPieceBorder()
    {
        Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(imageRootPath + "PieceBorder.png");
        if (texture == null)
        {
            Debug.LogError("Failed to load texture at path: " + imageRootPath + "PieceBorder.png");
            return;
        }
        float w = texture.width / 2;
        // 从texture中切割出棋子图片
        pieceBorders = new Sprite[2];
        for (int i = 0; i < 2; i++)
        {
            Rect rect = new Rect(i * w, 0, w, w);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            pieceBorders[i] = sprite;
        }
    }

    private void DrawPiece(int row, int col, byte piece)
    {
        // 设置棋子位置
        float posX = leftBottom.x + col * cellWidth;
        float posY = rightTop.y - row * cellHeight;
        // 先绘制棋子边框
        GameObject borderObject = new GameObject("PieceBorder", typeof(Image));
        borderObject.transform.SetParent(boardObject.transform);
        RectTransform borderRect = borderObject.GetComponent<RectTransform>();
        borderRect.sizeDelta = new Vector2(pieceSize, pieceSize);
        borderRect.anchoredPosition = new Vector2(posX, posY);

        Image borderImage = borderObject.GetComponent<Image>();
        borderImage.sprite = pieceBorders[PieceUtil.GetPieceSide(piece)];
        borderImage.type = Image.Type.Sliced;

        // 绘制棋子
        float newSize = pieceSize * 0.8f;
        GameObject pieceObject = new GameObject("Piece", typeof(Image));
        pieceObject.transform.SetParent(borderObject.transform);
        RectTransform pieceRect = pieceObject.GetComponent<RectTransform>();
        pieceRect.sizeDelta = new Vector2(newSize, newSize);
        pieceRect.anchoredPosition = new Vector2(0, 0);

        Image pieceImage = pieceObject.GetComponent<Image>();
        pieceImage.sprite = pieceSprites[PieceUtil.GetPieceType(piece) - 1 + 7 * PieceUtil.GetPieceSide(piece)];
        pieceImage.type = Image.Type.Sliced;
    }

    private Sprite GetRandomPieceSprite()
    {
        int randomIndex = Random.Range(0, 14);
        return pieceSprites[randomIndex];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 先获取当前组件的左上角坐标
        // 将点击位置从屏幕坐标转换为父元素的本地坐标
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPosition);
        // 处理棋盘点击事件的逻辑
        float x = localPosition.x;
        float y = localPosition.y;
        int row = Mathf.RoundToInt((rightTop.y - y) / cellHeight);
        int col = Mathf.RoundToInt((x - leftBottom.x) / cellWidth);
        if (row < 0 || row >= rows || col < 0 || col >= columns)
        {
            return;
        }
        float x1 = leftBottom.x + col * cellWidth;
        float y1 = rightTop.y - row * cellHeight;
        if (Mathf.Abs(x - x1) > pieceSize / 2 || Mathf.Abs(y - y1) > pieceSize / 2)
        {
            return;
        }
        Debug.Log("Clicked on row: " + row + ", col: " + col + ", x: " + x + ", y: " + y + ", x1: " + x1 + ", y1: " + y1);
    }
}

using UnityEngine;
using UnityEngine.UI;
using Xiangqi;

public class BoardUI : MonoBehaviour
{
    // 静态变量
    public static readonly int rows = 10;
    public static readonly int columns = 9;
    public static string imageRootPath = "Assets/Resources/Images/Board/";
    private static readonly int boardImageWidth = 554;
    private static readonly int boardImageHeight = 694;
    private static readonly Vector2 boardImageLeftTop = new Vector2(41, 85);
    private static readonly Vector2 boardImageRightBottom = new Vector2(512, 612);
    
    // 非静态变量
    private RectTransform rectTransform;
    private float cellWidth;
    private float cellHeight;
    private float pieceSize;
    private Vector2 leftTop;
    private Vector2 rightBottom;
    private Sprite[] pieceSprites;
    private Sprite[] pieceBorders;
    private ChessNotation notation;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        notation = new ChessNotation();
        DrawBoard();
        LoadPieceSprites();
        LoadPieceBorder();
        DrawPieces();
    }

    public void DrawBoard()
    {
        // 根据BoardUI的RectTransform的大小，按比例缩放背景图片
        float originWidth = rectTransform.rect.width;
        float originHeight = rectTransform.rect.height;
        if (originWidth / originHeight > boardImageWidth / boardImageHeight)
        {
            rectTransform.sizeDelta = new Vector2(originHeight * boardImageWidth / boardImageHeight, originHeight);
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(originWidth, originWidth * boardImageHeight / boardImageWidth);
        }
        // 调整格子大小
        float newWidth = rectTransform.rect.width;
        float newHeight = rectTransform.rect.height;
        cellWidth = (boardImageRightBottom.x - boardImageLeftTop.x) / boardImageWidth * newWidth / (columns - 1);
        cellHeight = (boardImageRightBottom.y - boardImageLeftTop.y) / boardImageHeight * newHeight / (rows - 1);
        pieceSize = cellWidth * 0.9f;
        leftTop = new Vector2(-newWidth / 2 + boardImageLeftTop.x / boardImageWidth * newWidth, -newHeight / 2 + (boardImageHeight - boardImageRightBottom.y) / boardImageHeight * newHeight);
        rightBottom = new Vector2(leftTop.x + (columns - 1) * cellWidth, leftTop.y + (rows - 1) * cellHeight);
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
        // for (int row = 0; row < rows; row++)
        // {
        //     for (int col = 0; col < columns; col++)
        //     {
        //         GameObject cell = new GameObject("Cell", typeof(Image));
        //         cell.transform.SetParent(transform);

        //         RectTransform cellRect = cell.GetComponent<RectTransform>();
        //         cellRect.sizeDelta = new Vector2(pieceSize, pieceSize);

        //         // 设置格子位置
        //         float posX = leftTop.x + col * cellWidth;
        //         float posY = leftTop.y + row * cellHeight;
        //         cellRect.anchoredPosition = new Vector2(posX, posY);

        //         Image cellImage = cell.GetComponent<Image>();
        //         cellImage.sprite = GetRandomPieceSprite();
        //         cellImage.type = Image.Type.Sliced;
        //     }
        // }
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
        float posX = leftTop.x + col * cellWidth;
        float posY = rightBottom.y - row * cellHeight;
        // 先绘制棋子边框
        GameObject borderObject = new GameObject("PieceBorder", typeof(Image));
        borderObject.transform.SetParent(transform);
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
}

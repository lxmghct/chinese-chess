using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Xiangqi;
using System.Collections.Generic;

public partial class BoardUI : MonoBehaviour, IPointerClickHandler
{
    // 静态变量
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
    private GameObject[] choiceObjects;
    private short moveOfAnimation = 0;
    private Dictionary<byte, GameObject> pieceObjects = new Dictionary<byte, GameObject>();
    private void LoadResources()
    {
        LoadBoard();
        LoadPieceSprites();
        LoadPieceBorder();
        LoadChoiceObjects();
        GlobalConfig.LoadConfig();
    }
    
    public void LoadBoard()
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

    private void LoadChoiceObjects()
    {
        choiceObjects = new GameObject[3];
        Sprite[] sprite = new Sprite[3] {
            LoadSprite(imageRootPath + "choice1.png"),
            LoadSprite(imageRootPath + "choice2.png"),
            LoadSprite(imageRootPath + "choice2.png")
        };
        for (int i = 0; i < 3; i++)
        {
            choiceObjects[i] = new GameObject("Choice", typeof(Image));
            choiceObjects[i].transform.SetParent(boardObject.transform);
            RectTransform rect = choiceObjects[i].GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(pieceSize, pieceSize);
            rect.anchoredPosition = new Vector2(0, 0);
            Image image = choiceObjects[i].GetComponent<Image>();
            image.sprite = sprite[i];
            image.type = Image.Type.Sliced;
            choiceObjects[i].SetActive(false);
        }
    }

    private byte coordinateToPosition(float x, float y)
    {
        int row = Mathf.RoundToInt((rightTop.y - y) / cellHeight);
        int col = Mathf.RoundToInt((x - leftBottom.x) / cellWidth);
        if (row < 0 || row >= rows || col < 0 || col >= columns)
        {
            return 255;
        }
        float x1 = leftBottom.x + col * cellWidth;
        float y1 = rightTop.y - row * cellHeight;
        if (Mathf.Abs(x - x1) > pieceSize / 2 || Mathf.Abs(y - y1) > pieceSize / 2)
        {
            return 255;
        }
        return (byte)(row * 9 + col);
    }

    private Vector2 positionToCoordinate(byte position)
    {
        if (isReverseBoard)
        {
            position = (byte)(89 - position);
        }
        int row = position / 9;
        int col = position % 9;
        float x = leftBottom.x + col * cellWidth;
        float y = rightTop.y - row * cellHeight;
        return new Vector2(x, y);
    }

    private void DrawPiece(byte position, byte piece)
    {
        if (pieceObjects.ContainsKey(position))
        {
            Destroy(pieceObjects[position]);
            pieceObjects.Remove(position);
        }
        if (position > 89 || piece == PIECE.Empty)
        {
            return;
        }
        // 设置棋子位置
        Vector2 coordinate = positionToCoordinate(position);
        // 先绘制棋子边框
        GameObject borderObject = new GameObject("PieceBorder", typeof(Image));
        borderObject.transform.SetParent(boardObject.transform);
        RectTransform borderRect = borderObject.GetComponent<RectTransform>();
        borderRect.sizeDelta = new Vector2(pieceSize, pieceSize);
        borderRect.anchoredPosition = new Vector2(coordinate.x, coordinate.y);

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

        pieceObjects.Add(position, borderObject);
    }

    private void updateChoice(int index, byte position)
    {
        if (position > 89)
        {
            choiceObjects[index].SetActive(false);
            return;
        }
        Vector2 coordinate = positionToCoordinate(position);
        RectTransform rect = choiceObjects[index].GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(coordinate.x, coordinate.y);
        choiceObjects[index].SetActive(true);
    }

    //平滑移动棋子
    private IEnumerator SmoothMove(RectTransform chessPiece, Vector2 start, Vector2 end, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / duration); // 计算插值比例

            // 计算当前帧的位置
            Vector2 currentPosition = Vector2.Lerp(start, end, t);

            // 更新棋子的位置
            chessPiece.anchoredPosition = currentPosition;

            yield return null; // 等待下一帧
        }

        // 移动完成后，确保棋子的位置准确到达目标位置
        chessPiece.anchoredPosition = end;
        finishMovePiece();
    }

    private void finishMovePiece()
    {
        canOperate = true;
        byte from = (byte) (moveOfAnimation >> 8);
        if (notation.Current.Board.Pieces[from] == PIECE.Empty && pieceObjects.ContainsKey(from))
        {
            Destroy(pieceObjects[from]);
            pieceObjects.Remove(from);
        }
    }

    private void startMovePieceAnimation(byte from, byte to)
    {
        if (!pieceObjects.ContainsKey(from))
        {
            return;
        }
        GameObject startObject = pieceObjects[from];
        byte[] pieces = notation.Current.Board.Pieces;
        if (pieces[from] != PIECE.Empty) // 表明是悔棋
        {
            pieceObjects.Remove(from);
            DrawPiece(from, pieces[from]);
            Debug.Log("DrawPiece: " + from + ", " + pieces[from]);
        }
        else if (pieceObjects.ContainsKey(to))
        {
            pieceObjects[from] = pieceObjects[to];
        }
        else
        {
            pieceObjects.Remove(from);
        }
        pieceObjects[to] = startObject;
        Vector2 start = positionToCoordinate(from);
        Vector2 end = positionToCoordinate(to);
        // 确保移动的棋子在最上层
        startObject.transform.SetAsLastSibling();
        moveOfAnimation = (short) (from << 8 | to);
        StartCoroutine(SmoothMove(startObject.GetComponent<RectTransform>(), start, end, 0.3f));
    }

}
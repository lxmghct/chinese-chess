
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Xiangqi;

public class PieceChooseWindow : MonoBehaviour, IPointerClickHandler
{
    private List<byte> redPieces;
    private List<byte> blackPieces;
    private float pieceSize = 20;
    private float windowWidth;
    private float windowHeight;
    public BoardUI boardObject;

    private void Start()
    {
    }

    public void OpenWindow(byte[] pieces, Vector2 position, float pieceSize = 20)
    {
        this.pieceSize = pieceSize;
        getPieces(pieces);
        if (redPieces.Count == 0 && blackPieces.Count == 0)
        {
            return;
        }
        transform.parent.gameObject.SetActive(true);
        resizeWindow();
        moveWindow(position);
        drawPieces();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPosition);
        int temp = getRowAndColFromCoordinate(localPosition);
        int row = temp / 10, col = temp % 10;
        List<byte> pieces = row > 0 ? blackPieces : (redPieces.Count > 0 ? redPieces : blackPieces);
        if (col >= pieces.Count)
        {
            return;
        }
        boardObject.PlacePiece(pieces[col]);
        transform.parent.gameObject.SetActive(false);
    
    }

    private void getPieces(byte[] pieces)
    {
        redPieces = new List<byte>() {
            PIECE.RedKing, PIECE.RedAdvisor, PIECE.RedBishop, PIECE.RedKnight, PIECE.RedRook, PIECE.RedCannon, PIECE.RedPawn
        };
        blackPieces = new List<byte>() {
            PIECE.BlackKing, PIECE.BlackAdvisor, PIECE.BlackBishop, PIECE.BlackKnight, PIECE.BlackRook, PIECE.BlackCannon, PIECE.BlackPawn
        };
        Dictionary<byte, int> pieceCountMap = new Dictionary<byte, int>() {
            { PIECE.RedKing, 1 }, { PIECE.RedAdvisor, 2 }, { PIECE.RedBishop, 2 }, { PIECE.RedKnight, 2 }, { PIECE.RedRook, 2 }, { PIECE.RedCannon, 2 }, { PIECE.RedPawn, 5 },
            { PIECE.BlackKing, 1 }, { PIECE.BlackAdvisor, 2 }, { PIECE.BlackBishop, 2 }, { PIECE.BlackKnight, 2 }, { PIECE.BlackRook, 2 }, { PIECE.BlackCannon, 2 }, { PIECE.BlackPawn, 5 }
        };
        for (int i = 0; i < 90; i++)
        {
            if (pieces[i] != PIECE.Empty && pieceCountMap.ContainsKey(pieces[i]))
            {
                pieceCountMap[pieces[i]]--;
            }
        }
        foreach (KeyValuePair<byte, int> pair in pieceCountMap)
        {
            if (pair.Value <= 0)
            {
                if (PieceUtil.GetPieceSide(pair.Key) == SIDE.Red)
                {
                    redPieces.Remove(pair.Key);
                }
                else
                {
                    blackPieces.Remove(pair.Key);
                }
            }
        }
    }

    private void resizeWindow()
    {
        windowWidth = Mathf.Max(redPieces.Count, blackPieces.Count) * pieceSize;
        windowHeight = (redPieces.Count * blackPieces.Count == 0 ? 1 : 2) * pieceSize;
        // 重新设置窗口大小
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(windowWidth, windowHeight);
    }

    private void moveWindow(Vector2 position)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        // 把窗口移动到点击位置下方
        rectTransform.anchoredPosition = new Vector2(position.x, position.y - windowHeight / 2 - 20);
    }

    private void drawPieces()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 position = Vector2.zero;
        float x = position.x - windowWidth / 2 + pieceSize / 2;
        float y = position.y + windowHeight / 2 - pieceSize / 2;
        foreach (byte piece in redPieces)
        {
            UIUtil.DrawPiece(piece, new Vector2(x, y), pieceSize, transform);
            x += pieceSize;
        }
        x = position.x - windowWidth / 2 + pieceSize / 2;
        if (redPieces.Count > 0)
        {
            y -= pieceSize;
        }
        foreach (byte piece in blackPieces)
        {
            UIUtil.DrawPiece(piece, new Vector2(x, y), pieceSize, transform);
            x += pieceSize;
        }
    }

    private int getRowAndColFromCoordinate(Vector2 coordinate)
    {
        int row = (int)((windowHeight / 2 - coordinate.y) / pieceSize);
        int col = (int)((coordinate.x + windowWidth / 2) / pieceSize);
        return (int)(row * 10 + col);
    }

    
}
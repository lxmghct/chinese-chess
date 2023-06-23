
using UnityEngine;
using UnityEngine.UI;
using Xiangqi;

public class UIUtil
{
    public static Sprite[] pieceSprites = null;
    public static Sprite[] pieceBorders = null;

    public static void OpenMessageBox(string title, string message)
    {
        MessageBox messageBox = GameObject.Find("Canvas").transform.Find("MessageBox").GetComponent<MessageBox>();
        if (messageBox != null)
        {
            messageBox.ShowMessage(title, message); 
        }
    }

    public static GameObject DrawPiece(byte piece, Vector2 coordinate, float pieceSize, Transform parent)
    {
        if (pieceSprites == null || pieceBorders == null)
        {
            pieceBorders = BoardUI.pieceBorders;
            pieceSprites = BoardUI.pieceSprites;
        }
        // 先绘制棋子边框
        GameObject borderObject = new GameObject("PieceBorder", typeof(Image));
        borderObject.transform.SetParent(parent);
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

        return borderObject;
    }

    public static void ChangeEditBoardButtonStatus(bool isEdit)
    {
        GameObject.Find("Canvas").transform.Find("Img-BottomButtons").gameObject.SetActive(!isEdit);
        GameObject.Find("Canvas").transform.Find("Img-BottomButtonsForEditBoard").gameObject.SetActive(isEdit);
    }
    
}
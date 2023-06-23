using UnityEngine;
using UnityEngine.UI;
using Xiangqi;

// 变招列表
public class NotationWindow : MonoBehaviour
{
    public GameObject buttonPrefab; // 预制体，用于创建按钮
    public RectTransform notationContent;
    public RectTransform alterContent;
    public InputField commentInputField;
    public Button editButton;
    public Button cancelButton;

    private ChessNotation notation;
    GameObject[] notationButtons;
    GameObject[] alterButtons;
    string[] notationList;
    NotationNode[] notationNodeList;
    int currentNotationIndex = 0;
    int currentAlterIndex = 0;
    string[] alterList;

    private void Start()
    {
    }

    private delegate void ClickEvent(int buttonIndex);

    private void CreateButtonList(string[] stringList, RectTransform content, GameObject[] buttons, ClickEvent onButtonClick)
    {
        float btnHeight = buttonPrefab.GetComponent<RectTransform>().rect.height; // 获取按钮的高度
        content.sizeDelta = new Vector2(0, buttons.Length * btnHeight + 10); // 设置Content的高度
        float startY = content.rect.height / 2 - btnHeight / 2 - 5; // 计算第一个按钮的Y坐标
        for (int i = 0; i < buttons.Length; i++)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, content); // 创建按钮实例
            Button button = buttonObj.GetComponent<Button>(); // 获取按钮组件

            // 设置按钮的文本和点击事件
            button.GetComponentInChildren<Text>().text = stringList[i];
            int index = i; // 存储当前按钮的索引
            button.onClick.AddListener(() => onButtonClick(index));
            // 文本左对齐
            button.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleLeft;

            // 设置按钮的位置
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, startY - i * btnHeight);

            buttons[i] = buttonObj;
        }
    }

    private void OnNotationButtonClick(int buttonIndex)
    {
        highlightNode(ref currentNotationIndex, notationButtons, buttonIndex);
        notation.GoTo(buttonIndex);
        ReloadAlter();
    }

    private void OnAlterButtonClick(int buttonIndex)
    {
        highlightNode(ref currentAlterIndex, alterButtons, buttonIndex);
        notation.Current.Choice = buttonIndex;
        ReloadNotation();
    }

    void highlightNode(ref int oldIndex, GameObject[] buttons, int buttonIndex)
    {
        if (oldIndex < buttons.Length && oldIndex >= 0)
        {
            buttons[oldIndex].GetComponent<Image>().color = Color.white;
        }
        oldIndex = buttonIndex;
        if (oldIndex >= buttons.Length || oldIndex < 0) { oldIndex = 0; }
        if (oldIndex < buttons.Length)
        {
            buttons[oldIndex].GetComponent<Image>().color = Color.green;
        }
    }

    private void destroyButtons(GameObject[] buttons)
    {
        if (buttons == null) { return; }
        for (int i = 0; i < buttons.Length; i++)
        {
            Destroy(buttons[i]);
        }
    }

    private void ReloadNotation()
    {
        int len = notation.GetNotationNodeCount() - 1;
        notationList = new string[len];
        notationNodeList = new NotationNode[len];
        for (int i = 0; i < len; i++)
        {
            NotationNode node = notation.GetByIndex(i);
            notationNodeList[i] = node;
            string temp = node.Board.Side == SIDE.Red ? node.Board.Round + ". " : "    ";
            notationList[i] = "      " + temp + ChessNotationUtil.MoveToChineseNotation(node.Moves[node.Choice], node.Board.Pieces);
            if (node.Moves.Count > 1)
            {
                notationList[i] += "   M";
            }
        }
        destroyButtons(notationButtons);
        notationButtons = new GameObject[len];
        CreateButtonList(notationList, notationContent, notationButtons, OnNotationButtonClick);
        highlightNode(ref currentNotationIndex, notationButtons, currentNotationIndex);
    }

    private void ReloadAlter()
    {
        int len = notation.Current.Moves.Count;
        alterList = new string[len];
        for (int i = 0; i < len; i++)
        {
            alterList[i] = "      " + (i + 1) + ". " + ChessNotationUtil.MoveToChineseNotation(notation.Current.Moves[i], notation.Current.Board.Pieces);
        }
        destroyButtons(alterButtons);
        alterButtons = new GameObject[len];
        CreateButtonList(alterList, alterContent, alterButtons, OnAlterButtonClick);
        highlightNode(ref currentAlterIndex, alterButtons, notation.Current.Choice);
    }

    public void OpenWindow()
    {
        ChessNotation temp = GameObject.Find("Img-Board").GetComponent<BoardUI>().GetNotation();
        if (temp == null) { return; }
        notation = new ChessNotation(temp);
        currentNotationIndex = notation.GetCurrentIndex() - 1;
        currentAlterIndex = notation.Current.Choice;
        ReloadNotation();
        ReloadAlter();
        gameObject.SetActive(true);
    }

    private void updateCommentOfWholeNotation(NotationNode n1, NotationNode n2)
    {
        if (n1 == null || n2 == null) { return; }
        n1.Board.SetComment(n2.Board.Comment);
        for (int i = 0; i < n1.Next.Count; i++)
        {
            updateCommentOfWholeNotation(n1.Next[i], n2.Next[i]);
        }
    }
    

    public void CloseWindow()
    {
        gameObject.SetActive(false);
        BoardUI board = GameObject.Find("Img-Board").GetComponent<BoardUI>();
        ChessNotation temp = board.GetNotation();
        if (temp == null) { return; }
        updateCommentOfWholeNotation(temp.Root, notation.Root);
        board.UpdateChoiceAndComment();
    }

    public void GoToNotation()
    {
        notation.GoNext();
        GameObject.Find("Img-Board").GetComponent<BoardUI>().ReloadNotation(notation);
        gameObject.SetActive(false);
    }
}

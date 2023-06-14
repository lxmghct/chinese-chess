using UnityEngine;
using UnityEngine.UI;
using Xiangqi;

// 变招列表
public class ScrollViewAlter : MonoBehaviour
{
    public GameObject buttonPrefab; // 预制体，用于创建按钮
    public RectTransform content; // ScrollView的Content对象
    private string[] alterList;
    private int numberOfButtons = 0; // 按钮数量
    GameObject[] buttons;

    private void Start()
    {
    }

    private void CreateButtonList()
    {
        float btnHeight = buttonPrefab.GetComponent<RectTransform>().rect.height; // 获取按钮的高度
        content.sizeDelta = new Vector2(0, numberOfButtons * btnHeight + 10); // 设置Content的高度
        float startY = content.rect.height / 2 - btnHeight / 2 - 5; // 计算第一个按钮的Y坐标
        for (int i = 0; i < numberOfButtons; i++)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, content); // 创建按钮实例
            Button button = buttonObj.GetComponent<Button>(); // 获取按钮组件

            // 设置按钮的文本和点击事件
            button.GetComponentInChildren<Text>().text = alterList[i];
            int index = i; // 存储当前按钮的索引
            button.onClick.AddListener(() => OnButtonClick(index));

            // 设置按钮的位置
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, startY - i * btnHeight);

            buttons[i] = buttonObj;
        }
    }

    private void OnButtonClick(int buttonIndex)
    {
        GameObject.Find("Img-Board").GetComponent<BoardUI>().NotationGoNext(buttonIndex);
        gameObject.SetActive(false);
    }

    public void SetAlterList(NotationNode node)
    {
        for (int i = 0; i < numberOfButtons; i++)
        {
            Destroy(buttons[i]);
        }
        numberOfButtons = node.Moves.Count;
        buttons = new GameObject[numberOfButtons];
        alterList = new string[numberOfButtons];
        for (int i = 0; i < numberOfButtons; i++)
        {
            alterList[i] = (i + 1) + ". " + ChessNotationUtil.MoveToChineseNotation(node.Moves[i], node.Board.Pieces);
        }
        CreateButtonList();
        gameObject.SetActive(true);
    }
}

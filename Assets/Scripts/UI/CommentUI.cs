using UnityEngine;
using UnityEngine.UI;

public class CommentUI : MonoBehaviour
{
    private GameObject commentInputField;
    private GameObject commentEditButton;
    private GameObject commentCancelButton;
    private InputField inputField;
    private string comment = "";
    void Start()
    {
        commentInputField = GameObject.Find("CommentInputField");
        commentEditButton = GameObject.Find("Btn-EditComment");
        commentCancelButton = GameObject.Find("Btn-CancelEditComment");
        inputField = commentInputField.GetComponent<InputField>();
        commentInputField.SetActive(false);
        commentCancelButton.SetActive(false);
        inputField.readOnly = true;
        // 编辑按钮点击事件
        commentEditButton.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            // 如果取消按钮显示，则说明当前正在编辑注解，点击编辑按钮则保存注解
            if (commentCancelButton.activeSelf)
            {
                SetComment(inputField.text);
                BoardUI board = GameObject.Find("Img-Board").GetComponent<BoardUI>();
                board.SetComment(comment);
            }
            else
            {
                commentInputField.SetActive(true);
                inputField.readOnly = false;
                commentEditButton.GetComponentInChildren<Text>().text = "保存";
                commentCancelButton.SetActive(true);
            }
            
        });
        // 取消按钮点击事件
        commentCancelButton.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            SetComment(comment);
        });
    }

    public void SetComment(string comment)
    {
        this.comment = comment;
        commentInputField.SetActive(comment != "");
        inputField.readOnly = true;
        inputField.text = comment;
        commentEditButton.GetComponentInChildren<Text>().text = comment == "" ? "添加注解" : "编辑注解";
        commentCancelButton.SetActive(false);
    }
}
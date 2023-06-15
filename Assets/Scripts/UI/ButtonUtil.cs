/// <summary>
/// HomeButtun.cs
/// </summary>

using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonUtil : MonoBehaviour
{
    void Start()
    {

    }
    // 点击事件: 切换场景
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 点击事件: 退出程序
    public void Quit()
    {
        Application.Quit();
    }

    // 点击事件: 功能待完善
    public void NotImplementedInfo()
    {
        OpenMessageBox("提示", "功能待完善");
    }

    public void OpenMessageBox(string title, string message)
    {
        MessageBox messageBox = GameObject.Find("Canvas").transform.Find("Img-MessageBox").GetComponent<MessageBox>();
        if (messageBox != null)
        {
            messageBox.ShowMessage(title, message); 
        }
    }
}
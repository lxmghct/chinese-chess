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
}
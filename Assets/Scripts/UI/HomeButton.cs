/// <summary>
/// HomeButtun.cs
/// </summary>

using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButton : MonoBehaviour
{
    void Start()
    {

    }
    // 点击事件
    public void OnClick()
    {
        // 跳转到主页
        SceneManager.LoadScene("PlayScene");
    }
}
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
    // 点击事件
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
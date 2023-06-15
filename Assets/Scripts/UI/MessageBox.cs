
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{

    void Start()
    {
    }

    public void ShowMessage(string title, string message)
    {
        transform.Find("Text-MessageBox-Content").GetComponent<Text>().text = message;
        transform.Find("Text-MessageBox-Title").GetComponent<Text>().text = title;
        gameObject.SetActive(true);
    }
    
}
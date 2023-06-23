
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{

    void Start()
    {
    }

    public void ShowMessage(string title, string message)
    {
        Transform msgTransform = transform.Find("Img-MsgBox");
        msgTransform.Find("Text-MessageBox-Content").GetComponent<Text>().text = message;
        msgTransform.Find("Text-MessageBox-Title").GetComponent<Text>().text = title;
        gameObject.SetActive(true);
    }
    
}
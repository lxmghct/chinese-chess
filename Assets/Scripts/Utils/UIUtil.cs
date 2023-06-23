
using UnityEngine;

public class UIUtil
{
    public static void OpenMessageBox(string title, string message)
    {
        MessageBox messageBox = GameObject.Find("Canvas").transform.Find("MessageBox").GetComponent<MessageBox>();
        if (messageBox != null)
        {
            messageBox.ShowMessage(title, message); 
        }
    }
}
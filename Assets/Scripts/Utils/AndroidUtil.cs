
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
public class AndroidUtil
{
    public static void CopyFile(string sourcePath, string destinationPath)
    {
        byte[] fileData = null;
        // 从 StreamingAssets 文件夹读取文件数据
        if (Application.platform == RuntimePlatform.Android)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(sourcePath))
            {
                www.SendWebRequest();
                while (!www.isDone) { }
                fileData = www.downloadHandler.data;
            }
        }
        else
        {
            fileData = File.ReadAllBytes(sourcePath);
        }
        // 创建目标文件夹（如果不存在）
        string destinationFolder = Path.GetDirectoryName(destinationPath);
        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }
        // 将文件数据写入目标文件
        File.WriteAllBytes(destinationPath, fileData);
    }
}
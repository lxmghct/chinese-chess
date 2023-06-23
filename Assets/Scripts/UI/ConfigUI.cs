
using UnityEngine;
using UnityEngine.UI;

public class ConfigUI : MonoBehaviour
{
    private InputField hashSizeInput = null;
    private InputField maxDepthInput = null;
    private InputField maxTimeInput = null;
    private InputField threadCountInput = null;
    private Button isScoreShowButton = null;

    void Start()
    {
    }

    public void LoadConfig()
    {
        Transform configTransform = transform.Find("Img-Config");
        if (hashSizeInput == null || maxDepthInput == null || maxTimeInput == null || threadCountInput == null || isScoreShowButton == null)
        {
            hashSizeInput = configTransform.Find("Input-Config-HashSize").GetComponent<InputField>();
            maxDepthInput = configTransform.Find("Input-Config-MaxDepth").GetComponent<InputField>();
            maxTimeInput = configTransform.Find("Input-Config-MaxTime").GetComponent<InputField>();
            threadCountInput = configTransform.Find("Input-Config-ThreadCount").GetComponent<InputField>();
            isScoreShowButton = configTransform.Find("Btn-Config-ShowScore").GetComponent<Button>();
        }
        Debug.Log(GlobalConfig.Configs["HashSize"] + " " + GlobalConfig.Configs["MaxDepth"] + " " + GlobalConfig.Configs["MaxTime"] + " " + GlobalConfig.Configs["ThreadCount"] + " " + GlobalConfig.Configs["ShowScore"]);
        hashSizeInput.text = GlobalConfig.Configs["HashSize"];
        maxDepthInput.text = GlobalConfig.Configs["MaxDepth"];
        maxTimeInput.text = GlobalConfig.Configs["MaxTime"];
        threadCountInput.text = GlobalConfig.Configs["ThreadCount"];
        isScoreShowButton.GetComponentInChildren<Text>().text = GlobalConfig.Configs["ShowScore"] == "true" ? "是" : "否";
        gameObject.SetActive(true);
        // 给按钮添加点击事件
        isScoreShowButton.onClick.AddListener(delegate ()
        {
            isScoreShowButton.GetComponentInChildren<Text>().text = isScoreShowButton.GetComponentInChildren<Text>().text == "是" ? "否" : "是";
        });
    }

    public void SaveConfig()
    {
        try
        {
            int hashSize = int.Parse(hashSizeInput.text);
            if (hashSize < 1 || hashSize > 1024)
            {
                throw new System.Exception();
            }
            // 计算hashSize的最小2的幂次方
            int hashSizePower = 1;
            while ((hashSize >> hashSizePower) != 0)
            {
                hashSizePower++;
            }
            hashSizeInput.text = (1 << (hashSizePower - 1)).ToString();
            GlobalConfig.Configs["HashSize"] = hashSizeInput.text;
        }
        catch (System.Exception) { }
        try
        {
            int maxDepth = int.Parse(maxDepthInput.text);
            if (maxDepth > 0 && maxDepth <= 128)
            {
                maxDepthInput.text = maxDepth.ToString();
                GlobalConfig.Configs["MaxDepth"] = maxDepthInput.text;
            }
        }
        catch (System.Exception) { }
        try
        {
            int maxTime = int.Parse(maxTimeInput.text);
            if (maxTime > 0 && maxTime <= 30000)
            {
                maxTimeInput.text = maxTime.ToString();
                GlobalConfig.Configs["MaxTime"] = maxTimeInput.text;
            }
        }
        catch (System.Exception) { }
        try
        {
            int threadCount = int.Parse(threadCountInput.text);
            if (threadCount > 0 && threadCount <= 128)
            {
                threadCountInput.text = threadCount.ToString();
                GlobalConfig.Configs["ThreadCount"] = threadCountInput.text;
            }
        }
        catch (System.Exception) { }
        GlobalConfig.Configs["ShowScore"] = isScoreShowButton.GetComponentInChildren<Text>().text == "是" ? "true" : "false";
        GlobalConfig.SaveConfig();
        gameObject.SetActive(false);
    }   
}
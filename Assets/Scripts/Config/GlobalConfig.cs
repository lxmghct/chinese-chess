/// <summary>
/// configuration
/// </summary>

using System.Collections.Generic;
using System.IO;

public class GlobalConfig
{
    public static string ConfigPath = "Assets/Resources/Config/GlobalConfig.properties";
    public static Dictionary<string, string> Configs = new Dictionary<string, string>()
    {
        {"HashSize", "16"},
        {"MaxDepth", "18"},
        {"MaxTime", "2000"},
        {"ThreadCount", "1"},
        {"IsBoardReverse", "false"},
    };

    public static Dictionary<string, string> DefaultConfigs = new Dictionary<string, string>()
    {
        {"HashSize", "16"},
        {"MaxDepth", "18"},
        {"MaxTime", "2000"},
        {"ThreadCount", "1"},
        {"IsBoardReverse", "false"},
    };

    public static void LoadConfig()
    {
        // 先判断文件是否存在
        if (!File.Exists(ConfigPath))
        {
            // 不存在则创建文件
            File.Create(ConfigPath).Close();
            // 写入默认配置
            foreach (KeyValuePair<string, string> kv in DefaultConfigs)
            {
                File.AppendAllText(ConfigPath, kv.Key + "=" + kv.Value + "\n");
            }
            return;
        }
        string[] lines = File.ReadAllLines(ConfigPath);
        foreach (string line in lines)
        {
            string[] kv = line.Split('=');
            if (kv.Length == 2)
            {
                string key = kv[0].Trim();
                if (Configs.ContainsKey(key))
                {
                    Configs[key] = kv[1].Trim();
                }
            }
        }
    }

    public static void SaveConfig()
    {
        File.WriteAllText(ConfigPath, "");
        foreach (KeyValuePair<string, string> kv in Configs)
        {
            File.AppendAllText(ConfigPath, kv.Key + "=" + kv.Value + "\n");
        }
    }
}
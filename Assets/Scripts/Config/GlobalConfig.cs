/// <summary>
/// configuration
/// </summary>

using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GlobalConfig
{
    public static string ConfigRootPath = Application.persistentDataPath + "/configs/";
    public static string GlobalConfigPath = ConfigRootPath + "GlobalConfig.properties";
    public static Dictionary<string, string> Configs = new Dictionary<string, string>()
    {
        {"HashSize", "16"},
        {"MaxDepth", "18"},
        {"MaxTime", "2000"},
        {"ThreadCount", "1"},
        {"IsBoardReverse", "false"},
        {"ShowScore", "true"}
    };

    public static Dictionary<string, string> DefaultConfigs = new Dictionary<string, string>()
    {
        {"HashSize", "16"},
        {"MaxDepth", "18"},
        {"MaxTime", "2000"},
        {"ThreadCount", "1"},
        {"IsBoardReverse", "false"},
        {"ShowScore", "true"}
    };

    public static void LoadConfig()
    {
        // 先判断文件是否存在
        if (!Directory.Exists(ConfigRootPath))
        {
            Directory.CreateDirectory(ConfigRootPath);
        }
        if (!File.Exists(GlobalConfigPath))
        {
            // 不存在则创建文件
            File.Create(GlobalConfigPath).Close();
            // 写入默认配置
            foreach (KeyValuePair<string, string> kv in DefaultConfigs)
            {
                File.AppendAllText(GlobalConfigPath, kv.Key + "=" + kv.Value + "\n");
            }
            return;
        }
        string[] lines = File.ReadAllLines(GlobalConfigPath);
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
        File.WriteAllText(GlobalConfigPath, "");
        foreach (KeyValuePair<string, string> kv in Configs)
        {
            File.AppendAllText(GlobalConfigPath, kv.Key + "=" + kv.Value + "\n");
        }
    }
}
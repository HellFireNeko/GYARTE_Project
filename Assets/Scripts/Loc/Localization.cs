using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

public static class Localization
{
    private static Dictionary<string, string> LoqStrings = new Dictionary<string, string>();

    public static string GetString(string key)
    {
        if (LoqStrings.ContainsKey(key))
            return LoqStrings[key];
        return string.Empty;
    }

    public static void Load(string lang)
    {
        LoqStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText($"{Application.streamingAssetsPath}/{lang}.json"));
    }
}

[JsonObject]
public class Languages
{
    public Dictionary<string, string> Langs = new Dictionary<string, string>();

    public Languages()
    {
        this.Langs = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText($"{Application.streamingAssetsPath}/Languages.json"));
    }
}
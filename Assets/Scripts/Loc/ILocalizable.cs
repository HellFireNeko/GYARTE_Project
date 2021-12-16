using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class ILocalizable : MonoBehaviour
{
    [SerializeField] private string LoqKey;
    private TMP_Text TextObject;

    public void UpdateSelf()
    {
        TextObject.text = Localization.GetString(LoqKey);
    }

    void OnEnable()
    {
        if (TextObject is null)
            TextObject = GetComponent<TMP_Text>();
        UpdateSelf();
    }

    public static void UpdateLoq()
    {
        if (!File.Exists("loqSettings.txt")) 
            File.WriteAllText("loqSettings.txt", "eng");
        var loqName = File.ReadAllText("loqSettings.txt");
        Localization.Load(loqName);
        var objects = FindObjectsOfType<ILocalizable>();
        foreach (var o in objects)
        {
            o.UpdateSelf();
        }
    }
}

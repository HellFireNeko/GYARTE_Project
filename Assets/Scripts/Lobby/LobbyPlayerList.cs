using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.NetworkVariable;
using TMPro;

public class LobbyPlayerList : NetworkBehaviour
{
    [SerializeField] private TMP_Text PlayerName;
    [SerializeField] private TMP_Text ReadyText;
    [SerializeField] private Image ReadyPanel;
    [Space]
    [SerializeField] private Color NotReadyColor;
    [SerializeField] private Color ReadyColor;

    private NetworkVariableBool Ready = new NetworkVariableBool(false);

    public void ToggleReady()
    {
        Ready.Value = !Ready.Value;
    }

    private void OnGUI()
    {
        if (Ready.Value)
        {
            ReadyPanel.color = ReadyColor;
            ReadyText.text = "Ready";
        }
        else
        {
            ReadyPanel.color = NotReadyColor;
            ReadyText.text = "Not ready";
        }
    }

    void Start()
    {
        if (!IsOwner) return;


    }
}

[CustomEditor(typeof(LobbyPlayerList))]
public class LobbyPlayerListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Toggle ready"))
        {
            LobbyPlayerList t = (LobbyPlayerList)target;
            t.ToggleReady();
        }
    }
}

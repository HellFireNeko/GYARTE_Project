using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

using TMPro;

public class LobbyPlayerListObject : NetworkBehaviour
{
    [SerializeField] private TMP_Text PlayerNameText;
    [SerializeField] private TMP_Text ReadyText;
    [SerializeField] private Image ReadyPanel;
    [SerializeField] private RawImage PlayerModelView;
    [Space]
    [SerializeField] private Color NotReadyColor;
    [SerializeField] private Color ReadyColor;
    [Space]
    [SerializeField] private PlayerModelSelector Selector;

    private NetworkVariableString PlayerName = new NetworkVariableString("Display name");
    private NetworkVariableBool Ready = new NetworkVariableBool(false);

    private Button ReadyButton;

    public override void NetworkStart()
    {
        if (!IsServer) { return; }

        var data = LobbyManagerUI.GetPlayerData(OwnerClientId);

        if (data.HasValue)
        {
            PlayerName.Value = data.Value.Name;
            var modelData = Selector.GetItemById(data.Value.ModelId);
            if (modelData.HasValue)
            {
                PlayerModelView.texture = modelData.Value.Texture;
            }
        }
    }

    void Start()
    {
        if (IsOwner)
        {
            ReadyButton = LobbyManagerUI.Instance.GetLobbyReadyButton();
            if (ReadyButton != null)
            {
                ReadyButton.onClick.AddListener(ToggleReadyServerRpc);
            }
            else
                Debug.LogError("Failed to get lobby button");
        }
    }

    void OnEnable()
    {
        PlayerName.OnValueChanged += HandleNameChange;
    }

    void OnDisable()
    {
        PlayerName.OnValueChanged -= HandleNameChange;
    }

    private void HandleNameChange(string previousValue, string newValue)
    {
        PlayerNameText.text = newValue;
    }

    [ServerRpc]
    public void ToggleReadyServerRpc()
    {
        Ready.Value = !Ready.Value;
    }

    void OnGUI()
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

    void Update()
    {
        if (transform.parent == null)
        {
            var obj = GameObject.Find("PlayerList");
            transform.SetParent(obj.transform);
        }
    }
}

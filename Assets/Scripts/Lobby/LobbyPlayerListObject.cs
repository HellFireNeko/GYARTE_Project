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
    private NetworkVariableInt Texture = new NetworkVariableInt();

    private Button ReadyButton;
    private ModelSelector ModelPicker;

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
                Texture.Value = data.Value.ModelId;
                PlayerModelView.texture = modelData.Value.Texture;
            }
        }
    }

    void Start()
    {
        if (IsOwner)
        {
            ReadyButton = LobbyManagerUI.Instance.GetLobbyReadyButton();
            ModelPicker = FindObjectOfType<ModelSelector>();
            if (ReadyButton != null)
            {
                ReadyButton.onClick.AddListener(ToggleReadyServerRpc);
            }
            else
                Debug.LogError("Failed to get lobby button");

            ModelPicker.OnSelectionChanged.AddListener((model) =>
            {
                SetPlayerModelServerRpc(model.ID);
            });
        }
    }

    [ServerRpc]
    private void SetPlayerModelServerRpc(int model)
    {
        Texture.Value = model;
    }

    void OnEnable()
    {
        PlayerName.OnValueChanged += HandleNameChange;
        Texture.OnValueChanged += HandleTextureChange;
    }

    void OnDisable()
    {
        PlayerName.OnValueChanged -= HandleNameChange;
        Texture.OnValueChanged -= HandleTextureChange;
    }

    private void HandleTextureChange(int previousValue, int newValue)
    {
        var modelData = Selector.GetItemById(newValue);
        if (modelData.HasValue)
        {
            PlayerModelView.texture = modelData.Value.Texture;
        }
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
            ReadyText.text = Localization.GetString("ready");
        }
        else
        {
            ReadyPanel.color = NotReadyColor;
            ReadyText.text = Localization.GetString("not ready");
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

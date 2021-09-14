using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MLAPI;
using MLAPI.Spawning;

public class LobbyManagerUI : MonoBehaviour
{
    [Header("Lobby assignments:")]
    [SerializeField] private GameObject LobbyGroupPanel;
    [Header("Main menu assignments:")]
    [SerializeField] private GameObject MainMenuGroupPanel;
    [SerializeField] private MainMenuPanelObjects MainPanel;
    // Start is called before the first frame update
    void Start()
    {
        MainPanel.Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class MainMenuPanelObjects
{
    public TMP_InputField IpInputField;
    public TMP_InputField PortInputField;
    public TMP_InputField PasswordInputField;
    public TMP_InputField NameInputField;
    public Button HostButton;
    public Button ConnectButton;
    [SerializeField] private Button ExitButton;

    [SerializeField] private bool nameValid = false;
    [SerializeField] private bool portValid = false;
    [SerializeField] private bool ipValid = false;

    public void Init()
    {
        NameInputField.onValueChanged.AddListener(x =>
        {
            bool state = false;
            if (x.Length > 0)
                state = true;
            nameValid = state;
            RunValidationChecks();
        });

        PortInputField.onValueChanged.AddListener(x =>
        {
            bool state = false;
            if (int.TryParse(x, out int val))
                if (val >= 2000)
                    state = true;
            portValid = state;
            RunValidationChecks();
        });

        IpInputField.onValueChanged.AddListener(x =>
        {
            bool state = false;
            if (IPAddress.TryParse(x, out IPAddress ip))
                state = true;
            ipValid = state;
            RunValidationChecks();
        });

        ExitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    private void RunValidationChecks()
    {
        bool state = false;
        if (nameValid && portValid && ipValid)
            state = true;
        HostButton.interactable = state;
        ConnectButton.interactable = state;
    }
}
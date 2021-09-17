using System.IO;
using System.Net;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MLAPI;
using MLAPI.Transports.UNET;
using MLAPI.Spawning;

public class LobbyManagerUI : MonoBehaviour
{
    public static LobbyManagerUI Instance { get { return FindObjectOfType<LobbyManagerUI>(); } }

    [Header("Lobby assignments:")]
    [SerializeField] private GameObject LobbyGroupPanel;
    [SerializeField] private LobbyObjects LobbyPanel;
    [Header("Connection assignments:")]
    [SerializeField] private GameObject ConnectionGroupPanel;
    [SerializeField] private ConnectionObjects ConnectionPanel;
    [Header("Main menu assignments:")]
    [SerializeField] private GameObject MainMenuGroupPanel;
    [SerializeField] private MainMenuObjects MainPanel;

    private int ConnectedClients;
    private static Dictionary<ulong, NetworkPlayerData> ClientData;

    private void SetActivePanel(GameObject panel)
    {
        LobbyGroupPanel.SetActive(false);
        ConnectionGroupPanel.SetActive(false);
        MainMenuGroupPanel.SetActive(false);
        panel.SetActive(true);
    }

    public static NetworkPlayerData? GetPlayerData(ulong clientId)
    {
        if (ClientData.TryGetValue(clientId, out NetworkPlayerData r))
        {
            return r;
        }
        return null;
    }

    public static void ModifyPlayerModelId(ulong clientId, int modelId)
    {
        if (ClientData.TryGetValue(clientId, out NetworkPlayerData r))
        {
            r.ModelId = modelId;
            ClientData[clientId] = r;
        }
        throw new System.Exception();
    }

    public Button GetLobbyReadyButton()
    {
        return LobbyPanel.ReadyButton;
    }

    void Start()
    {
        SetActivePanel(MainMenuGroupPanel);

        Application.quitting += () =>
        {
            if (NetworkManager.Singleton.IsHost)
                NetworkManager.Singleton.StopHost();
        };

        NetworkManager.Singleton.OnServerStarted += HandleServerStart;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

        LobbyPanel.LeaveButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.StopHost();
                NetworkManager.Singleton.ConnectionApprovalCallback -= Singleton_ConnectionApprovalCallback;
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.StopClient();
            }

            SetActivePanel(ConnectionGroupPanel);
        });

        ConnectionPanel.Init();

        ConnectionPanel.HostButton.onClick.AddListener(() =>
        {
            ClientData = new Dictionary<ulong, NetworkPlayerData>();
            ClientData[NetworkManager.Singleton.LocalClientId] = new NetworkPlayerData(ConnectionPanel.NameInputField.text);

            var transport = NetworkManager.Singleton.GetComponent<UNetTransport>();
            transport.ConnectAddress = ConnectionPanel.IpInputField.text;
            transport.ConnectPort = int.Parse(ConnectionPanel.PortInputField.text);
            transport.ServerListenPort = int.Parse(ConnectionPanel.PortInputField.text);
            transport.MaxConnections = 6;
            NetworkManager.Singleton.ConnectionApprovalCallback += Singleton_ConnectionApprovalCallback;
            NetworkManager.Singleton.StartHost();
            SetActivePanel(LobbyGroupPanel);
            LobbyPanel.StartButton.gameObject.SetActive(true);
        });

        ConnectionPanel.ConnectButton.onClick.AddListener(() =>
        {
            var transport = NetworkManager.Singleton.GetComponent<UNetTransport>();
            transport.ConnectAddress = ConnectionPanel.IpInputField.text;
            transport.ConnectPort = int.Parse(ConnectionPanel.PortInputField.text);
            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new ConnectionPayload() { Name = ConnectionPanel.NameInputField.text, Password = ConnectionPanel.PasswordInputField.text });
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(payload);
            NetworkManager.Singleton.StartClient();
            ConnectionPanel.SetValidation(false);
            ConnectionPanel.RunValidationChecks();
            Invoke("ReEnableButtons", 10);
        });

        ConnectionPanel.ReturnButton.onClick.AddListener(() =>
        {
            SetActivePanel(MainMenuGroupPanel);
        });

        MainPanel.PlayButton.onClick.AddListener(() =>
        {
            SetActivePanel(ConnectionGroupPanel);
        });

        MainPanel.LanguageSelect.onValueChanged.AddListener(x =>
        {
            var lang = new Languages();
            var selection = lang.Langs[MainPanel.LanguageSelect.options[x].text];
            File.WriteAllText("loqSettings.txt", selection);
            ILocalizable.UpdateLoq();
        });

        MainPanel.Init();

        var l = new Languages();
        MainPanel.LanguageSelect.AddOptions(l.Langs.Keys.ToList());

        if (!File.Exists("loqSettings.txt")) File.WriteAllText("loqSettings.txt", "eng");
        var a = File.ReadAllText("loqSettings.txt");
        int i = 0;
        foreach (var item in l.Langs.Values)
        {
            if (!(item == a))
            {
                i++;
                continue;
            }
            break;
        }
        MainPanel.LanguageSelect.value = i;
        ILocalizable.UpdateLoq();
    }

    private void ReEnableButtons()
    {
        ConnectionPanel.SetValidation(true);
        ConnectionPanel.RunValidationChecks();
    }

    private void Singleton_ConnectionApprovalCallback(byte[] data, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        var payload = Newtonsoft.Json.JsonConvert.DeserializeObject<ConnectionPayload>(System.Text.Encoding.ASCII.GetString(data));

        bool ApproveConnection = payload.Password == ConnectionPanel.PasswordInputField.text;

        if (ApproveConnection)
        {
            ClientData[clientId] = new NetworkPlayerData(payload.Name);
        }

        callback(false, null, ApproveConnection, null, null);

        if (ApproveConnection)
        {
            LobbyPanel.PList.SpawnLobbyItem(clientId);
        }
    }

    private void HandleServerStart()
    {
        ConnectedClients = 1;
        if (NetworkManager.Singleton.IsHost)
        {
            HandleClientConnect(NetworkManager.Singleton.LocalClientId);
            LobbyPanel.PList.SpawnLobbyItem(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void HandleClientConnect(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            SetActivePanel(LobbyGroupPanel);
        }
        if (!NetworkManager.Singleton.IsHost)
        {
            LobbyPanel.StartButton.gameObject.SetActive(false);
        }
        else
        {
            ConnectedClients++;
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            SetActivePanel(ConnectionGroupPanel);
        }
        if (NetworkManager.Singleton.IsHost)
        {
            ConnectedClients--;
        }
    }
}

[System.Serializable]
public class LobbyObjects
{
    public Button LeaveButton;
    public Button ReadyButton;
    public Button StartButton;
    public LobbyPlayerList PList;
}

[System.Serializable]
public class ConnectionObjects
{
    public TMP_InputField IpInputField;
    public TMP_InputField PortInputField;
    public TMP_InputField PasswordInputField;
    public TMP_InputField NameInputField;
    public Button HostButton;
    public Button ConnectButton;
    public Button ReturnButton;

    [SerializeField] private bool nameValid = false;
    [SerializeField] private bool portValid = false;
    [SerializeField] private bool ipValid = false;

    private bool DoValidation = true;
    public bool SetValidation(bool val) => DoValidation = val;

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

        PortInputField.onValueChanged.Invoke(PortInputField.text);

        IpInputField.onValueChanged.AddListener(x =>
        {
            bool state = false;
            if (IPAddress.TryParse(x, out IPAddress ip))
                state = true;
            ipValid = state;
            RunValidationChecks();
        });

        IpInputField.onValueChanged.Invoke(IpInputField.text);
    }

    public void RunValidationChecks()
    {
        bool state = false;
        if (nameValid && portValid && ipValid && DoValidation)
            state = true;
        HostButton.interactable = state;
        ConnectButton.interactable = state;
    }
}

[System.Serializable]
public class MainMenuObjects
{
    public Button PlayButton;
    public TMP_Dropdown LanguageSelect;
    [SerializeField] private Button ExitButton;

    public void Init()
    {
        ExitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}

public struct NetworkPlayerData
{
    public string Name;
    public int ModelId;

    public NetworkPlayerData(string name, int modelId = 0)
    {
        Name = name;
        ModelId = modelId;
    }
}
using System.Net;
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
    [SerializeField] private LobbyPanelObjects LobbyPanel;
    [Header("Main menu assignments:")]
    [SerializeField] private GameObject MainMenuGroupPanel;
    [SerializeField] private MainMenuPanelObjects MainPanel;

    private static Dictionary<ulong, NetworkPlayerData> ClientData;

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

            LobbyGroupPanel.SetActive(false);
            MainMenuGroupPanel.SetActive(true);
        });

        MainPanel.Init();

        MainPanel.HostButton.onClick.AddListener(() =>
        {
            ClientData = new Dictionary<ulong, NetworkPlayerData>();
            ClientData[NetworkManager.Singleton.LocalClientId] = new NetworkPlayerData(MainPanel.NameInputField.text);

            var transport = NetworkManager.Singleton.GetComponent<UNetTransport>();
            transport.ConnectAddress = MainPanel.IpInputField.text;
            transport.ConnectPort = int.Parse(MainPanel.PortInputField.text);
            transport.ServerListenPort = int.Parse(MainPanel.PortInputField.text);
            //transport.MLAPIRelayPort = int.Parse(MainPanel.PortInputField.text);
            //string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            //var externalIp = IPAddress.Parse(externalIpString);
            //transport.MLAPIRelayAddress = externalIp.ToString();
            transport.MaxConnections = 6;
            NetworkManager.Singleton.ConnectionApprovalCallback += Singleton_ConnectionApprovalCallback;
            NetworkManager.Singleton.StartHost();
            MainMenuGroupPanel.SetActive(false);
            LobbyGroupPanel.SetActive(true);
            LobbyPanel.StartButton.gameObject.SetActive(true);
        });

        MainPanel.ConnectButton.onClick.AddListener(() =>
        {
            var transport = NetworkManager.Singleton.GetComponent<UNetTransport>();
            transport.ConnectAddress = MainPanel.IpInputField.text;
            transport.ConnectPort = int.Parse(MainPanel.PortInputField.text);
            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new ConnectionPayload() { Name = MainPanel.NameInputField.text, Password = MainPanel.PasswordInputField.text });
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(payload);
            NetworkManager.Singleton.StartClient();
            MainPanel.SetValidation(false);
            MainPanel.RunValidationChecks();
            Invoke("ReEnableButtons", 10);
        });
    }

    private void ReEnableButtons()
    {
        MainPanel.SetValidation(true);
        MainPanel.RunValidationChecks();
    }

    private void Singleton_ConnectionApprovalCallback(byte[] data, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        var payload = Newtonsoft.Json.JsonConvert.DeserializeObject<ConnectionPayload>(System.Text.Encoding.ASCII.GetString(data));

        bool ApproveConnection = payload.Password == MainPanel.PasswordInputField.text;

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
            MainMenuGroupPanel.SetActive(false);
            LobbyGroupPanel.SetActive(true);
        }
        if (!NetworkManager.Singleton.IsHost)
        {
            LobbyPanel.StartButton.gameObject.SetActive(false);
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            LobbyGroupPanel.SetActive(false);
            MainMenuGroupPanel.SetActive(true);
        }
    }
}

[System.Serializable]
public class LobbyPanelObjects
{
    public Button LeaveButton;
    public Button ReadyButton;
    public Button StartButton;
    public LobbyPlayerList PList;
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

        ExitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
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
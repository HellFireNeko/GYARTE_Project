using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using TMPro;
using MLAPI.Transports.UNET;

public class LobbyNetworkBehavior : MonoBehaviour
{
    [SerializeField] private TMP_InputField IpInputField;
    [SerializeField] private TMP_InputField PortInputField;
    [SerializeField] private TMP_InputField PasswordInputField;
    [SerializeField] private TMP_InputField NameInputField;
    [SerializeField] private UnityEngine.UI.Button HostButton;
    [SerializeField] private UnityEngine.UI.Button ClientButton;
    [SerializeField] private UnityEngine.UI.Button StartButton;
    [SerializeField] private GameObject LobbyConnectObject;
    [SerializeField] private GameObject LeaveObject;
    [SerializeField] private GameObject StartObject;

    [SerializeField] private List<SpawnPosition> SpawnPoints;

    private static Dictionary<ulong, PlayerData> ClientData;

    public static PlayerData? GetPlayerData(ulong clientId) 
    { 
        if (ClientData.TryGetValue(clientId, out PlayerData r))
        {
            return r;
        }
        return null;
    }

    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStart;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

        NameInputField.onValueChanged.AddListener(call =>
        {
            if (call.Length < 1)
            {
                HostButton.interactable = false;
                ClientButton.interactable = false;
            }
            else
            {
                HostButton.interactable = true;
                ClientButton.interactable = true;
            }
        });
    }

    void Update()
    {
        if (LobbyConnectObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (IpInputField.isFocused)
                    PortInputField.Select();
                else if (PortInputField.isFocused)
                    PasswordInputField.Select();
                else if (PasswordInputField.isFocused)
                    NameInputField.Select();
                else
                    IpInputField.Select();
            }
        }
        else
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if (NetworkManager.Singleton.ConnectedClients.Count >= 3)
                {
                    StartButton.interactable = true;
                }
                else
                {
                    StartButton.interactable = false;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= HandleServerStart;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
    }

    public void Host()
    {
        ClientData = new Dictionary<ulong, PlayerData>();
        ClientData[NetworkManager.Singleton.LocalClientId] = new PlayerData(NameInputField.text, 0);
        SpawnPoints[0].Occupied = true;

        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = IpInputField.text;
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = int.Parse(PortInputField.text);
        NetworkManager.Singleton.GetComponent<UNetTransport>().ServerListenPort = int.Parse(PortInputField.text);
        NetworkManager.Singleton.GetComponent<UNetTransport>().MaxConnections = SpawnPoints.Count;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApproveCallback;
        var pos = SpawnPoints[0].transform.position;
        NetworkManager.Singleton.StartHost(pos);
    }

    public void Connect()
    {
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = IpInputField.text;
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = int.Parse(PortInputField.text);

        var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new ConnectionPayload() { Name = NameInputField.text, Password = PasswordInputField.text });
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(payload);
        NetworkManager.Singleton.StartClient();
    }

    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.StopHost();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApproveCallback;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }

        foreach (var p in SpawnPoints)
        {
            p.Occupied = false;
        }

        LobbyConnectObject.SetActive(true);
        LeaveObject.SetActive(false);
        StartObject.SetActive(false);
    }

    private void HandleServerStart()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            StartObject.SetActive(true);
            HandleClientConnect(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void HandleClientConnect(ulong client)
    {
        if (client == NetworkManager.Singleton.LocalClientId)
        {
            LobbyConnectObject.SetActive(false);
            LeaveObject.SetActive(true);
        }
    }

    private void HandleClientDisconnect(ulong client)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPoints[ClientData[client].SpawnPointId].Occupied = false;
            ClientData.Remove(client);
        }

        if (client == NetworkManager.Singleton.LocalClientId)
        {
            LobbyConnectObject.SetActive(true);
            LeaveObject.SetActive(false);
            StartObject.SetActive(false);
        }
    }

    private void ApproveCallback(byte[] data, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        var payload = Newtonsoft.Json.JsonConvert.DeserializeObject<ConnectionPayload>(System.Text.Encoding.ASCII.GetString(data));

        bool ApproveConnection = payload.Password == PasswordInputField.text;

        Vector3 spawnPos = Vector3.zero;

        if (ApproveConnection)
        {
            int id = 0;
            foreach (var p in SpawnPoints)
            {
                if (p.Occupied) { id++; continue; }
                spawnPos = p.transform.position;
                p.Occupied = true;
                break;
            }

            ClientData[clientId] = new PlayerData(payload.Name, id);
        }

        callback(true, null, ApproveConnection, spawnPos, null);
    }
}

public struct PlayerData
{
    public string PlayerName;
    public int SpawnPointId;

    public PlayerData(string name, int id)
    {
        PlayerName = name;
        SpawnPointId = id;
    }
}

[System.Serializable]
public class ConnectionPayload
{
    public string Password;
    public string Name;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Transports.UNET;
using MLAPI.Spawning;
using MLAPI.SceneManagement;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Connection;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using MLAPI.Messaging;

public class NetworkServer : NetworkManager
{
    public string LocalName;

    void Start()
    {
        //OnClientConnectedCallback += NetworkServer_OnClientConnectedCallback;
        OnClientDisconnectCallback += NetworkServer_OnClientDisconnectCallback;
        //NetworkSceneManager.OnSceneSwitched += NetworkSceneManager_OnSceneSwitched;

    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
        //Your logic here
        bool approve = true;
        bool createPlayerObject = true;



        //If approve is true, the connection gets added. If it's false. The client gets disconnected
        callback(createPlayerObject, null, approve, null, null);
    }

    //private void NetworkSceneManager_OnSceneSwitched()
    //{

    //}

    //ulong test;

    private void NetworkServer_OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log($"Disconnect {obj}", this);

        Destroy(gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    //private void NetworkServer_OnClientConnectedCallback(ulong obj)
    //{
    //    Debug.Log($"Connected {obj}", this);
    //    if (IsServer)
    //    {
    //        test = obj;
    //        Invoke("DoThing1", 1);
    //    }
    //}

    public void SetConnectionData(string host, int port)
    {
        Singleton.GetComponent<UNetTransport>().ConnectAddress = host;
        Singleton.GetComponent<UNetTransport>().ConnectPort = port;
    }

    public void Host(string hostName = "Host")
    {
        LocalName = hostName;
        StartHost();
        NetworkSceneManager.SwitchScene("Lobby");
        //Invoke("DoThing", 1);
        //Invoke("DoThing3", 2);
    }

    public void Connect(string clientName = "Guest")
    {
        LocalName = clientName;
        StartClient();
    }

    public void StopHostMethod()
    {
        StopHost();
        Destroy(gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Transports.UNET;
using MLAPI.Spawning;
using MLAPI.SceneManagement;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;

public class ServerObject : MonoBehaviour
{
    public NetworkDictionary<ulong, string> NamedConnections = new NetworkDictionary<ulong, string>();

    private void Start()
    {
        DontDestroyOnLoad(this);
    }
}

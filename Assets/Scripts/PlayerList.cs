using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;

public class PlayerList : NetworkBehaviour
{
    public static PlayerList Instance { get { return FindObjectOfType<PlayerList>(); } }
    private NetworkDictionary<ulong, string> NamedConnections = new NetworkDictionary<ulong, string>();
    public string LocalName;

    [SerializeField]
    private Text Player1;
    [SerializeField]
    private Text Player2;
    [SerializeField]
    private Text Player3;
    [SerializeField]
    private Text Player4;
    [SerializeField]
    private Text Player5;
    [SerializeField]
    private Text Player6;

    private void Start()
    {
        NamedConnections.OnDictionaryChanged += NamedConnections_OnDictionaryChanged;
        CustomMessagingManager.RegisterNamedMessageHandler("PlayerList", (sender, stream) =>
        {
            using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
            {
                var cmdstring = reader.ReadString();
                print(cmdstring.ToString());
            }
        });
    }

    private void NamedConnections_OnDictionaryChanged(NetworkDictionaryEvent<ulong, string> changeEvent)
    {
        print($"{changeEvent.Type}, {changeEvent.Key}: {changeEvent.Value}");
    }

    public string GetName(ulong id)
    {
        print($"Trying to fetch name with id {id} value is {NamedConnections[id]}");
        return NamedConnections[id];
    }

    private void HideAll()
    {
        Player1.enabled = false;
        Player2.enabled = false;
        Player3.enabled = false;
        Player4.enabled = false;
        Player5.enabled = false;
        Player6.enabled = false;
    }

    //public void UpdateClientVars()
    //{
    //    if (!IsServer) return;
    //    FixClientRpc();
    //}

    //public void RegName(ulong id, string name)
    //{
    //    if (!IsClient) return;
    //    RegisterNewNameServerRpc(id, name);
    //}

    //public void UnRegName(ulong id)
    //{
    //    if (!IsClient) return;
    //    UnregisterNameServerRpc(id);
    //}

    //[ServerRpc]
    //private void RegisterNewNameServerRpc(ulong id, string name)
    //{
    //    NamedConnections.Add(id, name);
    //}

    //[ServerRpc]
    //private void UnregisterNameServerRpc(ulong id)
    //{
    //    NamedConnections.Remove(id);
    //}

    //[ClientRpc]
    //private void FixClientRpc()
    //{
    //    UpdateList();
    //}

    private void UpdateList()
    {
        HideAll();
        int num = 1;
        foreach (string name in NamedConnections.Values)
        {
            switch (num)
            {
                case 1:
                    Player1.text = name;
                    Player1.enabled = true;
                    break;

                case 2:
                    Player2.text = name;
                    Player2.enabled = true;
                    break;

                case 3:
                    Player3.text = name;
                    Player3.enabled = true;
                    break;

                case 4:
                    Player4.text = name;
                    Player4.enabled = true;
                    break;

                case 5:
                    Player5.text = name;
                    Player5.enabled = true;
                    break;

                case 6:
                    Player6.text = name;
                    Player6.enabled = true;
                    break;
            }
            num++;
        }
    }
}

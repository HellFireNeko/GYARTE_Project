using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using MLAPI;

public class LobbyPlayerList : MonoBehaviour
{
    [SerializeField] private GameObject Prefab;

    public void SpawnLobbyItem(ulong clientId)
    {
        var obj = Instantiate(Prefab, this.transform);
        obj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    }
}

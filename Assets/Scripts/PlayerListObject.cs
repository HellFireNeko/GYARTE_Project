using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;

[RequireComponent(typeof(NetworkObject))]
public class PlayerListObject : NetworkBehaviour
{
    Text TextObject;
    [SerializeField]
    private int PID = 1;

    // Use this for initialization
    void Start()
    {
        TextObject = GetComponent<Text>();
        CustomMessagingManager.RegisterNamedMessageHandler("AssignPlayer", (sender, a) =>
        {
            using (PooledNetworkReader reader = PooledNetworkReader.Get(a))
            {
                var Id = reader.ReadInt32();
                var Name = reader.
            }
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnGainedOwnership()
    {

        base.OnGainedOwnership();
    }

    public override void OnLostOwnership()
    {

        base.OnLostOwnership();
    }
}
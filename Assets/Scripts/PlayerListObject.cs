using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;

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
                print("recieved");
                var Id = reader.ReadInt32();
                var cid = reader.ReadUInt64();
                var Name = reader.ReadString().ToString();

                if (PID == Id)
                {
                    TextObject.text = Name;
                    TextObject.enabled = true;
                    NetworkObject.ChangeOwnership(cid);
                }
            }
        });
        NetworkObject.RemoveOwnership();
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
        TextObject.enabled = false;
        base.OnLostOwnership();
    }
}
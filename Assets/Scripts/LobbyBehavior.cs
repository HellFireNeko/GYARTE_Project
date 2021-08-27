using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;

public class LobbyBehavior : MonoBehaviour
{
    public static LobbyBehavior Instance { get { return FindObjectOfType<LobbyBehavior>(); } }
    //public List<Text> PlayerTexts;
    NetworkServer Server;

    //public Text DebugText;

    public Button LeaveButton;
    public Button StartButton;
    public GameObject HostOptions;

    // Start is called before the first frame update
    void Start()
    {
        Server = FindObjectOfType<NetworkServer>();
        if (Server != null)
        {
            Debug.Log("Successfully fetched the server object", this);

            if (Server.IsHost)
            {
                HostOptions.SetActive(true);
                StartButton.onClick.AddListener(StartBtn);
            }

            LeaveButton.onClick.AddListener(Leave);
        }
    }

    private void StartBtn()
    {

    }

    private void Leave()
    {
        if (Server.IsHost)
        {
            Server.StopHostMethod();
        }
        else
        {
            Server.StopClient();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    //[ClientRpc]
    //public void SetupClientRpc()
    //{
    //    if (!IsServer) return;

    //    string are = string.Empty;
    //    if (Server.IsClient) are = "Client";
    //    if (Server.IsHost) are = "Host";
    //    DebugText.text = $"You are {are}, id {Server.LocalClientId}, name {PlayerList.Instance.GetName(Server.LocalClientId)}";
    //}

    // Update is called once per frame
    void Update()
    {
        if (Server.ConnectedClients.Count > 2)
        {
            StartButton.interactable = true;
        }
        else
        {
            StartButton.interactable = false;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBehavior : MonoBehaviour
{
    [SerializeField]
    private MainPanel Main;
    [SerializeField]
    private HostPanel Host;
    [SerializeField]
    private JoinPanel Join;
    [SerializeField]
    private NetworkServer Server;

    // Start is called before the first frame update
    void Start()
    {
        InitJoin();
        InitHost();
        InitMain();
    }

    void InitJoin()
    {
        Join.JoinButton.onClick.AddListener(JJ);
        Join.ReturnButton.onClick.AddListener(Ret);
    }

    void JJ()
    {
        Server.SetConnectionData(Join.HostName.text, int.Parse(Join.Port.text));
        Server.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(Join.UName.text);
        Server.Connect();
    }

    void InitHost()
    {
        Host.HostButton.onClick.AddListener(HH);
        Host.ReturnButton.onClick.AddListener(Ret);
    }

    void HH()
    {
        Server.SetConnectionData(Host.HostName.text, int.Parse(Host.Port.text));
        Server.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(Host.UName.text);
        Server.Host();
    }

    void Ret()
    {
        Show(Main);
    }

    void InitMain()
    {
        Main.HostButton.onClick.AddListener(MH);
        Main.JoinButton.onClick.AddListener(MJ);
        Main.ExitButton.onClick.AddListener(ME);
    }

    void MH()
    {
        Show(Host);
    }

    void MJ()
    {
        Show(Join);
    }

    void ME()
    {
        Application.Quit();
    }

    void Show(GroupPanelObject panel)
    {
        Main.Group.SetActive(false);
        Host.Group.SetActive(false);
        Join.Group.SetActive(false);
        panel.Group.SetActive(true);
    }
}

public abstract class GroupPanelObject
{
    public GameObject Group;
}

[Serializable]
public class MainPanel : GroupPanelObject
{
    public Button HostButton;
    public Button JoinButton;
    public Button ExitButton;
}

[Serializable]
public class HostPanel : GroupPanelObject
{
    public InputField HostName;
    public InputField Port;
    public InputField UName;
    public Button HostButton;
    public Button ReturnButton;
}

[Serializable]
public class JoinPanel : GroupPanelObject
{
    public InputField HostName;
    public InputField Port;
    public InputField UName;
    public Button JoinButton;
    public Button ReturnButton;
}
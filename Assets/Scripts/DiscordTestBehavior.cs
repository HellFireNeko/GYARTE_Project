using System;
using System.Collections.Generic;
using UnityEngine;
using Discord;

internal class PlayerObject
{
    public GameObject Object;
    public DiscordPlayer Player;
    public MovementCharacter Movement;
}

public class DiscordTestBehavior : MonoBehaviour
{
    Discord.Discord discord;
    LobbyManager lobbyManager;
    ActivityManager activityManager;
    VoiceManager voiceManager;
    UserManager userManager;
    long lobbyid;

    public bool IsLobbyScene = false;

    [SerializeField]
    private GameObject PlayerPrefab;

    [SerializeField]
    private List<string> Scenes = new List<string>();

    private Dictionary<long, PlayerObject> Players = new Dictionary<long, PlayerObject>();

    public bool voice;
    //public NetworkServer Server;

    private void SpawnPlayer(long userid)
    {
        var obj = Instantiate(PlayerPrefab);
        var p = obj.GetComponent<DiscordPlayer>();
        p.ClientID = userid;
        var m = obj.GetComponent<MovementCharacter>();
        if (userManager.GetCurrentUser().Id != userid) m.ToggleLock();
        else 
        {
            p.Init();
            p.SetName(lobbyManager.GetMemberUser(lobbyid, userid).Username);
        }
        Players.Add(userid, new PlayerObject() { Object = obj, Player = p, Movement = m });
    }

    public void Button()
    {
        var txn = lobbyManager.GetLobbyCreateTransaction();
        txn.SetCapacity(6);
        txn.SetType(LobbyType.Private);

        lobbyManager.CreateLobby(txn, (Result result, ref Lobby lobby) =>
        {
            if (result != Result.Ok)
            {
                return;
            }
            print($"New lobby made {lobby.Id} secret {lobby.Secret}");

            lobbyid = lobby.Id;

            if (voice)
            {
                lobbyManager.ConnectVoice(lobby.Id, (Result result) => {
                    if (result == Result.Ok)
                    {
                        print("Connected to voice");
                    }
                    else
                    {
                        print("Error connecting to voice");
                    }
                });
            }

            lobbyManager.ConnectNetwork(lobby.Id);
            lobbyManager.OpenNetworkChannel(lobby.Id, 0, true);
            lobbyManager.OpenNetworkChannel(lobby.Id, 1, false);

            SpawnPlayer(userManager.GetCurrentUser().Id);

            var t = lobbyManager.GetMemberUpdateTransaction(lobby.Id, lobby.OwnerId);
            t.SetMetadata("test", "hello");

            lobbyManager.UpdateMember(lobby.Id, lobby.OwnerId, t, result => { if (result == Result.Ok) print("ok"); });

            var activity = new Activity
            {
                ApplicationId = 881860326096785409,
                State = "Waiting for players",
                Details = "Co-op",
                Party =
                {
                    Id = lobby.Id.ToString(),
                    Size =
                    {
                        MaxSize = 6,
                        CurrentSize = lobbyManager.MemberCount(lobby.Id)
                    }
                },
                Secrets =
                {
                    Join = lobbyManager.GetLobbyActivitySecret(lobby.Id)
                },
                Timestamps =
                {
                    End = DateTimeOffset.Now.AddMinutes(10).ToUnixTimeSeconds()
                },
                Instance = true
            };

            activityManager.UpdateActivity(activity, result =>
            {
                print($"Update Activity {result}");
            });

            //InvokeRepeating("DiscordLobbyUpdate", 0.25f, 0.25f);
        });
    }

    // Use this for initialization
    void Start()
    {
        discord = new Discord.Discord(881860326096785409, (UInt64)CreateFlags.Default);
        discord.SetLogHook(LogLevel.Debug, LogProblemsFunction);
        
        activityManager = discord.GetActivityManager();
        lobbyManager = discord.GetLobbyManager();
        voiceManager = discord.GetVoiceManager();
        userManager = discord.GetUserManager();

        lobbyManager.OnMemberConnect += LobbyManager_OnMemberConnect;
        lobbyManager.OnMemberDisconnect += LobbyManager_OnMemberDisconnect;
        lobbyManager.OnNetworkMessage += LobbyManager_OnNetworkMessage;
        lobbyManager.OnMemberUpdate += LobbyManager_OnMemberUpdate;

        activityManager.OnActivityJoinRequest += ActivityManager_OnActivityJoinRequest;
        activityManager.OnActivityJoin += ActivityManager_OnActivityJoin; 



        var activity = new Activity
        {
            ApplicationId = 881860326096785409,
            State = "Chillin",
            Details = "Solo",
            Instance = true
        };

        activityManager.UpdateActivity(activity, result =>
        {
            print($"Update Activity {result}");
        });
    }

    private void LobbyManager_OnMemberUpdate(long lobbyId, long userId)
    {
        print(lobbyManager.GetMemberMetadataValue(lobbyId, userId, "test"));
    }

    private void LobbyManager_OnNetworkMessage(long lobbyId, long userId, byte channelId, byte[] data)
    {
        string message = System.Text.Encoding.UTF8.GetString(data);
        print($"[{lobbyId}]Network message from {userId} on channel {channelId}: {message}");
        if (message.StartsWith("Spawn"))
        {
            SpawnPlayer(userId);
        }
        else
        {
            Players[userId].Player.SetPositionAndRotation(message);
        }
    }

    private void ActivityManager_OnActivityJoin(string secret)
    {
        lobbyManager.ConnectLobbyWithActivitySecret(secret, (Result result, ref Lobby lobby) =>
        {
            if (!(result == Result.Ok)) return;
            lobbyid = lobby.Id;
            if (voice)
            {
                lobbyManager.ConnectVoice(lobby.Id, (Result result) => {
                    if (result == Result.Ok)
                    {
                        print("Connected to voice");
                    }
                    else
                    {
                        print("Error connecting to voice");
                    }
                });
            }

            lobbyManager.ConnectNetwork(lobbyid);
            lobbyManager.OpenNetworkChannel(lobbyid, 0, true);
            lobbyManager.OpenNetworkChannel(lobbyid, 1, false);

            var activity = new Activity
            {
                ApplicationId = 881860326096785409,
                State = "Waiting for players",
                Details = "Co-op",
                Party =
                {
                    Id = lobby.Id.ToString(),
                    Size =
                    {
                        MaxSize = 6,
                        CurrentSize = lobbyManager.MemberCount(lobby.Id)
                    }
                },
                Secrets =
                {
                    Join = lobbyManager.GetLobbyActivitySecret(lobby.Id)
                },
                Timestamps =
                {
                    End = DateTimeOffset.Now.AddMinutes(10).ToUnixTimeSeconds()
                },
                Instance = true
            };

            activityManager.UpdateActivity(activity, result =>
            {
                print($"Update Activity {result}");
            });

            SpawnPlayer(userManager.GetCurrentUser().Id);
            lobbyManager.SendNetworkMessage(lobbyid, userManager.GetCurrentUser().Id, 0, System.Text.Encoding.UTF8.GetBytes("Spawn"));

            //InvokeRepeating("DiscordLobbyUpdate", 0.25f, 0.25f);
        });
    }

    private void ActivityManager_OnActivityJoinRequest(ref User user)
    {
        activityManager.AcceptInvite(user.Id, result =>
        {

        });
    }

    private void LobbyManager_OnMemberDisconnect(long lobbyId, long userId)
    {
        print($"Userid {userId} disconnected from lobbyid {lobbyId}");
        UpdateActivity();
    }

    private void LobbyManager_OnMemberConnect(long lobbyId, long userId)
    {
        print($"Userid {userId} connected to lobbyid {lobbyId}");
        var t = lobbyManager.GetMemberUpdateTransaction(lobbyId, userId);
        t.SetMetadata("test", "hello");

        lobbyManager.UpdateMember(lobbyId, userId, t, result => { if (result == Result.Ok) print("ok"); });
        UpdateActivity();
    }

    private void UpdateActivity()
    {
        var activity = new Activity
        {
            ApplicationId = 881860326096785409,
            State = "Waiting for players",
            Details = "Co-op",
            Party =
                {
                    Id = lobbyid.ToString(),
                    Size =
                    {
                        MaxSize = 6,
                        CurrentSize = lobbyManager.MemberCount(lobbyid)
                    }
                },
            Secrets =
                {
                    Join = lobbyManager.GetLobbyActivitySecret(lobbyid)
                },
            Instance = true
        };

        activityManager.UpdateActivity(activity, result =>
        {
            print($"Update Activity {result}");
        });
    }

    void OnDestroy()
    {
        activityManager.ClearActivity(result => { });
        if (lobbyManager.LobbyCount() > 0)
        {
            lobbyManager.DeleteLobby(lobbyid, result => { });
        }
        discord.Dispose();
    }

    public void LogProblemsFunction(LogLevel level, string message)
    {
        print($"Discord:{level} - {message}");
    }


    // Update is called once per frame
    void Update()
    {
        discord.RunCallbacks();
        lobbyManager = discord.GetLobbyManager();
        if (lobbyManager.LobbyCount() > 0)
        {
            string posrot = Players[userManager.GetCurrentUser().Id].Player.GetPositionAndRotation();
            lobbyManager.SendNetworkMessage(lobbyManager.GetLobbyId(0), userManager.GetCurrentUser().Id, 0, System.Text.Encoding.UTF8.GetBytes(posrot));
        }
    }

    private void LateUpdate()
    {
        lobbyManager = discord.GetLobbyManager();
        lobbyManager.FlushNetwork();
    }
}

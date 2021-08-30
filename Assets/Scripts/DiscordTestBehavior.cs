using System;
using System.Collections;
using UnityEngine;
using Discord;

public class DiscordTestBehavior : MonoBehaviour
{
    Discord.Discord discord;
    LobbyManager lobbyManager;
    ActivityManager activityManager;
    long lobbyid;
    // Use this for initialization
    void Start()
    {
        var clientID = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID");
        if (clientID == null)
        {
            clientID = "881860326096785409";
        }
        discord = new Discord.Discord(Int64.Parse(clientID), (UInt64)CreateFlags.Default);
        discord.SetLogHook(LogLevel.Debug, LogProblemsFunction);

        activityManager = discord.GetActivityManager();
        lobbyManager = discord.GetLobbyManager();
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
                Instance = true
            };

            activityManager.UpdateActivity(activity, result =>
            {
                print($"Update Activity {result}");
            });
        });
    }

    void OnDestroy()
    {
        activityManager.ClearActivity(result => { });
        lobbyManager.DeleteLobby(lobbyid, result => { });
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
    }
}
using UnityEngine;
using System.Collections;
using MLAPI;
using MLAPI.NetworkVariable;
using TMPro;
using System;

namespace Assets.Scripts
{
    public class PlayerDisplayName : NetworkBehaviour
    {
        [SerializeField] private TMP_Text DisplayText;

        public override void NetworkStart()
        {
            if (!IsServer) { return; }

            var data = LobbyNetworkBehavior.GetPlayerData(OwnerClientId);

            if (data.HasValue)
            {
                DisplayVar.Value = data.Value.PlayerName;
            }
        }

        private NetworkVariableString DisplayVar = new NetworkVariableString();

        private void OnEnable()
        {
            DisplayVar.OnValueChanged += HandleNameChange;
        }

        private void OnDisable()
        {
            DisplayVar.OnValueChanged -= HandleNameChange;
        }

        private void HandleNameChange(string previousValue, string newValue)
        {
            DisplayText.text = newValue;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    [SerializeField] private TMP_Text playerNameTxt;

    //Variable to hold the name of the player
    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(
        new FixedString64Bytes("Player Name"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnNameChanged;

        playerNameTxt.SetText(playerName.Value.ToString());
        gameObject.name = "Player_" + playerName.Value.ToString();

        if(IsLocalPlayer)
        {
            GameManager.instance.SetLocalPlayer(NetworkObject);
        }

        GameManager.instance.OnPlayerJoined(NetworkObject);
    }

    private void OnNameChanged(FixedString64Bytes preVal, FixedString64Bytes newVal)
    {
        if(newVal != preVal)
        {
            playerNameTxt.SetText(newVal.Value);
            GameManager.instance.SetPlayerName(NetworkObject, newVal.Value.ToString());

        }
    }

    public void SetName(string name)
    {
        playerName.Value = new FixedString64Bytes(name);
    }

    public override void OnNetworkDespawn()
    {
        playerName.OnValueChanged -= OnNameChanged;
    }
}

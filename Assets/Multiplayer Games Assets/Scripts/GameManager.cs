using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    [SerializeField] private Transform[] startPositions;
    void Singleton()
    {
        if(instance != null && instance != this)
        {
            Destroy(instance);
        }
        instance = this;
    }

    [SerializeField] private TMP_InputField playerNameField;

    private NetworkObject _localPlayer;

    Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();
    Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();

    private void Awake()
    {
        Singleton();
    }
    public void SetLocalPlayer(NetworkObject localPlayer)
    {
            _localPlayer = localPlayer;

        //Set name of player
        if(playerNameField.text.Length > 0)
        {
            _localPlayer.GetComponent<PlayerInfo>().SetName(playerNameField.text);
        }
        else
        {
            _localPlayer.GetComponent<PlayerInfo>().SetName($"Player {_localPlayer.OwnerClientId}");
        }

        playerNameField.gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    internal void OnPlayerJoined(NetworkObject networkObject)
    {
      networkObject.transform.position = startPositions[(int)networkObject.OwnerClientId].position;

    }

    public void SetPlayerName(NetworkObject playerObject, string name)
    {
        if (playerNames.ContainsKey(playerObject.OwnerClientId))
        {
            playerNames[playerObject.OwnerClientId] = name;
        }
        else
        {
            playerNames.Add(playerObject.OwnerClientId, name);
        }
    }
}

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

    [SerializeField] private TMP_Text scoreUITxt;
    [SerializeField] private GameObject endGameScreen;
    [SerializeField] private TMP_Text endGameMessage;
    private NetworkObject _localPlayer;

    /// <summary>
    /// 0: Offline, 1 -In Game, 2 : Game Ended // Everyone can read but only the server can change state value
    /// </summary>
    /// 
    public NetworkVariable<short> state = new NetworkVariable<short>(
        0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();
    Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();

    private void Awake()
    {
        Singleton();

        if (IsServer)
        {
            state.Value = 0;
        }
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
        playerScores.Add(networkObject.OwnerClientId, 0);

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

    public void StartGame()
    {
        state.Value = 1;
        ShowScoreUI();

    }

    public void AddScore(ulong playerID)
    {
        if (IsServer)
        {
            playerScores[playerID]++;
            ShowScoreUI();
            CheckWinner(playerID);
        }
    }
    void CheckWinner(ulong playerID)
    {
        if (playerScores[playerID] >= 10)
        {
            EndGame(playerID);
        }
    }

    private void EndGame(ulong winnerID)
    {
        if (IsServer)
        {
            //Show UI directly
            endGameScreen.SetActive(true);
            if(winnerID == NetworkManager.LocalClientId)
            {
                endGameMessage.text = "YOU WIN :)";
            }
            else
            {
                endGameMessage.text = $"YOU LOSE!\n The WINNER IS {playerNames[winnerID]}";
            }

            ScoreInfo tempScoreInfo = new();
            tempScoreInfo.score = playerScores[winnerID];
            tempScoreInfo.id = winnerID;
            tempScoreInfo.name = playerNames[winnerID];

            ShowGameEndUIClientRPC(JsonUtility.ToJson(tempScoreInfo));
        }
    }

    [ClientRpc]
    public void ShowGameEndUIClientRPC(string winnerInfo)
    {
        endGameScreen.SetActive(true );
        ScoreInfo scoreInfo = JsonUtility.FromJson<ScoreInfo>(winnerInfo);

        if (scoreInfo.id == NetworkManager.LocalClientId)
        {
            endGameMessage.text = "YOU WIN";
        }
        else
        {
            endGameMessage.text = $"YOU LOSE!\n Winner is {scoreInfo.name}";
        }
    }

    public void ShowScoreUI()
    {
        scoreUITxt.text = "";

        PlayerScores scores = new();
        scores.scores = new List<ScoreInfo>();

        foreach (var playerScore in playerScores)
        {
            ScoreInfo tempScoreInfo = new();
            tempScoreInfo.score = playerScore.Value;
            tempScoreInfo.id = playerScore.Key;
            tempScoreInfo.name = playerNames[playerScore.Key];

            scores.scores.Add(tempScoreInfo);

            scoreUITxt.text += $"[{playerScore.Key}] {playerNames[playerScore.Key]} : {playerScore.Value}/10\n";
        }

        //Update all Clients
        UpdateClientScoreClientRpc(JsonUtility.ToJson(scores));
    }

    [ClientRpc]
    public void UpdateClientScoreClientRpc(string scoreInfo)
    {
        PlayerScores scores = JsonUtility.FromJson<PlayerScores>(scoreInfo);
        scoreUITxt.text = "";
        foreach (var score in scores.scores)
        {
            scoreUITxt.text += $"[{score.id}] {score.name} : {score.score}/10 \n";
        }
    }
}

[System.Serializable]
public class PlayerScores
{
    public List<ScoreInfo> scores;
}

[System.Serializable]
public class ScoreInfo
{
    public ulong id;
    public string name;
    public int score;
}

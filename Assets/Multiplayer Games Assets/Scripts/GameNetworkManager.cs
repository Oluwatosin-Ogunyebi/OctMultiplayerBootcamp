using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{

    [SerializeField] private TMP_Text joinStatusTxt;
    [SerializeField] private TMP_InputField ipAddress;
    [SerializeField] private UnityTransport transport;
    [SerializeField] private TMP_InputField playerNameTxt;

    public void JoinHost()
    {
        NetworkManager.Singleton.StartHost();
        joinStatusTxt.SetText($" {playerNameTxt.text} Joined As Host");
    }

    public void JoinClient()
    {
        //transport.ConnectionData.Address = ipAddress.text.Replace(" ", "");
        NetworkManager.Singleton.StartClient();
        joinStatusTxt.SetText($" {playerNameTxt.text} Joined as Client");
    }

    public void JoinServer()
    {
        NetworkManager.Singleton.StartServer();
        joinStatusTxt.SetText("Joined as Server");
    }

    public string GetPlayerName()
    {
       return playerNameTxt.text;
    }


}

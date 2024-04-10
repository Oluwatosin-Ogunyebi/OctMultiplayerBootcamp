using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using System;
using Unity.VisualScripting;

using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;

public class GameNetworkManager : MonoBehaviour
{

    [SerializeField] private TMP_Text joinStatusTxt;
    [SerializeField] private TMP_InputField ipAddress;
    [SerializeField] private UnityTransport transport;
    [SerializeField] private TMP_InputField playerNameTxt;

    [SerializeField] private TMP_Text playerIDText;
    [SerializeField] private TMP_InputField joinCodeTxt;

    [SerializeField] private int maxConnections = 8;


    private string playerID;
    private bool clientAuthenticated = false;
    private string joinCode;


    private async void Start()
    {
        await AuthenticatePlayer();
    }
    async Task AuthenticatePlayer()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerID = AuthenticationService.Instance.PlayerId;
            clientAuthenticated = true;
            playerIDText.text = $"Player ID is {playerID}";

            Debug.Log($"Client Authentication successful {playerID}");
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async Task<RelayServerData> AllocateRelayServerAndGetCode(int maxConnections, string region = null)
    {
        Allocation allocation;

        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch (Exception e)
        {
            Debug.Log($"Relay allocation failed - {e}");
            throw;
        }

        Debug.Log($"server: {allocation.ConnectionData[0]}, {allocation.ConnectionData[1]}");
        Debug.Log($"server; {allocation.AllocationId}");

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (Exception e)
        {
            Debug.Log($"Unable to create join code {e}");
            throw;
        }

        return new RelayServerData(allocation, "dtls");
    } 

    IEnumerator ConfigureJoinCodeAndJoinHost()
    {
        var allocationCode = AllocateRelayServerAndGetCode(maxConnections);

        while(!allocationCode.IsCompleted)
        {   
            //Wait until we create allocation and get code
            yield return null;
        }

        if (allocationCode.IsFaulted)
        {
            Debug.Log($"Unable to create server due to {allocationCode.Exception.Message}");
            yield break;
        }

        var relayServerData = allocationCode.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();

        joinCodeTxt.gameObject.SetActive(true);
        joinCodeTxt.text = joinCode;
        joinStatusTxt.SetText($" {playerNameTxt.text} Joined As Host");
    }

    public void JoinHost()
    {
        if(!clientAuthenticated)
        {
            Debug.Log("Client not Authenticated, try again");
            return;
        }
        StartCoroutine(ConfigureJoinCodeAndJoinHost());
    }

    public async Task<RelayServerData> JoinRelayServerWithCode(string joinCode)
    {
        JoinAllocation allocation;

        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.Log($"Request join allocation failed: {e}");
            throw;
        }

        Debug.Log($"server: {allocation.ConnectionData[0]}, {allocation.ConnectionData[1]}");
        Debug.Log($"server: {allocation.HostConnectionData[0]}, {allocation.HostConnectionData[1]}");
        Debug.Log($"server; {allocation.AllocationId}");


        return new RelayServerData(allocation, "dtls");
    }

    IEnumerator ConfigureAndUseJoinClient(string joinCode)
    {
        var joinAllocationFromCode = JoinRelayServerWithCode(joinCode);

        while (!joinAllocationFromCode.IsCompleted)
        {
            yield return null;
        }

        if (joinAllocationFromCode.IsFaulted)
        {
            Debug.Log($"Unable to join host due to {joinAllocationFromCode.Exception.Message}");
            yield break;
        }

        var relayServerData = joinAllocationFromCode.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();

        joinStatusTxt.SetText($" {playerNameTxt.text} Joined As Client");
    }
    public void JoinClient()
    {
        if (!clientAuthenticated)
        {
            Debug.Log("CLient not Authenticated, Try again!");
            return;
        }

        if (joinCodeTxt.text.Length <= 0)
        {
            Debug.Log("Enter Appropriate Join Code");
            joinStatusTxt.SetText("Enter Appropriate Join Code");
        }

        StartCoroutine(ConfigureAndUseJoinClient(joinCodeTxt.text));
    }

    public void JoinServer()
    {


    }

    public string GetPlayerName()
    {
       return playerNameTxt.text;
    }


}

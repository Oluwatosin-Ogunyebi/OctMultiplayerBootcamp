using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

//TO DO: Instead of just posting on the server, update everyone
//TO DO: Let each chat show up in a new line
public class BasicChat : NetworkBehaviour
{
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private TMP_InputField chatInputPrivate;
    [SerializeField] private TMP_Text chatText;

    public void SendChat()
    {
        ulong id = ulong.Parse(chatInputPrivate.text);
        if (IsServer)
        {

            ChatClientRPC(NetworkManager.Singleton.LocalClientId + ": " + chatInput.text,id); //Calls Clients
        }
        else if (IsClient)
        {
            ChatServerRPC(NetworkManager.Singleton.LocalClientId + ": " + chatInput.text, id); //Calls the server
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChatServerRPC(string message)
    {
        if (!IsHost)
            chatText.text += "\n" + message;

        ChatClientRPC(message);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChatServerRPC(string message, ulong id)
    {
        if (!IsHost)
            chatText.text += "\n" + message;

        ChatClientRPC(message, id);
    }

    [ClientRpc]
    public void ChatClientRPC(string message)
    {
        chatText.text += "\n" + message;
    }

    [ClientRpc]
    public void ChatClientRPC(string message, ulong clientID)
    {
        if (clientID == NetworkManager.Singleton.LocalClientId)
        {
            chatText.text += "\n" + message;
        }

    }
}

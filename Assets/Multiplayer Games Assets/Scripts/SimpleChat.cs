using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SimpleChat : NetworkBehaviour
{
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private TMP_Text chatText;
    
    public void SendChat()
    {
        if(IsServer)
        {
            ChatClientRPC();
        }else if(IsClient)
        {
            ChatServerRPC();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChatServerRPC()
    {
        chatText.text = "A client says hi";
    }

    [ClientRpc]
    public void ChatClientRPC()
    {
        chatText.text = "Server says hi";
    }
}

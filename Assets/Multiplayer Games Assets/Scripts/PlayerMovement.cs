using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float tankSpeed = 10.0f;
    [SerializeField] private float tankTurnSpeed = 10.0f;
    [SerializeField] private TMP_Text playerNameTxt;


    Rigidbody tankRb;
    GameNetworkManager gameNetworkManager;
    float horizontal;
    float vertical;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        tankRb = GetComponent<Rigidbody>();
        //gameNetworkManager = FindObjectOfType<GameNetworkManager>();
        //playerNameTxt.SetText(gameNetworkManager.GetPlayerName());
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (IsServer && IsLocalPlayer)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }else if (IsClient && IsLocalPlayer) {
            MovementServerRPC(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        }



        playerNameTxt.transform.parent.LookAt(Camera.main.transform.position);
        playerNameTxt.transform.parent.RotateAround(transform.position, transform.up, 0f);
        Debug.Log("Looking at Player");
    }

    private void FixedUpdate()
    {
        tankRb.velocity = tankRb.transform.forward * tankSpeed * vertical;
        tankRb.rotation =  Quaternion.Euler(transform.eulerAngles + transform.up * horizontal * tankTurnSpeed);
    }

    [ServerRpc]
    public void MovementServerRPC(float horizontal, float vertical)
    {
        this.horizontal = horizontal;
        this.vertical = vertical;
    }
}

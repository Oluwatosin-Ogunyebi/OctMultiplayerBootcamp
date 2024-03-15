using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float tankSpeed = 10.0f;
    [SerializeField] private float tankTurnSpeed = 10.0f;

    Rigidbody tankRb;

    float horizontal;
    float vertical;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        tankRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        tankRb.velocity = tankRb.transform.forward * tankSpeed * vertical;
        tankRb.rotation =  Quaternion.Euler(transform.eulerAngles + transform.up * horizontal * tankTurnSpeed);
    }
}

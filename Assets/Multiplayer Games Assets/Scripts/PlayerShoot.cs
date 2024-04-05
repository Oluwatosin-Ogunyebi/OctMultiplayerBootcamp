using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    [SerializeField] private GameObject bullet;

    [SerializeField] private float shootSpeed;
    [SerializeField] private Transform shootPoint;

    private Rigidbody tankRb;

    public override void OnNetworkSpawn()
    {
        tankRb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        

        if(Input.GetButtonDown("Jump"))
        {
            if (IsServer && IsLocalPlayer)
            {
                Shoot(OwnerClientId);
            }else if (IsClient && IsLocalPlayer)
            {
                RequestShootServerRPC();
            }
            
        }
    }
    [ServerRpc]
    public void RequestShootServerRPC(ServerRpcParams serverRpcParams = default)
    {
        Shoot(serverRpcParams.Receive.SenderClientId);
    }
    void Shoot(ulong ownerID)
    {
        GameObject bulletObject = Instantiate(bullet, shootPoint.position,shootPoint.rotation);

        bulletObject.GetComponent<NetworkObject>().Spawn();
        bulletObject.GetComponent<Bullet>().clientID = ownerID;

        bulletObject.GetComponent<Rigidbody>().AddForce(tankRb.velocity + bulletObject.transform.forward * shootSpeed, ForceMode.VelocityChange);

        Destroy(bulletObject, 5.0f);
    }
}

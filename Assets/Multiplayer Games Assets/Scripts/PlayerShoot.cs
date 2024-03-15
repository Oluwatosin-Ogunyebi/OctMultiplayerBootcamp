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
        if(!IsOwner) return;

        if(Input.GetButtonDown("Jump"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject bulletObject = Instantiate(bullet, shootPoint.position,shootPoint.rotation);

        bulletObject.GetComponent<NetworkObject>().Spawn();

        bullet.GetComponent<Rigidbody>().AddForce(tankRb.velocity + bulletObject.transform.forward * shootSpeed, ForceMode.VelocityChange);

        Destroy(bulletObject, 5.0f);
    }
}

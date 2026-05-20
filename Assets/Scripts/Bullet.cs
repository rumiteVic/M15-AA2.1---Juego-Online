using UnityEngine;
using Unity.Netcode;
public class Bullet : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke(nameof(DeleteBullet), 10f);
    }

    void DeleteBullet()
    {
        if (IsServer)
        {
            NetworkObject.Despawn(true);
        }
    }
}

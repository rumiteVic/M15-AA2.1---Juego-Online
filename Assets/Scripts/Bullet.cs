using UnityEngine;
using Unity.Netcode;
public class Bullet : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke(nameof(DeleteBullet), 5f);
    }
    //Borrar el bulletHole despues de un tiempo (lo borra el server y despues de 5seg)
    void DeleteBullet()
    {
        if (IsServer)
        {
            NetworkObject.Despawn(true);
        }
    }
}

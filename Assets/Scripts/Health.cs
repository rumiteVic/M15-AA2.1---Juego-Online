using UnityEngine;
using Unity.Netcode;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
public class Health : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    public NetworkVariable<int> vidaActual =  new NetworkVariable<int>(100);
    public NetworkVariable<int> puntuation = new NetworkVariable<int>(0);

    public NetworkVariable<Vector3> inicio;
    public Rigidbody rb;

    public ClientNetworkTransform transformer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            vidaActual.Value = health.Value;
            inicio.Value = transformer.transform.position;

        }
    }



    public void Muere()
    {
        transformer.transform.position = inicio.Value;
        vidaActual.Value = health.Value;
        Debug.Log("muerto");
    }

    public void MorePuntos()
    {
        puntuation.Value +=100;
        Debug.Log(puntuation);
    }
    [Rpc(SendTo.Server)]
    public void PlsDoDamageRpc(int damage, ulong whoAttack)
    {
        if(OwnerClientId == whoAttack)
        {
            return;
        }
        vidaActual.Value -= damage;
        if(vidaActual.Value <= 0)
        {
            Muere();
        }

    }
}

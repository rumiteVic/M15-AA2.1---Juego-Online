using UnityEngine;
using Unity.Netcode;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
public class Health : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    public NetworkVariable<int> vidaActual =  new NetworkVariable<int>(100);

    public NetworkVariable<Vector3> inicio;
    public Rigidbody rb;

    public ClientNetworkTransform transformer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        //Aplica vida al jugador
        if (IsServer)
        {
            vidaActual.Value = health.Value;
            inicio.Value = transformer.transform.position;

        }
    }

    //Server se entera de la muerte
    [Rpc(SendTo.Server)]
    public void MuereRpc()
    {
        vidaActual.Value = health.Value;

        TeleportPlayerRpc(inicio.Value);

        Debug.Log("muerto");
    }
    //La pantalla del owner hace que se haga tp
    [Rpc(SendTo.Owner)]
    void TeleportPlayerRpc(Vector3 pos)
    {
        transform.position = pos;
    }
    //Server recibe que alguien hizo daño y le aplica el daño
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
            MuereRpc();
            GivePoints(whoAttack);
        }

    }

    //Encontramos al cliente que ha matado (su script de score) y le damos puntos
    void GivePoints(ulong attackerId)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == attackerId)
            {
                Score s = client.PlayerObject.GetComponent<Score>();

                if (s != null)
                {
                    s.MorePuntos();
                    Debug.Log("Puntos para: " + attackerId);
                }

                break;
            }
        }
    }
}

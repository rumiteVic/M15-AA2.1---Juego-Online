using UnityEngine;
using Unity.Netcode;
public class Score : NetworkBehaviour
{
    public NetworkVariable<int> puntuation =  new NetworkVariable<int>(0);
    public void MorePuntos()
    {
        MorePuntosRpc();
    }
    //Obtiene 100 puntos por matar
    [Rpc(SendTo.Server)]
    void MorePuntosRpc()
    {
        puntuation.Value += 100;
    }
}

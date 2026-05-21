using UnityEngine;
using Unity.Netcode;
public class Health : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    public NetworkVariable<int> vidaActual;
    public NetworkVariable<int> puntuation = new NetworkVariable<int>(0);

    public NetworkVariable<Vector3> inicio;
    public Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
{
    if (IsServer)
    {
        vidaActual.Value = health.Value;
        inicio.Value = transform.position;

    }
}

    public void TakeDamage(int damage)
    {
        vidaActual.Value -= damage;
        Debug.Log("piu, piu");
        if(vidaActual.Value <= 0)
        {
            Muere();
        }
    }

    void Muere()
    {
        transform.position = inicio.Value;
        vidaActual.Value = health.Value;
    }

    public void MorePuntos()
    {
        puntuation.Value +=100;
    }
}

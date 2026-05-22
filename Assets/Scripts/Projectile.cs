using UnityEngine;
using Unity.Netcode;
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : NetworkBehaviour
{
    public float disappearTime = 5f;
    public Vector3 forceMin = new Vector3(-1, -1, 50);
    public Vector3 forceMax = new Vector3(1, 1, 100);
    public LayerMask layers;
    public float collisionForceMultiplier = 2f;
    public float radius = .1f;
    public GameObject spawnOnCollide;
    public Rigidbody rb;

    public Health healthy = null;
    public ulong ownerID;
    int damage = 21;
    Vector3 lastPos;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddRelativeForce(new Vector3(Random.Range(forceMin.x, forceMax.x), Random.Range(forceMin.y, forceMax.y), Random.Range(forceMin.z, forceMax.z)));
        Invoke(nameof(DeleteCosa), disappearTime);
        lastPos = transform.position;
        transform.parent = null;
    }

    private void FixedUpdate()
    {
        Vector3 dir = transform.position - lastPos;

        Debug.DrawRay(lastPos, dir, Color.blue, disappearTime);

        RaycastHit hit;

        if (Physics.SphereCast(lastPos, radius, dir, out hit, dir.magnitude, layers))
        {
            Hitted(hit);
        }

        lastPos = transform.position;
    }

    void Hitted(RaycastHit hit)
    {
        if (spawnOnCollide)
        {
            SpawnObjectServerRPC(hit.point, hit.normal);
        }
        if (hit.rigidbody)
        {
            hit.rigidbody.AddForceAtPosition(rb.linearVelocity * rb.mass * collisionForceMultiplier, this.transform.position);
            //Miramos si le damos a un jugador y hacemos que vaya a recibir daño
            Health targetHealth = hit.rigidbody.GetComponent<Health>();
            if(targetHealth != null)
            {
                targetHealth.PlsDoDamageRpc(damage, ownerID);
            }
        }
        DeleteCosa();
    }
    //Instanciamos el bullethole
    [Rpc(SendTo.Server)]
    void SpawnObjectServerRPC(Vector3 point, Vector3 normal)
    {
        GameObject go = Instantiate(spawnOnCollide, point, Quaternion.LookRotation(normal));
        go.GetComponent<NetworkObject>().Spawn();

    }
    //Si aun esta spawneado, el server borra ese objeto
    void DeleteCosa()
    {
        if (IsServer && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }

}

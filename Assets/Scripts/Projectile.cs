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
            Health health = hit.rigidbody.GetComponent<Health>();
            if(health != null)
            {
                if(health.OwnerClientId != ownerID)
                {
                    health.TakeDamage(damage);
                    DeleteCosa();
                }
            }
        }
        DeleteCosa();
    }

    [ServerRpc]
    void SpawnObjectServerRPC(Vector3 point, Vector3 normal)
    {
        GameObject go = Instantiate(spawnOnCollide, point, Quaternion.LookRotation(normal));
        go.GetComponent<NetworkObject>().Spawn();

    }

    void DeleteCosa()
    {
        if (IsServer)
        {
            NetworkObject.Despawn(true);
        }
    }

}

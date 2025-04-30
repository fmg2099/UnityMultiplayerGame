using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    public float damage = 55f; //daño del proyectil
    public PlayerController instigator; //quien disparo el proyectil
    public Vector3 direction;
    public GameObject impactPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //el servidor es el que tiene la autoridad sobre el proyectil
        if (IsServer)
        {
            lifetime -= Time.deltaTime;

            if (lifetime <= 0)
            {
                //Destroy(gameObject);
                GetComponent<NetworkObject>().Despawn();
            }
            transform.position += direction * speed * Time.deltaTime;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(!IsServer) return;


        PlayerController otherPlayer = other.GetComponent<PlayerController>();

        if(otherPlayer != null && otherPlayer != instigator)
        {
            otherPlayer.TakeDamage((int)damage);
            OnImpactRpc();
            GetComponent<NetworkObject>().Despawn();
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void OnImpactRpc()
    {
        //spawnear el efecto de impacto
        if (impactPrefab != null)
        {
            GameObject impact = Instantiate(impactPrefab, transform.position, Quaternion.identity);
            Destroy(impact, 2f); //destruir el efecto de impacto después de 2 segundos
        }
    }
}


using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class DamageVolume : MonoBehaviour
{
    public float initialDamage = 20;
    //dano causado en cada paso
    public float damagePerStep = 10;
    //cada cuanos segundos causa dano
    public float damageRate = 0.5f;
    float damageTimer=0;

    //como es juego multiplayer, uede ser que haya varios personajes adentro del fuego purificador
    public List<PlayerController> players;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        damageTimer += Time.deltaTime;
    }

    public void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            Debug.Log(other + "ha entrado");
            players.Add(pc);
            pc.TakeDamage( (int)initialDamage  );
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Debug.Log(other + " se salio");
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            players.Remove(pc);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if( damageTimer > damageRate )
        {
            foreach (PlayerController pc in players)
            {
                pc.TakeDamage((int)damagePerStep);
            }
            damageTimer = 0;
        }
    }
}

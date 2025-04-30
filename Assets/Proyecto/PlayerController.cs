using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    //hacia donde apunta el character
    Vector3 desiredDirection;
    public float speed = 1.0f;

    //salud del personaje
    //public int health = 100;

    [Header("Camera")]
    public Vector3 cameraOffset = new Vector3(0, 4f, -3);
    public Vector3 cameraViewOffset = new Vector3(0, 1.5f, 0);
    Camera cam;

    [Header("Weapon")]
    public GameObject projectilePrefab;
    public Transform weaponSocket;
    public float weaponCadence = 0.8f; //tiempo entre disparos
    float lastShotTimer = 0; //tiempo del ultimo disparo

    //networkVariable para poder replicar health
    NetworkVariable<int> health = new NetworkVariable<int>(100,  //valor inicial
        NetworkVariableReadPermission.Everyone, //permisos de lectura
        NetworkVariableWritePermission.Server   //permisos d eescritura
        );

    private GameManager gameManager;    
    private UIManager hud;
    private TMP_Text playerName;

    [Header("SFX")]
    public AudioClip DamageSound;
    public AudioClip DeathSound;
    AudioSource audioSource;

    public override void OnNetworkSpawn()
    {
        Debug.Log("hola mundo soy un " + (IsClient ? "cliente" : "servidor"));
        Debug.Log("isClient=" + IsClient + ", server=" + IsServer + ", host=" + IsHost);
        Debug.Log(name + " is owner=" + IsOwner);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hud = GameObject.Find("GameManager").GetComponent<UIManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        audioSource = GetComponent<AudioSource>();
        //pedirle al gamemanager un spawnpoint valido
        transform.position = gameManager.GetSpawnPoint();

        //asignar la camara
        if (IsOwner)
        {
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();
            cam.transform.position = transform.position + cameraOffset;
            cam.transform.LookAt(transform.position + cameraViewOffset);
        }

        createPlayerNameHUD();
    }

    void createPlayerNameHUD()
    {
        if (IsClient)
        {
            playerName = Instantiate(hud.playerNameTemplate, hud.PanelHUD).GetComponent<TMP_Text>();
            playerName.gameObject.SetActive(true);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            // inputs
            desiredDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            desiredDirection.Normalize();

            

            if (isAlive())
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    FireWeaponRpc();
                }

                //movimientos
                float mag = desiredDirection.magnitude;
                if (mag > 0)
                {
                    //transform.forward = desiredDirection;
                    //interpolar entre la rotacion actual y la deseada
                    Quaternion q = Quaternion.LookRotation(desiredDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * 10);



                    transform.Translate(0, 0, speed * Time.deltaTime);
                }

                //temporal para probar el sistema de vida
                if (Input.GetKeyDown(KeyCode.T))
                {
                    TakeDamage(55);
                }
            }

            //actualizar la posicion de la camara
            cam.transform.position = transform.position + cameraOffset;
            cam.transform.LookAt(transform.position + cameraViewOffset);

            //actualizar hud
            hud.labelHealth.text = health.Value + "";
        }

        if (IsClient)
        {
            //playerName arriba del jugador
            Camera maincam = GameObject.Find("Main Camera").GetComponent<Camera>();
            playerName.transform.position = maincam.WorldToScreenPoint(transform.position + new Vector3(0, 1.2f, 0));
        }

        if (IsServer)
        {
            lastShotTimer += Time.deltaTime;
        }

        //


    }

    //RPC - Remote Call Procedure, llamada a procedimiento remoto
    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(int amount)
    {
        Debug.Log(" rpc recibido takedamage");
        TakeDamage(amount);
    }

    public void  TakeDamage(int amount)
    {
        if (!isAlive()) return;

        if (!IsServer)
        {
            TakeDamageRpc(amount);
        }
        else
        {
            health.Value -= amount;
            if (health.Value <= 0)
            {
                Debug.Log("muere");
                health.Value = 0;
                OnDeath();
            }
            else
            {
                audioSource.clip = DamageSound;
                audioSource.Play();
            }
        }
    }

    public void OnDeath()
    {
        //efectos
        Debug.Log(name+" me muero");
        audioSource.clip = DeathSound;
        audioSource.Play();
    }

    public bool isAlive()
    {
        return health.Value > 0;
    }

    [Rpc(SendTo.Server)]
    public void FireWeaponRpc()
    {
        //no disparar si no ha pasado el tiempo de recarga
        if (lastShotTimer < weaponCadence) return; 

        if ( projectilePrefab!= null)
        {
            Projectile proj = Instantiate(projectilePrefab,
                weaponSocket.position,
                 weaponSocket.rotation).GetComponent<Projectile>();

            proj.direction =  transform.forward; //sale en la direccion que apunta el personaje
            proj.instigator = this; //quien disparo el proyectil

            proj.GetComponent<NetworkObject>().Spawn(); //spawnear el proyectil en la red para que se replique
            lastShotTimer = 0; //reiniciar el timer
        }
    }
}

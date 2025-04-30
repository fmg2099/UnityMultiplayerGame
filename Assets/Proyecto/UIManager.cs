using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Netcode.Transports.UTP;

public class UIManager : MonoBehaviour
{
    public RectTransform PanelMainMenu;
    public RectTransform PanelClient;
    public RectTransform PanelHUD;
    public TMP_Text labelHealth;
    public GameObject playerNameTemplate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PanelMainMenu.gameObject.SetActive(true);
        PanelClient.gameObject.SetActive(false);
        PanelHUD.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonStartHost()
    {
        //crear una partida hospedada
        NetworkManager.Singleton.StartHost();
        Debug.Log("host created");
        PanelMainMenu.gameObject.SetActive(false);
        PanelHUD.gameObject.SetActive(true);
    }

    public void OnButtonClientConnect()
    {
        GameObject go = GameObject.Find("inputIP");
        string ip = go.GetComponent<TMP_InputField>().text;
        Debug.Log("conectando a " + ip);
        PanelMainMenu.gameObject.SetActive(false);
        PanelClient.gameObject.SetActive(false);

        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = ip;

        NetworkManager.Singleton.StartClient();
        PanelHUD.gameObject.SetActive(true);
    }
}

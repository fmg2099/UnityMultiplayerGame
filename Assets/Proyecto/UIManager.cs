using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Netcode.Transports.UTP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking; //para poder hacer peticiones web



//tipo de dato para convertir string JSON a un objeto
public struct NamesData
{
    //el nombre de este campo debe coincidir con el nombre del campo en el JSON
    public string[] names;
}


public class UIManager : MonoBehaviour
{
    [Header("Menus")]
    public RectTransform PanelMainMenu;
    public TMP_Dropdown namesSelector;
    public RectTransform PanelClient;

    [Header("HUD")]
    public RectTransform PanelHUD;
    public TMP_Text labelHealth;
    public GameObject playerNameTemplate;

    //lista de los nombres permitidos
    public List<string> namesList = new List<string>();

    public int selectedNameIndex { 
        get { return namesSelector.value; } 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PanelMainMenu.gameObject.SetActive(true);
        PanelClient.gameObject.SetActive(false);
        PanelHUD.gameObject.SetActive(false);

        //Debug.Log(namesSelector.ToString()) ;
        GetNames();

    }

    // obtener la lista de nombres permitidos y ponerla en el dropdown
    public void GetNames()
    {
        namesSelector.ClearOptions();
        StartCoroutine(GetNamesFromServer());
    }

    //Las peticiones web son asincronas, por lo que debemos usar una coroutine
    IEnumerator GetNamesFromServer()
    {
        //URL del endpoint 
        string url = "http://monsterballgo.com/api/names";
        UnityWebRequest www = UnityWebRequest.Get(url); //peticion GET
        yield return www.SendWebRequest(); //esperar a que se complete la peticion

        //retorna codigo 200 si todo va bien
        if (www.result == UnityWebRequest.Result.Success)
        {
            //convertir el cuerpo de la respuesta a un string JSON
            string json = www.downloadHandler.text;
            NamesData namesData = JsonUtility.FromJson<NamesData>(json);
            namesList.AddRange(namesData.names); //agregar los nombres a la lista

            //poner la lista de nombres en el dropdown
            namesSelector.AddOptions(namesList);

        }
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UImanager : MonoBehaviour
{
    public static UImanager instance;
    public GameObject startmenu;
    public TMP_InputField usernamefield;

    public TMP_InputField IPfield;

    public TextMeshProUGUI publicIP;

    private void Awake()
    {   
        if(instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        publicIP = GameObject.Find("IPtext").GetComponent<TextMeshProUGUI>();
    }

    public void ConnectToServer()
    {
        startmenu.SetActive(false);
        usernamefield.interactable = false;
        IPfield.interactable = false;
        if(!string.IsNullOrEmpty(IPfield.text))
        {
            Client.instance.SetIP(IPfield.text);
        }
        else
        {
            //if we don't have any input, assume the server is being hosted on the same computer
            Client.instance.SetIP("127.0.0.1");
        }
        Client.instance.ConnectToServer();
    }

    //this is done in the client script. I know it's stupid, im sorry
    public void SetPublicIP(string _ip)
    {
        publicIP.text = "Your IP: " + _ip;
    }
}
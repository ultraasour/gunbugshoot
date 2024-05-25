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
            Client.instance.SetIP("127.0.0.1");
        }
        Client.instance.ConnectToServer();
    }
}
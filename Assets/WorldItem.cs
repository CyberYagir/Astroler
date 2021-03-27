using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public TMP_Text text_name;
    public TMP_Text text_vers;
    public string ip;


    public void DeleteWorld()
    {
        FindObjectOfType<MenuInitialize>().DeleteFile(text_name.text);
    }

    public void Connect()
    {
        FindObjectOfType<MenuInitialize>().address.text = text_name.text;
        FindObjectOfType<MenuInitialize>().Connect();
    }
    public void Set(string nm, string ver)
    {
        text_name.text = nm;
        text_vers.text = ver;
    }
    public void Play()
    {
        PlayerPrefs.SetString("World", text_name.text);
        if (text_vers.text == Application.version)
        {
            FindObjectOfType<Menu>().SetName();
            FindObjectOfType<MenuInitialize>().StartHost();
        }
        else
        {
            FindObjectOfType<MenuInitialize>().worldUpdateError.gameObject.SetActive(true);
            FindObjectOfType<MenuInitialize>().worldsListMenu.gameObject.SetActive(false);

        }
    }
    public void NoThrowPlay()
    {
        FindObjectOfType<Menu>().SetName();
        FindObjectOfType<MenuInitialize>().StartHost();
    }
}

using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Inventory;

public class Menu : NetworkManager
{
    Renderer playerSkin;

    private void FixedUpdate()
    {
        //print("Server active: " + NetworkServer.active);
        if (GetComponent<LoaderResources>().skin != null)
        {
            if (playerSkin != null)
            {
                playerSkin.material.SetTexture("_MainTex", GetComponent<LoaderResources>().skin);
            }
        }
    }
    public override void OnClientDisconnect(NetworkConnection conn)
    {
       var serv =   FindObjectsOfType<NetworkPlayer>().ToList().Find(x => x.isLocalPlayer && x.isServer);
        if (serv != null)
        {
            var pl = FindObjectOfType<WorldStaticData>().players.Find(x => x.unetID == conn.identity.netId);
            pl.pos = Vect.FromVector3(pl.@object.transform.position);
            pl.rot = Vect.FromVector3(pl.@object.transform.localEulerAngles);
        }
        base.OnClientDisconnect(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        var serv = FindObjectsOfType<NetworkPlayer>().ToList().Find(x => x.isLocalPlayer && x.isServer);
        if (serv != null)
        {
            var pl = FindObjectOfType<WorldStaticData>().players.Find(x => x.unetID == conn.identity.netId);
            pl.pos = Vect.FromVector3(pl.@object.transform.position); //Сохраниение позиций сделай, а после сохранение мира в файл
            pl.rot = Vect.FromVector3(pl.@object.transform.localEulerAngles);
        }
        base.OnServerDisconnect(conn);
    }
    public void StartCustomHost()
    {
        ///PlayerPrefs.SetString("Name", FindObjectOfType<MenuInitialize>().playerName.text);
        FindObjectOfType<NetworkManager>().networkAddress = FindObjectOfType<MenuInitialize>().address.text;
        
    }
    public void Connect()
    {
       /// PlayerPrefs.SetString("Name", FindObjectOfType<MenuInitialize>().playerName.text);
        FindObjectOfType<NetworkManager>().networkAddress = FindObjectOfType<MenuInitialize>().address.text;
        StartClient();
    }

    public void SetName()
    {
       // PlayerPrefs.SetString("Name", FindObjectOfType<MenuInitialize>().playerName.text);
    }

    
}

using Mirror;
using Mirror.Discovery;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static LoaderResources;

public class MenuInitialize : MonoBehaviour
{
    public Button connectButton;
    public Button playButton;
    public TMP_InputField address, worldName, playerName;
    public TMP_Text worldCreateError, errorMenuText;
    public Transform createWorldMenu, worldsListMenu, errorMenu, worldUpdateError;
    public WorldsManager worldsManager;
    public TMP_Text sensText;
    public Slider sensSlider;

    public void SetInput(Slider scrollbar)
    {
        sensText.text = "Sens [" + scrollbar.value.ToString("F2") + "]: ";
    }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (PlayerPrefs.HasKey("Error"))
        {
            errorMenu.gameObject.SetActive(true);
            errorMenuText.text = PlayerPrefs.GetString("Error", "Error of error. WTF NIGGA???!! HOW??? FUKING RUSSIAN HACKERS");
        }

        playerName.text = FindObjectOfType<Config>().cfg.playerName;
    }
    
    public void ClearError()
    {
        PlayerPrefs.DeleteKey("Error");
    }
    public void StartHost (){
        FindObjectOfType<NetworkManager>().networkAddress = address.text;
        FindObjectOfType<Menu>().StartHost();
        FindObjectOfType<NetworkDiscovery>().AdvertiseServer();
    }


    public void Connect()
    {
        //PlayerPrefs.DeleteKey("World");
        FindObjectOfType<NetworkManager>().networkAddress = address.text;
        FindObjectOfType<Menu>().StartClient();
    }

    public List<List<string>> RefreshWorlds()
    {
        string exe = FindObjectOfType<LoaderResources>().exe;
        DirectoryInfo dir = new DirectoryInfo(exe + @"\Worlds\");
        foreach (FileInfo item in dir.GetFiles())
        {
            if (Path.GetExtension(item.FullName) == ".world")
            {
                string name = Path.GetFileNameWithoutExtension(item.FullName);
                Directory.CreateDirectory(exe + @"\Worlds\" + @"\" + name + @"\");
                File.Move(item.FullName, exe + @"\Worlds\" + name + @"\" + name + ".world");
            }
        }


        List<string> worlds = new List<string>();
        List<string> versions = new List<string>();
        foreach (DirectoryInfo item in dir.GetDirectories())
        {
            var world = item.GetFiles().ToList().Find(x => Path.GetFileName(x.FullName) == (Path.GetFileName(item.FullName) + ".world"));
            if (world != null)
            {
                worlds.Add(item.FullName);
                var bak = LoaderResources.GetBak(item.FullName + @"\" + (Path.GetFileName(item.FullName) + ".info"));
                if (bak != null) {
                    versions.Add(bak.version);
                }
                else
                {
                    versions.Add("No version available. Mistakes in the world are possible.");
                }
            }
        }
        return new List<List<string>>() { worlds, versions};
    }

    public void NameChange()
    {
        playerName.text = playerName.text.ToLower();
        FindObjectOfType<Config>().SaveConfig();
    }
    public string CreateWorld()
    {
        string exe = FindObjectOfType<LoaderResources>().exe;
        if (!Directory.Exists(exe + @"\Worlds\"))
        {
            return "Worlds folder error";
        }
        DirectoryInfo dir = new DirectoryInfo(exe + @"\Worlds\");
        foreach (DirectoryInfo item in dir.GetDirectories())
        {
            if (Path.GetFileNameWithoutExtension(item.FullName).ToLower().Trim() == worldName.text.ToLower().Trim())
            {
                return "World not null";
            }
        }
        var path = exe + @"\worlds\" + worldName.text.ToLower().Trim() + @"\";
        Directory.CreateDirectory(path);
        File.Create(path + worldName.text.ToLower().Trim() + ".world").Dispose();
        SaveBak(path + worldName.text.ToLower().Trim() + ".info");
        return "OK";
    }

    public void DeleteFile(string str)
    {
        Directory.Delete(FindObjectOfType<LoaderResources>().exe + @"\Worlds\" + str + @"\", true);
        worldsManager.UpdateList();
    }

    public void WorldCreate()
    {
        string g = CreateWorld();

        if (g == "OK")
        {
            createWorldMenu.gameObject.SetActive(false);
            worldsListMenu.gameObject.SetActive(true);
            worldCreateError.text = "world name:";
        }
        else
        {
            worldCreateError.text = "world name: <color=\"red\"> " + g;
        }
        worldsManager.UpdateList();
    }

    private void Update()
    {
        playerName.text = playerName.text.ToLower();
        connectButton.interactable = !NetworkClient.active;
        playButton.interactable = !NetworkClient.active;
        address.interactable = !NetworkClient.active;
        if (NetworkClient.active)
        {
            connectButton.GetComponentInChildren<TMP_Text>().text = "Wait..";
        }
        else
        {
            connectButton.GetComponentInChildren<TMP_Text>().text = "Connect";
        }
    }
}

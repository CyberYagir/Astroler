using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

public class Config : MonoBehaviour
{
    public PlayerCFG cfg = null;
    [System.Serializable]
    public class PlayerCFG
    {
        public string playerName = "";
        public List<InputManager.InputKey> inputKeys = new List<InputManager.InputKey>();
        public VideoOptions.VideoCFG video = new VideoOptions.VideoCFG();
        public float sens = 1;
        public string info = "Yagir.inc | Astroler | Version: 2020.11.15 | Date: ";
    }
    private void Start()
    {
        LoadConfig();
    }

    public void SaveConfig()
    {
        cfg = new PlayerCFG() { sens = FindObjectOfType<MenuInitialize>().sensSlider.value, inputKeys = FindObjectOfType<InputManager>().inputs, video = FindObjectOfType<VideoOptions>().getVideo(), info = $"Yagir.inc | Astroler | Version: {Application.version} | Date: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} | Unity: {Application.unityVersion}", playerName = FindObjectOfType<MenuInitialize>().playerName.text };
        if (cfg.playerName.Trim() == "")
        {
            cfg.playerName = "player";
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(FindObjectOfType<LoaderResources>().exe + "player.cfg", FileMode.Create);
        bf.Serialize(fs, cfg);
        fs.Close();
        LoadConfig();
    }

    public void LoadConfig()
    {
        if (!File.Exists(FindObjectOfType<LoaderResources>().exe + "player.cfg"))
        {
            SaveConfig();
            return;
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(FindObjectOfType<LoaderResources>().exe + "player.cfg", FileMode.OpenOrCreate);
        try
        {
            cfg = (PlayerCFG)bf.Deserialize(fs);
        }
        catch (Exception)
        {
            fs.Close();
            SaveConfig();
            return;
        }
        fs.Close();

        FindObjectOfType<InputManager>().inputs = cfg.inputKeys;
        FindObjectOfType<MenuInitialize>().sensSlider.value = cfg.sens;
        FindObjectOfType<VideoOptions>().setVideo(cfg.video);
        FindObjectOfType<MenuInitialize>().playerName.text = cfg.playerName;
    }
}

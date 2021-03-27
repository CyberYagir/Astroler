using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VideoOptions : MonoBehaviour
{
    public TMP_Dropdown graphic, res;
    public Slider shadows;
    public TMP_Text shadowsText;
    public Toggle Vsync, postProcess, ambient, bloom, tonemap, chrom;
    public GameObject dopOptions;

    private void Update()
    {
        dopOptions.SetActive(!postProcess.isOn);
    }

    [System.Serializable]
    public class VideoCFG {
        public int graphic;
        public float shadows;
        public bool vsync, ambient, bloom, tonemap, chrom;
    }


    public VideoCFG getVideo()
    {
        return new VideoCFG() { graphic = graphic.value, shadows = shadows.value, ambient = ambient.isOn, bloom = bloom.isOn, chrom = chrom.isOn, tonemap = tonemap.isOn, vsync = Vsync.isOn };
    }
    public void setVideo(VideoCFG v)
    {
        graphic.value = v.graphic;
        ChangeGraphicLevel();
        ambient.isOn = v.ambient;
        bloom.isOn = v.bloom;
        chrom.isOn = v.chrom;
        tonemap.isOn = v.tonemap;
        Vsync.isOn = v.vsync;
        shadows.value = v.shadows;
        ChangeShadows();
        VSync();
    }
    public void ChangeGraphicLevel()
    {
        QualitySettings.SetQualityLevel((int)graphic.value);
        shadows.value = QualitySettings.shadowDistance;
        ChangeShadows();

    }

    public void ChangeRes()
    {
        Screen.SetResolution(int.Parse(res.options[res.value].text.Split('x')[0]), int.Parse(res.options[res.value].text.Split('x')[1]), Screen.fullScreen);
        PlayerPrefs.SetInt("Res", res.value);
    }

    public void FullScreen()
    {
        if (Screen.fullScreen)
        {
            Screen.fullScreen = false;
        }
        else
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
    }
    private void Start()
    {
        res.value = PlayerPrefs.GetInt("Res", 0);
    }

    public void ChangeShadows()
    {
        shadowsText.text = "Shadow distance: [" + shadows.value.ToString("F2") + "]";
        QualitySettings.shadowDistance = shadows.value;
    }
    public void VSync()
    {
        QualitySettings.vSyncCount = Vsync.isOn ? 1 : 0;
    }
}

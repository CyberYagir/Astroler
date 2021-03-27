using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class PlayerCfgLoader : MonoBehaviour
{

    public void Init()
    {
        var cfg = FindObjectOfType<Config>();
        if (cfg.cfg != null)
        {
            if (!cfg.cfg.video.ambient)
            {
                GetComponentInChildren<ScreenSpaceAmbientOcclusion>().enabled = false;
            }
            if (!cfg.cfg.video.bloom)
            {
                GetComponentInChildren<BloomOptimized>().enabled = false;
            }
            if (!cfg.cfg.video.chrom)
            {
                GetComponentInChildren<VignetteAndChromaticAberration>().enabled = false;
            }
            if (!cfg.cfg.video.tonemap)
            {
                GetComponentInChildren<Tonemapping>().enabled = false;
            }
            GetComponent<Player>().sens = cfg.cfg.sens;
        }
    }
}

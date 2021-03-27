using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Directions : MonoBehaviour
{
    GameObject firstPlayer;
    private void Start()
    {
    }
    void Update()
    {
        if (firstPlayer == null)
        {
            var p = FindObjectsOfType<NetworkPlayer>().ToList().Find(x => x.isLocalPlayer);
            if (p != null)
            {
                firstPlayer = p.gameObject;
            }
            return;
        }
        transform.localEulerAngles = firstPlayer.transform.localEulerAngles;
    }
}

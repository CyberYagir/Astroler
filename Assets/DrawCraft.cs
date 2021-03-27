using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCraft : MonoBehaviour
{
    public string craft;

    public void Click()
    {
        GetComponentInParent<SpawnCrafts>().DrawCraft(craft);
    }
}

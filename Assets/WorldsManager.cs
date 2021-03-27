using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class WorldsManager : MonoBehaviour
{
    public Transform worldsHolder, worldsItem;

    public void UpdateList()
    {
        var worlds = FindObjectOfType<MenuInitialize>().RefreshWorlds();

        foreach (Transform item in worldsHolder)
        {
            Destroy(item.gameObject);
        }

        for (int i = 0; i < worlds[0].Count; i++)
        {
            Instantiate(worldsItem.gameObject, worldsHolder).GetComponent<WorldItem>().Set(Path.GetFileNameWithoutExtension(worlds[0][i]), worlds[1][i]);
        }
    }
}

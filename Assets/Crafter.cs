using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class Crafter : MonoBehaviour
{
    public Transform invHolder, finalHolder;
    public Transform invItem, finalItem;
    public Inventory inv;
    public Transform linesHolder;
    public WorldStaticData wsd;
    public Transform itemOnTable;
    private void Start()
    {
        inv = FindObjectOfType<Inventory>();
        wsd = FindObjectOfType<WorldStaticData>(); 
        FullUpdate();
    }

    public void FullUpdate()
    {
        DrawInventoryLeft();
        GetComponentInChildren<Crf_MainPlace>().oldcrafts.Add(new Inventory.Item());
        var main = GetComponentInChildren<Crf_MainPlace>();
        foreach (Transform item in main.transform)
        {
            if (!item.GetComponent<UILineRenderer>())
            {
                item.GetComponent<Crf_MoveToCraft>().ReInit();
            }
        }
    }

    public void DrawInventoryLeft()
    {
        inv = FindObjectOfType<Inventory>();
        wsd = FindObjectOfType<WorldStaticData>();
        foreach (Transform item in invHolder)
        {
            if (item.gameObject.active)
                Destroy(item.gameObject);
        }
        for (int y = 0; y < inv.items.GetLength(0); y++)
        {
            for (int x = 0; x < inv.items.GetLength(1); x++)
            {
                if (inv.items[y, x] != null)
                {

                    if (inv.items[y, x].value == 0)
                    {
                        inv.items[y, x] = null;
                        continue;
                    }
                    var item = wsd.items[inv.items[y, x].itemid];
                    if (item == null) continue;
                    var gm = Instantiate(invItem.gameObject, invHolder.transform);
                    gm.transform.GetChild(0).GetComponent<Image>().sprite = item.sprite;
                    gm.transform.GetChild(1).GetComponent<TMP_Text>().text = item.name;
                    gm.transform.GetChild(2).GetComponent<TMP_Text>().text = item.description;
                    gm.transform.GetChild(3).GetComponent<TMP_Text>().text = inv.items[y, x].value.ToString();
                    gm.GetComponent<Crf_ClickOnInvItem>().invPos = new Vector2Int(x, y);
                    gm.GetComponent<Crf_ClickOnInvItem>().itemid = inv.items[y, x].itemid;
                    gm.GetComponent<Crf_ClickOnInvItem>().value = inv.items[y, x].value;
                    gm.SetActive(true);
                    gm.transform.GetChild(0).GetComponent<Image>().enabled = true;
                    gm.transform.GetChild(1).GetComponent<TMP_Text>().enabled = true;
                    gm.transform.GetChild(2).GetComponent<TMP_Text>().enabled = true;
                    gm.transform.GetChild(3).GetComponent<TMP_Text>().enabled = true;
                    gm.GetComponent<Image>().enabled = true;
                }
            }
        }
    }

    internal void DrawCrafts(List<Inventory.Item> crafts)
    {
        foreach (Transform item in finalHolder)
        {
            if (item.gameObject.active)
            {
                Destroy(item.gameObject);
            }
        }
        for (int i = 0; i < crafts.Count; i++)
        {
            var find = wsd.crafts.Find(x => x.finalItemName.Trim().ToLower() == crafts[i].name.Trim().ToLower()) ;
            if (find  == null) continue;
            var gm = Instantiate(finalItem, finalHolder);
            gm.GetComponent<Crf_CraftTable>().craftID = crafts[i].name;
            gm.GetChild(0).GetComponent<Image>().sprite = crafts[i].sprite;
            gm.GetComponentInChildren<TMP_Text>().text = find.craftCount.ToString();
            gm.gameObject.active = true;
        }
    }
}

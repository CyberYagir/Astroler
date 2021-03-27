using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnCrafts : MonoBehaviour
{
    public Transform craftListHolder, toCraftListHolder, craftListItem;
    public Image craftBlock, finalBlock;
    public GameObject line, line2;
    void Start()
    {
        var wsd = FindObjectOfType<WorldStaticData>();
        for (int i = 0; i < wsd.crafts.Count; i++)
        {
            var gm = Instantiate(craftListItem.gameObject, craftListHolder);
            gm.SetActive(true);
            var final = wsd.GetItem(wsd.crafts[i].finalItemName);
            gm.transform.GetChild(0).GetComponent<Image>().sprite = final.sprite;
            gm.GetComponentInChildren<TMP_Text>().text = final.name + "\nCount: " + wsd.crafts[i].craftCount;
            gm.GetComponent<DrawCraft>().craft = final.name;
        }
    }


    public void DrawCraft(string nm)
    {
        foreach (Transform item in toCraftListHolder)
        {
            Destroy(item.gameObject);
        }
        var wsd = FindObjectOfType<WorldStaticData>();
        var craft = wsd.crafts.Find(x=>x.finalItemName.ToLower().Trim() == nm.ToLower().Trim());
        craftBlock.gameObject.SetActive(craft != null);
        finalBlock.gameObject.SetActive(craft != null);
        line.SetActive(craft != null);
        line2.SetActive(craft != null);
        if (craft == null)
        {
            return;
        }
        var g = wsd.GetItem(craft.finalItemName);
        var crf = wsd.items.Find(x => x.blockID == craft.tableblockId);
        craftBlock.sprite = crf.sprite;
        finalBlock.sprite = g.sprite;
        craftBlock.GetComponentInChildren<TMP_Text>().text = crf.name;
        finalBlock.GetComponentInChildren<TMP_Text>().text = nm;

        for (int i = 0; i < craft.craftItems.Count; i++)
        {
            var gm = Instantiate(craftListItem.gameObject, toCraftListHolder);
            gm.SetActive(true);
            var final = wsd.GetItem(craft.craftItems[i].name);
            gm.transform.GetChild(0).GetComponent<Image>().sprite = final.sprite;
            gm.GetComponentInChildren<TMP_Text>().text = final.name + "\nCount: " + craft.craftItems[i].count;
            gm.GetComponent<DrawCraft>().craft = final.name;
            gm.GetComponent<RectTransform>().sizeDelta = new Vector2(gm.transform.parent.GetComponent<RectTransform>().sizeDelta.x, gm.GetComponent<RectTransform>().sizeDelta.y);
        }

    }
}

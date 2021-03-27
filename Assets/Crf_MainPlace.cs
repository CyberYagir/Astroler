using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;
using static Inventory;

public class Crf_MainPlace : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool over;
    public List<InventoryItem> itemsInCraft = new List<InventoryItem>();
    public List<Item> oldcrafts = new List<Item>();
    public WorldStaticData wsd;
    public Vector3 offcet;
    public bool drawPoints = true;
    public void OnPointerEnter(PointerEventData eventData)
    {
        over = true;
        wsd = FindObjectOfType<WorldStaticData>();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        over = false;
    }
    private void Start()
    {
        oldcrafts = new List<Item>();
    }
    private void Update()
    {
        if (drawPoints)
        {
            if (transform.childCount >= 2)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).GetComponent<UILineRenderer>()) break;
                    if (i != transform.childCount - 1)
                    {
                        transform.GetChild(i).GetComponentInChildren<Crf_MoveToCraft>().lineRenderer.Points = new Vector2[2] { transform.GetChild(i).localPosition + offcet, transform.GetChild(i + 1).localPosition + offcet };
                    }
                    else
                    {
                        transform.GetChild(i).GetComponentInChildren<Crf_MoveToCraft>().lineRenderer.Points = new Vector2[2] { transform.GetChild(i).localPosition + offcet, transform.GetChild(0).localPosition + offcet };
                    }
                }
            }
            else
            {
                if (transform.childCount == 1)
                {
                    transform.GetChild(0).GetComponent<Crf_MoveToCraft>().lineRenderer.Points = new Vector2[2] { new Vector2(0, 0), new Vector2(0, 0) };
                }
            }
        }

        itemsInCraft = new List<InventoryItem>();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<UILineRenderer>()) continue;
            itemsInCraft.Add(wsd.items[transform.GetChild(i).GetComponent<Crf_MoveToCraft>().itemid].CreateInventoryItem(transform.GetChild(i).GetComponent<Crf_MoveToCraft>().value));
        }
        
        if (itemsInCraft.Count == 0)
        {
            GetComponentInParent<Crafter>().DrawCrafts(new List<Item>());
            if (0 != oldcrafts.Count)
            {
                UpdateAll(new List<Item>());
                oldcrafts = new List<Item>();
            }
            return;
        }
        var crafts = wsd.actualCrafts(itemsInCraft);
        if (crafts.Count != oldcrafts.Count)
        {
            UpdateAll(crafts);
            return;
        }

        for (int i = 0; i < crafts.Count; i++)
        {
            if (crafts[i] != oldcrafts[i])
            {
                UpdateAll(crafts);
                return;
            }
        }
    }
    public void UpdateAll(List<Item> crafts)
    {
        GetComponentInParent<Crafter>().DrawCrafts(crafts);
        oldcrafts = crafts;
    }
}

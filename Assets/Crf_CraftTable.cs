using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Crf_CraftTable : MonoBehaviour, IMoveHandler, IPointerClickHandler
{
    public string craftID;

    public void OnMove(AxisEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var craftInfo = FindObjectOfType<WorldStaticData>().crafts.Find(x => x.finalItemName.Trim().ToLower() == craftID.Trim().ToLower());
        var inv = FindObjectOfType<Inventory>();
        var wsd = FindObjectOfType<WorldStaticData>();
        if (craftInfo != null)
        {
            if (inv.AddItem(wsd.GetItem(craftInfo.finalItemName)?.CreateInventoryItem(craftInfo.craftCount)))
            {
                for (int i = 0; i < craftInfo.craftItems.Count; i++)
                {
                    inv.RemoveItem(wsd.GetItem(craftInfo.craftItems[i].name).CreateInventoryItem(craftInfo.craftItems[i].count));
                }
            }
        }
        FindObjectOfType<Crafter>().FullUpdate();
    }
}

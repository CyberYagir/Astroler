using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Inventory;

public class DropItemInventory : MonoBehaviour, IPointerUpHandler, IMoveHandler, IPointerDownHandler
{
    public Inventory inventory;

    public void OnMove(AxisEventData eventData)
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (inventory.mouseItem.gameObject.active)
        {
            var it = inventory.items[inventory.mouseItem.oldPos.y, inventory.mouseItem.oldPos.x].clone();
            it.value = inventory.mouseItem.value;
            if (inventory.mouseItem.getfullstack)
            {
                inventory.items[inventory.mouseItem.oldPos.y, inventory.mouseItem.oldPos.x] = null;
            }
            else
            {
                if (inventory.items[inventory.mouseItem.oldPos.y, inventory.mouseItem.oldPos.x].value <= 0)
                {
                    inventory.items[inventory.mouseItem.oldPos.y, inventory.mouseItem.oldPos.x] = null;
                }
            }
            inventory.mouseItem.gameObject.active = false;
            inventory.RedrawInventory();
            GetComponentInParent<NetworkPlayer>().CmdDropItem(it, GetComponentInParent<NetworkPlayer>().transform.position + GetComponentInParent<NetworkPlayer>().transform.forward);
        }
    }

    

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Inventory;

public class ItemBack : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public PosInArray pos;

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            var mi = FindObjectOfType<Inventory>().mouseItem;
            if (mi.oldPos != new Vector2Int(-1, -1))
            {
                if (mi.gameObject.active == false) return;
                var inv = FindObjectOfType<Inventory>().items;
                if (inv[pos.pos.y, pos.pos.x] == null)
                {
                    if (!mi.getfullstack)
                    {
                        inv[pos.pos.y, pos.pos.x] = inv[mi.oldPos.y, mi.oldPos.x].clone();
                        inv[pos.pos.y, pos.pos.x].value = mi.value;
                        if (inv[mi.oldPos.y, mi.oldPos.x].value == 0)
                        {
                            inv[mi.oldPos.y, mi.oldPos.x] = null;
                        }
                    }
                    else
                    {

                        inv[pos.pos.y, pos.pos.x] = inv[mi.oldPos.y, mi.oldPos.x].clone();
                        inv[pos.pos.y, pos.pos.x].value = mi.value;
                        inv[mi.oldPos.y, mi.oldPos.x] = null;
                    }
                }
                else
                {
                    if (inv[pos.pos.y, pos.pos.x].itemid == mi.id)
                    {
                        if (inv[pos.pos.y, pos.pos.x].value + mi.value <= FindObjectOfType<WorldStaticData>().items[inv[pos.pos.y, pos.pos.x].itemid].stack)
                        {
                            inv[pos.pos.y, pos.pos.x] = inv[mi.oldPos.y, mi.oldPos.x].clone();
                            inv[pos.pos.y, pos.pos.x].value += mi.value;
                        }
                    }
                }
                mi.gameObject.SetActive(false);
                FindObjectOfType<Inventory>().RedrawInventory();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }
}

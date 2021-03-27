using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static Inventory;

public class ItemDraw : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public PosInArray pos;
    public TMP_Text text;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            var inv = FindObjectOfType<Inventory>();
            var inv1 = FindObjectOfType<Inventory>().items;
            var mi = inv.mouseItem;


            if (pos.pos == mi.oldPos)
            {
                if (mi.gameObject.active)
                {
                    if (inv1[pos.pos.y, pos.pos.x].itemid == mi.id)
                    {
                        if (inv1[pos.pos.y, pos.pos.x].value + mi.value <= FindObjectOfType<WorldStaticData>().items[inv1[mi.oldPos.y, mi.oldPos.x].itemid].stack)
                        {
                            inv1[pos.pos.y, pos.pos.x] = inv1[mi.oldPos.y, mi.oldPos.x].clone();
                            if (!mi.getfullstack)
                                inv1[pos.pos.y, pos.pos.x].value += mi.value;
                            mi.gameObject.SetActive(false);
                            inv.RedrawInventory();
                            return;
                        }
                    }
                }
                mi.oldPos = new Vector2Int(-1, -1);
                mi.gameObject.SetActive(false);
                inv.RedrawInventory();
                return;
            }
            if (inv.items[pos.pos.y, pos.pos.x] != null)
            {
                if (mi.gameObject.active)
                {
                    if (inv1[pos.pos.y, pos.pos.x].itemid == mi.id)
                    {
                        print(inv1[pos.pos.y, pos.pos.x].itemid + "|" + mi.id);
                        if (inv1[pos.pos.y, pos.pos.x].value + mi.value <= FindObjectOfType<WorldStaticData>().items[inv1[mi.oldPos.y, mi.oldPos.x].itemid].stack)
                        {
                            inv1[pos.pos.y, pos.pos.x].value += mi.value;
                            if (mi.getfullstack)
                            {
                                inv1[mi.oldPos.y, mi.oldPos.x] = null;
                            }else if (inv1[mi.oldPos.y, mi.oldPos.x].value == 0)
                            {
                                inv1[mi.oldPos.y, mi.oldPos.x] = null;
                            }
                            mi.gameObject.SetActive(false);
                            inv.RedrawInventory();
                            return;
                        }
                    }
                    else
                    {
                        if (!mi.getfullstack)
                            inv1[mi.oldPos.y, mi.oldPos.x].value += mi.value;
                        
                        InventoryItem inventoryItem = inv1[pos.pos.y, pos.pos.x].clone();
                        InventoryItem miinventoryItem = inv1[mi.oldPos.y, mi.oldPos.x].clone();
                        inv1[pos.pos.y, pos.pos.x] = miinventoryItem;
                        inv1[mi.oldPos.y, mi.oldPos.x] = inventoryItem;


                        mi.gameObject.SetActive(false);
                        inv.RedrawInventory();
                        return;
                    }
                }

                mi.oldPos = pos.pos;
                mi.value = inv.items[pos.pos.y, pos.pos.x].value;
                mi.id = inv.items[pos.pos.y, pos.pos.x].itemid;
                mi.SetSprite();
                mi.transform.position = transform.position;
                mi.gameObject.SetActive(true);
            }
            else
            {
                InventoryItem inventoryItem = inv1[pos.pos.y, pos.pos.x].clone();
                inv1[pos.pos.y, pos.pos.x] = inv1[mi.oldPos.y, mi.oldPos.x].clone();
                inv1[mi.oldPos.y, mi.oldPos.x] = inventoryItem;
                mi.value = inventoryItem.value;
                mi.id = inventoryItem.itemid;
            }
            mi.getfullstack = true;
            inv.RedrawInventory();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var inv = FindObjectOfType<Inventory>();
            var inv1 = FindObjectOfType<Inventory>().items;
            var mi = inv.mouseItem;
            if (inv.items[pos.pos.y, pos.pos.x] != null)
            {
                mi.oldPos = pos.pos;
                int removeVal = (int)(inv.items[pos.pos.y, pos.pos.x].value/2);
                mi.value = inv.items[pos.pos.y, pos.pos.x].value - removeVal;
                inv.items[pos.pos.y, pos.pos.x].value = removeVal;
                mi.id = inv.items[pos.pos.y, pos.pos.x].itemid;
                mi.SetSprite();
                mi.transform.position = transform.position;
                mi.gameObject.SetActive(true);
            }

            mi.getfullstack = false;
            inv.RedrawInventory();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var inv1 = FindObjectOfType<Inventory>().items;
        var item = FindObjectOfType<WorldStaticData>().items[inv1[pos.pos.y, pos.pos.x].itemid];

        FindObjectOfType<PlayerInterface>().infoImage.enabled = true;
        FindObjectOfType<PlayerInterface>().infoImage.sprite = item.sprite;
        FindObjectOfType<PlayerInterface>().naming.text = item.name;
        FindObjectOfType<PlayerInterface>().descriptions.text = item.description;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FindObjectOfType<PlayerInterface>().infoImage.enabled = false;
        FindObjectOfType<PlayerInterface>().naming.text = "";
        FindObjectOfType<PlayerInterface>().descriptions.text = "";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    public InventoryItem[,] items;
    public GameObject[,] backitems;
    
    public GameObject[,] drawitems;
    public Transform grid, drawgrid;
    public GameObject itemback;
    public GameObject itemdraw;
    public MouseItem mouseItem;
    [Space]
    public Transform gridHotBar, drawGridHotBar;
    public GameObject[] hotbackitems;
    public GameObject[] drawhotitems;
    public Vector2Int selectInHotBar;
    public int selected = 1;
    public int oldselected = 1;
    public Transform selectCaret;
    public Transform hand;
    public GameObject cubePrefab;
    public InventoryItem inhands = new InventoryItem() { itemid = -1};
    private void Start()
    {
        oldselected = selected;
        items = new InventoryItem[3,9];
        backitems = new GameObject[3,9];
        drawitems = new GameObject[3,9];
        hotbackitems = new GameObject[9];
        drawhotitems = new GameObject[9];
        for (int y = 0; y < items.GetLength(0); y++)
        {
            for (int x = 0; x < items.GetLength(1); x++)
            {
                var gm = Instantiate(itemback.gameObject, grid);
                gm.GetComponent<PosInArray>().pos = new Vector2Int(x, y);
                gm.SetActive(true);
                backitems[y, x] = gm;
            }
        }
        for (int i = 0; i < 9; i++)
        {
            var gm = Instantiate(itemback.gameObject, gridHotBar);
            gm.GetComponent<PosInArray>().pos = new Vector2Int(i, items.GetLength(0)-1);
            gm.SetActive(true);
            hotbackitems[i] = gm;
        }
        //for (int i = FindObjectOfType<WorldStaticData>().items.Count-1; i > 0; i--)
        //{
        //    AddItem(FindObjectOfType<WorldStaticData>().items[i].CreateInventoryItem(99));
        //}
        inhands = items[items.GetLength(0)-1, 0];
        RedrawInventory();
    }

    public Texture2D Convert(Sprite sprite)
    {
        var croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                (int)sprite.textureRect.y,
                                                (int)sprite.textureRect.width,
                                                (int)sprite.textureRect.height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.filterMode = FilterMode.Point;
        croppedTexture.Apply();
        return croppedTexture;
    }

    public void Update()
    {
        for (int i = 1; i < 10; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                selected = i;
                break;
            }
        }
        selectCaret.transform.position = Vector3.Lerp(selectCaret.transform.position, hotbackitems[selected - 1].transform.position, 8f * Time.deltaTime);

        if (selected != oldselected)
        {
            SetHand();
        }
    }
    public void SetHand()
    {
        oldselected = selected;
        var pos = hotbackitems[selected - 1].GetComponent<PosInArray>().pos;
        if (hand.childCount == 1)
        {
            if (items[pos.y, pos.x] == null)
            {
                Destroy(hand.GetChild(0).gameObject);
            }
            else
            {
                if (inhands.itemid != items[pos.y, pos.x].itemid)
                    Destroy(hand.GetChild(0).gameObject);
            }
        }
        if (items[pos.y, pos.x] != null)
        {
            if (inhands != null && inhands.itemid == items[pos.y, pos.x].itemid)
            {
                inhands = items[pos.y, pos.x];
                return;
            }
            Item it = FindObjectOfType<WorldStaticData>().items[items[pos.y, pos.x].itemid];
            if (it.type == Item.ItemType.instrument)
            {
                var gm = Instantiate(it.prefab, hand);
            }
            if (it.type == Item.ItemType.block)
            {
                var gm = Instantiate(cubePrefab.gameObject, hand);
                gm.GetComponent<PlaceCube>().invPos = new UnityEngine.Vector2Int(pos.x, pos.y);
                gm.GetComponent<Renderer>().material.SetTexture("_MainTex", it.texture);
            }
        }
        inhands = items[pos.y, pos.x];
    }

    public void InventoryChange()
    {
        //XmlSerializer xmlSerializer = new XmlSerializer(typeof(InventoryItem[]));
        //string text = "";
        //using (StringWriter textWriter = new StringWriter())
        //{
        //    xmlSerializer.Serialize(textWriter, items);
        //    text = textWriter.ToString();
        //}
        List<InventoryItem> its = new List<InventoryItem>();
        for (int y = 0; y < items.GetLength(0); y++)
        {
            for (int x = 0; x < items.GetLength(1); x++)
            {
                if (items[y, x] != null)
                {
                    items[y, x].pos = new Vect(x, y);
                    its.Add(items[y, x]);
                }
            }
        }

        GetComponent<NetworkPlayer>().SendInventory(FindObjectOfType<Config>().cfg.playerName, its);
    }

    public bool AddItem(InventoryItem inventoryItem, bool add = true)
    {
        int findItemId = inventoryItem.itemid;
        var itemslist = FindObjectOfType<WorldStaticData>().items;
        for (int y = 0; y < items.GetLength(0); y++)
        {
            for (int x = 0; x < items.GetLength(1); x++)
            {
                if (items[y, x] == null) continue;
                if (items[y, x].itemid == findItemId)
                {
                    if (items[y, x].value + inventoryItem.value <= itemslist[findItemId].stack)
                    {
                        if (add)
                        {
                            items[y, x].value += inventoryItem.value;
                            InventoryChange();
                            RedrawInventory();
                        }
                        return true;
                    }
                }
            }
        }
        int ff = 0;
        for (int y = 0; y < items.GetLength(0); y++)
        {
            for (int x = 0; x < items.GetLength(1); x++)
            {
                if (items[y, x] == null)
                {
                    ff++;
                }
            }
        }
        if (ff <= 0) return false;
        for (int y = 0; y < items.GetLength(0); y++)
        {
            for (int x = 0; x < items.GetLength(1); x++)
            {
                if (items[y,x] == null)
                {
                    if (add)
                    {
                        items[y, x] = inventoryItem;
                        items[y, x].pos = new Vect(x, y);
                        InventoryChange();
                        RedrawInventory();
                    }
                    return true;
                }
            }
        }
        return false;
    }
    public bool RemoveItem(InventoryItem invit)
    {
        for (int y = 0; y < items.GetLength(0); y++)
        {
            for (int x = 0; x < items.GetLength(1); x++)
            {
                if (items[y, x] != null)
                {
                    if (invit.itemid == items[y, x].itemid)
                    {
                        if (items[y, x].value >= invit.value)
                        {
                            items[y, x].value -= invit.value;
                            if (items[y,x].value <= 0)
                            {
                                items[y, x] = null;
                            }
                            RedrawInventory();
                            InventoryChange();
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }


    public void RedrawInventory()
    {
        var itemslist = FindObjectOfType<WorldStaticData>().items;
        for (int y = 0; y < items.GetLength(0); y++)
        {
            for (int x = 0; x < items.GetLength(1); x++)
            {
                if (drawitems[y, x] != null)
                {
                    Destroy(drawitems[y, x].gameObject);
                }
                if (items[y, x] == null) continue;
                var gm = Instantiate(itemdraw, drawgrid);
                gm.transform.localPosition = backitems[y, x].transform.localPosition;
                gm.GetComponent<PosInArray>().pos = new Vector2Int(x, y);
                gm.GetComponent<Image>().sprite = itemslist[items[y, x].itemid].sprite;
                if (items[y, x].value != 1)
                {
                    gm.GetComponent<ItemDraw>().text.text = items[y, x].value.ToString();
                }
                else
                    gm.GetComponent<ItemDraw>().text.text = "";
                gm.SetActive(true);
                drawitems[y, x] = gm;
            }
        }
        for (int i = 0; i < 9; i++)
        {
            if (drawhotitems[i] != null)
            {
                Destroy(drawhotitems[i].gameObject);
            }
            if (items[items.GetLength(0) - 1, i] == null) continue;

            var hot = Instantiate(itemdraw, drawGridHotBar);
            drawhotitems[i] = hot;
            hot.transform.localPosition = hotbackitems[i].transform.localPosition;
            hot.GetComponent<PosInArray>().pos = new Vector2Int(i, items.GetLength(0)-1);
            hot.GetComponent<Image>().sprite = itemslist[items[items.GetLength(0) - 1, i].itemid].sprite;
            if (items[items.GetLength(0) - 1, i].value != 1)
            {
                hot.GetComponent<ItemDraw>().text.text = items[items.GetLength(0) - 1, i].value.ToString();
            }
            else 
                hot.GetComponent<ItemDraw>().text.text = "";
            hot.SetActive(true);
        }
        SetHand();
    }

    [System.Serializable]
    public class Item
    {
        public string name;
        public Sprite sprite;
        [TextArea]
        public string description;
        public Sprite texturesprite;
        public Texture2D texture;
        public enum ItemType {instrument, block, none};
        public ItemType type;
        public int stack = 64;
        [Header("Block")]
        public int blockID;
        public bool spawnPrefab;
        [Header("Instrument")]
        public GameObject prefab;
        
        public InventoryItem CreateInventoryItem(int value = 0)
        {
            if (value == 0)
            {
                value = 1;
            }
            if (value > stack)
            {
                value = stack;
            }
            var stc = FindObjectOfType<WorldStaticData>().items;
            int id = -1;
            for (int i = 0; i < stc.Count; i++)
            {
                if (stc[i].name == name)
                {
                    id = i;
                    break;
                }
            }
            return new InventoryItem() { value = value, itemid = id};
        }
    }
    [System.Serializable]
    public class Vect {
        public int x, y,z;
        public float xf, yf,zf;
        public float dop = 0;
        public Vect(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public static Vect FromVector3(Vector3 vect)
        {
            return new Vect(vect.x, vect.y, vect.z);
        }
        public Vect(float xf, float yf)
        {
            this.xf = xf;
            this.yf = yf;
        }
        public Vect(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vect(float xf, float yf, float zf)
        {
            this.xf = xf;
            this.yf = yf;
            this.zf = zf;
        }
        public Vector3 toVector3()
        {
            return new Vector3(xf, yf, zf);
        }
        public string ToString(bool t)
        {
            return "VecInt: " + x + " " + y;
        }
        public Vect()
        {
        }
    }

    [System.Serializable]
    public class InventoryItem {
        public int itemid = -1;
        public int value = -1;
        public Vect pos;
        public InventoryItem()
        {

        }
        public InventoryItem clone()
        {
            return new InventoryItem() { itemid = itemid, value = value, pos = pos };
        }
    }
    public void DropItem(InventoryItem item, Vector3 pos)
    {
        GetComponentInParent<NetworkPlayer>().CmdDropItem(item, pos);

    }
}

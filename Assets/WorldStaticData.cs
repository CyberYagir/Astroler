using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Inventory;
[System.Serializable]
public struct Meteor
{
    public Vect pos;
    public float radius;
}
public class WorldStaticData : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    public List<Craft> crafts = new List<Craft>();
    public List<NetPlayer> players;
    public List<Meteor> meteors;

    public List<Item> actualCrafts(List<InventoryItem> it)
    {
        var blocks = FindObjectOfType<PlayerActiveTables>();
        var can = new List<Item>();
        for (int i = 0; i < crafts.Count; i++)
        {
            bool canCraft = true;

            if (crafts[i].tableblockId != blocks.currentBlock) continue;
            if (crafts[i].craftItems.Count != it.Count) continue;
            for (int x = 0; x < crafts[i].craftItems.Count; x++)
            {
                if (crafts[i].craftItems[x].name.Trim().ToLower() !=  items[it[x].itemid].name.Trim().ToLower())
                {
                    canCraft = false;
                    break;
                }
                if (crafts[i].craftItems[x].count > it[x].value)
                {
                    canCraft = false;
                    break;
                }
            }
            if (canCraft)
            {
                can.Add(items.Find(x => x.name.Trim().ToLower() == crafts[i].finalItemName.Trim().ToLower()));
            }
        }
        return can;
    }
    public Item GetItem(string nm)
    {
        return items.Find(x => x.name.ToLower().Trim() == nm.ToLower().Trim());
    }

    [System.Serializable]
    public class NetPlayer
    {
        public string name;
        public uint unetID;
        public byte[] texture2D;
        public Vect pos, rot;
        public GameObject @object;
        public List<InventoryItem> inventoryItems = new List<InventoryItem>();
    }
    [System.Serializable]
    public class Craft
    {
        public string finalItemName;
        public int tableblockId;
        public int craftCount = 1;
        public List<CraftItem> craftItems;
    }
    [System.Serializable]
    public class CraftItem {
        public string name;
        public int count;
    }



    string exe = "";
    private void Start()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].type == Item.ItemType.block)
            {
                items[i].texture = Convert(items[i].texturesprite);
            }
        }

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
}

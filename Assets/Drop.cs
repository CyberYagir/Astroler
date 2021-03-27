using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Inventory;

public class Drop : MonoBehaviour
{
    public InventoryItem inventoryItem;
    public SpriteRenderer spriteRenderer;

    private void Start()
    {
        Init();
    }
    public void Init()
    {
        if (inventoryItem.itemid == -1)
        {
            this.enabled = false;
            spriteRenderer.sprite = null;
                return;
        }

        spriteRenderer.sprite = FindObjectOfType<WorldStaticData>().items[inventoryItem.itemid].sprite;
    }
    private void Update()
    {
        spriteRenderer.transform.LookAt(Camera.main.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<NetworkPlayer>())
        {
            bool ok = false;
            if (other.GetComponent<NetworkPlayer>().isLocalPlayer) {
                ok = other.GetComponent<Inventory>().AddItem(inventoryItem);
                if (ok)
                {
                    other.GetComponent<NetworkPlayer>().CmdDestroyItem(transform);
                }
            }

            if (other.GetComponent<NetworkPlayer>().isServer)
            {
                if (ok)
                {
                    NetworkServer.Destroy(gameObject);
                }
            }
        }
    }
}

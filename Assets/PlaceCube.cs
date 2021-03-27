using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceCube : MonoBehaviour
{
    public float maxCooldown, time;
    public Vector2Int invPos;
    BlocksAddRemove addRemove;
    Inventory inventory;
    private void Start()
    {
        inventory = GetComponentInParent<Inventory>();
        addRemove = GetComponentInParent<BlocksAddRemove>();
        time = maxCooldown;
    }
    private void Update()
    {

        if (time >= maxCooldown)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (addRemove.can && addRemove.close)
                {
                    var it = inventory.items[invPos.y, invPos.x];
                    if (it.value > 0)
                    {
                        inventory.RemoveItem(FindObjectOfType<WorldStaticData>().items[it.itemid].CreateInventoryItem(1));
                        addRemove.Control(addRemove.add.transform.position, (byte)FindObjectOfType<WorldStaticData>().items[it.itemid].blockID);
                        time = 0;
                    }
                }
            }
        }
        time += Time.deltaTime;
    }
}

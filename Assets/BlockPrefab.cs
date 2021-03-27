using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPrefab : MonoBehaviour
{
    public Vector3 pos;
    public Chunk chunk;
    public Ship ship;
    private void Start()
    {
        ship = chunk.GetComponent<Ship>();
        if (ship != null)
        {
            transform.parent = chunk.transform;
        }
    }
}

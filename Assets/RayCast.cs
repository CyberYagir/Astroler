using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RayCast : MonoBehaviour
{
    public Chunks chunks;
    void LateUpdate()
    {
        for (int i = 0; i < chunks.center.Count; i++)
        {
            var ch = chunks.center[i].GetComponent<Chunk>();
            ch.active = Vector3.Distance(chunks.center[i].transform.position, transform.position) < 400;
            ch.gameObject.SetActive(ch.active);
            ch.oldactive = ch.active;
        }
    }
}

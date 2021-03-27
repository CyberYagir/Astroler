using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chunks : MonoBehaviour
{
    public List<Chunk> chunks = new List<Chunk>();
    public Chunk chunk;
    public Transform player;
    public float viewSize;
    public Vector3 lastChunk;
    public List<GameObject> center;
    private void Start()
    {
        
    }

    public float Perlin3D(float x, float y, float z)
    {
        float ab = Mathf.PerlinNoise(x + transform.position.x, y + transform.position.y);
        float bc = Mathf.PerlinNoise(y + transform.position.y, z + transform.position.z);
        float ac = Mathf.PerlinNoise(x + transform.position.x, z + transform.position.z);

        float ba = Mathf.PerlinNoise(y + transform.position.y, x + transform.position.x);
        float cb = Mathf.PerlinNoise(z + transform.position.z, y + transform.position.y);
        float ca = Mathf.PerlinNoise(z + transform.position.z, x + transform.position.x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
    }

    public void AddChunks()
    {
        for (float x = transform.position.x - viewSize; x <= transform.position.x + viewSize; x += chunk.width)
        {
            for (float y = transform.position.y - viewSize; y <= transform.position.y + viewSize; y += chunk.width)
            {
                for (float z = transform.position.z - viewSize; z <= transform.position.z + viewSize; z += chunk.width)
                {
                    if (Exists(x, y, z)) continue;

                    AddAsteroid(x, y, z, true);
                }
            }
        }
    }
    int lastChankID = 0;


    public bool Exists(float x, float y, float z)
    {
        if (lastChankID > chunks.Count) lastChankID = 0;
        for (int a = lastChankID; a < chunks.Count; a++)
        {
            if (chunks[a].transform.parent != null) if (!chunks[a].transform.parent.gameObject.active) continue;
            if ((x < chunks[a].transform.position.x) ||
                (z < chunks[a].transform.position.z) ||
                (y < chunks[a].transform.position.y) ||
                (x >= chunks[a].transform.position.x + chunk.width) ||
                (y >= chunks[a].transform.position.y + chunk.height) ||
                (z >= chunks[a].transform.position.z + chunk.width))
                continue;
            lastChankID = a;
            return true;
        }
        if (lastChankID > chunks.Count) lastChankID = 0;
        for (int a = 0; a < lastChankID; a++)
        {
            if (chunks[a].transform.parent != null) if (!chunks[a].transform.parent.gameObject.active) continue;
            if ((x < chunks[a].transform.position.x) ||
                (z < chunks[a].transform.position.z) ||
                (y < chunks[a].transform.position.y) ||
                (x >= chunks[a].transform.position.x + chunk.width) ||
                (y >= chunks[a].transform.position.y + chunk.height) ||
                (z >= chunks[a].transform.position.z + chunk.width))
                continue;
            lastChankID = a;
            return true;
        }

        return false;

    }

    public void AddAsteroid(float x, float y, float z, bool create = false)
    {
        int chunkX = Mathf.FloorToInt(x / chunk.width) * chunk.width;
        int chunkY = Mathf.FloorToInt(y / chunk.width) * chunk.width;
        int chunkZ = Mathf.FloorToInt(z / chunk.width) * chunk.width;
        var perlin = Perlin3D((float)(chunkX) * 1.543125f, (float)(chunkY) * 1.241532f, (float)(chunkZ) * 1.67454f);// Mathf.PerlinNoise((float)(chunkX + chunkY + chunkZ) * 1.543125f, (float)(chunkX + chunkY + chunkZ) * 1.12432f);
        if (perlin >= 0.6f && perlin < 0.605f)
        {
            if (GameObject.FindGameObjectsWithTag("CenterObject").ToList().FindAll(c => Vector3.Distance(c.transform.position, new Vector3((int)chunkX, (int)chunkY, (int)chunkZ)) < 600).Count == 0)
            {
                print("ADD");
                var gm = Instantiate(chunk.gameObject, new Vector3((int)chunkX, (int)chunkY, (int)chunkZ), Quaternion.identity);
                gm.name = "chunk " + (int)chunkX + " " + (int)chunkY + " " + (int)chunkZ;
                var ch = gm.GetComponent<Chunk>();
                ch.coords = new Vector3(chunkX, chunkY, chunkZ) / chunk.width;
                ch.radius = (int)(perlin * 75f) + 15f;
                if (create)
                {
                    if (GetComponent<NetworkPlayer>().isServer)
                    {
                        FindObjectOfType<WorldStaticData>().meteors.Add(new Meteor() { pos = Inventory.Vect.FromVector3(new Vector3(x, y, z)), radius = ch.radius });
                        GetComponent<NetworkPlayer>().AddAsteridForAll(new Vector3(x, y, z), ch.radius);
                    }else if (GetComponent<NetworkPlayer>().isClient)
                    {
                        print("Client");
                        GetComponent<NetworkPlayer>().AddAsteroid(new Vector3(x, y, z), ch.radius);
                    }
                }
                ch.oneBigCenter = true;
                ch.Init();
                chunks.Add(ch);
                return;
            }
            else
            {
                //ch.Init();
            }
        }
    }
    public void AddAsteroid(Vector3 pos, float radius, bool create)
    {
        int chunkX = Mathf.FloorToInt(pos.x / chunk.width) * chunk.width;
        int chunkY = Mathf.FloorToInt(pos.y / chunk.width) * chunk.width;
        int chunkZ = Mathf.FloorToInt(pos.z / chunk.width) * chunk.width;

        if (Exists(pos.x, pos.y, pos.z)) return;
        if (Exists(chunkX, chunkY, chunkZ)) return;
        if (GameObject.FindGameObjectsWithTag("Center").ToList().FindAll(c => Vector3.Distance(c.transform.position, new Vector3((int)chunkX, (int)chunkY, (int)chunkZ)) < 600).Count == 0)
        {
            var gm = Instantiate(chunk.gameObject, new Vector3((int)chunkX, (int)chunkY, (int)chunkZ), Quaternion.identity);
            gm.name = "chunk " + (int)chunkX + " " + (int)chunkY + " " + (int)chunkZ;
            var ch = gm.GetComponent<Chunk>();
            ch.coords = new Vector3(chunkX, chunkY, chunkZ) / chunk.width;
            ch.radius = radius;
            ch.oneBigCenter = true;
            ch.Init();
            chunks.Add(ch);
            return;
        }
    }

    private void LateUpdate()
    {
        AddChunks();
    }
    public bool Exist(Vector3 coors, bool create = false)
    {
        if (chunks.Find(x=>x.coords == coors) != null)
        {
            return true;
        }
        else
        {
            if (create)
            {
                GameObject gm = Instantiate(chunk.gameObject);
                gm.transform.position = coors * chunk.width;
                gm.GetComponent<Chunk>().coords = coors;
                gm.GetComponent<Chunk>().Init();
                gm.GetComponent<Chunk>().LoadChunk();
                chunks.Add(gm.GetComponent<Chunk>());
                return true;
            }
        }
        return false;
    }
    public Chunk GetChunk(Vector3 coords)
    {
        return (chunks.Find(x => x.coords == coords));

    }
    public Chunk GetChunkOut(int x, int y, int z)
    {
        for (int a = 0; a < chunks.Count; a++)
        {
            if ((x < chunks[a].transform.position.x) ||
                (z < chunks[a].transform.position.z) ||
                (y < chunks[a].transform.position.y) ||
                (x >= chunks[a].transform.position.x + chunk.width) ||
                (y >= chunks[a].transform.position.y + chunk.height)||
                (z >= chunks[a].transform.position.z + chunk.width) 
                )
                continue;
            return chunks[a];

        }
        return null;

    }

    public bool GetChunk(float x, float y, float z, bool create = false)
    {
        int chunkX = Mathf.FloorToInt(x / chunk.width);
        int chunkY = Mathf.FloorToInt(y / chunk.width);
        int chunkZ = Mathf.FloorToInt(z / chunk.width);
        return Exist(new Vector3(chunkX, chunkY, chunkZ), true);

    }


}

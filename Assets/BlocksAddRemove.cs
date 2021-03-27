using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlocksAddRemove : MonoBehaviour
{

	public Vector3 retDel, retAdd;
	public Transform del, add;
	public byte activeBlockType;
	public bool can, close;
	public Ship ship;
	public Chunks chunks;
	public NetworkPlayer networkPlayer;
	public WorldChages worldChages;
	// Update is called once per frame
	private void Start()
	{
		add.transform.parent = null;
	}
	void Update()
	{
		if (chunks == null)
        {
			chunks = FindObjectOfType<Chunks>();
			networkPlayer = GetComponentInParent<NetworkPlayer>();
			worldChages = FindObjectOfType<WorldChages>();
		}
		float wheel = Input.GetAxis("Mouse ScrollWheel");
		if (wheel > 0)
		{
			activeBlockType++;
			if (activeBlockType > 4) activeBlockType = 1;
		}
		else if (wheel < 0)
		{
			activeBlockType--;
			if (activeBlockType < 1) activeBlockType = 4;
		}

		Ray ray = new Ray(transform.position + transform.forward / 2, transform.forward);
		RaycastHit hit;

		ship = null;
		can = Physics.Raycast(ray, out hit, 4f);
		close = Vector3.Distance(transform.position, hit.point) >= 0.9f;
		if (can)
		{
			if (hit.transform.tag != "Player")
			{
				Vector3 p = hit.point - hit.normal / 2;
				retDel = new Vector3(Mathf.Floor(p.x), Mathf.Floor(p.y), Mathf.Ceil(p.z));
				p = hit.point + hit.normal / 2;
				retAdd = new Vector3(Mathf.Floor(p.x), Mathf.Floor(p.y), Mathf.Ceil(p.z));
				if (hit.transform.tag == "Ship")
				{
					ship = hit.transform.GetComponent<Ship>();
					if (ship.drive)
					{
						ship = null;
					}
				}
			}
		}
		add.position = retAdd;
	}
	Chunk ch = null;
	public byte Control(Vector3 pos, byte block, bool add = true, bool regen = true)
	{

		int x = Mathf.RoundToInt(pos.x);
		int y = Mathf.RoundToInt(pos.y);
		int z = Mathf.RoundToInt(pos.z);
 
		if (add)
		{
			if (chunks.GetChunk(pos.x, pos.y, pos.z, true))
			{
				if (networkPlayer.isServer)
				{
					//var g = FindObjectOfType<Chunks>();
					//int chunkX = Mathf.FloorToInt(pos.x / g.chunk.width);
					//int chunkY = Mathf.FloorToInt(pos.y / g.chunk.width);
					//int chunkZ = Mathf.FloorToInt(pos.z / g.chunk.width);
					//if (f == new Vector3())
					//{
					//	FindObjectOfType<WorldStaticData>().dopChunks.Add(new Vector3(chunkX, chunkY, chunkZ));
					//}

					networkPlayer.AddChunkForAll(new Vector3(pos.x, pos.y, pos.z));
				}
				else
				{
					networkPlayer.AddChunk(new Vector3(pos.x, pos.y, pos.z));
				}

				//GetComponentInParent<NetworkPlayer>().AddChunkForAll(new Vector3(pos.x, pos.y, pos.z));
				//GetComponentInParent<NetworkPlayer>().AddChunk(new Vector3(pos.x, pos.y, pos.z));
			}
		}

		ch = chunks.GetChunkOut(x, y, z);

		if (add)
		{
			if (worldChages == null)
            {
				worldChages = FindObjectOfType<WorldChages>();

			}
			if (networkPlayer.isServer)
			{
				worldChages.AddChangeLocal(new WorldChages.Change() { block = block, blockCoord = new Vector3(x, y, z), chunkCoord = ch.coords, regen = regen});
				worldChages.GetChanges(FindObjectOfType<WorldChages>().changes);
			}
			else
			{
				worldChages.AddChange(new WorldChages.Change() { block = block, blockCoord = new Vector3(x, y, z), chunkCoord =  ch.coords, regen = regen});
			}
		}
		if (ch == null) return 0;
		return ch.map[x - Mathf.FloorToInt(ch.transform.position.x), y - Mathf.FloorToInt(ch.transform.position.y), z - Mathf.FloorToInt(ch.transform.position.z)];
	}
	public List<Chunk> localEdited = new List<Chunk>();
	public byte ControlLocal(Vector3 pos, byte block, bool add)
	{
		int x = Mathf.RoundToInt(pos.x);
		int y = Mathf.RoundToInt(pos.y);
		int z = Mathf.RoundToInt(pos.z);
		if (worldChages == null){
			worldChages = FindObjectOfType<WorldChages>();
		}
		Chunk ch = FindObjectOfType<Chunks>().GetChunkOut(x, y, z);
		if (ch == null) return 0;
		if (!localEdited.Contains(ch))
			localEdited.Add(ch);
		byte id = ch.map[x - Mathf.FloorToInt(ch.transform.position.x), y - Mathf.FloorToInt(ch.transform.position.y), z - Mathf.FloorToInt(ch.transform.position.z)];
		if (add)
		{
			ch.SetBrick(x, y, z, block, false);
			worldChages.AddPrefabs(new WorldChages.Change() { block = block, blockCoord = new Vector3(x,y,z)}, ch);

		}
		return id;
	}

}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Unity.Jobs;
using System.IO.Compression;

[RequireComponent (typeof(MeshFilter))]
public class Chunk : MonoBehaviour {
	
	public int height = 64;
	public int width = 64;
	public byte[,,] map;
	public int cubes, maxcubes;
	public Vector3 coords;
	public int mass = 0;
	protected Mesh mesh;
	List<Vector3> verts = new List<Vector3>();
	List<int> tris = new List<int>();
	List<Vector2> uv = new List<Vector2>();
	
	MeshCollider meshCollider;
	
	public List<Transform> targets = new List<Transform>();
	
	public float offcet;
	[Range(0, 1)]
	public float threshold = .5f;
	public float noiseScale = .05f;
	public float radius = 20f;

	public bool oneBigCenter;
	public MeshRenderer meshRenderer;
	Thread thread;
	bool threadGen;
	public List<Chunk> chunksList = new List<Chunk>();
	public bool active;
	public bool oldactive;

	public List<BlockPrefab> blockPrefabs = new List<BlockPrefab>();

	public float Perlin3D(float x, float y, float z)
	{
		float ab = Mathf.PerlinNoise(x + transform.position.x, y+transform.position.y);
		float bc = Mathf.PerlinNoise(y+ transform.position.y, z+ transform.position.z);
		float ac = Mathf.PerlinNoise(x+ transform.position.x, z+ transform.position.z);

		float ba = Mathf.PerlinNoise(y+ transform.position.y, x+ transform.position.x);
		float cb = Mathf.PerlinNoise(z+ transform.position.z, y+ transform.position.y);
		float ca = Mathf.PerlinNoise(z+ transform.position.z, x+ transform.position.x);

		float abc = ab + bc + ac + ba + cb + ca;
		return abc / 6f;
	}
	public float Perlin3D(float x, float y, float z, bool not)
	{
		float ab = Mathf.PerlinNoise(x, y);
		float bc = Mathf.PerlinNoise(y, z);
		float ac = Mathf.PerlinNoise(x, z);

		float ba = Mathf.PerlinNoise(y, x);
		float cb = Mathf.PerlinNoise(z, y);
		float ca = Mathf.PerlinNoise(z, x);

		float abc = ab + bc + ac + ba + cb + ca;
		return abc / 6f;
	}
	// Use this for initialization
	void Start()
	{
		threadGen = true;
		oldactive = active;
		maxcubes = width * height * width;
		meshRenderer = GetComponent<MeshRenderer>();
		map = new byte[width, height, width];
		meshRenderer.material.SetTexture("_MainTex", FindObjectOfType<LoaderResources>().tiles);
	}

	public void Init(bool gen = false)
	{
		map = new byte[width, height, width];
		Regenerate();
		if (!oneBigCenter)
		{
			Regenerate();
		}
		else
		{
			InitBig();
		}
	}

	public void InitBig()
	{
		transform.name = "startBig";
		var chunks = FindObjectOfType<Chunks>();
		Chunk center = null;
		for (int x = 0; x < 7; x++)
		{
			for (int y = 0; y < 7; y++)
			{
				for (int z = 0; z < 7; z++)
				{
					chunks.Exist(coords + new Vector3(x, y, z), true);
					if (x == 3 && y ==3 && z == 3)
					{
						center = chunks.GetChunk(coords + new Vector3(x, y, z));
						center.transform.name = "Center";
						center.transform.tag = "Center";
						FindObjectOfType<Chunks>().center.Add(center.gameObject);
						GameObject centerObject = new GameObject();
						centerObject.transform.position = center.transform.position + (new Vector3(width, height, width) / 2);
						centerObject.tag = "CenterObject";
						centerObject.AddComponent<SphereCollider>().isTrigger = true;
					}
				}
			}
		}
		int ms = 0;
		for (int x = 0; x < 7; x++)
		{
			for (int y = 0; y <7; y++)
			{
				for (int z = 0; z < 7; z++)
				{
					var c = chunks.GetChunk(coords + new Vector3(x, y, z));
					c.map = new byte[width, height, width];
					c.transform.parent = center.transform;
					if (Vector3.Distance(c.transform.position + (new Vector3(c.width, c.height, c.width) / 2), center.transform.position + new Vector3(center.width, center.height, center.width) / 2) - c.width <= radius)
					{
						chunksList.Add(c);
						c.StartGen(center.transform.position + new Vector3(center.width, center.height, center.width) / 2, radius, x+y+z);
					}
					else
					{
						c.Init();
					}
					ms += c.mass;
				}
			}
		}
		mass = ms;
	}
	public async void StartGen(Vector3 center, float rad, int wait)
	{
		await Task.Delay((int)(wait/2) * 1000);
		thread = new Thread(new ParameterizedThreadStart(Gen));
		object d = new object[7] { center.x, center.y, center.z,transform.position.x, transform.position.y, transform.position.z, rad };
		thread.IsBackground = true;
		thread.Start(d);
	}
	public void Gen(object data)
	{
		Array argArray = new object[7];
		argArray = (Array)data;
		float _x = (float)argArray.GetValue(0);
		float _y = (float)argArray.GetValue(1);
		float _z = (float)argArray.GetValue(2);


		float _xt = (float)argArray.GetValue(3);
		float _yt = (float)argArray.GetValue(4);
		float _zt = (float)argArray.GetValue(5);
		cubes = 0;
		float rad = (float)argArray.GetValue(6);
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				for (int z = 0; z < width; z++)
				{
					float distance = (float)System.Math.Sqrt(Math.Pow((_xt + x) - _x, 2) + Math.Pow((_yt + y) - _y, 2) + Math.Pow((_zt + z) - _z, 2));

					if (distance < rad)
					{
						double noiseValue = (double)Perlin3D((x + _xt) * noiseScale, (y + _yt) * noiseScale, (z + _zt) * noiseScale, true);
						if (noiseValue >= threshold)
						{
							map[x, y, z] = 1;
							mass += 2;
						}
					}
					cubes++;
				}
			}
		}

		if (cubes >= 500)
		{
			int count = (int)((Perlin3D(_x + _xt * 1.001244f, _y + _yt * 1.32224f, _z + _zt * 1.62124f, true) * 15) + 1);
			for (int i = 0; i <count; i++)
			{
				int maxIter = 0;
				int x = (int)((Perlin3D(_x + _xt * 1.25124f + maxIter + i, _y + _yt * 1.32224f + maxIter + i, _z + _zt * 1.12124f + maxIter + i, true) * width));
				int y = (int)((Perlin3D(_x + _xt * 1.65124f + maxIter + i, _y + _yt * 1.10024f + maxIter + i, _z + _zt * 1.52324f + maxIter + i, true) * height));
				int z = (int)((Perlin3D(_x + _xt * 1.15124f + maxIter + i, _y + _yt * 1.52224f + maxIter + i, _z + _zt * 1.22124f + maxIter + i, true) * width));
				
				while (map[x, y, z] == 0)
				{
					maxIter++;
					x = (int)((Perlin3D(_x + _xt * 1.25124f + maxIter + i, _y + _yt * 1.32224f + maxIter + i, _z + _zt * 1.12124f + maxIter + i, true) * width));
					y = (int)((Perlin3D(_x + _xt * 1.65124f + maxIter + i, _y + _yt * 1.10024f + maxIter + i, _z + _zt * 1.52324f + maxIter + i, true) * height));
					z = (int)((Perlin3D(_x + _xt * 1.15124f + maxIter + i, _y + _yt * 1.52224f + maxIter + i, _z + _zt * 1.22124f + maxIter + i, true) * width));

					if (maxIter > 100) { maxIter = -1; break; }
				}
				if (maxIter == -1) break;


				float ore = Perlin3D(_x - _xt * 1.18124f,  _y - _yt * 1.02224f, _z - _zt * 1.4304f, true);
				int radius = (int)((Perlin3D(_x + _xt * 1.25124f, _y + _yt * 1.32224f, _z + _zt * 1.12124f, true) * 30) + 3);
				for (int xr = -radius; xr < radius; xr++)
				{
					if (x + xr + 1 > width) break;
					if (xr < 0) continue;
					for (int yr = -radius; yr < radius; yr++)
					{
						if (y + yr + 1 > height) break;
						if ( yr < 0) continue;
						for (int zr = -radius; zr < radius; zr++)
						{
							if (z + zr + 1 > width) break;
							if (zr < 0) continue;
							if (map[x + xr, y + yr, z + zr] == 0)
								continue;

							if ((((xr + x) - x) * ((xr + x) - x)) - ((yr + y) - y) * (((yr + y) - y)) - (((zr + z) - z) * ((zr + z) - z)) > radius) continue;
							if (ore >= 0.45f)
							{
								map[x + xr, y + yr, z + zr] = 18;
								mass += 4 - 2;
							}else
							if (ore >= 0.4f)
							{
								map[x + xr, y + yr, z + zr] = 3;
								mass += 10 - 2;
							}
							else
							if (ore >= 0.38f)
							{
								map[x + xr, y + yr, z + zr] = 2;
								mass += 8 - 2;
							}
							else
							if (ore >= 0.35f)
							{
								map[x + xr, y + yr, z + zr] = 15;
								mass += 20 - 2;
							}
						}
					}
				}
			}
		}
		threadGen = false;
	}

	// Update is called once per frame
	void LateUpdate()
	{
		if (threadGen == false)
		{
			if (cubes >= maxcubes-1)
			{
				LoadChunk();
				threadGen = true;
			}
		}
	}
	
	public void LoadChunk()
	{
		var changes = FindObjectOfType<WorldChages>().changes;
		//thread.Abort();
		var chkall = changes.FindAll(x => x.chunkCoord == coords);
		foreach (WorldChages.Change chk in chkall)
		{
			SetBrick((int)chk.blockCoord.x, (int)chk.blockCoord.y, (int)chk.blockCoord.z, (byte)chk.block, false);
			worldChages.AddPrefabs(chk, this);
		}

		Regenerate();

	}

	public void CloseMeshTarget() {
		
		mesh.vertices = verts.ToArray();
		mesh.triangles = tris.ToArray();
		mesh.uv = uv.ToArray();
		mesh.RecalculateNormals();
		if (GetComponent<MeshCollider>())
			meshCollider.sharedMesh = mesh; 


	}
		
	public void CreateMeshTarget(bool reset=false) {
		
		if (GetComponent<MeshCollider>())
			meshCollider = GetComponent<MeshCollider>();
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		
		
		verts.Clear();
		tris.Clear();
		uv.Clear ();
		
	}
	
	public void DrawBrick(int x, int y, int z, byte block) 	{
		
		Vector3 start = new Vector3(x, y, z);
		Vector3 offset1, offset2;
		
		if (IsTransparent(x, y - 1, z))
		{
			offset1 = Vector3.left;
			offset2 = Vector3.back;
			DrawFace(start + Vector3.right, offset1, offset2, block);
		}
		if (IsTransparent(x, y + 1, z))
		{
			offset1 = Vector3.right;
			offset2 = Vector3.back;
			DrawFace(start + Vector3.up, offset1, offset2, block);
		}
		if (IsTransparent(x - 1, y, z))
		{
			offset1 = Vector3.up;
			offset2 = Vector3.back;
			DrawFace(start, offset1, offset2, block);
		}
		
		if (IsTransparent(x + 1, y, z))
		{
			offset1 = Vector3.down;
			offset2 = Vector3.back;
			DrawFace(start + Vector3.right + Vector3.up, offset1, offset2, block);
		}
		
		if (IsTransparent(x, y, z - 1))
		{
			offset1 = Vector3.left;
			offset2 = Vector3.up;
			DrawFace(start + Vector3.right + Vector3.back, offset1, offset2, block);
		}
		
		if (IsTransparent(x, y, z + 1))
		{
			offset1 = Vector3.right;
			offset2 = Vector3.up;
			DrawFace(start, offset1, offset2, block);
		}
	}
	
	
	public void DrawFace(Vector3 start, Vector3 offset1, Vector3 offset2, byte block)
	{
		int index = verts.Count;
		
		verts.Add (start);
		verts.Add (start + offset1);
		verts.Add (start + offset2);
		verts.Add (start + offset1 + offset2);
		block -= 1;
		Vector2 uvBase = new Vector2(0.0625f * block - (float)Math.Floor((0.0625 * block)), (float)Math.Floor(0.0625f * block) * 0.0625f);
		uv.Add (uvBase);
		uv.Add (uvBase + new Vector2(0.0620f, 0));
		uv.Add (uvBase + new Vector2(0, 0.0620f));
		uv.Add (uvBase + new Vector2(0.0620f, 0.0620f));

		tris.Add (index + 0);
		tris.Add (index + 1);
		tris.Add (index + 2);
		tris.Add (index + 3);
		tris.Add (index + 2);
		tris.Add (index + 1);
	}
	
	public bool IsTransparent(int x, int y, int z)
	{
		if ((x< 0) || (y < 0) || (z < 0) || (x >= width) || (y >= height) || (z >= width)) return true;
		return map[x,y,z] == 0;
	}
	
	public void Regenerate() {

		CreateMeshTarget(true);
		
		mesh.triangles = tris.ToArray();
		
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				for (int z = 0; z < width; z++)
				{
					byte block = map[x,y,z];
					if (block == 0) continue;
					DrawBrick(x, y, z, block);	
				}
			}
		}
		CloseMeshTarget();
	}
	WorldChages worldChages;
	public void SetBrick(int x, int y, int z, byte block, bool regen = true)
	{
		if (y == 0) return;
		x -= Mathf.RoundToInt(transform.position.x);
		y -= Mathf.RoundToInt(transform.position.y);
		z -= Mathf.RoundToInt(transform.position.z);
		
		if ((x< 0) || (y < 0) || (z < 0) || (x >= width) || (y >= height) || (z >= width)) return;


		if (worldChages == null)
		{
			worldChages = FindObjectOfType<WorldChages>();
		}

		if (map[x,y,z] != block)
		{
			worldChages.AddPrefabs(new WorldChages.Change() { block = block, blockCoord = new Vector3(x, y, z) }, this);	
			map[x,y,z] = block;
			if (regen)
				Regenerate();
		}
	}

	private void OnDrawGizmos()
	{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube(transform.position + new Vector3(width / 2, height / 2, width / 2), new Vector3(width, height, width));
		
	}
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawCube(transform.position + new Vector3(width / 2, height / 2, width / 2), new Vector3(width, height, width));

	}

}


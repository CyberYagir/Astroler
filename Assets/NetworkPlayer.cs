using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using static Inventory;

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkIdentity networkIdentity;
    public MonoBehaviour[] monoBehaviours;
    public MeshRenderer rig;
    public GameObject canvas;
    public GameObject shipPrefab;
    public GameObject dropPrefab;
    [SyncVar]
    public bool inParent;
    [SyncVar]
    public Transform parentgm;
    public Transform oldparentgm;
    private void Start()
    {
        if (!networkIdentity.isLocalPlayer)
        {
            for (int i = 0; i < monoBehaviours.Length; i++)
            {
                Destroy(monoBehaviours[i]);
            }
            foreach (var item in GetComponentsInChildren<Camera>())
            {
                Destroy(item);
            }
            Destroy(GetComponentInChildren<AudioListener>());
            if (isServer)
            {
                FindObjectOfType<WorldChages>().GetChanges(FindObjectOfType<Storage>().changes);
            }
            Destroy(canvas.gameObject);
        }
        else
        {
            GetComponent<PlayerCfgLoader>().Init();
            if (!isServer)
            {
                setName(FindObjectOfType<Config>().cfg.playerName, FindObjectOfType<LoaderResources>().skin.GetRawTextureData());
                print(FindObjectOfType<LoaderResources>().skin.GetRawTextureData().Length);
                GetGenedChunks();
            }
            else
            {
                gameObject.name = FindObjectOfType<Config>().cfg.playerName;                
                FindObjectOfType<LoaderResources>().LoadWorld();
                if (FindObjectOfType<WorldStaticData>().players.Find(x => x.name == FindObjectOfType<Config>().cfg.playerName) == null)
                {
                    FindObjectOfType<WorldStaticData>().players.Add(new WorldStaticData.NetPlayer() { name = gameObject.name, @object = gameObject, unetID = netId, texture2D = FindObjectOfType<LoaderResources>().skin.GetRawTextureData() });
                    GetComponent<Inventory>().AddItem(FindObjectOfType<WorldStaticData>().items[0].CreateInventoryItem(1));
                    GetComponent<Inventory>().AddItem(FindObjectOfType<WorldStaticData>().items[7].CreateInventoryItem(1));
                }
            }
            rig.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
        rig.gameObject.SetActive(true);
        rig.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", FindObjectOfType<LoaderResources>().skin);
    }
    [Command]
    public void SendInventory(string pname, List<InventoryItem> items)
    {
        var pl = FindObjectOfType<WorldStaticData>().players.Find(x => x.name == pname);
        if (pl != null)
        {
            FindObjectOfType<WorldStaticData>().players.Find(x => x.name == pname).inventoryItems = items;
        }
    }
    private void Update()
    {
        if (isLocalPlayer)
        {
            inParent = transform.parent != null;
            parentgm = transform.parent;
            if (isClient)
            {
                if (oldparentgm != parentgm)
                {
                    setParent(parentgm, GetComponent<NetworkIdentity>().netId);
                    oldparentgm = parentgm;
                }
            }
        }
        else
        {
            transform.parent = parentgm;
        }
    }
    [Command]
    public void setParent(Transform tr, uint id)
    {
        var pl = FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == id).GetComponent<NetworkPlayer>();
        pl.parentgm = tr;
        if (tr != null)
        {
            pl.inParent = true;
        }
        pl.transform.parent = tr;
        setParentRPC(tr, id);
    }
    [ClientRpc]
    public void setParentRPC(Transform tr, uint id)
    {
        var pl = FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == id).GetComponent<NetworkPlayer>();
        pl.parentgm = tr;
        if (tr != null)
        {
            pl.inParent = true;
        }
        pl.transform.parent = tr;
    }
    [Command]
    public void setName(string _name, byte[] tex)
    {
        transform.name = name;
        var data = FindObjectOfType<WorldStaticData>();
        var pl = data.players.Find(x => x.name == _name);
        bool playerExist = pl != null;
        if (!playerExist)
        {
            data.players.Add(new WorldStaticData.NetPlayer() { name = _name, @object = gameObject, unetID = networkIdentity.netId, texture2D = tex });
            SendDataAboutPlayerStart(data.players[data.players.Count - 1]);
        }
        else
        {
            if (pl.@object != null)
            {
                PlayerPrefs.SetString("Error", "Player is already on the server");
                gameObject.GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
            }
            pl.@object = gameObject;
            pl.unetID = networkIdentity.netId;
            pl.inventoryItems = pl.inventoryItems.FindAll(x => x != null);
            SendDataAboutPlayer(pl);
        }
        for (int i = 0; i < data.players.Count; i++)
        {
            var skin = new Texture2D(32, 48, TextureFormat.ARGB32, false);
            skin.filterMode = FilterMode.Point;
            skin.LoadRawTextureData(tex);
            skin.Apply();
            data.players[i].@object.name = data.players[i].name;
            data.players[i].@object.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", skin);
        }
        GivePlayersData(data.players);
    }
    [ClientRpc]
    public void SendDataAboutPlayer(WorldStaticData.NetPlayer player)
    {
        var pl = FindObjectsOfType<NetworkPlayer>().ToList().Find(x => x.isLocalPlayer && x.netId == player.unetID);
        if (pl != null)
        {
            var inv = pl.GetComponent<Inventory>();
            inv.items = new InventoryItem[3,9];
            if (player.pos != null){
                pl.transform.position = player.pos.toVector3();
            }
            
            if (player.rot != null){
                pl.transform.localEulerAngles = player.rot.toVector3();
            }
            for (int y = 0; y < player.inventoryItems.Count; y++)
            {
                inv.items[player.inventoryItems[y].pos.y, player.inventoryItems[y].pos.x] = player.inventoryItems[y];
            }
        }
    }
    [ClientRpc]
    public void SendDataAboutPlayerStart(WorldStaticData.NetPlayer player)
    {
        var pl = FindObjectsOfType<NetworkPlayer>().ToList().Find(x => x.isLocalPlayer && x.netId == player.unetID);
        if (pl != null)
        {
            var inv = pl.GetComponent<Inventory>();
            inv.AddItem(FindObjectOfType<WorldStaticData>().items[0].CreateInventoryItem(1));
            inv.AddItem(FindObjectOfType<WorldStaticData>().items[7].CreateInventoryItem(1));
            inv.RedrawInventory();
        }
    }

    [ClientRpc]
    public void GivePlayersData(List<WorldStaticData.NetPlayer> player)
    {
        var data = FindObjectOfType<WorldStaticData>();
        data.players = player;
        for (int i = 0; i < data.players.Count; i++)
        {
            var skin = new Texture2D(32, 48, TextureFormat.ARGB32, false);
            skin.filterMode = FilterMode.Point;
            skin.LoadRawTextureData(data.players[i].texture2D);
            skin.Apply();
            data.players[i].@object.name = data.players[i].name;
            data.players[i].inventoryItems = data.players[i].inventoryItems;
            data.players[i].@object.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", skin);
        }
    }

    [Command]
    public void AddAsteroid(Vector3 pos, float radius)
    {
        FindObjectOfType<WorldStaticData>().meteors.Add(new Meteor() { pos = Inventory.Vect.FromVector3(pos), radius = radius });
        print("ADD ASTEROID");
        AddAsteridForAll(pos, radius);
        var chunks = FindObjectOfType<Chunks>();
        if (chunks.Exists(pos.x, pos.y, pos.z)) return;
        chunks.AddAsteroid(pos, radius, false);
    }
    [ClientRpc]
    public void AddAsteridForAll(Vector3 pos, float radius)
    {
        if (!isServer)
        {
            var chunks = FindObjectOfType<Chunks>();
            if (chunks.Exists(pos.x, pos.y, pos.z)) return;
            chunks.AddAsteroid(pos, radius, false);
        }
    }
    [Command]
    public void GetGenedChunks()
    {
        ReciveAsteroidsAndChunks(FindObjectOfType<WorldStaticData>().meteors, FindObjectOfType<WorldChages>().changes);
    }
    [ClientRpc]
    public void ReciveAsteroidsAndChunks(List<Meteor> meteors, List<WorldChages.Change> changes)
    {
        FindObjectOfType<WorldChages>().changes = changes;
        FindObjectOfType<WorldStaticData>().meteors = meteors;
        var chunksobj = FindObjectOfType<Chunks>();


        for (int i = 0; i < meteors.Count; i++)
        {
            if (chunksobj.Exists(meteors[i].pos.x, meteors[i].pos.y, meteors[i].pos.z)) return;
            chunksobj.AddAsteroid(meteors[i].pos.toVector3(), meteors[i].radius, false);
        }
        for (int i = 0; i < changes.Count; i++)
        {
            FindObjectOfType<Chunks>().GetChunk(changes[i].blockCoord.x, changes[i].blockCoord.y, changes[i].blockCoord.z, true);
            FindObjectOfType<Chunks>().GetChunkOut((int)changes[i].blockCoord.x, (int)changes[i].blockCoord.y, (int)changes[i].blockCoord.z).LoadChunk();
        }
        var cng = FindObjectOfType<WorldChages>().changes;

        for (int i = 0; i < cng.Count; i++)
        {
            var c = chunksobj.GetChunk(cng[i].chunkCoord);
            if (c != null)
            {
                print("block");
                c.SetBrick((int)cng[i].blockCoord.x, (int)cng[i].blockCoord.y, (int)cng[i].blockCoord.z, (byte)cng[i].block);
            }
        }
    }
    [Command]
    public void AddChunk(Vector3 pos)
    {
        print("AddChunk");
        FindObjectOfType<Chunks>().GetChunk(pos.x, pos.y, pos.z, true);
        AddChunkForAll(pos);
    }
    [ClientRpc]
    public void AddChunkForAll(Vector3 pos)
    {
        FindObjectOfType<Chunks>().GetChunk(pos.x, pos.y, pos.z, true);
    }
    [Command]
    public void AddChunkShip(Vector3 pos, int id, List<Vector3> poses, List<byte> blocks, NetworkIdentity unetdriverid)
    {
        AddChunkForAllShip(pos, id, poses, blocks, AddShipLocal(pos, id, poses, blocks, unetdriverid), unetdriverid);
    }
    [ClientRpc]
    public void AddChunkForAllShip(Vector3 pos, int id, List<Vector3> poses, List<byte> blocks, uint unetid, NetworkIdentity unetdriverid)
    {
        //AddShipLocal(pos, id, poses, blocks);
        var ch = FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == unetid).GetComponent<Chunk>();
        ch.gameObject.GetComponent<Ship>().id = FindObjectsOfType<Ship>().Length;
        ch.GetComponent<Ship>().networkPlayer = unetdriverid;
        ch.GetComponent<Ship>().drive = true;
        var newposes = new List<Vector3>();
        for (int i = 0; i < poses.Count; i++)
        {
            newposes.Add(poses[i] - new Vector3(Mathf.FloorToInt(ch.transform.position.x), Mathf.FloorToInt(ch.transform.position.y), Mathf.FloorToInt(ch.transform.position.z)));
        }
        FindObjectOfType<PlayerActiveTables>().poses = poses;
        FindObjectOfType<PlayerActiveTables>().bl = blocks;
        FindObjectOfType<PlayerActiveTables>().ch = ch;
        ch.Init();
        ch.GetComponent<Ship>().vector3s = newposes;
        ch.GetComponent<Ship>().blocks = blocks;
        ch.GetComponent<Ship>().Init();
        FindObjectOfType<PlayerActiveTables>().GenShip(ch);
    }
    public uint AddShipLocal(Vector3 pos, int id, List<Vector3> poses, List<byte> blocks, NetworkIdentity unetdriverid)
    {
        var chunk = Instantiate(shipPrefab.gameObject, pos - new Vector3Int(16, 16, 16), Quaternion.identity);
        var ch = chunk.GetComponent<Chunk>();
        chunk.GetComponent<Ship>().blocks = blocks;
        chunk.GetComponent<Ship>().drive = true;
        chunk.GetComponent<Ship>().networkPlayer = unetdriverid;
        NetworkServer.Spawn(chunk);
        return chunk.GetComponent<NetworkIdentity>().netId;
    }

    [Command]
    public void CmdShipMove(Vector3 add, uint networkIdentity)
    {
        var ship = FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == networkIdentity);
        if (ship == null) return;
        ship.GetComponent<Rigidbody>().AddForce(add);
        RpcMove(ship.transform.position, networkIdentity);
    }
    [ClientRpc]
    public void RpcMove(Vector3 add, uint networkIdentity)
    {
        var local = FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.isLocalPlayer && x.isServer);
        if (local != null) return;

        var ship = FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == networkIdentity);
        if (ship == null) return;
        ship.transform.position = add;
    }

    [Command]
    public void CmdShipRot(Vector3 pos, Vector3 dir, float speed, NetworkIdentity networkIdentity)
    {
        RpcRot(pos, dir, speed, networkIdentity);
    }
    [ClientRpc]
    public void RpcRot(Vector3 pos, Vector3 dir, float speed, NetworkIdentity networkIdentity)
    {
        FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == networkIdentity.netId).transform.RotateAround(pos, dir, speed);
    }


    [Command]
    public void CmdShipSetPos(Vector3 add, Vector3 rot, NetworkIdentity networkIdentity)
    {
        FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == networkIdentity.netId).transform.position = add;
        FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == networkIdentity.netId).transform.localEulerAngles = rot;
        RpcShipSetPos(add, rot, networkIdentity);
    }
    [ClientRpc]
    public void RpcShipSetPos(Vector3 add, Vector3 rot, NetworkIdentity networkIdentity)
    {
        FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == networkIdentity.netId).transform.position = add;
        FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == networkIdentity.netId).transform.localEulerAngles = rot;
    }

    [Command]
    public void CmdShipDel(NetworkIdentity networkIdentity)
    {
        RPCDestroy(networkIdentity.netId);
        var stat = FindObjectOfType<WorldStaticData>();
        var ship = FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == networkIdentity.netId);
        ship.GetComponent<Ship>().destroy = true;
        if (ship == null) return;
        for (int i = 0; i < stat.players.Count; i++)
        {
            if (stat.players[i].@object.transform.parent == ship.transform)
            {
                stat.players[i].@object.transform.parent = null;
            }
        }
        NetworkServer.Destroy(ship.gameObject);
    }
    [Command]
    public void CmdDespawnShipOnServer(NetworkIdentity networkIdentity)
    {
        var stat = FindObjectOfType<WorldStaticData>();
        var ship = FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == networkIdentity.netId);
        ship.GetComponent<Ship>().destroy = true;
        for (int i = 0; i < stat.players.Count; i++)
        {
            if (stat.players[i].@object.transform.parent == ship.transform)
            {
                stat.players[i].@object.transform.parent = null;
            }
        }
        FindObjectOfType<PlayerActiveTables>().DeGenShip(FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == networkIdentity.netId).GetComponent<Ship>());
    }
    [ClientRpc]
    public void RPCDestroy(uint networkIdentity)
    {
        var stat = FindObjectOfType<WorldStaticData>();
        var ship = FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == networkIdentity);
        if (ship == null) return;
        for (int i = 0; i < stat.players.Count; i++)
        {
            if (stat.players[i].@object.transform.parent == ship.transform)
            {
                stat.players[i].@object.transform.parent = null;
            }
        }
    }
    [Command]
    public void CmdDropItem(InventoryItem item, Vector3 pos)
    {
        var k = Instantiate(dropPrefab, pos, Quaternion.identity);
        k.GetComponent<Drop>().inventoryItem = item;
        NetworkServer.Spawn(k);
        RpcLoadDropItem(k.GetComponent<NetworkIdentity>().netId, item);
    }

    [ClientRpc]
    public void RpcLoadDropItem(uint id, InventoryItem it)
    {
        FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == id).GetComponent<Drop>().inventoryItem = it;
        FindObjectsOfType<NetworkIdentity>().ToList().Find(x => x.netId == id).GetComponent<Drop>().Init();
    }

    [Command]
    public void CmdDestroyItem(Transform id)
    {
        NetworkServer.Destroy(id.gameObject);
        RpcDestroyItem(id);
    }
    [ClientRpc]
    public void RpcDestroyItem(Transform id)
    {
        if (id != null)
        {
            NetworkServer.Destroy(id.gameObject);
        }
    }
}

using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static Inventory;
using static WorldChages;

public class LoaderResources : MonoBehaviour
{
    public string exe = "";
    public Texture2D tiles, skin;
    public Texture2D stdTiles, stdPlayer;
    public string[] changelog;


    [System.Serializable]
    public class GamePlayer {
        public string nick;
        public Vect pos;
        public Vect rot;
        public List<InventoryItem> items = new List<InventoryItem>();

    }

    [System.Serializable]
    public class World {
        public List<GamePlayer> players = new List<GamePlayer>();
        public List<ChangeSave> changes = new List<ChangeSave>();
        public List<Meteor> asters = new List<Meteor>();
    }
    [System.Serializable]
    public class Bak
    {
        public string version = "";
    }


    void Start()
    {
        exe = Path.GetFullPath(Path.Combine(Application.dataPath, @"..\"));
        if (!Directory.Exists(exe + @"\textures\"))
        {
            tiles = stdTiles;
            skin = stdPlayer;
        }
        if (File.Exists(exe + @"\change.log"))
        {
            changelog = File.ReadAllLines(exe + @"\change.log");
        }
        Directory.CreateDirectory(exe + @"\Worlds\");

        StartCoroutine(loadTiles());
        StartCoroutine(loadSkin());
        StartCoroutine(autoSave());
    }
    string path = ""; 

    public void SaveWorld()
    {
        path = exe + @"\worlds\" + PlayerPrefs.GetString("World") + @"\";
        BinaryFormatter bf = new BinaryFormatter();
        Directory.CreateDirectory(path);
        FileStream fs = new FileStream(path + PlayerPrefs.GetString("World") + ".world", FileMode.Create);
        List<GamePlayer> players = new List<GamePlayer>();
        var wsd = FindObjectOfType<WorldStaticData>();
        var playersStatic = wsd.players;

        for (int i = 0; i < playersStatic.Count; i++)
        {
            if (playersStatic[i].@object != null)
            {
                playersStatic[i].pos = new Vect(playersStatic[i].@object.transform.position.x, playersStatic[i].@object.transform.position.y, playersStatic[i].@object.transform.position.z);
                playersStatic[i].rot = new Vect(playersStatic[i].@object.transform.localEulerAngles.x, playersStatic[i].@object.transform.localEulerAngles.y, playersStatic[i].@object.transform.localEulerAngles.z);
            }
            players.Add(new GamePlayer() { nick = playersStatic[i].name, items = playersStatic[i].inventoryItems, pos = playersStatic[i].pos, rot = playersStatic[i].rot});
        }
        var world = new World() { changes = WorldChages.toChangeSave(FindObjectOfType<WorldChages>().changes), players = players, asters = FindObjectOfType<WorldStaticData>().meteors };

        bf.Serialize(fs, world);
        fs.Close();
        SaveBak();

    }
    public void SaveBak()
    {
        path = exe + @"\worlds\" + PlayerPrefs.GetString("World") + @"\";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(path + PlayerPrefs.GetString("World") + ".info", FileMode.Create);
        var bak = new Bak() { version = Application.version };
        bf.Serialize(fs, bak);
        fs.Close();
    }
    public static void SaveBak(string path)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(path, FileMode.Create);
        var bak = new Bak() { version = Application.version };
        bf.Serialize(fs, bak);
        fs.Close();
    }
    public static Bak GetBak(string path)
    {
        if (!File.Exists(path)) {
            return null;
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        var world = (Bak)bf.Deserialize(fs);
        fs.Close();
        return world;
    }



    public void LoadWorld()
    {

        path = exe + @"\worlds\" + PlayerPrefs.GetString("World") + @"\";
        if (!PlayerPrefs.HasKey("World")) return;
        if (File.ReadAllText(path + PlayerPrefs.GetString("World") + ".world").Trim().Replace(" ", "") == "")
        {
            //FindObjectOfType<Inventory>().AddItem(FindObjectOfType<WorldStaticData>().items[0].CreateInventoryItem(1));
            //FindObjectOfType<Inventory>().AddItem(FindObjectOfType<WorldStaticData>().items[7].CreateInventoryItem(1));
            return;
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(path + PlayerPrefs.GetString("World") + ".world", FileMode.OpenOrCreate);
        var world = (World)bf.Deserialize(fs);
        fs.Close();
        var g = WorldChages.fromChangeSave(world.changes);
        var cfg = FindObjectOfType<Config>();
        var st = FindObjectOfType<WorldStaticData>();

        for (int i = 0; i < world.players.Count; i++)
        {
            var p = world.players[i];
            st.players.Add(new WorldStaticData.NetPlayer() { inventoryItems = p.items, name = p.nick, pos = p.pos, rot = p.rot });
            var n = st.players.Find(x=>x.name == world.players[i].nick);
            if (n != null)
            {
                if (n.name == cfg.cfg.playerName)
                {
                    n.inventoryItems = world.players[i].items;
                    n.pos = world.players[i].pos;
                    n.rot = world.players[i].rot;
                    n.@object = GameObject.Find(n.name);
                    if (n.@object != null)
                    {
                        n.@object.transform.position = n.pos.toVector3();
                        n.@object.transform.localEulerAngles = n.rot.toVector3();
                        var inv = n.@object.GetComponent<Inventory>();
                        for (int y = 0; y < n.inventoryItems.Count; y++)
                        {
                            inv.items[n.inventoryItems[y].pos.y, n.inventoryItems[y].pos.x] = n.inventoryItems[y];
                        }
                    }
                }
            }
        }
        for (int i = 0; i < g.Count; i++)
        {
            FindObjectOfType<WorldChages>().AddChangeLocal(g[i]);
        }
        var wsd = FindObjectOfType<WorldStaticData>();
        wsd.meteors = world.asters;
        var chunks = FindObjectOfType<Chunks>();
        var player = FindObjectOfType<NetworkPlayer>();
        for (int i = 0; i < world.asters.Count; i++)
        {
            if (Vector3.Distance(player.transform.position, world.asters[i].pos.toVector3()) < 350)
            {
                chunks.AddAsteroid(world.asters[i].pos.toVector3(), world.asters[i].radius, true);
            }
        }
        for (int i = 0; i < world.changes.Count; i++)
        {
            if (!chunks.Exist(new Vector3(world.changes[i].chunkCoord.xf, world.changes[i].chunkCoord.yf, world.changes[i].chunkCoord.zf), false))
            {
                print("Not exists");
                chunks.Exist(new Vector3(world.changes[i].chunkCoord.xf, world.changes[i].chunkCoord.yf, world.changes[i].chunkCoord.zf), true);
            }
            
        }
    }
    IEnumerator autoSave()
    {
        while (true)
        {
            if (Application.loadedLevel == 1)
                if (FindObjectOfType<Ship>() == null)
                    SaveWorld();
            yield return new WaitForSeconds(60);
        }
    }
    IEnumerator loadSkin()
    {
        var tex = new Texture2D(32, 48, TextureFormat.RGBA32, false);
        using (WWW www = new WWW(exe + @"\textures\skin.png"))
        {
            yield return www;
            www.LoadImageIntoTexture(tex);
            tex.filterMode = FilterMode.Point;
            skin = tex;
        }
        if (skin == null || !File.Exists(exe + @"\textures\skin.png") || tex.width == 8)
        {
            skin = stdPlayer;
        }
    }
    IEnumerator loadTiles()
    {
        Texture2D tex;
        tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        using (WWW www = new WWW(exe + @"\textures\tiles.png"))
        {
            yield return www;
            www.LoadImageIntoTexture(tex);
            tex.filterMode = FilterMode.Point;
            tiles = tex;
        }
        if (tiles == null && !File.Exists(exe + @"\textures\tiles.png") || tex.width == 8)
        {
            tiles = stdTiles;
        }
    }
}

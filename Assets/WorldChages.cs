using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Inventory;

public class WorldChages : NetworkBehaviour
{
    public List<Change> changes = new List<Change>();
    public TMP_Text text;


    private void Start()
    {

    }
    private void FixedUpdate()
    {
        if (isServer)
        {
            changes = FindObjectOfType<Storage>().changes;
        }
    }

    [ClientRpc]
    public void GetChanges(List<Change> _changes)
    {
        var wrld = FindObjectOfType<WorldChages>();
        wrld.changes = _changes;
        var cng = wrld.changes;
        //cng = _changes;
        var chunks = FindObjectOfType<Chunks>();
        for (int i = 0; i < cng.Count; i++)
        {
            var c = chunks.GetChunk(cng[i].chunkCoord);
            if (c != null)
            { 
                c.SetBrick((int)cng[i].blockCoord.x, (int)cng[i].blockCoord.y, (int)cng[i].blockCoord.z, (byte)cng[i].block, cng[i].regen);
                AddPrefabs(cng[i], c);
            }
        }
    }
    public void AddPrefabs(Change cng, Chunk c)
    {
        var b = c.blockPrefabs.Find(x => x.pos == cng.blockCoord);
        if ((byte)cng.block == 0)
        {
            if (b != null)
            {
                c.blockPrefabs.Remove(b);
                Destroy(b.gameObject);
            }
        }
        if (b == null)
        {
            var item = FindObjectOfType<WorldStaticData>().items.Find(x => x.blockID == cng.block);
            if (item.spawnPrefab)
            {
                var bl = Instantiate(item.prefab.gameObject, cng.blockCoord, Quaternion.identity).GetComponent<BlockPrefab>();
                bl.pos = cng.blockCoord;
                bl.chunk = c;
                c.blockPrefabs.Add(bl);
            }
        }
    }

    public static List<ChangeSave> toChangeSave(List<Change> ch)
    {
        List<ChangeSave> sv = new List<ChangeSave>();
        for (int i = 0; i < ch.Count; i++)
        {
            sv.Add(ch[i].toSave());
        }
        return sv;
    }


    public static List<Change> fromChangeSave(List<ChangeSave> ch)
    {
        List<Change> sv = new List<Change>();
        for (int i = 0; i < ch.Count; i++)
        {
            sv.Add(ch[i].toChange());
        }
        return sv;
    }


    [Command]
    public void QueryGetChanges()
    {
        //Debug.LogError("QueryGetChanges");
        FindObjectOfType<WorldChages>().GetChanges(FindObjectOfType<Storage>().changes);

    }
    public void AddChangeLocal(Change change)
    {
        var chk = changes.Find(x => x.chunkCoord == change.chunkCoord);
        if (chk != null)
        {
            var bl = changes.Find(x => x.chunkCoord == change.chunkCoord && x.blockCoord == change.blockCoord);
            if (bl != null)
            {
                changes.Remove(bl);
            }
        }
        changes.Add(change);
        FindObjectOfType<Storage>().changes = changes;
    }

    [Command]
    public void AddChange(Change change)
    {
        FindObjectOfType<WorldChages>().AddChangeLocal(change);
        FindObjectOfType<WorldChages>().GetChanges(FindObjectOfType<WorldChages>().changes);
    }




    [System.Serializable]
    public class Change {
        public Vector3 chunkCoord;
        public Vector3 blockCoord;
        public int block;
        public bool regen;

        public string ToString()
        {
            return chunkCoord.ToString() + " | " + blockCoord.ToString() + " | " + block;
        }

        public ChangeSave toSave()
        {
            return new ChangeSave() { block = block, blockCoord = Vect.FromVector3(blockCoord), chunkCoord = Vect.FromVector3(chunkCoord) };
        }
    }
    [System.Serializable]
    public class ChangeSave{
        public Vect chunkCoord;
        public Vect blockCoord;
        public int block;
        

        public Change toChange()
        {
            return new Change() { chunkCoord = chunkCoord.toVector3(), block = block, blockCoord = blockCoord.toVector3(), regen = false };
        }
    }

}

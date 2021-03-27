using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlayerActiveTables : MonoBehaviour
{
    public int currentBlock = -1;
    public BlocksAddRemove blocksAdd;
    public GameObject pressF;
    public Vector3Int pos;
    public GameObject consoleWindow, crafterWindow, forgeWindow;
    public NetworkPlayer networkPlayer;
    public GameObject genShipButton;
    bool init;
    private void Start()
    {
        networkPlayer = GetComponentInParent<NetworkPlayer>();
        pressF.GetComponent<TMP_Text>().text = "Press " + InputManager.k.GetAxisFull("use").plus.ToString();
    }
    void Update()
    {
        if (!init)
        {
            if (FindObjectOfType<InputManager>() != null)
            {
                init = true;
            }
            return;
        }
        if (blocksAdd.can)
        {
            int x = Mathf.RoundToInt(blocksAdd.retDel.x);
            int y = Mathf.RoundToInt(blocksAdd.retDel.y);
            int z = Mathf.RoundToInt(blocksAdd.retDel.z);
            pos = new Vector3Int(x, y, z);
            currentBlock = blocksAdd.Control(blocksAdd.retDel, 0, false);
        }
        else
        {
            currentBlock = -1;
        }
        pressF.SetActive(currentBlock == 4 || currentBlock == 6 || currentBlock == 16 || currentBlock == 20);

        if (InputManager.k.GetAxisFull("use").down)
        {
            if (currentBlock == 4)
            {
                consoleWindow.SetActive(!consoleWindow.active);
            }
            else
            if (currentBlock == 6)
            {
                crafterWindow.SetActive(!crafterWindow.active);
                if (crafterWindow.active)
                    FindObjectOfType<Crafter>().FullUpdate();
            }
            if (currentBlock == 20)
            {
                crafterWindow.SetActive(!crafterWindow.active);
                if (crafterWindow.active)
                    FindObjectOfType<Crafter>().FullUpdate();
            }
            else if (currentBlock == 16)
            {
                forgeWindow.SetActive(!forgeWindow.active);
                if (forgeWindow.active)
                    FindObjectOfType<Crafter>().FullUpdate();
            }
        }
    }

    public void ClickToDrive()
    {
        StartCoroutine(StartDrive());
    }
    IEnumerator StartDrive()
    {
        consoleWindow.GetComponent<ConsoleMenu>().buttonText.text = "Please wait";
        yield return new WaitForSeconds(0.4f);
        InitActivateShip(pos, false);
        consoleWindow.GetComponent<ConsoleMenu>().buttonText.text = "Drive";
    }

    public List<Vector3> poses = new List<Vector3>();
    public List<byte> bl = new List<byte>();
    public int recursions = 0;
    public Chunk ch;
    int cmdCount = 0;
    Vector3 consolePos;
    public void InitActivateShip(Vector3 _pos, bool isquery = true)
    {
        consolePos = _pos;
        cmdCount = 0;
        poses = new List<Vector3>();
        bl = new List<byte>();
        recursions = 0;
        AddToShip(_pos, blocksAdd.Control(pos, 0, false));
        cmdCount += 1;

        print(System.Environment.StackTrace);
        print(recursions);
        if (isquery == false)
        {
            networkPlayer.AddChunkShip(_pos, FindObjectsOfType<Ship>().Length, poses, bl, GetComponentInParent<NetworkIdentity>());
            return;
        }
    }

    byte[,,] shipMap = new byte[32, 32, 32];
    public async void GenShip(Chunk chunk)
    {
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                for (int z = 0; z < 32; z++)
                {
                    shipMap[x, y, z] = 0;
                }
            }
        }
        genShipButton.SetActive(false);
        chunk.GetComponent<Ship>().enabled = false;
        blocksAdd.localEdited = new List<Chunk>();
        var w = FindObjectOfType<WorldChages>();
        for (int i = 0; i < poses.Count; i++)
        {
            await Task.Delay(2);
            blocksAdd.ControlLocal(poses[i], 0, false);
            chunk.SetBrick((int)poses[i].x, (int)poses[i].y, (int)poses[i].z, bl[i], false);
            w.AddPrefabs(new WorldChages.Change { block = bl[i], blockCoord = new Vector3((int)poses[i].x, (int)poses[i].y, (int)poses[i].z) }, chunk);
        }
        for (int i = 0; i < blocksAdd.localEdited.Count; i++)
        {
            blocksAdd.localEdited[i].Regenerate();
        }
        chunk.Regenerate();

        chunk.GetComponent<Ship>().enabled = true;
        consoleWindow.SetActive(false);
        genShipButton.SetActive(true);
    }
    public async void DeGenShip(Ship ship)
    {
        var ident = GetComponentInParent<NetworkPlayer>();
        if (!ident.isServer)
        {
            GetComponentInParent<NetworkPlayer>().CmdDespawnShipOnServer(ship.GetComponent<NetworkIdentity>());
            return;
        }
        blocksAdd.localEdited = new List<Chunk>();
        var ch = ship.GetComponent<Chunk>();
        for (int i = 0; i < ship.vector3s.Count; i++)
        {
            Vector3 globalpos = ship.transform.TransformPoint(ship.vector3s[i]);//  new Vector3(Mathf.FloorToInt(ship.vector3s[i].x + ship.transform.position.x), Mathf.FloorToInt(ship.vector3s[i].y + ship.transform.position.y), Mathf.FloorToInt(ship.vector3s[i].z + ship.transform.position.z));
            FindObjectOfType<Chunks>().GetChunk((int)ship.vector3s[i].x, (int)ship.vector3s[i].y, (int)ship.vector3s[i].z, true);
            Chunk newch = FindObjectOfType<Chunks>().GetChunkOut((int)ship.vector3s[i].x, (int)ship.vector3s[i].y, (int)ship.vector3s[i].z);
            if (newch == null) continue;
            await Task.Delay(10);
            blocksAdd.Control(globalpos, ch.map[(int)ship.vector3s[i].x, (int)ship.vector3s[i].y, (int)ship.vector3s[i].z], true, true);
            //newch.SetBrick((int)globalpos.x, (int)globalpos.y, (int)globalpos.z, 1);//);
            if (!blocksAdd.localEdited.Contains(newch))
                blocksAdd.localEdited.Add(newch);
        }
        for (int i = 0; i < blocksAdd.localEdited.Count; i++)
        {
            blocksAdd.localEdited[i].Regenerate();
        }
        if (ident.isClient)
        {
            ship.destroy = true;

            ident.CmdShipDel(ship.GetComponent<NetworkIdentity>());
        }
        FindObjectOfType<PlayerInterface>().degenShip.SetActive(false);
    }
    public void AddToShip(Vector3 pos, byte block)
    {
        if (recursions > 199) return;
        Vector3Int localpos = new Vector3Int(16, 16, 16) + Vector3Int.CeilToInt(pos - consolePos);
        if (shipMap[localpos.x, localpos.y, localpos.z] != 0) return;
        shipMap[localpos.x, localpos.y, localpos.z] = block;
        recursions++;
        poses.Add(pos);
        bl.Add(block);
        blocksAdd.Control(pos, 0, true, false);
        if (block == 4)
        {
            cmdCount += 1;
        }
        byte down = blocksAdd.ControlLocal(pos + Vector3.down, 0, false);
        byte up = blocksAdd.ControlLocal(pos + Vector3.up, 0, false);
        byte left = blocksAdd.ControlLocal(pos + Vector3.left, 0, false);
        byte right = blocksAdd.ControlLocal(pos + Vector3.right, 0, false);
        byte forw = blocksAdd.ControlLocal(pos + Vector3.forward, 0, false);
        byte back = blocksAdd.ControlLocal(pos + Vector3.back, 0, false);

        if (down != 0 && Mathf.Abs(((pos + Vector3.down) - consolePos).magnitude) <= 16)
        {
            AddToShip(pos + Vector3.down, down);
        }
        if (up != 0 && Mathf.Abs(((pos + Vector3.up) - consolePos).magnitude) <= 16)
        {
            AddToShip(pos + Vector3.up, up);
        }
        if (left != 0 && Mathf.Abs(((pos + Vector3.left) - consolePos).magnitude) <= 16)
        {
            AddToShip(pos + Vector3.left, left);
        }
        if (right != 0 && Mathf.Abs(((pos + Vector3.right) - consolePos).magnitude) <= 16)
        {
            AddToShip(pos + Vector3.right, right);
        }
        if (forw != 0 && Mathf.Abs(((pos + Vector3.forward) - consolePos).magnitude) <= 16)
        {
            AddToShip(pos + Vector3.forward, forw);
        }
        if (back != 0 && Mathf.Abs(((pos + Vector3.back) - consolePos).magnitude) <= 16)
        {
            AddToShip(pos + Vector3.back, back);
        }
    }
}

using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Ship : NetworkBehaviour
{
    public int id;
    public List<Vector3> vector3s;
    public List<byte> blocks;
    [SyncVar]
    public bool drive;
    public NetworkIdentity networkPlayer;
    public GameObject pos, center;
    public float mass;
    public int enerny, minenergy;
    public WorldStaticData wsd;

    public GameObject cubeColliderObject;
    public byte[] thrusters = new byte[7];// redThruster, blueThruster, pinkThruster, greenThruster, whiteThruster, yellowThruster, lightGreenThruster;
    [SyncVar]
    public bool destroy;
    public List<GameObject> playersIn;

    public List<ShipThruster> thruster = new List<ShipThruster>();

    public GameObject canvas;
    public TMP_Text energyT, massT;
    [System.Serializable]
    public class ShipThruster {
        public string key;
        public byte type;
        public byte axis;
        public byte count;
    }
    private void Start()
    {
        if (!FindObjectsOfType<NetworkIdentity>().ToList().Find(x=>x.isLocalPlayer).isServer)
            Destroy(GetComponent<Rigidbody>());
        wsd = FindObjectOfType<WorldStaticData>();

        if (networkPlayer.isLocalPlayer)
        {
            networkPlayer.GetComponent<Rigidbody>().isKinematic = true;
        }
    }
    public void Init()
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            Instantiate(cubeColliderObject.gameObject, transform.position + vector3s[i] + new Vector3(.5f, .5f, -.5f), Quaternion.identity, transform);
            if (blocks[i] == 1)
            {
                mass += 2;
            }
            else if (blocks[i] == 2)
            {
                mass += 8;
            }
            else if (blocks[i] == 3)
            {
                mass += 6;
            }
            else if (blocks[i] == 19)
            {
                mass += 1;
            }
            else
            {
                mass += 5;
            }
            if (blocks[i] == 7)
            {
                thrusters[0]++;
            }
            else if (blocks[i] == 8)
            {
                thrusters[1]++;
            }
            else if (blocks[i] == 9)
            {
                thrusters[2]++;
            }
            else if (blocks[i] == 10)
            {
                thrusters[3]++;
            }
            else if (blocks[i] == 11)
            {
                thrusters[4]++;
            }
            else if (blocks[i] == 12)
            {
                thrusters[5]++;
            }
            else if (blocks[i] == 13)
            {
                thrusters[6]++;
            }else if (blocks[i] == 14)
            {
                enerny += 2;
            }
        }

        minenergy = thrusters[0] + thrusters[1] + thrusters[2] + thrusters[3] + thrusters[4] + thrusters[5] + thrusters[6];
        var c = FindObjectOfType<PlayerActiveTables>().consoleWindow.GetComponent<ConsoleMenu>();
        for (int i = 0; i < 7; i++)
        {
            thruster.Add(new ShipThruster() { axis = (byte)c.keysList[i].axis.value, count = thrusters[i], type = (byte)c.keysList[i].type.value, key = c.keysList[i].key.text});
        }
        mass = mass / 100;
        energyT.text = enerny + "/" + minenergy;
        massT.text = mass + "t.";
        canvas.SetActive(networkPlayer.isLocalPlayer);
    }

    private void OnDestroy()
    {
    }
    private void Update()
    {
        if (!destroy)
        {
            for (int i = 0; i < wsd.players.Count; i++)
            {
                if (Vector3.Distance(GetComponent<Renderer>().bounds.center, wsd.players[i].@object.transform.position) < mass * 2)
                {
                    if (wsd.players[i].@object.transform.parent == null)
                    {
                        if (wsd.players[i].@object.GetComponent<NetworkIdentity>().isLocalPlayer)
                        {
                            wsd.players[i].@object.transform.parent = transform;
                            break;
                        }
                    }
                }
                else
                {
                    if (wsd.players[i].@object.transform.parent == transform)
                    {
                        if (wsd.players[i].@object.GetComponent<NetworkIdentity>().isLocalPlayer)
                        {
                            wsd.players[i].@object.transform.parent = null;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < wsd.players.Count; i++)
            {
                if (wsd.players[i].@object.transform.parent == transform)
                {
                    wsd.players[i].@object.transform.parent = null;
                }
            }
        }
        if (drive)
        {
            if (networkPlayer.isLocalPlayer)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    FindObjectOfType<PlayerInterface>().degenShip.SetActive(false);
                    networkPlayer.GetComponent<Rigidbody>().isKinematic = false;
                    Vector3 rot = new Vector3(Mathf.RoundToInt((transform.localEulerAngles.x / 90)) * 90, Mathf.RoundToInt((transform.localEulerAngles.y / 90)) * 90, Mathf.RoundToInt((transform.localEulerAngles.z / 90)) * 90);
                    Vector3 pos = new Vector3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
                    networkPlayer.GetComponent<NetworkPlayer>().CmdShipSetPos(pos, rot, GetComponent<NetworkIdentity>());
                    networkPlayer.GetComponentInChildren<PlayerActiveTables>().DeGenShip(this);
                    networkPlayer.GetComponent<Player>().freezePos = false;
                    networkPlayer = null;
                    drive = false;
                    return;
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (drive)
        {
            if (networkPlayer == null)
            {
                if (pos != null)
                {
                    Destroy(pos.gameObject);
                }
                drive = false;
                return;
            }
            if (networkPlayer.isLocalPlayer)
            {
                networkPlayer.GetComponent<Player>().freezePos = true;
                if (center == null)
                {
                    center = new GameObject();
                    center.transform.parent = transform;
                    center.transform.localPosition = new Vector3(16, 16, 16);
                }
                //networkPlayer.transform.position = transform.Lerp(networkPlayer.transform.position, pos.transform.position, 10 * Time.deltaTime);
                if (enerny < minenergy) return;
                Vector3 p = center.transform.position;
                for (int i = 0; i < thruster.Count; i++)
                {
                    if (thruster[i].key == "") continue;
                    if (Input.GetKey(thruster[i].key.ToLower()))
                    {
                        if (thruster[i].axis == 0)
                        {
                            if (thruster[i].type == 0)
                            {
                                networkPlayer.GetComponent<NetworkPlayer>().CmdShipMove(transform.up * Time.deltaTime * thruster[i].count * 8f / mass, GetComponent<NetworkIdentity>().netId);
                            }
                            else
                            {
                                networkPlayer.GetComponent<NetworkPlayer>().CmdShipRot(p, transform.up, Time.deltaTime * thruster[i].count * 5f / mass, GetComponent<NetworkIdentity>());

                            }
                        }
                        else if (thruster[i].axis == 1)
                        {
                            if (thruster[i].type == 0)
                            {
                                networkPlayer.GetComponent<NetworkPlayer>().CmdShipMove(-transform.up * Time.deltaTime * thruster[i].count * 8f / mass, GetComponent<NetworkIdentity>().netId);

                            }
                            else
                            {
                                networkPlayer.GetComponent<NetworkPlayer>().CmdShipRot(p, -transform.up, Time.deltaTime * thruster[i].count * 5f / mass, GetComponent<NetworkIdentity>());
                            }
                        }
                        else if (thruster[i].axis == 2)
                        {
                            if (thruster[i].type == 0)
                            {
                                networkPlayer.GetComponent<NetworkPlayer>().CmdShipMove(transform.right * Time.deltaTime * thruster[i].count * 8f / mass, GetComponent<NetworkIdentity>().netId);

                            }
                            else
                            {
                                networkPlayer.GetComponent<NetworkPlayer>().CmdShipRot(p, transform.right, Time.deltaTime * thruster[i].count * 4f / mass, GetComponent<NetworkIdentity>());
                            }
                        }
                        else if (thruster[i].axis == 3)
                        {
                            if (thruster[i].type == 0)
                            {
                                networkPlayer.GetComponent<NetworkPlayer>().CmdShipMove(-transform.right * Time.deltaTime * thruster[i].count * 8f / mass, GetComponent<NetworkIdentity>().netId);

                            }
                            else
                            {
                                networkPlayer.GetComponent<NetworkPlayer>().CmdShipRot(p, -transform.right, Time.deltaTime * thruster[i].count * 5f / mass, GetComponent<NetworkIdentity>());

                            }
                        }
                        else if (thruster[i].axis == 4)
                        {
                            if (thruster[i].type == 0)
                            {
                                networkPlayer.GetComponent<NetworkPlayer>().CmdShipMove(transform.forward * Time.deltaTime * thruster[i].count * 8f / mass, GetComponent<NetworkIdentity>().netId);
                            }
                            else
                            {
                                networkPlayer.GetComponent<NetworkPlayer>().CmdShipRot(p, transform.forward, Time.deltaTime * thruster[i].count * 5f / mass, GetComponent<NetworkIdentity>());
                            }
                        }
                        else if (thruster[i].axis == 5)
                        {
                            if (thruster[i].type == 0)
                            {
                                networkPlayer.GetComponent<NetworkPlayer>().CmdShipMove(-transform.forward * Time.deltaTime * thruster[i].count * 8f / mass, GetComponent<NetworkIdentity>().netId);
                            }
                            else
                            {
                                networkPlayer.GetComponent<NetworkPlayer>().CmdShipRot(p, -transform.forward, Time.deltaTime * thruster[i].count * 5f / mass, GetComponent<NetworkIdentity>());
                            }
                        }
                    }
                }
            }
        }
    }
    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(GetComponent<Renderer>().bounds.center, mass*2);
    }
}

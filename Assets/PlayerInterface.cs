using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInterface : MonoBehaviour
{
    public GameObject inventory, hotbar;
    public List<GameObject> windows;
    public Player player;
    bool firstInv, firstInEnd = false;

    public int oldMeteorsLength, oldPlayersLength;
    public List<GameObject> meteorsTargets;
    public GameObject target, target2, isServer;
    public GameObject hand;
    public Transform targets;
    public WorldStaticData staticData;
    [Space]
    public Image infoImage;
    public TMP_Text naming, descriptions;
    public RectTransform boost;
    public GameObject help;
    public Image mode;
    public Sprite m1, m2;
    public GameObject degenShip;
    private void Start()
    {
        staticData = FindObjectOfType<WorldStaticData>();
        inventory.SetActive(false);
        isServer.gameObject.SetActive(GetComponentInParent<NetworkPlayer>().isServer);
    }
    public bool IsSee(GameObject Object)
    {

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        if (GeometryUtility.TestPlanesAABB(planes, Object.GetComponent<Collider>().bounds))
            return true;
        else
            return false;
    }
    
    private void FixedUpdate()
    {
        mode.sprite = player.mode ? m2 : m1;
        boost.localScale = new Vector3(0.9731f * (player.boost/5), 1, 1);
        var meteors = GameObject.FindGameObjectsWithTag("CenterObject").ToList();

        for (int i = 0; i < meteorsTargets.Count; i++)
        {
            Destroy(meteorsTargets[i].gameObject);
        }
        meteorsTargets = new List<GameObject>();
        for (int i = 0; i < meteors.Count; i++)
        {
            var gm = Instantiate(target, targets);
            gm.transform.position = Camera.main.WorldToScreenPoint(meteors[i].transform.position);
            meteorsTargets.Add(gm);

            gm.SetActive(IsSee(meteors[i]));
        }
        for (int i = 0; i < staticData.players.Count; i++)
        {
            if(staticData.players[i].@object == null)
            {
                continue;
            }
            if (staticData.players[i].@object.GetComponent<NetworkIdentity>().isLocalPlayer) continue;
            var gm = Instantiate(target2, targets);
            gm.GetComponentInChildren<TMP_Text>().text = staticData.players[i].name;
            gm.transform.position = Camera.main.WorldToScreenPoint(staticData.players[i].@object.transform.position);
            meteorsTargets.Add(gm);
            gm.SetActive(IsSee(staticData.players[i].@object));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            help.SetActive(!help.active);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].active)
                {
                    windows[i].active = false;
                    return;
                }
            }
            help.active = true;
        }
        if (firstInEnd)
        {
            FindObjectOfType<Inventory>().RedrawInventory();
            firstInEnd = false;
        }
        hotbar.active = !inventory.active;
        player.enabled = windows.FindAll(x => x.active).Count == 0;
        hand.SetActive(player.enabled);
if (Input.GetKeyDown(KeyCode.F2))
	gameObject.SetActive(false);
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            inventory.SetActive(!inventory.active);
            FindObjectOfType<Inventory>().RedrawInventory();
            if (!firstInv)
            {
                firstInEnd = true;
                firstInv = true;
            }
        }
        if (windows.FindAll(x => x.active).Count != 0)
        {
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true; GetComponentInParent<Rigidbody>().velocity = Vector3.zero;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        }
    }
}

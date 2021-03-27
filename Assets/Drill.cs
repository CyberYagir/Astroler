using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drill : MonoBehaviour
{
    public Camera camera;
    public BlocksAddRemove blocks;
    public float digTime, maxTime;
    public Vector3 oldRetDel;
    public Transform deleteBlock;
    public Sprite[] deleteStudyesSprites;
    public Texture2D[] deleteStudyes;
    public GameObject destroyPart;
    private void Start()
    {
        deleteStudyes = new Texture2D[deleteStudyesSprites.Length];
        var p = FindObjectOfType<Inventory>();
        for (int i = 0; i < deleteStudyes.Length; i++)
        {
            deleteStudyes[i] = p.Convert(deleteStudyesSprites[i]);
        } 
        camera = GetComponentInParent<Camera>();
        blocks = GetComponentInParent<BlocksAddRemove>();
    }
    private void OnDestroy()
    {
        Destroy(deleteBlock.gameObject);
    }
    private void Update()
    {
        if (blocks.can)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                deleteBlock.parent = null;
                deleteBlock.localEulerAngles = Vector3.zero;
                deleteBlock.localScale = Vector3.one * 1.1f;
            }
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (deleteBlock.parent != null)
                {
                    deleteBlock.parent = null;
                    deleteBlock.localEulerAngles = Vector3.zero;
                    deleteBlock.localScale = Vector3.one * 1.1f;
                }
                deleteBlock.gameObject.SetActive(true);
                digTime += Time.deltaTime;
                if (oldRetDel != blocks.retDel)
                {
                    digTime = 0;
                    oldRetDel = blocks.retDel;
                }
                if (digTime >= maxTime)
                {
                    byte block = blocks.Control(oldRetDel, 0);
                    if (block != 0)
                    {
                        var part = Instantiate(destroyPart, oldRetDel + new Vector3(0.5f, .5f, -.5f), Quaternion.identity);
                        Destroy(part.gameObject, 2f);
                        var it = FindObjectOfType<WorldStaticData>().items.Find(x => x.blockID == block);
                        part.GetComponent<ParticleSystemRenderer>().material.SetTexture("_MainTex", it.texture);
                        if (!FindObjectOfType<Inventory>().AddItem(it.CreateInventoryItem(1)))
                        {
                            FindObjectOfType<Inventory>().DropItem(it.CreateInventoryItem(1), deleteBlock.transform.position);
                        }
                    }
                    digTime = 0;
                    return;
                }
                if (digTime < maxTime / 3f)
                {
                    deleteBlock.GetComponent<Renderer>().material.SetTexture("_MainTex", deleteStudyes[0]);
                }else
                if (digTime < maxTime / 2)
                {
                    deleteBlock.GetComponent<Renderer>().material.SetTexture("_MainTex", deleteStudyes[1]);
                }
                else if (digTime < maxTime / 1.5f)
                {
                    deleteBlock.GetComponent<Renderer>().material.SetTexture("_MainTex", deleteStudyes[2]);
                }
                else
                {
                    deleteBlock.GetComponent<Renderer>().material.SetTexture("_MainTex", deleteStudyes[3]);
                }
                deleteBlock.transform.position = oldRetDel + new Vector3(0.5f, 0.5f, -0.5f);
            }
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                deleteBlock.gameObject.SetActive(false);
                digTime = 0;
            }
        }
        else
        {
            deleteBlock.gameObject.SetActive(false);
            digTime = 0;
        }
    }
}

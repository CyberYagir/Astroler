using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public float sens;
    [Space]
    public Rigidbody Rigidbody;
    public GameObject Camera;
    public Camera _Camera;
    public bool freezePos;
    public InputManager inp;
    public float boost = 5;

    public float rotationY = 0f;


    public bool mode = false;
    void Start()
    {
        inp = FindObjectOfType<InputManager>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void FixedUpdate()
    {
        
        var oldspeed = speed;
        GameObject activeCam = mode ? _Camera.gameObject : Camera;
        if (!freezePos)
        {
            if (inp.IsPress("Boost"))
            {
                if (boost > 1)
                {
                    speed = oldspeed * 4;
                    boost -= Time.fixedDeltaTime;
                }
            }

            Rigidbody.AddForce(activeCam.transform.up * speed * inp.GetAxis("UD"));
            Rigidbody.AddForce(activeCam.transform.forward * speed * inp.GetAxis("FB"));
            Rigidbody.AddForce(activeCam.transform.right * speed * inp.GetAxis("LR"));
        }

        transform.Rotate(Vector3.forward * speed / 10 * inp.GetAxis("RT"));


        if (Input.GetKey(KeyCode.Mouse1))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        speed = oldspeed;
    }

    private void LateUpdate()
    {
        var centers = GameObject.FindGameObjectsWithTag("Center").ToList();
        for (int i = 0; i < centers.Count; i++)
        {
            var c = centers[i].GetComponent<Chunk>();
            if (Vector3.Distance(centers[i].transform.position, transform.position) < 200)
            {
                c.active = true;
            }
            else
            {
                c.active = false;
            }

            if (c.active != c.oldactive)
            {
                for (int j = 0; j < c.chunksList.Count; j++)
                {
                    c.chunksList[j].gameObject.SetActive(c.active);
                }
            }
            c.oldactive = c.active;
        }
    }
    Quaternion newrotOnGround = new Quaternion();
    bool canrot = false;
    void Update()
    {
        if (!inp.IsPress("Boost"))
        {
            if (boost < 5)
            {
                boost += Time.deltaTime / 2;
            }
        }
        if (inp.IsDown("Mode"))
        {
            if (mode)
            {
                _Camera.transform.localEulerAngles = Vector3.zero;
            }
            else
            {
                _Camera.transform.localEulerAngles = Vector3.zero; rotationY = 0;
            }
            mode = !mode;
        }
        if (inp.IsDown("FIX"))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, !mode ? -transform.up : -_Camera.transform.up, out hit, 5f))
            {
                canrot = true;
                if (hit.collider != null)
                {
                    newrotOnGround = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                }
            }
            else
            {
                canrot = false;
            }
        }
        if (inp.IsPress("FIX"))
        {
            if (canrot)
            {
                if (mode)
                {
                    _Camera.transform.localEulerAngles = Vector3.Lerp(_Camera.transform.localEulerAngles, Vector3.zero, 10f * Time.deltaTime);
                }
                transform.rotation = Quaternion.Lerp(transform.rotation, newrotOnGround, 0.5f);
            }
        }
        if (mode)
        {
            float yrot = Input.GetAxisRaw("Mouse X");
            Vector3 rot = new Vector3(0, yrot, 0f) * sens;
            transform.rotation = (transform.rotation * Quaternion.Euler(rot));
            rotationY += Input.GetAxis("Mouse Y") * sens;
            rotationY = Mathf.Clamp(rotationY, -90, 90);

            _Camera.transform.localEulerAngles = new Vector3(-rotationY, 0, 0);
        }
        else
        {

            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * sens);
            transform.Rotate(Vector3.left * Input.GetAxis("Mouse Y") * sens);
        }
    }
}

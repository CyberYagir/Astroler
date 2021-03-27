using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeeRigMenu : MonoBehaviour
{
    public Renderer renderer;
    public float speed;
    void Update()
    {
        renderer.material.SetTexture("_MainTex", FindObjectOfType<LoaderResources>().skin);

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            transform.LookAt(hit.point);
        }

        //transform.Rotate(new Vector3(-Input.GetAxisRaw("Mouse Y"), -Input.GetAxisRaw("Mouse X")) * Time.deltaTime * 40);
    }
    private void LateUpdate()
    {

        renderer.transform.rotation = Quaternion.Lerp(renderer.transform.rotation, transform.rotation, speed);
    }
}

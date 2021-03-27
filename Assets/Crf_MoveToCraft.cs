using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class Crf_MoveToCraft : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IMoveHandler
{
    bool drag;
    public Crafter crafter;
    public UILineRenderer lineRenderer;
    public int itemid;
    public int value;
    public Vector2Int pos;
    private void Start()
    {
        crafter = GetComponentInParent<Crafter>();
        lineRenderer.transform.parent = crafter.linesHolder;
        lineRenderer.transform.localPosition = Vector3.zero;
    }
    public void ReInit()
    {
        var inv = FindObjectOfType<Inventory>();
        if (inv.items[pos.y, pos.x] == null)
        {
            Destroy(gameObject);
            return;
        }
        if (inv.items[pos.y, pos.x].value == 0)
        {
            Destroy(gameObject);
        }
        if (inv.items[pos.y, pos.x].itemid != itemid)
        {
            Destroy(gameObject);
            return;
        }
        value = inv.items[pos.y, pos.x].value;
        GetComponentInChildren<TMP_Text>().text = inv.items[pos.y, pos.x].value.ToString();

    }
    private void OnDestroy()
    {
        Destroy(lineRenderer.gameObject);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
        GetComponentInChildren<Image>().raycastTarget = false;
        drag = true;
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        if (crafter.GetComponentInChildren<Crf_MainPlace>().over)
        {
            transform.parent = crafter.GetComponentInChildren<Crf_MainPlace>().transform;
        }
        else
        {
            transform.parent = crafter.transform.parent;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponentInChildren<Image>().raycastTarget = true;
        transform.localScale = Vector3.one * 0.8f;
        if (transform.parent != crafter.GetComponentInChildren<Crf_MainPlace>().transform)
        {
            Destroy(gameObject);
        }
    }

    public void OnMove(AxisEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (drag == false)
        {
            Destroy(gameObject);
        }
        drag = false;
    }
}

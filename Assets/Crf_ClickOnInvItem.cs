using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class Crf_ClickOnInvItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IMoveHandler, IDragHandler
{
    public GameObject item;
    public int itemid;
    public int value;
    public Vector2Int invPos;
    public void OnBeginDrag(PointerEventData eventData)
    {

        if (item != null)
        {
            Destroy(item.gameObject);
        }
        item = Instantiate(GetComponentInParent<Crafter>().itemOnTable.gameObject, Input.mousePosition, Quaternion.identity, GetComponentInParent<Crafter>().transform);
        item.SetActive(true);
        item.transform.GetChild(1).GetComponent<Image>().sprite = FindObjectOfType<WorldStaticData>().items[itemid].sprite;
        item.transform.GetChild(2).GetComponent<TMP_Text>().text = transform.GetChild(3).GetComponent<TMP_Text>().text;
        item.GetComponent<Crf_MoveToCraft>().itemid = itemid;
        item.GetComponent<Crf_MoveToCraft>().value = value;
        item.GetComponent<Crf_MoveToCraft>().pos = invPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item)
        {
            var p = GetComponentInParent<Crafter>().GetComponentInChildren<Crf_MainPlace>();
            item.transform.position = Input.mousePosition;
            if (p.over)
            {
                item.transform.parent = p.transform;
            }
            else
            {
                item.transform.parent = GetComponentInParent<Crafter>().transform;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (item)
        {
            var p = GetComponentInParent<Crafter>().GetComponentInChildren<Crf_MainPlace>();
            if (item.transform.parent != p.transform)
            {
                Destroy(item.gameObject);
            }
            else
            {
                item.transform.localScale = Vector3.one * 0.8f;
                item.GetComponentInChildren<Image>().raycastTarget = true;
            }
        }
    }

    public void OnMove(AxisEventData eventData)
    {
    }
}

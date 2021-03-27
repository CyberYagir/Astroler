using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MouseItem : MonoBehaviour
{
    public Vector2Int oldPos;
    public Image spriteRenderer;
    public int value;
    public int id;
    public TMP_Text text;
    public bool getfullstack;
    public void SetSprite()
    {
        spriteRenderer.sprite = FindObjectOfType<WorldStaticData>().items[FindObjectOfType<Inventory>().items[oldPos.y, oldPos.x].itemid].sprite;
        text.text = value.ToString();
    }

    private void Update()
    {
        transform.position =Vector3.Lerp(transform.position, Input.mousePosition, 8f * Time.deltaTime);
    }
}

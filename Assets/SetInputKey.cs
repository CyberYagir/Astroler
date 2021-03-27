using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetInputKey : MonoBehaviour
{
    public string inpName;
    public bool plus;
    public TMP_Dropdown drop;

    private void Start()
    {
        drop = GetComponent<TMP_Dropdown>();
        var optios = new List<TMP_Dropdown.OptionData>();
        string[] PieceTypeNames = System.Enum.GetNames(typeof(KeyCode));
        for (int i = 0; i < PieceTypeNames.Length; i++)
        {
            if (PieceTypeNames[i].Length <= 3 || PieceTypeNames[i].Contains("Shift") || PieceTypeNames[i].Contains("Control") || PieceTypeNames[i].Contains("Alt") || PieceTypeNames[i].Contains("Space"))
                optios.Add(new TMP_Dropdown.OptionData() { text = PieceTypeNames[i] });
        }
        drop.options = optios;
        if (plus)
            drop.value = drop.options.FindIndex(x => x.text == FindObjectOfType<InputManager>().GetAxisFull(inpName).plus.ToString());
        else
            drop.value = drop.options.FindIndex(x => x.text == FindObjectOfType<InputManager>().GetAxisFull(inpName).minus.ToString());
    }

    public void SetKey()
    {
        if (InputManager.k == null) return;
        KeyCode find = (KeyCode)System.Enum.Parse(typeof(KeyCode), drop.options[drop.value].text);
        var n = InputManager.k.inputs.Find(x => x.name == inpName);
        if (n == null) return;
        if (plus)
            n.plus = find;
        else
            n.minus = find;

        
    }
}

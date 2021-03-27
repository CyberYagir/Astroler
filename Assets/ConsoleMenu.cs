using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConsoleMenu : MonoBehaviour
{
    public List<ThusterOption> keysList;
    public TMP_Text buttonText;
    [System.Serializable]
    public class ThusterOption {
        public TMP_InputField key;
        public TMP_Dropdown axis;
        public TMP_Dropdown type;
    }

}

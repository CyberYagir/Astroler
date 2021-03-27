using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    public static InputManager k ;


    [System.Serializable]
    public class InputKey {
        public string name;
        public KeyCode plus;
        public KeyCode minus;
        [XmlIgnore]
        public bool down, press, up;
        [XmlIgnore]
        public float value;
    }
    public List<InputKey> inputs = new List<InputKey>();
    private void Start()
    {
        k = this;
    }

    private void Update()
    {
        for (int i = 0; i < inputs.Count; i++)
        {
            inputs[i].value = 0;

            inputs[i].press = Input.GetKey(inputs[i].plus);
            inputs[i].up = Input.GetKeyUp(inputs[i].plus);
            inputs[i].down = Input.GetKeyDown(inputs[i].plus);


            if (inputs[i].press)
            {
                inputs[i].value += 1;
            }
            if (Input.GetKey(inputs[i].minus))
            {
                inputs[i].value -= 1;
            }
        }
    }

    public bool IsDown(string _name) {

        return inputs.Find(x => x.name.ToLower().Trim() == _name.ToLower().Trim()).down;
    }

    public float GetAxis(string _name)
    {
        return inputs.Find(x => x.name.ToLower().Trim() == _name.ToLower().Trim()).value;
    }
    public bool IsPress(string _name)
    {
        return inputs.Find(x => x.name.ToLower().Trim() == _name.ToLower().Trim()).value != 0;
    }
    public InputKey GetAxisFull(string _name)
    {
        return inputs.Find(x => x.name.ToLower().Trim() == _name.ToLower().Trim());
    }

}

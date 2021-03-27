using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Load : MonoBehaviour
{
    public RectTransform loadBar;
    public RectTransform text;
    public TMP_Text vers;
    public RectTransform menu;
    public TMP_Text changelog;

    public TMP_Text aboutText;
    public SeeRigMenu rigMenu;
    bool init;
    // Start is called before the first frame update
    void Start()
    {
        vers.text = Application.version;
        loadBar.sizeDelta = new Vector2(Screen.width, 20);
        loadBar.localScale = new Vector2(0, 1);
        menu.position = new Vector3(Screen.width + 200, menu.position.y);
        aboutText.text = "Code: Yagir\nTest: NilDab\n\n<size=70%>Verison: " + Application.version + "\nUnity verison: " + Application.unityVersion+ "\n<color=#aaaaaa>Multiplayer by <color=#ffff> Mirror [Unet]";
    }

    // Update is called once per frame
    void Update()
    {
        loadBar.localScale += new Vector3(1, 0, 0) * Time.deltaTime;
        if (loadBar.localScale.x >= 1)
        {
            if (!init)
            {
                if (FindObjectOfType<LoaderResources>().skin != null && FindObjectOfType<LoaderResources>().tiles != null)
                {
                    loadBar.gameObject.SetActive(false);
                }
                text.gameObject.SetActive(false);
                var f = FindObjectOfType<LoaderResources>();
                changelog.text = "";
                for (int i = 0; i < f.changelog.Length; i++)
                {
                    changelog.text += (f.changelog[i].Contains(" add") ? "<color=#34e8eb>" : (f.changelog[i].Contains(" fix") ? "<color=#1aed2c>" : (f.changelog[i].Contains(":") ? "<color=#ffff><size=30><u>" : "<color=#ffff>"))) +  f.changelog[i] + "\n</u><size=24>";
                }
                init = true;
            }
            rigMenu.enabled = true;
            menu.transform.localPosition = Vector3.Lerp(menu.transform.localPosition, new Vector3(-Screen.width/2, 0), 10 * Time.deltaTime);
        }
    }
}

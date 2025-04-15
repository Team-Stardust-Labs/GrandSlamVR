using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomDebugLog : MonoBehaviour
{

    public static CustomDebugLog Singleton;

    public GameObject textObject;
    public GameObject textObjectNetworking;

    void Awake() {
        Singleton = this;
    }

    private string text = "";
    private string ntext = "";


    public void Log(string txt) {
        text += txt + "\n";
        Debug.Log(txt);
        textObject.GetComponent<UnityEngine.UI.Text>().text = getText(text);
    }


    public void LogNetworkManager(string txt) {
        ntext += txt + "\n";
        Debug.Log(txt);
        textObjectNetworking.GetComponent<UnityEngine.UI.Text>().text = getText(ntext);
    }
    

    string getText(string txt = "") {
        return txt.Substring(Mathf.Max(txt.Length - 500, 0), Mathf.Min(500, txt.Length));
    }
}

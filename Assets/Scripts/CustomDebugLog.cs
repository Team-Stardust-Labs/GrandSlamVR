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


    public void Log(string txt) {
        text += txt + "\n";
        Debug.Log(txt);
        textObject.GetComponent<UnityEngine.UI.Text>().text = getText();
    }


    public void LogNetworkManager(string txt) {
        text += txt + "\n";
        Debug.Log(txt);
        textObjectNetworking.GetComponent<UnityEngine.UI.Text>().text = getText();
    }
    

    string getText() {
        return text.Substring(Mathf.Max(text.Length - 500, 0), Mathf.Min(500, text.Length));
    }
}

using TMPro;
using UnityEngine;

public class DebugDisplayObjectName : MonoBehaviour
{
    public string PrefixText;
    public TMP_Text TextArea;

    public void DisplayObjectName(GameObject obj)
    {
        if (!TextArea) { return; }
        TextArea.text = $"{PrefixText}{(obj ? obj.name : "null")}";
    }
}

using System.Text;
using UnityEngine;
using UnityEngine.Events;
using VARLab.SCORM;

public class ScormMessageHandler : MonoBehaviour
{
    public UnityEvent<string> OnMessageReady;

    // Start is called before the first frame update
    public void HandleScormMessage(ScormManager.Event value)
    {
        StringBuilder buffer = new($"SCORM Message: {value}");

        if (ScormManager.Instance != null)
        {
            buffer.Append($", ID: {ScormManager.GetLearnerId()}");
        }

        OnMessageReady?.Invoke(buffer.ToString());
    }
}

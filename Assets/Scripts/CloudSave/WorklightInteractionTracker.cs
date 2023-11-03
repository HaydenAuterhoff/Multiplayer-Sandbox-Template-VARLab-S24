using Newtonsoft.Json;
using UnityEngine;
using VARLab.CloudSave;

[CloudSaved]
[JsonObject(MemberSerialization.OptIn)]
public class WorklightInteractionTracker : InteractionTracker
{
    public Light Worklight;

    public Renderer Bulb;

    private Color defaultEmissionColor;
    private Color disabledEmissionColor = Color.black;

    [JsonProperty]
    protected bool _on = false;

    public void OnValidate()
    {
        Worklight = GetComponentInChildren<Light>();
    }

    public void Awake()
    {
        if (Bulb)
        {
            defaultEmissionColor = Bulb.material.GetColor("_EmissionColor");
        }
    }

    public override void HandleClickInteraction()
    {
        base.HandleClickInteraction();

        if (Worklight)
        {
            Worklight.enabled = !Worklight.enabled;
        }

        UpdateBulb();
    }

    public override void OnSerialize()
    {
        if (Worklight)
        {
            _on = Worklight.enabled;
        }
    }

    public override void OnDeserialize()
    {
        Debug.Log($"Loaded state of Worklight '{name}' with {interactionCount} interactions. " +
            $"Light is {(_on ? "on" : "off")}");

        if (Worklight)
        {
            Worklight.enabled = _on;
        }

        UpdateBulb();
    }

    private void UpdateBulb()
    {
        if (Bulb)
        {
            Bulb.material.SetColor("_EmissionColor", Worklight.enabled ? defaultEmissionColor : disabledEmissionColor);
        }
    }
}

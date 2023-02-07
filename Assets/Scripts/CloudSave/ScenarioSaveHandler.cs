using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VARLab.CloudSave;

public class ScenarioSaveHandler : MonoBehaviour
{
    private ICloudSerializer cloudSerializer;

    [Header("Configuration")]
    [SerializeField] private AzureSaveSystem saveSystem;

    [SerializeField] private string saveFilePath = "azureplayground/tools.txt";

    [Header("Startup")]
    
    [Tooltip("The application should load data on startup")]
    [SerializeField] protected bool loadOnStartup = false;

    [Header("Events")]
    public UnityEvent OnLoadStart;
    public UnityEvent OnLoadSuccess;
    public UnityEvent OnLoadFailed;

    public UnityEvent OnSaveStart;
    public UnityEvent OnSaveSuccess;
    public UnityEvent OnSaveFailed;


    // Properties

    public ICloudSerializer Serializer
    {
        get { 
            cloudSerializer ??= new SerializerJson();
            return cloudSerializer;
        }
    }

    // Methods

    public void Start()
    {
        saveSystem.OnRequestComplete += HandleRequestComplete;
        
        if (loadOnStartup) { Load(); }
    }

    private void HandleRequestComplete(AzureSaveSystem.ActionType action, bool success, string saveData = null)
    {
        Debug.Log($"{action} Action {(success ? "Success" : "Failed")}");

        switch (action)
        {
            case AzureSaveSystem.ActionType.Load:
                if (success) { OnLoadSuccess?.Invoke(); }
                else { OnLoadFailed?.Invoke(); }
                break;
            case AzureSaveSystem.ActionType.Save:
                if (success) { OnSaveSuccess?.Invoke(); }
                else { OnSaveFailed?.Invoke(); }
                break;
            case AzureSaveSystem.ActionType.Delete:
                break;
        }
    }

    public void Save()
    {
        OnSaveStart?.Invoke();
        StartCoroutine(InternalSave());
    }

    IEnumerator InternalSave()
    {
        var data = Serializer.Serialize();
        var save = saveSystem.Save(saveFilePath, data);
        yield return save;
    }

    public void Load()
    {
        OnLoadStart?.Invoke();
        StartCoroutine(InternalLoad());
    }

    IEnumerator InternalLoad()
    {
        var load = saveSystem.Load(saveFilePath);
        yield return load.Routine;

        var data = load.Result as string;
        Serializer.Deserialize(data);
    }


}

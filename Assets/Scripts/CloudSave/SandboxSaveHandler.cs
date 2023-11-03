using UnityEngine;
using UnityEngine.Events;
using VARLab.CloudSave;

/// <summary>
///     This save handler wraps the Complete events exposed by the 
///     <see cref="ExperienceSaveHandler"/> and extends them to
///     explicit actions for Success and Failure.
/// </summary>
public class SandboxSaveHandler : ExperienceSaveHandler
{

    [Header("Events")]
    public UnityEvent OnLoadSuccess;
    public UnityEvent OnLoadFailed;

    public UnityEvent OnSaveSuccess;
    public UnityEvent OnSaveFailed;

    public UnityEvent OnDeleteSuccess;
    public UnityEvent OnDeleteFailed;

    public string DataCache { get; private set; }


    [Header("Startup")]
    [Tooltip("The application should load data on startup")]
    [SerializeField] protected bool loadOnStartup = false;


    // Properties

    public ICloudSerializer Serializer
    {
        get { 
            m_Serializer ??= new JsonCloudSerializer();
            return m_Serializer;
        }
    }

    // Methods

    public void Start()
    {       
        if (loadOnStartup) { Load(); }

        OnSaveComplete.AddListener((response) =>
        {
            if (response) { OnSaveSuccess?.Invoke(); }
            else { OnSaveFailed?.Invoke(); }
        });

        OnLoadComplete.AddListener((response) =>
        {
            if (response) { OnLoadSuccess?.Invoke(); }
            else { OnLoadFailed?.Invoke(); }
        });

        OnDeleteComplete.AddListener((response) =>
        {
            if (response) { OnDeleteSuccess?.Invoke(); }
            else { OnDeleteFailed?.Invoke(); }
        });
    }

    public virtual string UpdateCache()
    {
        DataCache = m_Serializer.Serialize();
        return DataCache;
    }


}

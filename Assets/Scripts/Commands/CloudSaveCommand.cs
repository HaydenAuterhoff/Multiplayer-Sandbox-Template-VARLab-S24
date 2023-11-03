using Newtonsoft.Json.Linq;
using UnityEngine;
using VARLab.DeveloperTools;

/// <summary>
///     Uses <see cref="VARLab.DeveloperTools.CommandInterpreter"/>
/// </summary>
public class CloudSaveCommand : MonoBehaviour
{

    private const string Name = "cloudsave";
    private const string Description = "Access Azure cloud save from the terminal.";
    private readonly string Usage = $"{Name} " +
        $"[ {KeywordList} | {KeywordSave} | {KeywordLoad} | {KeywordDelete} ]";

    private const string KeywordSave = "save";
    private const string KeywordLoad = "load";
    private const string KeywordDelete = "delete";
    private const string KeywordList = "list";

    private ICommand _cloudSaveCommand;
    private ICommandInterpreter _commandInterpreter;

    public SandboxSaveHandler SaveHandler;

    public void OnValidate()
    {
        SaveHandler = FindObjectOfType<SandboxSaveHandler>();
    }

    public void Start()
    {
        _commandInterpreter = CommandInterpreter.Instance;

        if (!SaveHandler)
        {
            Debug.LogWarning("Can't find a SaveHandler");
            return;
        }

        _cloudSaveCommand = new Command(Name, Usage, Description, CloudSaveCallback);

        _commandInterpreter.Add(_cloudSaveCommand);
    }

    public void CloudSaveCallback(CommandEventArgs e)
    {
        if (e.Args.Length != 2)
        {
            e.Response = _cloudSaveCommand.ErrorResponse();
            return;
        }

        switch (e.Args[1])
        {
            case KeywordSave:
                SaveCommand(e);
                break;
            case KeywordLoad:
                LoadCommand(e);
                break;
            case KeywordDelete:
                DeleteCommand(e);
                break;
            case KeywordList:
                ListCommand(e);
                break;
            default:
                e.Response = _cloudSaveCommand.ErrorResponse();
                break;
        }
    }

    public void SaveCommand(CommandEventArgs e)
    {
        e.Response = GetJsonSaveContents();
        Debug.Log(e.Response);
        SaveHandler.Save();
    }

    public void LoadCommand(CommandEventArgs e)
    {
        SaveHandler.Load();
        e.Response = "Load request sent";
    }

    public void DeleteCommand(CommandEventArgs e)
    {
        SaveHandler.Delete();
        e.Response = "Delete request sent";
    }

    public void ListCommand(CommandEventArgs e)
    {
        e.Response = GetJsonSaveContents();
        Debug.Log(e.Response);
    }

    private string GetJsonSaveContents()
    {
        return JToken.Parse(SaveHandler.UpdateCache()).ToString(Newtonsoft.Json.Formatting.Indented);
    }
}

using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using VARLab.Interactions;

public class DebugDisplayObjectNameIntegrationTests
{
    const string INTERACTABLE_NAME = "Interactable Object";
    const string CLICKED_PREFIX = "Clicked: ";
    const string HOVERED_PREFIX = "Hovered: ";

    GameObject _interactableObject;
    GameObject _clickDisplayObject;
    GameObject _hoverDisplayObject;
    GameObject _interactionHandlerObject;

    Interactable _interactable;
    InteractionTracker _interactionTracker;
    TMP_Text _clickedTextArea;
    TMP_Text _hoveredTextArea;
    DebugDisplayObjectName _clickedDebugDisplayObjectName;
    DebugDisplayObjectName _hoveredDebugDisplayObjectName;
    InteractionHandler _interactionHandler;

    [SetUp]
    public void SetUp()
    {
        // Setting up the Interactable GameObject.
        _interactableObject = new GameObject();
        _interactableObject.SetActive(false);
        _interactableObject.name = INTERACTABLE_NAME;

        _interactableObject.AddComponent<MeshRenderer>();
        _interactableObject.AddComponent<BoxCollider>().isTrigger = true;
        _interactable = _interactableObject.AddComponent<Interactable>();
        _interactionTracker = _interactableObject.AddComponent<InteractionTracker>();

        // All the mouse events need to be set up, or else InteractionHandler will throw a null ref error when we tear down the tests.
        _interactable.MouseEnter = new UnityEvent<GameObject>();
        _interactable.MouseExit = new UnityEvent<GameObject>();
        _interactable.MouseDown = new UnityEvent<GameObject>();
        _interactable.MouseUp = new UnityEvent<GameObject>();
        _interactable.MouseClick = new UnityEvent<GameObject>();
        
        _interactable.MouseClick.AddListener((x) => _interactionTracker.HandleClickInteraction());

        // Setting up the Clicked Display GameObject.
        _clickDisplayObject = new GameObject();
        _clickDisplayObject.SetActive(false);

        _clickedTextArea = _clickDisplayObject.AddComponent<TextMeshProUGUI>();
        _clickedDebugDisplayObjectName = _clickDisplayObject.AddComponent<DebugDisplayObjectName>();
        _clickedDebugDisplayObjectName.PrefixText = CLICKED_PREFIX;
        _clickedDebugDisplayObjectName.TextArea = _clickedTextArea;

        // Setting up the Hovered Display GameObject.
        _hoverDisplayObject = new GameObject();
        _hoverDisplayObject.SetActive(false);

        _hoveredTextArea = _hoverDisplayObject.AddComponent<TextMeshProUGUI>();
        _hoveredDebugDisplayObjectName = _hoverDisplayObject.AddComponent<DebugDisplayObjectName>();
        _hoveredDebugDisplayObjectName.PrefixText = HOVERED_PREFIX;
        _hoveredDebugDisplayObjectName.TextArea = _hoveredTextArea;

        // Setting up the Interaction Handler.
        _interactionHandlerObject = new GameObject();
        _interactionHandlerObject.SetActive(false);

        _interactionHandler = _interactionHandlerObject.AddComponent<InteractionHandler>();

        _interactionHandler.MouseClick = new UnityEvent<GameObject>();
        _interactionHandler.MouseClick.AddListener((x) => _clickedDebugDisplayObjectName.DisplayObjectName(x));

        _interactionHandler.MouseEnter = new UnityEvent<GameObject>();
        _interactionHandler.MouseEnter.AddListener((x) => _hoveredDebugDisplayObjectName.DisplayObjectName(x));

        // Setting all back to active.
        _interactableObject.SetActive(true);
        _clickDisplayObject.SetActive(true);
        _hoverDisplayObject.SetActive(true);
        _interactionHandlerObject.SetActive(true);
    }

    [TearDown]
    public void TearDown() 
    {
        foreach (GameObject gameObject in Object.FindObjectsOfType<GameObject>())
        {
            if (gameObject != null)
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }

    [UnityTest]
    public IEnumerator Clicked_Debug_Displays_Correctly()
    {
        string expectedDisplay = $"{_clickedDebugDisplayObjectName.PrefixText}{_interactableObject.name} ({_interactionTracker.InteractionCount + 1})";
        _interactable.MouseClick.Invoke(_interactableObject);

        yield return new WaitForSeconds(2);

        Assert.AreEqual(expectedDisplay, _clickedTextArea.text);
    }

    [UnityTest]
    public IEnumerator Hovered_Debug_Displays_Correctly()
    {
        string expectedDisplay = $"{_hoveredDebugDisplayObjectName.PrefixText}{_interactableObject.name} (0)";
        _interactable.MouseEnter.Invoke(_interactableObject);

        yield return new WaitForSeconds(2);

        Assert.AreEqual(expectedDisplay, _hoveredTextArea.text);
    }
}

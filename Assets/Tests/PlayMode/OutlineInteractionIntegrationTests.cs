using System.Collections;
using EPOOutline;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using VARLab.Interactions;
using VARLab.Sandbox.Interactions;

public class OutlineInteractionIntegrationTests
{
    const string INTERACTABLE_NAME = "Interactable Object";
    const int VISIBLE_LAYER = 1;
    const int HIDDEN_LAYER = 0;

    GameObject _interactionHandlerObject;
    GameObject _effectsObject;
    GameObject _interactableObject;

    InteractionHandler _interactionHandler;
    OutlineInteraction _outlineInteraction;
    Interactable _interactable;
    Outlinable _outlinable;

    [SetUp]
    public void SetUp()
    {
        // Setting up the interaction handler game object.
        _interactionHandlerObject = new GameObject();
        _interactionHandlerObject.SetActive(false);
        _interactionHandler = _interactionHandlerObject.AddComponent<InteractionHandler>();

        // Setting up the effects game object.
        _effectsObject = new GameObject();
        _effectsObject.transform.parent = _interactionHandlerObject.transform;
        _effectsObject.SetActive(false);
        _outlineInteraction = _effectsObject.AddComponent<OutlineInteraction>();

        // Setting up the interactable game object.
        _interactableObject = new GameObject();
        _interactableObject.SetActive(false);
        _interactableObject.name = INTERACTABLE_NAME;

        _interactableObject.AddComponent<MeshRenderer>();
        _interactableObject.AddComponent<BoxCollider>().isTrigger = true;
        _outlinable = _interactableObject.AddComponent<Outlinable>();
        _interactable = _interactableObject.AddComponent<Interactable>();

        // All the mouse events need to be set up, or else InteractionHandler will throw a null ref error when we tear down the tests.
        _interactable.MouseEnter = new UnityEvent<GameObject>();
        _interactable.MouseExit = new UnityEvent<GameObject>();
        _interactable.MouseDown = new UnityEvent<GameObject>();
        _interactable.MouseUp = new UnityEvent<GameObject>();
        _interactable.MouseClick = new UnityEvent<GameObject>();

        // Setting up interaction handler events.
        _interactionHandler.MouseEnter = new UnityEvent<GameObject>();
        _interactionHandler.MouseEnter.AddListener((x) => _outlineInteraction.ShowOutline(x));

        _interactionHandler.MouseExit = new UnityEvent<GameObject>();
        _interactionHandler.MouseExit.AddListener((x) => _outlineInteraction.HideOutline(x));

        // Setting objects to active again.
        _effectsObject.SetActive(true);
        _interactableObject.SetActive(true);
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
    public IEnumerator OutlineIsShown()
    {
        _interactable.MouseEnter.Invoke(_interactableObject);

        yield return new WaitForSeconds(1);

        Assert.AreEqual(VISIBLE_LAYER, _outlinable.OutlineLayer);
    }

    [UnityTest]
    public IEnumerator OutlineIsHidden()
    {
        _interactable.MouseExit.Invoke(_interactableObject);

        yield return new WaitForSeconds(1);

        Assert.AreEqual(HIDDEN_LAYER, _outlinable.OutlineLayer);
    }
}

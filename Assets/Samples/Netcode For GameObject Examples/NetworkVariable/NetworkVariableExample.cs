using Unity.Netcode;
using UnityEngine;

public class NetworkVariableExample : NetworkBehaviour
{
    //Define the NetworkVariable with a default value of 0, anyone can read the variable but only the server can change it.
    //Each client will have their own version of this, but everyone is able to see it and use it for UI or something (Ex, a healh value on a player)
    private NetworkVariable<int> sharedInteger = new NetworkVariable<int>
        (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Start()
    {
        //Set up a callback to be called for when the value changes
        sharedInteger.OnValueChanged += OnValueChanged;
    }

    void Update()
    {
        //If someone other then ourself is trying to access this method, return
        if (!IsLocalPlayer) { return; }

        if (Input.GetKeyDown(KeyCode.S))
        {
            UpdateSharedIntegerRpc();
        }
    }

    /// <summary>
    /// This is called on every client subscribed to this event when the value of the NetworkVariable changes
    /// </summary>
    /// <param name="previousValue">The previous value of the NetworkVariable</param>
    /// <param name="currentValue">The new/current value of the NetworkVariable</param>
    private void OnValueChanged(int previousValue, int currentValue)
    {
        Debug.Log($"The old value was: {previousValue} and the new value is: {currentValue}");
    }

    /// <summary>
    /// It is ok to not understand what an RPC it at this point, just know the attribute attached to this method means
    /// this method is called from a client to run on the server. Since we have the write value of the NetworkVariable 
    /// set to only the server, we need to change the value of the variable on the server for it to be shared properly.
    /// </summary>
    [Rpc(SendTo.Server)]
    private void UpdateSharedIntegerRpc()
    {
        sharedInteger.Value += 1;
    }
}
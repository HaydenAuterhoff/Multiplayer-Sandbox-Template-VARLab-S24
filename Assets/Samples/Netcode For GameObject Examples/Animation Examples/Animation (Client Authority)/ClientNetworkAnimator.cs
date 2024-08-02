using Unity.Netcode.Components;

public class ClientNetworkAnimator : NetworkAnimator
{
    /// <summary>
    /// By making this boolean false, we are allowing clients to play their own animations on objects they own
    /// </summary>
    /// <returns>false</returns>
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}


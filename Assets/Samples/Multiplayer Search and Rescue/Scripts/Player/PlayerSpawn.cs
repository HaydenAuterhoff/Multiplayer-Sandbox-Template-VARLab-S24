using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSpawn : NetworkBehaviour
{
    // Very simple spawn point finder, see if this works with networked play

    [Header("Settings")]
    public string RespawnTag = "Respawn";

    [Tooltip("Time (seconds) to wait after Start before attempting to find a spawn point")]
    public float InitialDelay = 1f;

    [Tooltip("Time (seconds) between attempts")]
    public float RefreshRate = 1f;

    [Tooltip("Time (seconds) to try finding spawn points. " +
        "After this time, the operation will exit even if a spawn point has not been found")]
    public float Timeout = 10f;

    [Header("Events")]
    public UnityEvent<Transform> Spawned;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        Debug.Log("Managing spawn of this client's character controller");

        StartCoroutine(SpawnAsync());
    }


    private IEnumerator SpawnAsync()
    {
        float timeElapsed = 0f;

        yield return new WaitForSeconds(InitialDelay);
        
        while (timeElapsed < Timeout)
        {
            if (TryFindSpawnPoint())
            {
                yield break;
            }

            timeElapsed += Time.deltaTime;
            yield return new WaitForSeconds(RefreshRate);
            yield return null;
        }
    }

    private bool TryFindSpawnPoint()
    {
        GameObject[] spawns = GameObject.FindGameObjectsWithTag(RespawnTag);

        if (spawns == null || spawns.Length == 0) { return false; }

        GameObject spawn = spawns[Random.Range(0, spawns.Length - 1)];
        transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);
        Debug.Log($"Set spawn position to {spawn.name} ({transform.position} {transform.rotation})");

        Spawned?.Invoke(spawn.transform);

        return true;
    }

}

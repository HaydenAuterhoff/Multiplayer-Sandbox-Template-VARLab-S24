using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn : NetworkBehaviour
{
    // Very simple spawn point finder, see if this works with networked play

    public string RespawnTag = "Respawn";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        
        StartCoroutine(SpawnAsync());
    }

    private IEnumerator SpawnAsync()
    {
        float timeElapsed = 0f;
        
        while (timeElapsed < 10f)
        {
            GameObject[] spawns = GameObject.FindGameObjectsWithTag(RespawnTag);

            if (spawns != null && spawns.Length > 0)
            {
                GameObject spawn = spawns[UnityEngine.Random.Range(0, spawns.Length - 1)];
                transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);
                Debug.Log($"Set spawn position to {spawn.name} ({transform.position} {transform.rotation})");
                yield break;
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

}

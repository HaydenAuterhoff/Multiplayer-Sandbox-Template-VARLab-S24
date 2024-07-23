using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// This class is a SpawnManager, it is to be utilized when you want to have custom spawns inside of a mutiplayer game. 
/// How to use: Create an empty gameobject, name it whatever, add this script to it. This manager can be in any scene, but it makes the most sense
/// to make it in your game scene
/// 
/// NOTE: This script does not currently support the use of multiple scenes, it was deemed as unnecessary as of making it, if this functionality wants to be added
/// later, this script will need to be reworked to allow for a list of transforms (spawn points) per scene.
/// </summary>
public class SpawnManager : NetworkBehaviour
{
    //The one and only instance of this script, to access the scripts methods, please use SpawnManager.Instance.MethodName
    public static SpawnManager Instance { get; private set; }

    //Enum containing the spawning methods, if more want to be added later, put them in this enum and add the functionality below
    private enum SpawnMethods { RoundRobin = 0, Random = 1, Specific = 2 }

    //The spawning method to be used in game (set in editor)
    [SerializeField]
    [Tooltip("Spawn method to be used in game")]
    private SpawnMethods spawnMethod;

    //The list of spawn points (set in editor)
    [SerializeField]
    [Tooltip("List of Transforms for where the player can spawn")]
    private List<Transform> spawnPointsData = new();

    //A backup spawn point to be used in case something goes wrong (set in editor)
    [SerializeField]
    [Tooltip("Backup spawn point to use in case something goes wrong")]
    private Transform backupSpawnPoint;

    //Whether or not this should be destoryed on scene change, may not have much use now, but added it in case
    [SerializeField]
    [Tooltip("Whether or not this system should persist through scenes")]
    private bool DoNotDestroyOnLoad = false;

    //A private index of where the round robin index is 
    private int roundRobinIndex = 0;

    /// <summary>
    /// Upon awake, set the instance and set Dont Destory on Load based on editor setting
    /// </summary>
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        if (DoNotDestroyOnLoad) { DontDestroyOnLoad(gameObject); }
    }

    //Private constructor so no more instances can be made
    private SpawnManager() { }

    /// <summary>
    /// Gets a spawn point for the player depending on the spawn method given. Function CAN return null, ensure it is checked for
    /// </summary>
    /// <param name="specificIndex">Optional Parameter. Used when attempting to get a specific spawn point</param>
    /// <returns></returns>
    public Transform GetPlayerSpawnPoint(int specificIndex = 0)
    {
        //If this is not being ran by the server, then immediatly return
        if (!IsServer) { return null; }

        Transform playerSpawnPoint;

        //If no data was given attempt to get backup spawnpoint
        if (spawnPointsData == null || spawnPointsData.Count <= 0)
        {
            return GetBackupSpawnPoint();
        }
        else
        {
            //Otherwise get a spawnpoint based on method
            playerSpawnPoint = spawnMethod switch
            {
                SpawnMethods.RoundRobin => GetNextSpawnPoint(),
                SpawnMethods.Random => GetRandomSpawnPoint(),
                SpawnMethods.Specific => GetSpecificSpawnPoint(specificIndex),
                _ => GetBackupSpawnPoint()
            };

            //If the method is round robin, update the index
            if (spawnMethod == SpawnMethods.RoundRobin) { roundRobinIndex = (roundRobinIndex + 1) % spawnPointsData.Count; }
        }
        return playerSpawnPoint;
    }

    /// <summary>
    /// Attempts to get the backup spawnpoint
    /// </summary>
    /// <returns>Returns the backup spwan point or null if there is none available</returns>
    private Transform GetBackupSpawnPoint()
    {
        if (backupSpawnPoint != null)
        {
            Debug.Log("Using backup spawn point.");
            return backupSpawnPoint.transform;
        }

        Debug.LogWarning("No backup spawn point found. Unable to find a spawn point for the player.");
        return null;
    }

    /// <summary>
    /// Gets a random spawn point from the ones available
    /// </summary>
    /// <returns>The spawn point to be used</returns>
    private Transform GetRandomSpawnPoint()
    {
        return spawnPointsData[Random.Range(0, spawnPointsData.Count)].transform;
    }

    /// <summary>
    /// Gets the next spawn point in a round robin format
    /// </summary>
    /// <returns>The spawn point to be used</returns>
    private Transform GetNextSpawnPoint()
    {
        return spawnPointsData[roundRobinIndex].transform;
    }

    /// <summary>
    /// Gets a specific spawn point
    /// </summary>
    /// <param name="index">Index to be used to search the list of transforms</param>
    /// <returns>The spawn point to be used</returns>
    private Transform GetSpecificSpawnPoint(int index)
    {
        //Check if the provided index is higher then the list count
        if (index > spawnPointsData.Count)
        {
            //If this is the case use the backup
            Debug.LogWarning("Index provided was out of range of available spawn points, now attempting to use backup spawn point");
            return GetBackupSpawnPoint();
        }

        //Otherwise obtain the spawn point
        return spawnPointsData[index].transform;
    }
}
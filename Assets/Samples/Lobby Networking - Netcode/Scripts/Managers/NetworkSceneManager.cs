using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VARLab.Multiplayer
{
    public class NetworkSceneManager : NetworkBehaviour
    {

#if UNITY_EDITOR
        public UnityEditor.SceneAsset SceneAsset;
        private void OnValidate()
        {
            if (SceneAsset != null)
            {
                m_SceneName = SceneAsset.name;
            }
        }
#endif

        [SerializeField]
        private string m_SceneName;
        private Scene m_LoadedScene;

        public bool IsSceneLoaded => m_LoadedScene.IsValid() && m_LoadedScene.isLoaded;


        public override void OnNetworkSpawn()
        {
            if (!enabled) 
            {
                Debug.Log("NetworkSceneManager disabled. Will not load shared scene");
                return; 
            }

            LoadScene();

            base.OnNetworkSpawn();
        }

        private void CheckStatus(SceneEventProgressStatus status, bool isLoading = true)
        {
            var sceneEventAction = isLoading ? "load" : "unload";
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to {sceneEventAction} {m_SceneName} with" +
                    $" a {nameof(SceneEventProgressStatus)}: {status}");
            }
        }

        /// <summary>
        /// Handles processing notifications when subscribed to OnSceneEvent
        /// </summary>
        /// <param name="sceneEvent">class that has information about the scene event</param>
        private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
        {
            var clientOrServer = sceneEvent.ClientId == NetworkManager.ServerClientId ? "server" : "client";
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.LoadComplete:
                    {
                        // We want to handle this for only the server-side
                        if (sceneEvent.ClientId == NetworkManager.ServerClientId)
                        {
                            // *** IMPORTANT ***
                            // Keep track of the loaded scene, you need this to unload it
                            m_LoadedScene = sceneEvent.Scene;
                        }
                        Debug.Log($"Loaded the {sceneEvent.SceneName} scene on " +
                            $"{clientOrServer}-({sceneEvent.ClientId}).");
                        break;
                    }
                case SceneEventType.UnloadComplete:
                    {
                        Debug.Log($"Unloaded the {sceneEvent.SceneName} scene on " +
                            $"{clientOrServer}-({sceneEvent.ClientId}).");
                        break;
                    }
                case SceneEventType.LoadEventCompleted:
                case SceneEventType.UnloadEventCompleted:
                    {
                        var loadUnload = sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted ? "Load" : "Unload";
                        Debug.Log($"{loadUnload} event completed for the following client " +
                            $"identifiers:({sceneEvent.ClientsThatCompleted})");
                        if (sceneEvent.ClientsThatTimedOut.Count > 0)
                        {
                            Debug.LogWarning($"{loadUnload} event timed out for the following client " +
                                $"identifiers:({sceneEvent.ClientsThatTimedOut})");
                        }
                        break;
                    }
            }
        }

        public void LoadScene()
        {
            if (IsServer && !string.IsNullOrEmpty(m_SceneName))
            {
                NetworkManager.SceneManager.ActiveSceneSynchronizationEnabled = true;
                NetworkManager.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Additive);

                NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
                var status = NetworkManager.SceneManager.LoadScene(m_SceneName, LoadSceneMode.Additive);
                CheckStatus(status, isLoading: true);
            }
        }

        public void UnloadScene()
        {
            // Assure only the server calls this when the NetworkObject is
            // spawned and the scene is loaded.
            if (!IsServer || !IsSpawned || !m_LoadedScene.IsValid() || !m_LoadedScene.isLoaded)
            {
                return;
            }

            // Unload the scene
            var status = NetworkManager.SceneManager.UnloadScene(m_LoadedScene);
            CheckStatus(status, false);
        }
    }
}
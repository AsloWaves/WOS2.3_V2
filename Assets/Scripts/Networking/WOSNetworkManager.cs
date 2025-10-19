using UnityEngine;
using Mirror;
using WOS.Player;
using WOS.Testing;

namespace WOS.Networking
{
    /// <summary>
    /// Custom NetworkManager for WOS Naval MMO
    /// Handles player ship spawning, scene transitions, and network lifecycle
    /// </summary>
    public class WOSNetworkManager : NetworkManager
    {
        [Header("WOS Naval Configuration")]
        [Tooltip("Default spawn positions for players (ocean spawn points)")]
        public Transform[] oceanSpawnPoints;

        [Tooltip("Use random spawn point or round-robin")]
        public PlayerSpawnMethod spawnMethod = PlayerSpawnMethod.Random;

        [Header("Network Settings for Naval Game")]
        [Tooltip("Naval games don't need high tick rate - 20-30Hz is sufficient")]
        [Range(10, 60)]
        public int navalSendRate = 30;

        private int nextSpawnPointIndex = 0;

        public override void Start()
        {
            base.Start();

            // Apply naval-specific send rate
            sendRate = navalSendRate;

            Debug.Log($"🌊 WOS NetworkManager initialized - Send Rate: {sendRate}Hz");
        }

        /// <summary>
        /// Called on the server when a client is ready (after scene load)
        /// This is where we spawn the player's ship
        /// </summary>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // Get spawn position
            Transform spawnPoint = GetNextSpawnPoint();
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            // Check if we're returning from a port
            ScenePortManager portManager = FindFirstObjectByType<ScenePortManager>();
            if (portManager != null && PlayerPrefs.GetInt("PortExit_Valid", 0) == 1)
            {
                Debug.Log("🚢 Player returning from port - using port exit position");
                // ScenePortManager will handle positioning after spawn
            }

            // Spawn the player ship
            GameObject playerShip = Instantiate(playerPrefab, spawnPosition, spawnRotation);
            playerShip.name = $"PlayerShip_{conn.connectionId}";

            // Add to network
            NetworkServer.AddPlayerForConnection(conn, playerShip);

            Debug.Log($"✅ Spawned player ship for connection {conn.connectionId} at {spawnPosition}");
        }

        /// <summary>
        /// Get the next spawn point based on spawn method
        /// </summary>
        private Transform GetNextSpawnPoint()
        {
            if (oceanSpawnPoints == null || oceanSpawnPoints.Length == 0)
            {
                Debug.LogWarning("⚠️ No ocean spawn points configured! Using world origin.");
                return null;
            }

            Transform spawnPoint;

            switch (spawnMethod)
            {
                case PlayerSpawnMethod.Random:
                    spawnPoint = oceanSpawnPoints[Random.Range(0, oceanSpawnPoints.Length)];
                    break;

                case PlayerSpawnMethod.RoundRobin:
                    spawnPoint = oceanSpawnPoints[nextSpawnPointIndex];
                    nextSpawnPointIndex = (nextSpawnPointIndex + 1) % oceanSpawnPoints.Length;
                    break;

                default:
                    spawnPoint = oceanSpawnPoints[0];
                    break;
            }

            return spawnPoint;
        }

        /// <summary>
        /// Called on server when a player disconnects
        /// </summary>
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            Debug.Log($"🚪 Player disconnected: {conn.connectionId}");
            base.OnServerDisconnect(conn);
        }

        /// <summary>
        /// Called on client when connected to server
        /// </summary>
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("🌊 Connected to WOS server!");
        }

        /// <summary>
        /// Called on client when disconnected from server
        /// </summary>
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("🚪 Disconnected from WOS server");
        }

        /// <summary>
        /// Called when server starts
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("🌊 WOS Server started!");
        }

        /// <summary>
        /// Called when server stops
        /// </summary>
        public override void OnStopServer()
        {
            base.OnStopServer();
            Debug.Log("🛑 WOS Server stopped");
        }

        #region Scene Management

        /// <summary>
        /// Called on server when changing scenes
        /// Useful for port transitions
        /// </summary>
        public override void OnServerChangeScene(string newSceneName)
        {
            base.OnServerChangeScene(newSceneName);
            Debug.Log($"🏝️ Server changing scene to: {newSceneName}");
        }

        /// <summary>
        /// Called on server AFTER scene has finished loading
        /// This is where we find spawn points in the new scene
        /// </summary>
        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);

            // Automatically find spawn points in the new scene
            FindSpawnPointsInScene();

            Debug.Log($"✅ Server scene loaded: {sceneName}");
        }

        /// <summary>
        /// Called on client when server changes scene
        /// </summary>
        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
            Debug.Log($"🏝️ Client loading scene: {newSceneName}");
        }

        /// <summary>
        /// Automatically find all spawn points in the current scene
        /// Looks for GameObjects tagged with "SpawnPoint" or children of GameObject named "SpawnPoints"
        /// </summary>
        private void FindSpawnPointsInScene()
        {
            // Method 1: Find by tag "SpawnPoint"
            GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");

            // Method 2: Find children of GameObject named "SpawnPoints" (fallback if no tags)
            if (spawnPointObjects.Length == 0)
            {
                GameObject spawnPointsParent = GameObject.Find("SpawnPoints");
                if (spawnPointsParent != null)
                {
                    Transform[] childTransforms = spawnPointsParent.GetComponentsInChildren<Transform>();
                    // Filter out the parent itself
                    System.Collections.Generic.List<GameObject> childObjects = new System.Collections.Generic.List<GameObject>();
                    foreach (Transform t in childTransforms)
                    {
                        if (t != spawnPointsParent.transform)
                        {
                            childObjects.Add(t.gameObject);
                        }
                    }
                    spawnPointObjects = childObjects.ToArray();
                }
            }

            if (spawnPointObjects.Length > 0)
            {
                // Convert GameObjects to Transforms
                oceanSpawnPoints = new Transform[spawnPointObjects.Length];
                for (int i = 0; i < spawnPointObjects.Length; i++)
                {
                    oceanSpawnPoints[i] = spawnPointObjects[i].transform;
                }

                Debug.Log($"🎯 Found {oceanSpawnPoints.Length} spawn points in scene");
            }
            else
            {
                Debug.LogWarning("⚠️ No spawn points found in scene! Players will spawn at origin (0,0,0)");
                Debug.LogWarning("💡 Create GameObjects with tag 'SpawnPoint' or children of GameObject named 'SpawnPoints'");
                oceanSpawnPoints = new Transform[0];
            }
        }

        #endregion

        #region Debug Helpers

        [ContextMenu("Start Host")]
        public void StartHostFromMenu()
        {
            StartHost();
        }

        [ContextMenu("Start Server")]
        public void StartServerFromMenu()
        {
            StartServer();
        }

        [ContextMenu("Start Client")]
        public void StartClientFromMenu()
        {
            StartClient();
        }

        [ContextMenu("Stop")]
        public void StopFromMenu()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                StopClient();
            }
            else if (NetworkServer.active)
            {
                StopServer();
            }
        }

        #endregion
    }
}

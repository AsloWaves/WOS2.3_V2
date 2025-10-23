using UnityEngine;
using Mirror;

namespace WOS.Networking
{
    /// <summary>
    /// Development helper to start host/client with keyboard shortcuts.
    /// Only active in Unity Editor for quick testing.
    /// </summary>
    public class DevHostStarter : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("Keyboard Shortcuts")]
        [Tooltip("Key to start as Host (server + client)")]
        public KeyCode hostKey = KeyCode.H;

        [Tooltip("Key to start as Client")]
        public KeyCode clientKey = KeyCode.C;

        [Tooltip("Key to start as Server only")]
        public KeyCode serverKey = KeyCode.S;

        private NetworkManager networkManager;

        private void Start()
        {
            networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogWarning("[DevHostStarter] NetworkManager not found!");
            }
            else
            {
                Debug.Log($"[DevHostStarter] Ready! Press {hostKey} for Host, {clientKey} for Client, {serverKey} for Server");
            }
        }

        private void Update()
        {
            if (networkManager == null) return;

            // Only allow starting if not already active
            if (NetworkServer.active || NetworkClient.active) return;

            // Start Host (H key)
            if (Input.GetKeyDown(hostKey))
            {
                Debug.Log("[DevHostStarter] Starting Host...");
                networkManager.StartHost();
            }

            // Start Client (C key)
            if (Input.GetKeyDown(clientKey))
            {
                Debug.Log("[DevHostStarter] Starting Client...");
                networkManager.StartClient();
            }

            // Start Server (S key)
            if (Input.GetKeyDown(serverKey))
            {
                Debug.Log("[DevHostStarter] Starting Server...");
                networkManager.StartServer();
            }
        }
#endif
    }
}

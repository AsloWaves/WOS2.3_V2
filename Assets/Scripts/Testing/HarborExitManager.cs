using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using WOS.Debugging;

namespace WOS.Testing
{
    /// <summary>
    /// Manages the harbor scene and handles returning to the main game world.
    /// Reads saved entry data and provides exit functionality.
    /// </summary>
    public class HarborExitManager : MonoBehaviour
    {
        [Header("Harbor Scene Configuration")]
        [Tooltip("UI Button to exit harbor (optional - can use keyboard)")]
        public Button exitHarborButton;

        [Tooltip("Enable keyboard shortcut to exit (R key)")]
        public bool enableKeyboardExit = true;

        [Tooltip("Show debug information about saved data")]
        public bool showDebugInfo = true;

        [Header("Exit Transition")]
        [Tooltip("Use async loading when returning to main scene")]
        public bool useAsyncLoading = true;

        [Tooltip("Delay before starting scene transition (for effects)")]
        [Range(0f, 2f)]
        public float exitDelay = 0.5f;

        [Header("Saved Data (Read-Only)")]
        [SerializeField] private Vector3 savedEntryPosition;
        [SerializeField] private Vector3 savedEntryRotation;
        [SerializeField] private string savedPortID;
        [SerializeField] private string savedExitSceneName;

        // Exit state
        private bool isExiting = false;

        private void Start()
        {
            LoadSavedPortData();
            SetupExitButton();

            if (showDebugInfo)
            {
                DisplayHarborInfo();
            }
        }

        private void Update()
        {
            // Keyboard shortcut to exit
            if (enableKeyboardExit && !isExiting)
            {
                if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Escape))
                {
                    ExitHarbor();
                }
            }
        }

        private void LoadSavedPortData()
        {
            // Read the saved data from PlayerPrefs
            if (PlayerPrefs.HasKey("PortEntry_Position"))
            {
                string posJson = PlayerPrefs.GetString("PortEntry_Position");
                savedEntryPosition = JsonUtility.FromJson<Vector3>(posJson);
            }

            if (PlayerPrefs.HasKey("PortEntry_Rotation"))
            {
                string rotJson = PlayerPrefs.GetString("PortEntry_Rotation");
                Quaternion savedQuat = JsonUtility.FromJson<Quaternion>(rotJson);
                savedEntryRotation = savedQuat.eulerAngles;
            }

            if (PlayerPrefs.HasKey("PortEntry_PortID"))
            {
                savedPortID = PlayerPrefs.GetString("PortEntry_PortID");
            }

            if (PlayerPrefs.HasKey("PortEntry_ExitScene"))
            {
                savedExitSceneName = PlayerPrefs.GetString("PortEntry_ExitScene");
                if (string.IsNullOrEmpty(savedExitSceneName))
                {
                    savedExitSceneName = "MainScene"; // Fallback scene name
                    Debug.LogWarning("No exit scene saved, defaulting to 'MainScene'");
                }
            }
            else
            {
                savedExitSceneName = "MainScene"; // Fallback if nothing saved
                Debug.LogWarning("No exit scene data found, defaulting to 'MainScene'");
            }

            Debug.Log($"📚 Loaded Harbor Data: Port={savedPortID}, Exit Scene={savedExitSceneName}");
            Debug.Log($"📍 Entry Position={savedEntryPosition}, Rotation={savedEntryRotation}");
        }

        private void SetupExitButton()
        {
            if (exitHarborButton != null)
            {
                exitHarborButton.onClick.RemoveAllListeners();
                exitHarborButton.onClick.AddListener(ExitHarbor);

                // Update button text if it has a Text component
                var buttonText = exitHarborButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "Exit Harbor";
                }
            }
        }

        private void DisplayHarborInfo()
        {
            Debug.Log("=== HARBOR SCENE LOADED ===");
            Debug.Log($"🏗️ You are in port: {savedPortID}");
            Debug.Log($"🚢 Your ship is docked at: {savedEntryPosition}");
            Debug.Log($"🔄 Ship rotation when entered: {savedEntryRotation}");
            Debug.Log($"📍 Will return to scene: {savedExitSceneName}");
            Debug.Log("=== CONTROLS ===");
            Debug.Log("Press R or ESC to exit harbor and return to your ship");
            if (exitHarborButton != null)
            {
                Debug.Log("Or click the Exit Harbor button");
            }
        }

        public void ExitHarbor()
        {
            if (isExiting) return;

            isExiting = true;
            Debug.Log("🚢 Exiting harbor...");

            // Save the exit data with 180-degree rotation
            PrepareExitData();

            // Start the exit sequence
            StartCoroutine(ExitHarborSequence());
        }

        private void PrepareExitData()
        {
            Vector3 exitPosition;

            // Check if we have port-relative data (new system)
            if (PlayerPrefs.HasKey("PortEntry_PortCenter") && PlayerPrefs.HasKey("PortEntry_RelativeOffset"))
            {
                // NEW: Use port-relative positioning
                Vector3 originalPortCenter = JsonUtility.FromJson<Vector3>(PlayerPrefs.GetString("PortEntry_PortCenter"));
                Vector3 relativeOffset = JsonUtility.FromJson<Vector3>(PlayerPrefs.GetString("PortEntry_RelativeOffset"));

                // Find the port in the target scene by searching for SimplePortTest with matching config
                string portID = PlayerPrefs.GetString("PortEntry_PortID", "");

                // For now, use original port center + offset (this will be improved when we return to main scene)
                // The ScenePortManager will need to find the correct port and calculate the real exit position
                exitPosition = originalPortCenter + relativeOffset;

                Debug.Log($"🏗️ Using port-relative exit: PortCenter={originalPortCenter}, Offset={relativeOffset}");
                Debug.Log($"🎯 Calculated exit position: {exitPosition}");
            }
            else
            {
                // FALLBACK: Use absolute position (old system)
                exitPosition = savedEntryPosition;
                Debug.Log("⚠️ Using absolute position fallback");
            }

            // Calculate exit rotation (180 degrees from entry)
            Quaternion entryRotation = Quaternion.Euler(savedEntryRotation);
            Quaternion exitRotation = entryRotation * Quaternion.Euler(0, 180, 0);

            // Save exit data for the main scene to read
            PlayerPrefs.SetString("PortExit_Position", JsonUtility.ToJson(exitPosition));
            PlayerPrefs.SetString("PortExit_Rotation", JsonUtility.ToJson(exitRotation));
            PlayerPrefs.SetFloat("PortExit_Throttle", 0f); // Ensure throttle starts at 0
            PlayerPrefs.SetFloat("PortExit_Speed", 0f); // Ensure speed starts at 0
            PlayerPrefs.SetInt("PortExit_Valid", 1); // Flag to indicate valid exit data
            PlayerPrefs.Save();

            Debug.Log($"💾 Exit data saved: Position={exitPosition}, Rotation={exitRotation.eulerAngles}");
            Debug.Log("🔄 Ship will be rotated 180° from entry direction");
        }

        private IEnumerator ExitHarborSequence()
        {
            // Optional delay for any exit effects
            if (exitDelay > 0)
            {
                Debug.Log($"⏳ Preparing to exit in {exitDelay} seconds...");
                yield return new WaitForSeconds(exitDelay);
            }

            // Load the main scene
            if (useAsyncLoading)
            {
                yield return LoadMainSceneAsync();
            }
            else
            {
                LoadMainScene();
            }
        }

        private void LoadMainScene()
        {
            Debug.Log($"🏗️ Loading scene: {savedExitSceneName}");
            SceneManager.LoadScene(savedExitSceneName);
        }

        private IEnumerator LoadMainSceneAsync()
        {
            Debug.Log($"🏗️ Loading scene async: {savedExitSceneName}");

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(savedExitSceneName);

            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                Debug.Log($"Loading progress: {progress * 100}%");

                // You could update a loading bar UI here if you have one

                yield return null;
            }

            Debug.Log("✅ Main scene loaded! Ship should be at exit position.");
        }

        // Public method that can be called by UI buttons
        public void OnExitButtonClicked()
        {
            ExitHarbor();
        }

        // Optional: Method to exit to a specific scene
        public void ExitToSpecificScene(string sceneName)
        {
            savedExitSceneName = sceneName;
            ExitHarbor();
        }
    }
}
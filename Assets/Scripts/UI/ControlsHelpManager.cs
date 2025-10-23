using UnityEngine;
using TMPro;
using Michsky.MUIP;

namespace WOS.UI
{
    /// <summary>
    /// Manages the F1 controls help panel with configurable keybindings.
    /// Displays all game controls in a multi-line TMP text field.
    /// Supports both direct TMP assignment and MUIP CustomInputField components.
    /// </summary>
    public class ControlsHelpManager : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The panel GameObject containing the controls text")]
        public GameObject controlsPanel;

        [Header("Text Component (Choose ONE)")]
        [Tooltip("Option 1: Direct TextMeshProUGUI reference")]
        public TextMeshProUGUI controlsTextField;

        [Tooltip("Option 2: MUIP CustomInputField (auto-extracts TMP component)")]
        public CustomInputField muipInputField;

        [Tooltip("Option 3: TMP_InputField (auto-extracts text component)")]
        public TMP_InputField tmpInputField;

        // Internal reference to the actual TMP component being used
        private TMP_Text activeTextField;

        [Header("Toggle Settings")]
        [Tooltip("Key to toggle the controls panel")]
        public KeyCode toggleKey = KeyCode.F1;

        [Tooltip("Start with panel visible?")]
        public bool startVisible = false;

        [Header("Ship Movement Controls")]
        public ControlBinding steerLeft = new ControlBinding("A / Left Arrow", "Steer left (rudder port)");
        public ControlBinding steerRight = new ControlBinding("D / Right Arrow", "Steer right (rudder starboard)");
        public ControlBinding throttleUp = new ControlBinding("W", "Increase throttle");
        public ControlBinding throttleDown = new ControlBinding("S", "Decrease throttle");
        public ControlBinding emergencyStop = new ControlBinding("SPACE", "Emergency stop (full stop)");

        [Header("Navigation Controls")]
        public ControlBinding setWaypoint = new ControlBinding("Right Mouse Click", "Set navigation waypoint");
        public ControlBinding toggleAutopilot = new ControlBinding("Z", "Toggle autopilot");
        public ControlBinding clearWaypoints = new ControlBinding("X", "Clear all waypoints");
        public ControlBinding interact = new ControlBinding("E (hold)", "Interact with ports/objects");

        [Header("Camera Controls")]
        public ControlBinding zoomIn = new ControlBinding("Mouse Wheel Up", "Zoom in");
        public ControlBinding zoomOut = new ControlBinding("Mouse Wheel Down", "Zoom out");
        public ControlBinding panCamera = new ControlBinding("Middle Mouse + Drag", "Pan camera");

        [Header("UI Controls")]
        public ControlBinding openMenu = new ControlBinding("ESC", "Open/close menu");
        public ControlBinding openInventory = new ControlBinding("I", "Open inventory");
        public ControlBinding toggleHelp = new ControlBinding("F1", "Toggle this help panel");

        private bool isPanelVisible;

        private void Start()
        {
            // Resolve which text component to use
            ResolveTextComponent();

            // Set initial visibility
            isPanelVisible = startVisible;
            if (controlsPanel != null)
            {
                controlsPanel.SetActive(isPanelVisible);
            }

            // Generate and display controls text
            UpdateControlsText();

            Debug.Log($"[ControlsHelp] Controls help panel initialized. Press {toggleKey} to toggle.");
        }

        /// <summary>
        /// Resolve which text component is assigned and extract TMP reference
        /// </summary>
        private void ResolveTextComponent()
        {
            // Priority 1: Direct TextMeshProUGUI reference
            if (controlsTextField != null)
            {
                activeTextField = controlsTextField;
                Debug.Log("[ControlsHelp] Using direct TextMeshProUGUI reference");
                return;
            }

            // Priority 2: MUIP CustomInputField (extract TMP from inputText.textComponent)
            if (muipInputField != null)
            {
                if (muipInputField.inputText != null)
                {
                    activeTextField = muipInputField.inputText.textComponent;
                    Debug.Log("[ControlsHelp] ✅ Extracted TMP from MUIP CustomInputField.inputText.textComponent");

                    // Set to read-only mode
                    muipInputField.inputText.readOnly = true;
                    Debug.Log("[ControlsHelp] Set MUIP input field to read-only mode");
                    return;
                }
                else
                {
                    Debug.LogError("[ControlsHelp] MUIP CustomInputField.inputText is NULL!");
                }
            }

            // Priority 3: TMP_InputField (extract textComponent)
            if (tmpInputField != null)
            {
                activeTextField = tmpInputField.textComponent;
                tmpInputField.readOnly = true;
                Debug.Log("[ControlsHelp] ✅ Extracted TMP from TMP_InputField.textComponent (set to read-only)");
                return;
            }

            // No valid component assigned
            Debug.LogError("[ControlsHelp] ❌ No text component assigned! Please assign one of:");
            Debug.LogError("[ControlsHelp]   - controlsTextField (TextMeshProUGUI)");
            Debug.LogError("[ControlsHelp]   - muipInputField (MUIP CustomInputField)");
            Debug.LogError("[ControlsHelp]   - tmpInputField (TMP_InputField)");
        }

        private void Update()
        {
            // Toggle panel with F1 (or configured key)
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }
        }

        /// <summary>
        /// Toggle the controls panel visibility
        /// </summary>
        public void TogglePanel()
        {
            isPanelVisible = !isPanelVisible;

            if (controlsPanel != null)
            {
                controlsPanel.SetActive(isPanelVisible);
                Debug.Log($"[ControlsHelp] Panel {(isPanelVisible ? "opened" : "closed")}");
            }
            else
            {
                Debug.LogWarning("[ControlsHelp] Controls panel is not assigned!");
            }
        }

        /// <summary>
        /// Show the controls panel
        /// </summary>
        public void ShowPanel()
        {
            isPanelVisible = true;
            if (controlsPanel != null)
            {
                controlsPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the controls panel
        /// </summary>
        public void HidePanel()
        {
            isPanelVisible = false;
            if (controlsPanel != null)
            {
                controlsPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Generate formatted controls text and update the TMP field
        /// </summary>
        public void UpdateControlsText()
        {
            if (activeTextField == null)
            {
                Debug.LogWarning("[ControlsHelp] Active text field is not resolved! Check component assignments.");
                return;
            }

            string controlsText = GenerateControlsText();
            activeTextField.text = controlsText;
            Debug.Log("[ControlsHelp] Controls text updated successfully");
        }

        /// <summary>
        /// Generate formatted controls text from all configured bindings
        /// </summary>
        private string GenerateControlsText()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("=== GAME CONTROLS ===");
            sb.AppendLine();

            // Ship Movement
            sb.AppendLine("[ SHIP MOVEMENT ]");
            sb.AppendLine($"{steerLeft.key,-25} - {steerLeft.description}");
            sb.AppendLine($"{steerRight.key,-25} - {steerRight.description}");
            sb.AppendLine($"{throttleUp.key,-25} - {throttleUp.description}");
            sb.AppendLine($"{throttleDown.key,-25} - {throttleDown.description}");
            sb.AppendLine($"{emergencyStop.key,-25} - {emergencyStop.description}");
            sb.AppendLine();

            // Navigation
            sb.AppendLine("[ NAVIGATION ]");
            sb.AppendLine($"{setWaypoint.key,-25} - {setWaypoint.description}");
            sb.AppendLine($"{toggleAutopilot.key,-25} - {toggleAutopilot.description}");
            sb.AppendLine($"{clearWaypoints.key,-25} - {clearWaypoints.description}");
            sb.AppendLine($"{interact.key,-25} - {interact.description}");
            sb.AppendLine();

            // Camera
            sb.AppendLine("[ CAMERA ]");
            sb.AppendLine($"{zoomIn.key,-25} - {zoomIn.description}");
            sb.AppendLine($"{zoomOut.key,-25} - {zoomOut.description}");
            sb.AppendLine($"{panCamera.key,-25} - {panCamera.description}");
            sb.AppendLine();

            // UI
            sb.AppendLine("[ UI ]");
            sb.AppendLine($"{openMenu.key,-25} - {openMenu.description}");
            sb.AppendLine($"{openInventory.key,-25} - {openInventory.description}");
            sb.AppendLine($"{toggleHelp.key,-25} - {toggleHelp.description}");

            return sb.ToString();
        }

        /// <summary>
        /// Refresh the controls text (useful if you change bindings at runtime)
        /// </summary>
        [ContextMenu("Refresh Controls Text")]
        public void RefreshControlsText()
        {
            UpdateControlsText();
            Debug.Log("[ControlsHelp] Controls text refreshed!");
        }
    }

    /// <summary>
    /// Serializable class representing a single control binding
    /// </summary>
    [System.Serializable]
    public class ControlBinding
    {
        [Tooltip("The key(s) to press")]
        public string key;

        [Tooltip("What this control does")]
        public string description;

        public ControlBinding(string key, string description)
        {
            this.key = key;
            this.description = description;
        }
    }
}

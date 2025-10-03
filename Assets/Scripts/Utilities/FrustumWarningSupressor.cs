using UnityEngine;
using WOS.Debugging;

namespace WOS.Utilities
{
    /// <summary>
    /// Suppresses the annoying "Screen position out of view frustum" warnings
    /// that occur from Unity's internal SendMouseEvents system.
    /// Place this on a GameObject in your scene (preferably on the main camera).
    /// </summary>
    public class FrustumWarningSupressor : MonoBehaviour
    {
        private void Awake()
        {
            // This disables the legacy mouse event system for this camera
            // which prevents the frustum warnings
            UnityEngine.Camera cam = GetComponent<UnityEngine.Camera>();
            if (cam != null)
            {
                // Disable legacy event mask to prevent SendMouseEvents from processing
                cam.eventMask = 0;
                DebugManager.Log(DebugCategory.System, "Disabled legacy mouse events on camera to prevent frustum warnings", this);
            }
        }

        private void OnEnable()
        {
            // Alternative approach: Filter debug logs to suppress the specific warning
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            // Suppress the specific frustum warning
            if (logString.Contains("Screen position out of view frustum"))
            {
                // Don't propagate this specific warning
                return;
            }
        }
    }
}
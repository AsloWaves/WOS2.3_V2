using UnityEngine;
using WOS.Debugging;

namespace WOS.Environment
{
    /// <summary>
    /// Emergency script to force correct ocean tile colors.
    /// Bypasses all other color systems and directly sets tile colors.
    /// </summary>
    public class OceanColorForcer : MonoBehaviour
    {
        [Header("Emergency Color Override")]
        [Tooltip("Force this color on all ocean tiles")]
        [SerializeField] private Color forcedOceanColor = new Color(0.05f, 0.1f, 0.25f, 1f); // MidnightZone color

        [Tooltip("Apply color override continuously")]
        [SerializeField] private bool continuousOverride = true;

        [Tooltip("Apply override on Start")]
        [SerializeField] private bool applyOnStart = true;

        [Tooltip("Show debug info")]
        [SerializeField] private bool showDebugInfo = true;

        private void Start()
        {
            if (applyOnStart)
            {
                ForceOceanColors();
            }
        }

        private void Update()
        {
            if (continuousOverride)
            {
                ForceOceanColors();
            }
        }

        /// <summary>
        /// Emergency: Force all ocean tiles to use the specified color
        /// </summary>
        [ContextMenu("Force Ocean Colors")]
        public void ForceOceanColors()
        {
            var allGameObjects = FindObjectsOfType<GameObject>();
            int coloredCount = 0;

            foreach (var go in allGameObjects)
            {
                if (go.name.Contains("OceanTile"))
                {
                    var spriteRenderer = go.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = forcedOceanColor;
                        coloredCount++;
                    }

                    var renderer = go.GetComponent<Renderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        renderer.material.color = forcedOceanColor;
                    }
                }
            }

            if (showDebugInfo)
            {
                DebugManager.Log(DebugCategory.Ocean, $"EMERGENCY: Forced color {forcedOceanColor} on {coloredCount} ocean tiles", this);
            }
        }

        /// <summary>
        /// Test different ocean colors
        /// </summary>
        [ContextMenu("Test Dark Blue")]
        public void TestDarkBlue()
        {
            forcedOceanColor = new Color(0.05f, 0.1f, 0.25f, 1f);
            ForceOceanColors();
        }

        [ContextMenu("Test Medium Blue")]
        public void TestMediumBlue()
        {
            forcedOceanColor = new Color(0.1f, 0.3f, 0.6f, 1f);
            ForceOceanColors();
        }

        [ContextMenu("Test Light Blue")]
        public void TestLightBlue()
        {
            forcedOceanColor = new Color(0.3f, 0.6f, 0.9f, 1f);
            ForceOceanColors();
        }

        [ContextMenu("Test Red (Debug)")]
        public void TestRed()
        {
            forcedOceanColor = Color.red;
            ForceOceanColors();
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            var allGameObjects = FindObjectsOfType<GameObject>();
            int oceanTileCount = 0;
            foreach (var go in allGameObjects)
            {
                if (go.name.Contains("OceanTile"))
                    oceanTileCount++;
            }

            GUILayout.BeginArea(new Rect(Screen.width - 250, Screen.height - 150, 240, 140));
            GUILayout.Label("<size=14><color=red><b>EMERGENCY COLOR FORCER</b></color></size>");
            GUILayout.Label($"<color=white>Ocean Tiles Found: {oceanTileCount}</color>");
            GUILayout.Label($"<color=white>Forced Color: R={forcedOceanColor.r:F2} G={forcedOceanColor.g:F2} B={forcedOceanColor.b:F2}</color>");

            if (GUILayout.Button("Force Dark Blue"))
            {
                TestDarkBlue();
            }

            if (GUILayout.Button("Force Red (Test)"))
            {
                TestRed();
            }

            GUILayout.EndArea();
        }
    }
}
using UnityEngine;
using UnityEditor;

namespace WOS.Debugging
{
#if UNITY_EDITOR
    /// <summary>
    /// Custom property drawer for DebugSettings to improve Inspector layout.
    /// Fixes checkbox overlap issues and provides better visual organization.
    /// </summary>
    [CustomPropertyDrawer(typeof(DebugSettings))]
    public class DebugSettingsDrawer : PropertyDrawer
    {
        private const float CHECKBOX_WIDTH = 20f;
        private const float SPACING = 2f;
        private const float HEADER_HEIGHT = 18f;
        private const float LINE_HEIGHT = 18f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate positions
            float yPos = position.y;
            float fullWidth = position.width;
            float labelWidth = fullWidth - CHECKBOX_WIDTH - SPACING;

            // Helper method to draw a category with proper spacing
            void DrawDebugCategory(ref float y, string displayName, string propertyName, Color headerColor)
            {
                var prop = property.FindPropertyRelative(propertyName);
                if (prop != null)
                {
                    Rect checkboxRect = new Rect(position.x, y, CHECKBOX_WIDTH, LINE_HEIGHT);
                    Rect labelRect = new Rect(position.x + CHECKBOX_WIDTH + SPACING, y, labelWidth, LINE_HEIGHT);

                    // Draw checkbox first (on the left)
                    prop.boolValue = EditorGUI.Toggle(checkboxRect, prop.boolValue);

                    // Draw label with better formatting
                    var originalColor = GUI.color;
                    GUI.color = prop.boolValue ? headerColor : Color.gray;

                    var labelStyle = new GUIStyle(EditorStyles.label);
                    labelStyle.fontStyle = prop.boolValue ? FontStyle.Bold : FontStyle.Normal;

                    EditorGUI.LabelField(labelRect, displayName, labelStyle);
                    GUI.color = originalColor;

                    y += LINE_HEIGHT + SPACING;
                }
            }

            // Draw header
            var headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 12;
            EditorGUI.LabelField(new Rect(position.x, yPos, fullWidth, HEADER_HEIGHT), "Debug Categories", headerStyle);
            yPos += HEADER_HEIGHT + SPACING * 2;

            // Core Systems Section
            var sectionStyle = new GUIStyle(EditorStyles.miniLabel);
            sectionStyle.fontStyle = FontStyle.Bold;
            EditorGUI.LabelField(new Rect(position.x, yPos, fullWidth, LINE_HEIGHT), "Core Systems", sectionStyle);
            yPos += LINE_HEIGHT;

            DrawDebugCategory(ref yPos, "Ship Physics & Movement", "enableShipDebug", Color.green);
            DrawDebugCategory(ref yPos, "Ocean Tiles & Wake Effects", "enableOceanDebug", Color.cyan);
            DrawDebugCategory(ref yPos, "Environment & Ports", "enableEnvironmentDebug", new Color(0.8f, 0.6f, 0.2f));
            DrawDebugCategory(ref yPos, "Camera & Visual Effects", "enableCameraDebug", Color.yellow);
            DrawDebugCategory(ref yPos, "Input & Controls", "enableInputDebug", Color.magenta);

            yPos += SPACING * 2;

            // Technical Section
            EditorGUI.LabelField(new Rect(position.x, yPos, fullWidth, LINE_HEIGHT), "Technical Systems", sectionStyle);
            yPos += LINE_HEIGHT;

            DrawDebugCategory(ref yPos, "Physics Calculations", "enablePhysicsDebug", Color.red);
            DrawDebugCategory(ref yPos, "Performance Metrics", "enablePerformanceDebug", Color.yellow);
            DrawDebugCategory(ref yPos, "System Core & Initialization", "enableSystemDebug", new Color(0.6f, 0.4f, 0.2f));

            yPos += SPACING * 2;

            // Features Section
            EditorGUI.LabelField(new Rect(position.x, yPos, fullWidth, LINE_HEIGHT), "Game Features", sectionStyle);
            yPos += LINE_HEIGHT;

            DrawDebugCategory(ref yPos, "User Interface & HUD", "enableUIDebug", Color.cyan);
            DrawDebugCategory(ref yPos, "Economy & Trading", "enableEconomyDebug", Color.green);
            DrawDebugCategory(ref yPos, "Audio & Sound Effects", "enableAudioDebug", Color.magenta);
            DrawDebugCategory(ref yPos, "Networking & Multiplayer", "enableNetworkingDebug", Color.gray);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Calculate total height needed
            float totalHeight = HEADER_HEIGHT + SPACING * 2; // Header

            // Core Systems (5 items + section header)
            totalHeight += LINE_HEIGHT + (LINE_HEIGHT + SPACING) * 5 + SPACING * 2;

            // Technical (3 items + section header)
            totalHeight += LINE_HEIGHT + (LINE_HEIGHT + SPACING) * 3 + SPACING * 2;

            // Features (4 items + section header)
            totalHeight += LINE_HEIGHT + (LINE_HEIGHT + SPACING) * 4;

            return totalHeight;
        }
    }

    /// <summary>
    /// Custom property drawer for DebugStats to display debug statistics nicely
    /// </summary>
    [CustomPropertyDrawer(typeof(DebugStats))]
    public class DebugStatsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw as read-only info box
            var statsStyle = new GUIStyle(EditorStyles.helpBox);
            statsStyle.fontSize = 10;
            statsStyle.alignment = TextAnchor.MiddleLeft;

            var messagesThisFrame = property.FindPropertyRelative("messagesThisFrame");
            var maxMessagesPerFrame = property.FindPropertyRelative("maxMessagesPerFrame");
            var throttledMessages = property.FindPropertyRelative("throttledMessages");
            var enabledCategories = property.FindPropertyRelative("enabledCategories");

            string statsText = $"Messages: {messagesThisFrame?.intValue ?? 0}/{maxMessagesPerFrame?.intValue ?? 0} per frame  |  " +
                              $"Categories: {enabledCategories?.intValue ?? 0} enabled  |  " +
                              $"Throttled: {throttledMessages?.intValue ?? 0}";

            EditorGUI.LabelField(position, statsText, statsStyle);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4f;
        }
    }
#endif
}
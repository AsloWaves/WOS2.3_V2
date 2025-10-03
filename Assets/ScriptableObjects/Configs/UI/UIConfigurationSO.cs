using UnityEngine;
using Unity.Mathematics;

namespace WOS.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject configuration for UI theming and behavior.
    /// Provides consistent UI styling across the naval game interface.
    /// </summary>
    [CreateAssetMenu(fileName = "UIConfiguration", menuName = "WOS/UI Configuration")]
    public class UIConfigurationSO : ScriptableObject
    {
        [Header("Color Theme")]
        [Tooltip("Primary UI color (backgrounds, panels)")]
        public Color primaryColor = new Color(0.15f, 0.25f, 0.35f, 0.9f);

        [Tooltip("Secondary UI color (highlights, accents)")]
        public Color secondaryColor = new Color(0.25f, 0.4f, 0.6f, 1f);

        [Tooltip("Text color for primary content")]
        public Color textPrimary = new Color(0.9f, 0.9f, 0.9f, 1f);

        [Tooltip("Text color for secondary content")]
        public Color textSecondary = new Color(0.7f, 0.7f, 0.7f, 1f);

        [Tooltip("Success color (confirmations, positive status)")]
        public Color successColor = new Color(0.2f, 0.8f, 0.3f, 1f);

        [Tooltip("Warning color (cautions, important notices)")]
        public Color warningColor = new Color(0.9f, 0.7f, 0.2f, 1f);

        [Tooltip("Error color (failures, critical alerts)")]
        public Color errorColor = new Color(0.8f, 0.2f, 0.2f, 1f);

        [Tooltip("Accent color (buttons, interactive elements)")]
        public Color accentColor = new Color(0.3f, 0.7f, 0.9f, 1f);

        [Header("Naval Theme Colors")]
        [Tooltip("Water/ocean color for maritime elements")]
        public Color oceanColor = new Color(0.1f, 0.3f, 0.6f, 1f);

        [Tooltip("Compass/navigation color")]
        public Color navigationColor = new Color(0.9f, 0.8f, 0.3f, 1f);

        [Tooltip("Cargo/inventory color")]
        public Color cargoColor = new Color(0.6f, 0.4f, 0.2f, 1f);

        [Tooltip("Port/harbor color")]
        public Color portColor = new Color(0.4f, 0.6f, 0.3f, 1f);

        [Header("Animation Settings")]
        [Tooltip("Standard UI animation duration")]
        [Range(0.1f, 2f)]
        public float animationDuration = 0.3f;

        [Tooltip("Fast animation duration (quick feedback)")]
        [Range(0.05f, 0.5f)]
        public float fastAnimationDuration = 0.15f;

        [Tooltip("Slow animation duration (dramatic effects)")]
        [Range(0.5f, 3f)]
        public float slowAnimationDuration = 0.8f;

        [Tooltip("Standard easing curve for animations")]
        public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Bounce animation for button presses")]
        public AnimationCurve bounceEasing = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.5f, 1.2f),
            new Keyframe(1f, 1f)
        );

        [Header("Panel Configuration")]
        [Tooltip("Default panel fade-in animation")]
        public bool useFadeInAnimation = true;

        [Tooltip("Default panel slide-in animation")]
        public bool useSlideInAnimation = true;

        [Tooltip("Panel slide direction")]
        public SlideDirection slideDirection = SlideDirection.FromBottom;

        [Tooltip("Panel background opacity")]
        [Range(0f, 1f)]
        public float panelBackgroundOpacity = 0.85f;

        [Tooltip("Modal overlay opacity")]
        [Range(0f, 1f)]
        public float modalOverlayOpacity = 0.6f;

        [Header("Button Configuration")]
        [Tooltip("Button hover color multiplier")]
        [Range(0.8f, 1.5f)]
        public float buttonHoverMultiplier = 1.2f;

        [Tooltip("Button press color multiplier")]
        [Range(0.5f, 1.2f)]
        public float buttonPressMultiplier = 0.8f;

        [Tooltip("Button disable color multiplier")]
        [Range(0.3f, 0.8f)]
        public float buttonDisableMultiplier = 0.5f;

        [Tooltip("Button press scale effect")]
        [Range(0.8f, 1.1f)]
        public float buttonPressScale = 0.95f;

        [Header("Font Configuration")]
        [Tooltip("Primary font for headers and titles")]
        public Font primaryFont;

        [Tooltip("Secondary font for body text")]
        public Font secondaryFont;

        [Tooltip("Monospace font for data/numbers")]
        public Font monospaceFont;

        [Tooltip("Default font size for body text")]
        [Range(10f, 24f)]
        public float defaultFontSize = 14f;

        [Tooltip("Large font size for headers")]
        [Range(16f, 32f)]
        public float largeFontSize = 20f;

        [Tooltip("Small font size for details")]
        [Range(8f, 16f)]
        public float smallFontSize = 12f;

        [Header("Layout Settings")]
        [Tooltip("Standard spacing between UI elements")]
        [Range(2f, 20f)]
        public float standardSpacing = 8f;

        [Tooltip("Large spacing for major sections")]
        [Range(10f, 40f)]
        public float largeSpacing = 16f;

        [Tooltip("Small spacing for compact layouts")]
        [Range(1f, 10f)]
        public float smallSpacing = 4f;

        [Tooltip("Standard border radius for rounded elements")]
        [Range(0f, 20f)]
        public float borderRadius = 6f;

        [Header("Responsive Design")]
        [Tooltip("Enable responsive scaling based on screen size")]
        public bool enableResponsiveScaling = true;

        [Tooltip("Reference resolution for UI scaling")]
        public Vector2 referenceResolution = new Vector2(1920f, 1080f);

        [Tooltip("Match width or height for scaling")]
        [Range(0f, 1f)]
        public float matchWidthOrHeight = 0.5f;

        [Tooltip("Minimum scale factor")]
        [Range(0.5f, 1f)]
        public float minScaleFactor = 0.7f;

        [Tooltip("Maximum scale factor")]
        [Range(1f, 2f)]
        public float maxScaleFactor = 1.5f;

        [Header("Audio Configuration")]
        [Tooltip("UI button click sound")]
        public AudioClip buttonClickSound;

        [Tooltip("UI button hover sound")]
        public AudioClip buttonHoverSound;

        [Tooltip("Panel open sound")]
        public AudioClip panelOpenSound;

        [Tooltip("Panel close sound")]
        public AudioClip panelCloseSound;

        [Tooltip("Error notification sound")]
        public AudioClip errorSound;

        [Tooltip("Success notification sound")]
        public AudioClip successSound;

        [Tooltip("UI volume multiplier")]
        [Range(0f, 1f)]
        public float uiVolumeMultiplier = 0.7f;

        [Header("Performance Settings")]
        [Tooltip("Maximum UI update frequency")]
        [Range(30f, 120f)]
        public float maxUpdateRate = 60f;

        [Tooltip("Enable UI object pooling")]
        public bool enableObjectPooling = true;

        [Tooltip("Use GPU instancing for repeated UI elements")]
        public bool useGPUInstancing = false;

        [Tooltip("Cull off-screen UI elements")]
        public bool cullOffscreenElements = true;

        /// <summary>
        /// Get color variant with specified alpha
        /// </summary>
        public Color GetColorWithAlpha(Color baseColor, float alpha)
        {
            return new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }

        /// <summary>
        /// Get hover color for interactive elements
        /// </summary>
        public Color GetHoverColor(Color baseColor)
        {
            return baseColor * buttonHoverMultiplier;
        }

        /// <summary>
        /// Get pressed color for interactive elements
        /// </summary>
        public Color GetPressedColor(Color baseColor)
        {
            return baseColor * buttonPressMultiplier;
        }

        /// <summary>
        /// Get disabled color for inactive elements
        /// </summary>
        public Color GetDisabledColor(Color baseColor)
        {
            return baseColor * buttonDisableMultiplier;
        }

        /// <summary>
        /// Calculate responsive scale factor based on current screen size
        /// </summary>
        public float GetResponsiveScaleFactor()
        {
            if (!enableResponsiveScaling) return 1f;

            float screenAspect = (float)Screen.width / Screen.height;
            float referenceAspect = referenceResolution.x / referenceResolution.y;

            float scaleWidth = (float)Screen.width / referenceResolution.x;
            float scaleHeight = (float)Screen.height / referenceResolution.y;

            float scale = Mathf.Lerp(scaleWidth, scaleHeight, matchWidthOrHeight);
            return Mathf.Clamp(scale, minScaleFactor, maxScaleFactor);
        }

        /// <summary>
        /// Get scaled spacing value based on current scale factor
        /// </summary>
        public float GetScaledSpacing(float baseSpacing)
        {
            return baseSpacing * GetResponsiveScaleFactor();
        }

        /// <summary>
        /// Get color for specific UI context
        /// </summary>
        public Color GetContextColor(UIContext context)
        {
            return context switch
            {
                UIContext.Navigation => navigationColor,
                UIContext.Cargo => cargoColor,
                UIContext.Port => portColor,
                UIContext.Ocean => oceanColor,
                UIContext.Success => successColor,
                UIContext.Warning => warningColor,
                UIContext.Error => errorColor,
                UIContext.Accent => accentColor,
                _ => primaryColor
            };
        }

        private void OnValidate()
        {
            // Ensure reasonable values
            animationDuration = Mathf.Max(0.1f, animationDuration);
            fastAnimationDuration = Mathf.Min(fastAnimationDuration, animationDuration);
            slowAnimationDuration = Mathf.Max(slowAnimationDuration, animationDuration);

            // Clamp multipliers to reasonable ranges
            buttonHoverMultiplier = Mathf.Clamp(buttonHoverMultiplier, 0.8f, 2f);
            buttonPressMultiplier = Mathf.Clamp(buttonPressMultiplier, 0.3f, 1.2f);
            buttonDisableMultiplier = Mathf.Clamp(buttonDisableMultiplier, 0.2f, 0.8f);

            // Ensure scale factors are logical
            maxScaleFactor = Mathf.Max(maxScaleFactor, minScaleFactor);
        }
    }

    /// <summary>
    /// UI context enumeration for contextual styling
    /// </summary>
    public enum UIContext
    {
        Default,
        Navigation,
        Cargo,
        Port,
        Ocean,
        Success,
        Warning,
        Error,
        Accent
    }

    /// <summary>
    /// Slide direction for panel animations
    /// </summary>
    public enum SlideDirection
    {
        FromLeft,
        FromRight,
        FromTop,
        FromBottom,
        FromCenter
    }
}
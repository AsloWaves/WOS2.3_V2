using UnityEngine;

namespace WOS.Environment
{
    /// <summary>
    /// Simple sprite-based wake particle for 2D naval game.
    /// This component should be added to a GameObject with SpriteRenderer for wake particles.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SimpleWakeSprite : MonoBehaviour
    {
        [Header("Default Wake Sprite Settings")]
        [Tooltip("Default sprite for wake particles (will be overridden by particle settings)")]
        [SerializeField] private Sprite defaultWakeSprite;

        [Tooltip("Default color for wake particles")]
        [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f, 0.8f);

        [Tooltip("Default scale for wake particles")]
        [SerializeField] private float defaultScale = 1f;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            // Get or add SpriteRenderer
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            // Apply default settings
            ApplyDefaultSettings();
        }

        private void ApplyDefaultSettings()
        {
            if (spriteRenderer == null) return;

            // Set default sprite if none assigned
            if (spriteRenderer.sprite == null && defaultWakeSprite != null)
            {
                spriteRenderer.sprite = defaultWakeSprite;
            }

            // Set default color
            spriteRenderer.color = defaultColor;

            // Set default scale
            transform.localScale = Vector3.one * defaultScale;

            // Configure sprite settings for wake particles
            spriteRenderer.sortingLayerName = "Default";
            spriteRenderer.sortingOrder = 0;

            // For 2D wake effects, we want additive or alpha blending
            // Note: This requires a material with appropriate blend mode
            // You can create a material with Sprites/Default shader and Alpha blend mode
        }

        /// <summary>
        /// Update sprite properties (called by CustomWakeParticle)
        /// </summary>
        public void UpdateSprite(Sprite sprite, Color color, float scale)
        {
            if (spriteRenderer == null) return;

            if (sprite != null)
                spriteRenderer.sprite = sprite;

            spriteRenderer.color = color;
            transform.localScale = Vector3.one * scale;
        }

        /// <summary>
        /// Set sprite visibility
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;
        }

        /// <summary>
        /// Get sprite bounds for collision/culling calculations
        /// </summary>
        public Bounds GetSpriteBounds()
        {
            return spriteRenderer != null ? spriteRenderer.bounds : new Bounds();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Apply settings in editor when values change
            if (Application.isPlaying) return;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            ApplyDefaultSettings();
        }
#endif
    }
}
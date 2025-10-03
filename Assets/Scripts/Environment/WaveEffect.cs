using UnityEngine;
using Unity.Mathematics;

namespace WOS.Environment
{
    /// <summary>
    /// Individual wave effect component for ambient ocean wave animations.
    /// Handles wave crest movement, foam effects, and ripple animations.
    /// </summary>
    public class WaveEffect : MonoBehaviour
    {
        [Header("Wave Animation")]
        [Tooltip("Type of wave effect this represents")]
        [SerializeField] private WaveType waveType = WaveType.Crest;

        [Tooltip("Base scale multiplier for the wave")]
        [Range(0.5f, 3f)]
        [SerializeField] private float scaleMultiplier = 1f;

        [Tooltip("Enable rotation animation")]
        [SerializeField] private bool enableRotation = false;

        [Tooltip("Rotation speed in degrees per second")]
        [Range(-90f, 90f)]
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Movement")]
        [Tooltip("Enable wave movement with wind")]
        [SerializeField] private bool enableMovement = true;

        [Tooltip("Movement speed multiplier")]
        [Range(0f, 2f)]
        [SerializeField] private float movementSpeed = 0.5f;

        [Tooltip("Random movement variation")]
        [Range(0f, 1f)]
        [SerializeField] private float movementVariation = 0.3f;

        [Header("Lifecycle")]
        [Tooltip("Enable fade in animation")]
        [SerializeField] private bool enableFadeIn = true;

        [Tooltip("Fade in duration")]
        [Range(0.5f, 5f)]
        [SerializeField] private float fadeInDuration = 2f;

        [Tooltip("Enable fade out animation")]
        [SerializeField] private bool enableFadeOut = true;

        [Tooltip("Fade out duration")]
        [Range(0.5f, 5f)]
        [SerializeField] private float fadeOutDuration = 3f;

        // Animation State
        private float age = 0f;
        private float lifetime = 15f;
        private float animationSpeed = 1f;
        private Vector3 baseScale;
        private Vector3 movementDirection;
        private float movementPhase;

        // Components
        private Renderer waveRenderer;
        private MaterialPropertyBlock propertyBlock;

        // Shader Properties
        private static readonly int AlphaProperty = Shader.PropertyToID("_Alpha");
        private static readonly int TimeProperty = Shader.PropertyToID("_Time");
        private static readonly int PhaseProperty = Shader.PropertyToID("_Phase");

        // Animation Curves
        private AnimationCurve scaleCurve;
        private AnimationCurve alphaCurve;

        private void Awake()
        {
            // Get components
            waveRenderer = GetComponent<Renderer>();
            if (waveRenderer != null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            // Store base scale
            baseScale = transform.localScale;

            // Create animation curves
            CreateAnimationCurves();
        }

        /// <summary>
        /// Initialize wave with animation parameters
        /// </summary>
        public void Initialize(float animSpeed, float waveLifetime)
        {
            animationSpeed = animSpeed;
            lifetime = waveLifetime;
            age = 0f;

            // Generate random movement direction
            float randomAngle = UnityEngine.Random.Range(0f, 360f);
            movementDirection = new Vector3(
                math.cos(math.radians(randomAngle)),
                0f,
                math.sin(math.radians(randomAngle))
            );

            // Random movement phase
            movementPhase = UnityEngine.Random.Range(0f, 2f * math.PI);

            // Apply scale variation
            float scaleVariation = UnityEngine.Random.Range(0.8f, 1.2f);
            transform.localScale = baseScale * scaleMultiplier * scaleVariation;

            // Initialize shader properties
            if (propertyBlock != null && waveRenderer != null)
            {
                propertyBlock.SetFloat(PhaseProperty, movementPhase);
                waveRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        /// <summary>
        /// Update wave animation
        /// </summary>
        public void UpdateWave(float deltaTime, Vector2 windVelocity)
        {
            age += deltaTime;

            // Update position movement
            if (enableMovement)
            {
                UpdateMovement(deltaTime, windVelocity);
            }

            // Update rotation
            if (enableRotation)
            {
                UpdateRotation(deltaTime);
            }

            // Update visual properties
            UpdateVisualProperties();
        }

        private void UpdateMovement(float deltaTime, Vector2 windVelocity)
        {
            // Combine wind velocity with random movement
            Vector3 windVector = new Vector3(windVelocity.x, 0f, windVelocity.y);
            Vector3 movement = (windVector + movementDirection * movementVariation) * movementSpeed;

            // Add sinusoidal variation
            float sineWave = math.sin(age * animationSpeed + movementPhase) * 0.5f;
            movement *= (1f + sineWave * 0.3f);

            // Apply movement
            transform.position += movement * deltaTime;
        }

        private void UpdateRotation(float deltaTime)
        {
            float rotation = rotationSpeed * deltaTime * animationSpeed;
            transform.Rotate(0f, rotation, 0f);
        }

        private void UpdateVisualProperties()
        {
            if (propertyBlock == null || waveRenderer == null) return;

            float normalizedAge = age / lifetime;

            // Calculate alpha based on lifecycle
            float alpha = CalculateAlpha(normalizedAge);

            // Calculate scale based on wave type
            float scaleMultiplier = CalculateScale(normalizedAge);

            // Update shader properties
            propertyBlock.SetFloat(AlphaProperty, alpha);
            propertyBlock.SetFloat(TimeProperty, age * animationSpeed);

            waveRenderer.SetPropertyBlock(propertyBlock);

            // Update transform scale
            Vector3 currentScale = baseScale * this.scaleMultiplier * scaleMultiplier;
            transform.localScale = currentScale;
        }

        private float CalculateAlpha(float normalizedAge)
        {
            if (alphaCurve != null)
            {
                return alphaCurve.Evaluate(normalizedAge);
            }

            // Default alpha calculation
            if (enableFadeIn && normalizedAge < fadeInDuration / lifetime)
            {
                return normalizedAge / (fadeInDuration / lifetime);
            }
            else if (enableFadeOut && normalizedAge > 1f - (fadeOutDuration / lifetime))
            {
                float fadeStart = 1f - (fadeOutDuration / lifetime);
                return 1f - ((normalizedAge - fadeStart) / (fadeOutDuration / lifetime));
            }

            return 1f;
        }

        private float CalculateScale(float normalizedAge)
        {
            if (scaleCurve != null)
            {
                return scaleCurve.Evaluate(normalizedAge);
            }

            // Default scale based on wave type
            switch (waveType)
            {
                case WaveType.Crest:
                    return 1f + math.sin(normalizedAge * math.PI) * 0.2f;

                case WaveType.Foam:
                    return 1f + math.sin(normalizedAge * 2f * math.PI) * 0.1f;

                case WaveType.Ripple:
                    return normalizedAge < 0.5f ? normalizedAge * 2f : 2f - normalizedAge * 2f;

                default:
                    return 1f;
            }
        }

        private void CreateAnimationCurves()
        {
            // Create scale curve based on wave type
            scaleCurve = new AnimationCurve();
            alphaCurve = new AnimationCurve();

            switch (waveType)
            {
                case WaveType.Crest:
                    // Wave crest: start small, grow, then fade
                    scaleCurve.AddKey(0f, 0.5f);
                    scaleCurve.AddKey(0.3f, 1.2f);
                    scaleCurve.AddKey(0.7f, 1f);
                    scaleCurve.AddKey(1f, 0.8f);

                    alphaCurve.AddKey(0f, 0f);
                    alphaCurve.AddKey(0.2f, 1f);
                    alphaCurve.AddKey(0.8f, 1f);
                    alphaCurve.AddKey(1f, 0f);
                    break;

                case WaveType.Foam:
                    // Foam: quick appearance, steady, then dissolve
                    scaleCurve.AddKey(0f, 0.8f);
                    scaleCurve.AddKey(0.1f, 1.1f);
                    scaleCurve.AddKey(0.5f, 1f);
                    scaleCurve.AddKey(1f, 1.2f);

                    alphaCurve.AddKey(0f, 0f);
                    alphaCurve.AddKey(0.1f, 1f);
                    alphaCurve.AddKey(0.7f, 1f);
                    alphaCurve.AddKey(1f, 0f);
                    break;

                case WaveType.Ripple:
                    // Ripple: expand outward, then fade
                    scaleCurve.AddKey(0f, 0.1f);
                    scaleCurve.AddKey(0.5f, 1f);
                    scaleCurve.AddKey(1f, 1.5f);

                    alphaCurve.AddKey(0f, 1f);
                    alphaCurve.AddKey(0.3f, 0.8f);
                    alphaCurve.AddKey(1f, 0f);
                    break;
            }

            // Smooth the curves
            for (int i = 0; i < scaleCurve.keys.Length; i++)
            {
                scaleCurve.SmoothTangents(i, 0.5f);
            }
            for (int i = 0; i < alphaCurve.keys.Length; i++)
            {
                alphaCurve.SmoothTangents(i, 0.5f);
            }
        }

        /// <summary>
        /// Reset wave to initial state for pooling
        /// </summary>
        public void Reset()
        {
            age = 0f;
            transform.localScale = baseScale;

            if (propertyBlock != null && waveRenderer != null)
            {
                propertyBlock.SetFloat(AlphaProperty, 0f);
                propertyBlock.SetFloat(TimeProperty, 0f);
                waveRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        /// <summary>
        /// Get current wave age
        /// </summary>
        public float GetAge()
        {
            return age;
        }

        /// <summary>
        /// Check if wave has reached end of lifetime
        /// </summary>
        public bool IsExpired()
        {
            return age >= lifetime;
        }

        /// <summary>
        /// Get current alpha value
        /// </summary>
        public float GetAlpha()
        {
            return CalculateAlpha(age / lifetime);
        }

        /// <summary>
        /// Set wave type and reinitialize animation curves
        /// </summary>
        public void SetWaveType(WaveType newType)
        {
            waveType = newType;
            CreateAnimationCurves();
        }

        private void OnDrawGizmosSelected()
        {
            // Draw wave direction
            if (enableMovement)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(transform.position, movementDirection * 20f);
            }

            // Draw age indicator
            Gizmos.color = Color.Lerp(Color.green, Color.red, age / lifetime);
            Gizmos.DrawWireSphere(transform.position, 2f);

            // Draw wave type indicator
            Vector3 labelPos = transform.position + Vector3.up * 5f;
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, $"{waveType}\nAge: {age:F1}s\nAlpha: {GetAlpha():F2}");
            #endif
        }
    }

    /// <summary>
    /// Types of wave effects
    /// </summary>
    public enum WaveType
    {
        Crest,      // Wave peaks and crests
        Foam,       // Foam patches and whitecaps
        Ripple      // Small surface ripples
    }
}
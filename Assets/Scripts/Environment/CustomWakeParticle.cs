using UnityEngine;
using Unity.Mathematics;
using WOS.Debugging;

namespace WOS.Environment
{
    /// <summary>
    /// Individual wake particle with custom physics and lifecycle management.
    /// Provides fine-grained control over wake behavior for naval realism.
    /// </summary>
    public class CustomWakeParticle : MonoBehaviour
    {
        [Header("Particle Properties")]
        [Tooltip("Current age of the particle in seconds")]
        public float age;

        [Tooltip("Maximum lifetime before particle despawns")]
        public float lifetime = 15f;

        [Tooltip("Current size multiplier")]
        public float size = 1f;

        [Tooltip("Alpha transparency (0-1)")]
        public float alpha = 1f;

        [Header("Physics")]
        [Tooltip("Current velocity in world space")]
        public Vector3 velocity;

        [Tooltip("Drag coefficient affecting particle slowdown")]
        public float drag = 0.5f;

        [Tooltip("Buoyancy effect (positive = rises, negative = sinks)")]
        public float buoyancy = 0.1f;

        [Tooltip("Random drift influence")]
        public float turbulence = 0.2f;

        // Visual Components
        private SpriteRenderer spriteRenderer;
        private Transform particleTransform;

        // Physics State
        private Vector3 initialVelocity;
        private float initialSize;
        private Color initialColor;
        private float turbulencePhase;

        // Performance
        private bool isActive = true;
        private static UnityEngine.Camera oceanCamera;

        private void Awake()
        {
            particleTransform = transform;
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                DebugManager.LogWarning(DebugCategory.Ocean, "No SpriteRenderer found, added one automatically", this);
            }

            // Find camera reference (cached statically for performance)
            if (oceanCamera == null)
                oceanCamera = UnityEngine.Camera.main;

            // Generate random turbulence phase
            turbulencePhase = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
        }

        /// <summary>
        /// Initialize particle with ship-specific wake properties
        /// </summary>
        public void Initialize(WakeParticleSettings settings, Vector3 spawnVelocity, Vector3 spawnPosition)
        {
            // Store initial state
            lifetime = settings.baseLifetime;
            initialSize = settings.baseSize;
            initialColor = settings.baseColor;
            initialVelocity = spawnVelocity;

            // Apply ship-specific modifications
            lifetime *= settings.lifetimeMultiplier;
            size = initialSize * settings.sizeMultiplier;
            drag = settings.dragCoefficient;
            buoyancy = settings.buoyancyEffect;
            turbulence = settings.turbulenceStrength;

            // Set initial properties
            velocity = spawnVelocity;
            age = 0f;
            alpha = 1f;

            // Position in world space
            particleTransform.position = spawnPosition;

            // Apply initial visual properties
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = settings.particleSprite;
                spriteRenderer.color = initialColor;
                particleTransform.localScale = Vector3.one * size;
            }

            isActive = true;
        }

        private void Update()
        {
            if (!isActive) return;

            // Update particle age
            age += Time.deltaTime;

            // Check if particle should be destroyed
            if (age >= lifetime)
            {
                DestroyParticle();
                return;
            }

            // Update physics
            UpdatePhysics();

            // Update visual properties
            UpdateVisuals();

            // Performance: Cull distant particles
            if (ShouldCullParticle())
            {
                DestroyParticle();
            }
        }

        private void UpdatePhysics()
        {
            float deltaTime = Time.deltaTime;

            // Apply drag (water resistance)
            velocity *= (1f - drag * deltaTime);

            // Apply buoyancy (vertical water displacement)
            velocity.y += buoyancy * deltaTime;

            // Apply turbulence (random water movement)
            if (turbulence > 0f)
            {
                float turbulenceTime = Time.time + turbulencePhase;
                Vector3 turbulenceForce = new Vector3(
                    Mathf.Sin(turbulenceTime * 0.7f) * turbulence,
                    Mathf.Cos(turbulenceTime * 0.5f) * turbulence * 0.5f,
                    Mathf.Sin(turbulenceTime * 0.9f) * turbulence
                ) * deltaTime;

                velocity += turbulenceForce;
            }

            // Apply velocity to position
            particleTransform.position += velocity * deltaTime;
        }

        private void UpdateVisuals()
        {
            if (spriteRenderer == null) return;

            // Calculate lifecycle progress (0 = spawn, 1 = death)
            float lifecycleProgress = age / lifetime;

            // Size evolution: start small, grow, then shrink
            float sizeProgress;
            if (lifecycleProgress < 0.3f)
            {
                // Growth phase (0-30% of lifetime)
                sizeProgress = Mathf.Lerp(0.2f, 1f, lifecycleProgress / 0.3f);
            }
            else if (lifecycleProgress < 0.7f)
            {
                // Stable phase (30-70% of lifetime)
                sizeProgress = 1f;
            }
            else
            {
                // Decay phase (70-100% of lifetime)
                sizeProgress = Mathf.Lerp(1f, 0.3f, (lifecycleProgress - 0.7f) / 0.3f);
            }

            // Apply size
            float currentSize = initialSize * size * sizeProgress;
            particleTransform.localScale = Vector3.one * currentSize;

            // Alpha fade: sharp fade in final 20% of lifetime
            if (lifecycleProgress > 0.8f)
            {
                alpha = Mathf.Lerp(1f, 0f, (lifecycleProgress - 0.8f) / 0.2f);
            }
            else
            {
                alpha = 1f;
            }

            // Apply color with alpha
            Color currentColor = initialColor;
            currentColor.a = alpha;
            spriteRenderer.color = currentColor;

            // Subtle rotation based on velocity
            if (velocity.magnitude > 0.1f)
            {
                float rotationSpeed = velocity.magnitude * 10f;
                particleTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            }
        }

        private bool ShouldCullParticle()
        {
            if (oceanCamera == null) return false;

            // Cull particles too far from camera
            float distanceToCamera = Vector3.Distance(particleTransform.position, oceanCamera.transform.position);
            return distanceToCamera > 2000f; // Configurable cull distance
        }

        /// <summary>
        /// Manually destroy this particle (returns to pool if using object pooling)
        /// </summary>
        public void DestroyParticle()
        {
            isActive = false;

            // This could be enhanced to return to object pool instead of destroying
            Destroy(gameObject);
        }

        /// <summary>
        /// Get current particle status for debugging
        /// </summary>
        public WakeParticleStatus GetStatus()
        {
            return new WakeParticleStatus
            {
                age = age,
                lifetime = lifetime,
                size = size,
                alpha = alpha,
                velocity = velocity,
                position = particleTransform.position,
                isActive = isActive
            };
        }

        /// <summary>
        /// Apply external force to particle (e.g., ship wash, wind)
        /// </summary>
        public void ApplyForce(Vector3 force)
        {
            velocity += force;
        }

        private void OnDrawGizmosSelected()
        {
            // Debug visualization
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, size);

            // Draw velocity vector
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, velocity);
        }
    }

    /// <summary>
    /// Configuration settings for wake particles
    /// </summary>
    [System.Serializable]
    public struct WakeParticleSettings
    {
        [Header("Base Properties")]
        public float baseLifetime;          // Base particle lifetime in seconds
        public float baseSize;              // Base particle size
        public Color baseColor;             // Base particle color
        public Sprite particleSprite;       // Sprite to use for particle

        [Header("Ship-Specific Modifiers")]
        public float lifetimeMultiplier;    // Ship-specific lifetime modifier
        public float sizeMultiplier;        // Ship-specific size modifier
        public float densityMultiplier;     // Ship-specific spawn rate modifier

        [Header("Physics")]
        public float dragCoefficient;       // Water resistance
        public float buoyancyEffect;        // Vertical displacement
        public float turbulenceStrength;   // Random movement intensity

        [Header("Visual Effects")]
        public AnimationCurve sizeOverLifetime;   // Size evolution curve
        public AnimationCurve alphaOverLifetime;  // Alpha evolution curve
        public bool enableRotation;               // Particle rotation based on velocity

        /// <summary>
        /// Create default wake particle settings
        /// </summary>
        public static WakeParticleSettings Default()
        {
            return new WakeParticleSettings
            {
                baseLifetime = 15f,
                baseSize = 2f,
                baseColor = new Color(1f, 1f, 1f, 0.8f),
                particleSprite = null, // Will need to be assigned

                lifetimeMultiplier = 1f,
                sizeMultiplier = 1f,
                densityMultiplier = 1f,

                dragCoefficient = 0.5f,
                buoyancyEffect = 0.1f,
                turbulenceStrength = 0.2f,

                sizeOverLifetime = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 1f),
                alphaOverLifetime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f),
                enableRotation = true
            };
        }
    }

    /// <summary>
    /// Runtime particle status for debugging and monitoring
    /// </summary>
    [System.Serializable]
    public struct WakeParticleStatus
    {
        public float age;
        public float lifetime;
        public float size;
        public float alpha;
        public Vector3 velocity;
        public Vector3 position;
        public bool isActive;

        public override string ToString()
        {
            return $"WakeParticle: Age={age:F1}s/{lifetime:F1}s, Size={size:F2}, Alpha={alpha:F2}, Vel={velocity.magnitude:F1}";
        }
    }
}
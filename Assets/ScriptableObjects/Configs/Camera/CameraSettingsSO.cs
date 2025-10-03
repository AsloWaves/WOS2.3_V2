using UnityEngine;
using Unity.Mathematics;

namespace WOS.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject configuration for camera behavior and settings.
    /// Provides smooth camera follow and zoom controls for naval gameplay.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "WOS/Camera Settings")]
    public class CameraSettingsSO : ScriptableObject
    {
        [Header("Follow Behavior")]
        [Tooltip("How smoothly camera follows target (higher = smoother, slower)")]
        [Range(1f, 20f)]
        public float followSmoothness = 8f;

        [Tooltip("Maximum distance camera can be from target")]
        [Range(5f, 50f)]
        public float maxFollowDistance = 25f;

        [Tooltip("Look-ahead distance when ship is moving")]
        [Range(0f, 20f)]
        public float lookAheadDistance = 8f;

        [Tooltip("How far ahead to look based on ship speed")]
        [Range(0f, 2f)]
        public float speedLookAheadFactor = 1.2f;

        [Header("Zoom Controls")]
        [Tooltip("Minimum camera zoom (closest view)")]
        [Range(2f, 10f)]
        public float minZoom = 4f;

        [Tooltip("Maximum camera zoom (furthest view)")]
        [Range(8f, 30f)]
        public float maxZoom = 18f;

        [Tooltip("Default camera zoom level")]
        [Range(4f, 20f)]
        public float defaultZoom = 8f;

        [Tooltip("Zoom speed multiplier")]
        [Range(0.5f, 5f)]
        public float zoomSpeed = 2f;

        [Tooltip("Smoothness of zoom transitions")]
        [Range(1f, 15f)]
        public float zoomSmoothness = 6f;

        [Header("Pan Controls")]
        [Tooltip("Camera pan speed with mouse/input")]
        [Range(1f, 20f)]
        public float panSpeed = 8f;

        [Tooltip("Maximum distance camera can pan from target")]
        [Range(10f, 100f)]
        public float maxPanDistance = 40f;

        [Tooltip("Speed at which camera returns to target")]
        [Range(1f, 10f)]
        public float returnToTargetSpeed = 3f;

        [Tooltip("Time before auto-return to target (0 = never)")]
        [Range(0f, 10f)]
        public float autoReturnDelay = 4f;

        [Header("Rotation & Orientation")]
        [Tooltip("Allow camera rotation around target")]
        public bool allowRotation = true;

        [Tooltip("Camera rotation speed")]
        [Range(10f, 180f)]
        public float rotationSpeed = 45f;

        [Tooltip("Smoothness of rotation changes")]
        [Range(1f, 15f)]
        public float rotationSmoothness = 8f;

        [Tooltip("Default camera rotation angle")]
        [Range(0f, 360f)]
        public float defaultRotation = 0f;

        [Header("Screen Shake")]
        [Tooltip("Enable screen shake effects")]
        public bool enableScreenShake = true;

        [Tooltip("Base screen shake intensity")]
        [Range(0f, 2f)]
        public float shakeIntensity = 0.5f;

        [Tooltip("How long shake effects last")]
        [Range(0.1f, 2f)]
        public float shakeDuration = 0.3f;

        [Tooltip("Shake frequency")]
        [Range(10f, 60f)]
        public float shakeFrequency = 30f;

        [Header("Speed-Based Effects")]
        [Tooltip("Increase camera shake based on ship speed")]
        public bool speedBasedShake = true;

        [Tooltip("Speed threshold for shake effects")]
        [Range(5f, 25f)]
        public float shakeSpeedThreshold = 15f;

        [Tooltip("Auto-zoom out when moving fast")]
        public bool speedBasedZoom = true;

        [Tooltip("How much to zoom out at max speed")]
        [Range(1f, 5f)]
        public float maxSpeedZoomFactor = 2f;

        [Header("Boundary Constraints")]
        [Tooltip("Enable camera boundary limits")]
        public bool useBoundaries = true;

        [Tooltip("World boundary rectangle (min x, min y, max x, max y)")]
        public Rect worldBounds = new Rect(-500f, -500f, 1000f, 1000f);

        [Tooltip("Soft boundary padding (camera starts slowing near edge)")]
        [Range(5f, 50f)]
        public float boundaryPadding = 20f;

        [Header("Visual Settings")]
        [Tooltip("Background color when outside world bounds")]
        public Color backgroundColor = new Color(0.1f, 0.2f, 0.4f, 1f);

        [Tooltip("Use orthographic projection")]
        public bool useOrthographic = true;

        [Tooltip("Camera depth (negative values)")]
        [Range(-50f, -5f)]
        public float cameraDepth = -10f;

        [Header("Performance")]
        [Tooltip("Update frequency for smooth operations")]
        [Range(30f, 120f)]
        public float updateFrequency = 60f;

        [Tooltip("Use fixed timestep for camera updates")]
        public bool useFixedUpdate = false;

        [Tooltip("Cull layers not visible to camera")]
        public LayerMask cullingMask = -1;

        /// <summary>
        /// Get the effective zoom based on ship speed
        /// </summary>
        public float GetEffectiveZoom(float baseZoom, float shipSpeed)
        {
            if (!speedBasedZoom) return baseZoom;

            float speedFactor = Mathf.InverseLerp(0f, shakeSpeedThreshold, shipSpeed);
            float zoomModifier = Mathf.Lerp(1f, maxSpeedZoomFactor, speedFactor);

            return Mathf.Clamp(baseZoom * zoomModifier, minZoom, maxZoom);
        }

        /// <summary>
        /// Calculate screen shake intensity based on ship speed
        /// </summary>
        public float GetShakeIntensity(float shipSpeed)
        {
            if (!enableScreenShake || !speedBasedShake)
                return enableScreenShake ? shakeIntensity : 0f;

            float speedFactor = Mathf.InverseLerp(shakeSpeedThreshold * 0.5f, shakeSpeedThreshold, shipSpeed);
            return shakeIntensity * speedFactor;
        }

        /// <summary>
        /// Get look-ahead position based on ship velocity
        /// </summary>
        public float3 GetLookAheadPosition(float3 shipPosition, float3 shipVelocity)
        {
            float speed = math.length(shipVelocity);
            float lookAhead = lookAheadDistance + (speed * speedLookAheadFactor);

            if (speed > 0.1f)
            {
                float3 direction = math.normalize(shipVelocity);
                return shipPosition + (direction * lookAhead);
            }

            return shipPosition;
        }

        /// <summary>
        /// Check if position is within world boundaries
        /// </summary>
        public bool IsWithinBounds(float3 position)
        {
            if (!useBoundaries) return true;

            return position.x >= worldBounds.xMin && position.x <= worldBounds.xMax &&
                   position.z >= worldBounds.yMin && position.z <= worldBounds.yMax;
        }

        /// <summary>
        /// Clamp position to world boundaries with soft padding
        /// </summary>
        public float3 ClampToBounds(float3 position)
        {
            if (!useBoundaries) return position;

            float paddedMinX = worldBounds.xMin + boundaryPadding;
            float paddedMaxX = worldBounds.xMax - boundaryPadding;
            float paddedMinZ = worldBounds.yMin + boundaryPadding;
            float paddedMaxZ = worldBounds.yMax - boundaryPadding;

            return new float3(
                Mathf.Clamp(position.x, paddedMinX, paddedMaxX),
                position.y,
                Mathf.Clamp(position.z, paddedMinZ, paddedMaxZ)
            );
        }

        private void OnValidate()
        {
            // Ensure logical value relationships
            maxZoom = Mathf.Max(maxZoom, minZoom + 1f);
            defaultZoom = Mathf.Clamp(defaultZoom, minZoom, maxZoom);
            maxFollowDistance = Mathf.Max(maxFollowDistance, lookAheadDistance + 5f);
            maxPanDistance = Mathf.Max(maxPanDistance, maxFollowDistance);
        }
    }
}
using UnityEngine;
using WOS.Debugging;
using WOS.Environment;

namespace WOS.ScriptableObjects
{
    /// <summary>
    /// Configuration for port system integration with existing WOS2.3_V2 systems.
    /// Manages camera behavior, input handling, LOD integration, and performance coordination.
    /// </summary>
    [CreateAssetMenu(fileName = "PortIntegrationConfig", menuName = "WOS/Environment/Port Integration Configuration")]
    public class PortIntegrationConfigurationSO : ScriptableObject
    {
        [System.Serializable]
        public enum CameraMode
        {
            Automatic,      // Camera decides based on context
            FollowOnly,     // Continue following ship
            FocusPort,      // Center on port when docked
            FreeLook        // Allow manual camera control
        }

        [System.Serializable]
        public enum InputPriority
        {
            ShipControl,    // Ship movement has priority
            PortUI,         // Port UI has priority
            Balanced        // Share input focus
        }

        [System.Serializable]
        public class CameraIntegrationSettings
        {
            [Header("Docking Camera Behavior")]
            [Tooltip("Camera mode when ship is docked")]
            public CameraMode dockedCameraMode = CameraMode.FocusPort;

            [Tooltip("Camera mode when ship is approaching port")]
            public CameraMode approachingCameraMode = CameraMode.Automatic;

            [Tooltip("Camera mode when ship is undocking")]
            public CameraMode undockingCameraMode = CameraMode.FollowOnly;

            [Header("Camera Movement")]
            [Tooltip("Smooth transition to port focus")]
            public bool smoothCameraTransition = true;

            [Tooltip("Time to transition camera to port focus")]
            [Range(0.5f, 5f)]
            public float cameraTransitionTime = 2f;

            [Tooltip("Maintain ship visibility when port-focused")]
            public bool keepShipVisible = true;

            [Tooltip("Zoom level adjustment when docked")]
            [Range(0.5f, 2f)]
            public float dockedZoomLevel = 1.2f;

            [Header("Look-Ahead Integration")]
            [Tooltip("Disable look-ahead when docked")]
            public bool disableLookAheadWhenDocked = true;

            [Tooltip("Use centered mode when docked")]
            public bool useCenteredModeWhenDocked = true;

            [Tooltip("Restore previous camera mode on undock")]
            public bool restorePreviousModeOnUndock = true;

            [Header("Manual Camera Control")]
            [Tooltip("Allow manual camera panning while docked")]
            public bool allowManualPanningWhenDocked = true;

            [Tooltip("Allow manual zoom while docked")]
            public bool allowManualZoomWhenDocked = true;

            [Tooltip("Return to ship focus key")]
            public KeyCode returnToShipKey = KeyCode.F;
        }

        [System.Serializable]
        public class InputIntegrationSettings
        {
            [Header("Input Priority")]
            [Tooltip("How to handle input priority conflicts")]
            public InputPriority inputPriorityMode = InputPriority.Balanced;

            [Tooltip("Lock ship movement when UI is active")]
            public bool lockShipMovementInUI = true;

            [Tooltip("Lock camera input when UI is active")]
            public bool lockCameraInputInUI = false;

            [Header("Input Mapping")]
            [Tooltip("Use existing input action map")]
            public bool useExistingInputMap = true;

            [Tooltip("Port UI toggle key")]
            public KeyCode portUIToggleKey = KeyCode.Tab;

            [Tooltip("Quick exit/close key")]
            public KeyCode quickExitKey = KeyCode.Escape;

            [Tooltip("Confirm action key")]
            public KeyCode confirmActionKey = KeyCode.Return;

            [Header("Input Feedback")]
            [Tooltip("Show input hints in UI")]
            public bool showInputHints = true;

            [Tooltip("Update input hints dynamically")]
            public bool dynamicInputHints = true;

            [Tooltip("Input hint display duration")]
            [Range(1f, 10f)]
            public float inputHintDuration = 3f;
        }

        [System.Serializable]
        public class LODIntegrationSettings
        {
            [Header("LOD System Integration")]
            [Tooltip("Integrate with EnvironmentLODManager")]
            public bool integrateWithLODManager = true;

            [Tooltip("Port LOD priority level")]
            [Range(0f, 1f)]
            public float portLODPriority = 0.8f;

            [Tooltip("Force high LOD when ship is docked")]
            public bool forceHighLODWhenDocked = true;

            [Tooltip("LOD update frequency for ports")]
            [Range(0.1f, 2f)]
            public float portLODUpdateFrequency = 0.5f;

            [Header("Performance Thresholds")]
            [Tooltip("Port visibility range")]
            [Range(500f, 3000f)]
            public float portVisibilityRange = 1500f;

            [Tooltip("High detail range")]
            [Range(100f, 800f)]
            public float highDetailRange = 400f;

            [Tooltip("Medium detail range")]
            [Range(200f, 1200f)]
            public float mediumDetailRange = 800f;

            [Tooltip("Low detail range")]
            [Range(400f, 2000f)]
            public float lowDetailRange = 1200f;

            [Header("Dynamic LOD")]
            [Tooltip("Adjust LOD based on performance")]
            public bool enableDynamicLOD = true;

            [Tooltip("Performance threshold for LOD reduction")]
            [Range(0.5f, 0.9f)]
            public float performanceThreshold = 0.7f;

            [Tooltip("LOD recovery threshold")]
            [Range(0.6f, 0.95f)]
            public float recoveryThreshold = 0.8f;
        }

        [System.Serializable]
        public class PerformanceIntegrationSettings
        {
            [Header("Performance Monitoring")]
            [Tooltip("Monitor performance impact of port systems")]
            public bool enablePerformanceMonitoring = true;

            [Tooltip("Frame time threshold for warnings")]
            [Range(16f, 50f)]
            public float frameTimeThreshold = 20f; // 50 FPS

            [Tooltip("Memory usage threshold (MB)")]
            [Range(50f, 500f)]
            public float memoryThreshold = 200f;

            [Header("Automatic Optimization")]
            [Tooltip("Auto-reduce quality on performance issues")]
            public bool enableAutoOptimization = true;

            [Tooltip("Disable non-essential effects on low performance")]
            public bool disableEffectsOnLowPerformance = true;

            [Tooltip("Reduce UI update frequency on low performance")]
            public bool reduceUIUpdatesOnLowPerformance = true;

            [Header("Performance Thresholds")]
            [Tooltip("Performance threshold for optimization activation")]
            [Range(0.5f, 0.9f)]
            public float performanceThreshold = 0.7f;

            [Tooltip("Performance recovery threshold")]
            [Range(0.6f, 0.95f)]
            public float recoveryThreshold = 0.8f;

            [Header("Quality Scaling")]
            [Tooltip("Particle effect quality scaling")]
            [Range(0.1f, 2f)]
            public float particleQualityScale = 1f;

            [Tooltip("Audio quality scaling")]
            [Range(0.1f, 2f)]
            public float audioQualityScale = 1f;

            [Tooltip("UI animation quality scaling")]
            [Range(0.1f, 2f)]
            public float uiAnimationQualityScale = 1f;
        }

        [System.Serializable]
        public class SaveSystemIntegrationSettings
        {
            [Header("Save Integration")]
            [Tooltip("Auto-save when docking")]
            public bool autoSaveOnDocking = true;

            [Tooltip("Auto-save when undocking")]
            public bool autoSaveOnUndocking = false;

            [Tooltip("Save port service transactions")]
            public bool saveServiceTransactions = true;

            [Tooltip("Save docking history")]
            public bool saveDockingHistory = true;

            [Header("Save Timing")]
            [Tooltip("Delay before auto-save (seconds)")]
            [Range(0f, 30f)]
            public float autoSaveDelay = 3f;

            [Tooltip("Maximum auto-saves per session")]
            [Range(1, 50)]
            public int maxAutoSavesPerSession = 20;

            [Header("Data Persistence")]
            [Tooltip("Persist port reputation")]
            public bool persistPortReputation = true;

            [Tooltip("Persist service availability")]
            public bool persistServiceAvailability = false;

            [Tooltip("Persist economic data")]
            public bool persistEconomicData = true;
        }

        [System.Serializable]
        public class DebugIntegrationSettings
        {
            [Header("Debug Integration")]
            [Tooltip("Use WOS debugging system")]
            public bool useWOSDebugging = true;

            [Tooltip("Debug category for port systems")]
            public DebugCategory portDebugCategory = DebugCategory.Environment;

            [Tooltip("Log level for port operations")]
            public int logLevel = 1; // 0=errors, 1=warnings, 2=info, 3=verbose

            [Header("Visual Debug")]
            [Tooltip("Show debug gizmos in scene view")]
            public bool showDebugGizmos = true;

            [Tooltip("Show performance metrics in game")]
            public bool showPerformanceMetrics = false;

            [Tooltip("Debug UI overlay")]
            public bool enableDebugUI = false;

            [Header("Performance Profiling")]
            [Tooltip("Enable detailed profiling")]
            public bool enableDetailedProfiling = false;

            [Tooltip("Profile port systems separately")]
            public bool profilePortSystemsSeparately = true;

            [Tooltip("Log performance warnings")]
            public bool logPerformanceWarnings = true;
        }

        [Header("Camera Integration")]
        [Tooltip("Camera behavior integration settings")]
        public CameraIntegrationSettings cameraIntegration = new CameraIntegrationSettings();

        [Header("Input Integration")]
        [Tooltip("Input system integration settings")]
        public InputIntegrationSettings inputIntegration = new InputIntegrationSettings();

        [Header("LOD Integration")]
        [Tooltip("Level of detail system integration")]
        public LODIntegrationSettings lodIntegration = new LODIntegrationSettings();

        [Header("Performance Integration")]
        [Tooltip("Performance monitoring and optimization")]
        public PerformanceIntegrationSettings performanceIntegration = new PerformanceIntegrationSettings();

        [Header("Save System Integration")]
        [Tooltip("Save system integration settings")]
        public SaveSystemIntegrationSettings saveSystemIntegration = new SaveSystemIntegrationSettings();

        [Header("Debug Integration")]
        [Tooltip("Debug and profiling integration")]
        public DebugIntegrationSettings debugIntegration = new DebugIntegrationSettings();

        [Header("System Compatibility")]
        [Tooltip("Compatible Unity version")]
        public string targetUnityVersion = "6000.0.55f1";

        [Tooltip("Required URP version")]
        public string requiredURPVersion = "17.0.3";

        [Tooltip("Minimum required memory (MB)")]
        [Range(100f, 2000f)]
        public float minimumRequiredMemory = 500f;

        /// <summary>
        /// Get camera mode for current port interaction state
        /// </summary>
        public CameraMode GetCameraModeForState(string portState)
        {
            return portState.ToLower() switch
            {
                "docked" => cameraIntegration.dockedCameraMode,
                "approaching" => cameraIntegration.approachingCameraMode,
                "undocking" => cameraIntegration.undockingCameraMode,
                _ => CameraMode.Automatic
            };
        }

        /// <summary>
        /// Check if input should be locked for current state
        /// </summary>
        public bool ShouldLockInput(string inputType, bool uiActive)
        {
            if (!uiActive) return false;

            return inputType.ToLower() switch
            {
                "ship" => inputIntegration.lockShipMovementInUI,
                "camera" => inputIntegration.lockCameraInputInUI,
                _ => false
            };
        }

        /// <summary>
        /// Get LOD level for distance and performance
        /// </summary>
        public LODLevel GetLODForDistance(float distance, float performanceMultiplier = 1f)
        {
            float adjustedHighRange = lodIntegration.highDetailRange * performanceMultiplier;
            float adjustedMediumRange = lodIntegration.mediumDetailRange * performanceMultiplier;
            float adjustedLowRange = lodIntegration.lowDetailRange * performanceMultiplier;

            if (distance <= adjustedHighRange)
                return LODLevel.High;
            else if (distance <= adjustedMediumRange)
                return LODLevel.Medium;
            else if (distance <= adjustedLowRange)
                return LODLevel.Low;
            else if (distance <= lodIntegration.portVisibilityRange)
                return LODLevel.VeryLow;
            else
                return LODLevel.Culled;
        }

        /// <summary>
        /// Check if performance optimization should be applied
        /// </summary>
        public bool ShouldApplyPerformanceOptimization(float currentFrameTime, float memoryUsage)
        {
            if (!performanceIntegration.enableAutoOptimization) return false;

            bool frameTimeExceeded = currentFrameTime > performanceIntegration.frameTimeThreshold;
            bool memoryExceeded = memoryUsage > performanceIntegration.memoryThreshold;

            return frameTimeExceeded || memoryExceeded;
        }

        /// <summary>
        /// Get quality scale for current performance
        /// </summary>
        public float GetQualityScaleForPerformance(string qualityType, float performanceRatio)
        {
            if (performanceRatio >= performanceIntegration.recoveryThreshold)
                return 1f;

            float scaleFactor = Mathf.Clamp01(performanceRatio / performanceIntegration.performanceThreshold);

            return qualityType.ToLower() switch
            {
                "particles" => Mathf.Lerp(0.3f, performanceIntegration.particleQualityScale, scaleFactor),
                "audio" => Mathf.Lerp(0.5f, performanceIntegration.audioQualityScale, scaleFactor),
                "ui" => Mathf.Lerp(0.7f, performanceIntegration.uiAnimationQualityScale, scaleFactor),
                _ => scaleFactor
            };
        }

        /// <summary>
        /// Check if auto-save should trigger
        /// </summary>
        public bool ShouldAutoSave(string saveEvent, int currentSaveCount)
        {
            if (currentSaveCount >= saveSystemIntegration.maxAutoSavesPerSession) return false;

            return saveEvent.ToLower() switch
            {
                "docking" => saveSystemIntegration.autoSaveOnDocking,
                "undocking" => saveSystemIntegration.autoSaveOnUndocking,
                "service_transaction" => saveSystemIntegration.saveServiceTransactions,
                _ => false
            };
        }

        /// <summary>
        /// Get debug log level for message type
        /// </summary>
        public bool ShouldLogMessage(string messageType)
        {
            if (!debugIntegration.useWOSDebugging) return false;

            return messageType.ToLower() switch
            {
                "error" => debugIntegration.logLevel >= 0,
                "warning" => debugIntegration.logLevel >= 1,
                "info" => debugIntegration.logLevel >= 2,
                "verbose" => debugIntegration.logLevel >= 3,
                "performance" => debugIntegration.logPerformanceWarnings && debugIntegration.logLevel >= 1,
                _ => debugIntegration.logLevel >= 2
            };
        }

        /// <summary>
        /// Validate configuration and check compatibility
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            // Check camera transition time
            if (cameraIntegration.cameraTransitionTime <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Camera transition time must be greater than 0!", this);
                isValid = false;
            }

            // Check LOD ranges
            if (lodIntegration.highDetailRange >= lodIntegration.mediumDetailRange)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "High detail range must be less than medium detail range!", this);
                isValid = false;
            }

            if (lodIntegration.mediumDetailRange >= lodIntegration.lowDetailRange)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Medium detail range must be less than low detail range!", this);
                isValid = false;
            }

            // Check performance thresholds
            if (performanceIntegration.performanceThreshold >= performanceIntegration.recoveryThreshold)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Performance threshold must be less than recovery threshold!", this);
                isValid = false;
            }

            // Check frame time threshold
            if (performanceIntegration.frameTimeThreshold <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Frame time threshold must be greater than 0!", this);
                isValid = false;
            }

            // Check memory requirements
            if (minimumRequiredMemory <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Minimum required memory must be greater than 0!", this);
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Get integration statistics for monitoring
        /// </summary>
        public IntegrationStats GetIntegrationStats(float currentFrameTime, float memoryUsage, LODLevel currentLOD)
        {
            return new IntegrationStats
            {
                frameTimeThresholdExceeded = currentFrameTime > performanceIntegration.frameTimeThreshold,
                memoryThresholdExceeded = memoryUsage > performanceIntegration.memoryThreshold,
                currentLODLevel = currentLOD,
                performanceOptimizationActive = ShouldApplyPerformanceOptimization(currentFrameTime, memoryUsage),
                particleQualityScale = GetQualityScaleForPerformance("particles", currentFrameTime / performanceIntegration.frameTimeThreshold),
                audioQualityScale = GetQualityScaleForPerformance("audio", currentFrameTime / performanceIntegration.frameTimeThreshold),
                uiQualityScale = GetQualityScaleForPerformance("ui", currentFrameTime / performanceIntegration.frameTimeThreshold)
            };
        }

        private void OnValidate()
        {
            // Validate camera integration
            if (cameraIntegration != null)
            {
                cameraIntegration.cameraTransitionTime = Mathf.Max(0.1f, cameraIntegration.cameraTransitionTime);
                cameraIntegration.dockedZoomLevel = Mathf.Clamp(cameraIntegration.dockedZoomLevel, 0.1f, 5f);
            }

            // Validate input integration
            if (inputIntegration != null)
            {
                inputIntegration.inputHintDuration = Mathf.Max(0.5f, inputIntegration.inputHintDuration);
            }

            // Validate LOD integration
            if (lodIntegration != null)
            {
                lodIntegration.portLODPriority = Mathf.Clamp01(lodIntegration.portLODPriority);
                lodIntegration.portLODUpdateFrequency = Mathf.Max(0.1f, lodIntegration.portLODUpdateFrequency);
                lodIntegration.highDetailRange = Mathf.Max(50f, lodIntegration.highDetailRange);
                lodIntegration.mediumDetailRange = Mathf.Max(lodIntegration.highDetailRange + 50f, lodIntegration.mediumDetailRange);
                lodIntegration.lowDetailRange = Mathf.Max(lodIntegration.mediumDetailRange + 50f, lodIntegration.lowDetailRange);
                lodIntegration.portVisibilityRange = Mathf.Max(lodIntegration.lowDetailRange + 100f, lodIntegration.portVisibilityRange);
                lodIntegration.performanceThreshold = Mathf.Clamp(lodIntegration.performanceThreshold, 0.1f, 0.95f);
                lodIntegration.recoveryThreshold = Mathf.Max(lodIntegration.performanceThreshold + 0.05f, lodIntegration.recoveryThreshold);
            }

            // Validate performance integration
            if (performanceIntegration != null)
            {
                performanceIntegration.frameTimeThreshold = Mathf.Max(8f, performanceIntegration.frameTimeThreshold);
                performanceIntegration.memoryThreshold = Mathf.Max(50f, performanceIntegration.memoryThreshold);
                performanceIntegration.particleQualityScale = Mathf.Max(0.1f, performanceIntegration.particleQualityScale);
                performanceIntegration.audioQualityScale = Mathf.Max(0.1f, performanceIntegration.audioQualityScale);
                performanceIntegration.uiAnimationQualityScale = Mathf.Max(0.1f, performanceIntegration.uiAnimationQualityScale);
            }

            // Validate save system integration
            if (saveSystemIntegration != null)
            {
                saveSystemIntegration.autoSaveDelay = Mathf.Max(0f, saveSystemIntegration.autoSaveDelay);
                saveSystemIntegration.maxAutoSavesPerSession = Mathf.Max(1, saveSystemIntegration.maxAutoSavesPerSession);
            }

            // Validate debug integration
            if (debugIntegration != null)
            {
                debugIntegration.logLevel = Mathf.Clamp(debugIntegration.logLevel, 0, 3);
            }

            // Validate system requirements
            minimumRequiredMemory = Mathf.Max(100f, minimumRequiredMemory);
        }

        [System.Serializable]
        public struct IntegrationStats
        {
            public bool frameTimeThresholdExceeded;
            public bool memoryThresholdExceeded;
            public LODLevel currentLODLevel;
            public bool performanceOptimizationActive;
            public float particleQualityScale;
            public float audioQualityScale;
            public float uiQualityScale;

            public override string ToString()
            {
                return $"Integration: LOD={currentLODLevel}, Optimizing={performanceOptimizationActive}, " +
                       $"Quality: P={particleQualityScale:F2}, A={audioQualityScale:F2}, UI={uiQualityScale:F2}";
            }
        }
    }
}
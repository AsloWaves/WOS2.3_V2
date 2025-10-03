using UnityEngine;
using UnityEngine.Rendering.Universal;
using WOS.Debugging;

namespace WOS.ScriptableObjects
{
    /// <summary>
    /// Configuration for port visual effects including particles, lighting, and ambiance.
    /// Optimized for URP 2D rendering pipeline with performance considerations.
    /// </summary>
    [CreateAssetMenu(fileName = "PortVisualConfig", menuName = "WOS/Environment/Port Visual Configuration")]
    public class PortVisualConfigurationSO : ScriptableObject
    {
        [System.Serializable]
        public enum WeatherCondition
        {
            Clear,
            Overcast,
            Light_Rain,
            Heavy_Rain,
            Storm,
            Fog,
            Snow
        }

        [System.Serializable]
        public enum TimeOfDay
        {
            Dawn,
            Morning,
            Noon,
            Afternoon,
            Dusk,
            Night,
            Midnight
        }

        [Header("Water Effects")]
        [Tooltip("Prefab for water splash effects")]
        public GameObject waterSplashPrefab;

        [Tooltip("Number of water splash effects to spawn")]
        [Range(1, 20)]
        public int waterEffectCount = 8;

        [Tooltip("Intensity of water effects")]
        [Range(0.1f, 5f)]
        public float waterEffectIntensity = 2f;

        [Tooltip("Prefab for foam effects near shore")]
        public GameObject foamEffectPrefab;

        [Tooltip("Number of foam effects to spawn")]
        [Range(1, 15)]
        public int foamEffectCount = 6;

        [Header("Atmospheric Effects")]
        [Tooltip("Prefab for mist/fog effects")]
        public GameObject mistEffectPrefab;

        [Tooltip("Intensity of atmospheric effects")]
        [Range(0.1f, 3f)]
        public float atmosphericEffectIntensity = 1.5f;

        [Tooltip("Prefab for seagull effects (optional)")]
        public GameObject seagullEffectPrefab;

        [Tooltip("Number of seagulls in the area")]
        [Range(0, 10)]
        public int seagullCount = 3;

        [Header("Structural Effects")]
        [Tooltip("Prefab for smoke effects from buildings")]
        public GameObject smokeEffectPrefab;

        [Tooltip("Number of smoke sources")]
        [Range(0, 8)]
        public int smokeSourceCount = 2;

        [Tooltip("Intensity of structural effects")]
        [Range(0.1f, 3f)]
        public float structuralEffectIntensity = 1f;

        [Header("Lighting Configuration")]
        [Tooltip("Ambient light color during day")]
        public Color dayLightColor = new Color(1f, 0.95f, 0.8f, 1f);

        [Tooltip("Ambient light color during night")]
        public Color nightLightColor = new Color(0.3f, 0.4f, 0.7f, 1f);

        [Tooltip("Harbor light intensity multiplier")]
        [Range(0.1f, 3f)]
        public float harborLightIntensity = 1.2f;

        [Tooltip("Speed of time of day changes")]
        [Range(0f, 0.01f)]
        public float timeOfDaySpeed = 0.001f;

        [Header("Weather Effects")]
        [Tooltip("Enable dynamic weather effects")]
        public bool enableWeatherEffects = true;

        [Tooltip("Weather change frequency (lower = more frequent)")]
        [Range(0.001f, 0.1f)]
        public float weatherChangeFrequency = 0.01f;

        [Tooltip("Rain particle effect prefab")]
        public GameObject rainEffectPrefab;

        [Tooltip("Storm particle effect prefab")]
        public GameObject stormEffectPrefab;

        [Tooltip("Snow particle effect prefab")]
        public GameObject snowEffectPrefab;

        [Header("Audio Configuration")]
        [Tooltip("Ambient harbor sound (waves, seagulls, etc.)")]
        public AudioClip ambientHarborSound;

        [Tooltip("Maximum volume for ambient sounds")]
        [Range(0f, 1f)]
        public float maxAmbientVolume = 0.7f;

        [Tooltip("Rain sound effect")]
        public AudioClip rainAmbientSound;

        [Tooltip("Storm wind sound effect")]
        public AudioClip stormWindSound;

        [Tooltip("Harbor bell sound for docking")]
        public AudioClip harborBellSound;

        [Header("Performance Settings")]
        [Tooltip("Maximum particle count for all effects")]
        [Range(100, 5000)]
        public int maxParticleCount = 2000;

        [Tooltip("LOD distance for effect quality reduction")]
        [Range(100f, 1000f)]
        public float lodDistance = 400f;

        [Tooltip("Cull effects beyond this distance")]
        [Range(200f, 1500f)]
        public float cullDistance = 800f;

        [Tooltip("Update frequency for effect management")]
        [Range(0.1f, 2f)]
        public float effectUpdateInterval = 0.5f;

        [Header("Color Schemes")]
        [Tooltip("Color scheme for different times of day")]
        public TimeOfDayColors[] timeOfDayColorSchemes = new TimeOfDayColors[7];

        [Tooltip("Color scheme for different weather conditions")]
        public WeatherColors[] weatherColorSchemes = new WeatherColors[7];

        [Header("Dynamic Elements")]
        [Tooltip("Enable dynamic flags and banners")]
        public bool enableDynamicFlags = true;

        [Tooltip("Flag animation speed")]
        [Range(0.1f, 5f)]
        public float flagAnimationSpeed = 1.5f;

        [Tooltip("Enable dynamic water reflections")]
        public bool enableWaterReflections = true;

        [Tooltip("Water reflection quality")]
        [Range(0.1f, 2f)]
        public float waterReflectionQuality = 1f;

        [Header("URP 2D Specific")]
        [Tooltip("Use 2D lighting system")]
        public bool use2DLighting = true;

        [Tooltip("2D light layer mask")]
        public int lightLayerMask = 1;

        [Tooltip("Shadow casting for harbor structures")]
        public bool enableShadowCasting = true;

        [Tooltip("Shadow transparency")]
        [Range(0f, 1f)]
        public float shadowTransparency = 0.3f;

        [System.Serializable]
        public struct TimeOfDayColors
        {
            public TimeOfDay timeOfDay;
            public Color ambientColor;
            public Color lightColor;
            public Color fogColor;
            public Color waterColor;
        }

        [System.Serializable]
        public struct WeatherColors
        {
            public WeatherCondition weather;
            public Color ambientModifier;
            public Color lightModifier;
            public Color fogTint;
            public float visibilityMultiplier;
        }

        /// <summary>
        /// Get color scheme for current time of day
        /// </summary>
        public TimeOfDayColors GetTimeOfDayColors(TimeOfDay timeOfDay)
        {
            foreach (var colorScheme in timeOfDayColorSchemes)
            {
                if (colorScheme.timeOfDay == timeOfDay)
                    return colorScheme;
            }

            // Return default if not found
            return new TimeOfDayColors
            {
                timeOfDay = timeOfDay,
                ambientColor = Color.white,
                lightColor = Color.white,
                fogColor = Color.gray,
                waterColor = Color.blue
            };
        }

        /// <summary>
        /// Get color modifiers for current weather
        /// </summary>
        public WeatherColors GetWeatherColors(WeatherCondition weather)
        {
            foreach (var colorScheme in weatherColorSchemes)
            {
                if (colorScheme.weather == weather)
                    return colorScheme;
            }

            // Return default if not found
            return new WeatherColors
            {
                weather = weather,
                ambientModifier = Color.white,
                lightModifier = Color.white,
                fogTint = Color.clear,
                visibilityMultiplier = 1f
            };
        }

        /// <summary>
        /// Calculate effect intensity based on distance and LOD
        /// </summary>
        public float GetEffectIntensityForDistance(float distance)
        {
            if (distance <= lodDistance)
                return 1f;
            else if (distance <= cullDistance)
                return Mathf.Lerp(1f, 0.2f, (distance - lodDistance) / (cullDistance - lodDistance));
            else
                return 0f;
        }

        /// <summary>
        /// Get particle count limit based on performance settings
        /// </summary>
        public int GetParticleCountForLOD(float lodIntensity)
        {
            return Mathf.RoundToInt(maxParticleCount * lodIntensity);
        }

        /// <summary>
        /// Check if weather effects should be active
        /// </summary>
        public bool ShouldShowWeatherEffects(WeatherCondition weather)
        {
            if (!enableWeatherEffects) return false;

            return weather switch
            {
                WeatherCondition.Clear => false,
                WeatherCondition.Overcast => false,
                WeatherCondition.Light_Rain => true,
                WeatherCondition.Heavy_Rain => true,
                WeatherCondition.Storm => true,
                WeatherCondition.Fog => true,
                WeatherCondition.Snow => true,
                _ => false
            };
        }

        /// <summary>
        /// Get weather effect prefab for current conditions
        /// </summary>
        public GameObject GetWeatherEffectPrefab(WeatherCondition weather)
        {
            return weather switch
            {
                WeatherCondition.Light_Rain or WeatherCondition.Heavy_Rain => rainEffectPrefab,
                WeatherCondition.Storm => stormEffectPrefab,
                WeatherCondition.Snow => snowEffectPrefab,
                _ => null
            };
        }

        /// <summary>
        /// Get ambient audio clip for current weather
        /// </summary>
        public AudioClip GetWeatherAmbientSound(WeatherCondition weather)
        {
            return weather switch
            {
                WeatherCondition.Light_Rain or WeatherCondition.Heavy_Rain => rainAmbientSound,
                WeatherCondition.Storm => stormWindSound,
                _ => ambientHarborSound
            };
        }

        /// <summary>
        /// Calculate lighting settings for time and weather
        /// </summary>
        public LightingSettings CalculateLightingSettings(TimeOfDay timeOfDay, WeatherCondition weather)
        {
            var timeColors = GetTimeOfDayColors(timeOfDay);
            var weatherColors = GetWeatherColors(weather);

            return new LightingSettings
            {
                ambientColor = timeColors.ambientColor * weatherColors.ambientModifier,
                lightColor = timeColors.lightColor * weatherColors.lightModifier,
                lightIntensity = harborLightIntensity * weatherColors.visibilityMultiplier,
                fogColor = Color.Lerp(timeColors.fogColor, weatherColors.fogTint, 0.5f),
                shadowIntensity = enableShadowCasting ? shadowTransparency * weatherColors.visibilityMultiplier : 0f
            };
        }

        /// <summary>
        /// Validate configuration and log warnings
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            // Check effect counts
            if (waterEffectCount <= 0 || foamEffectCount <= 0)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Effect counts must be greater than 0!", this);
                isValid = false;
            }

            // Check intensity values
            if (waterEffectIntensity <= 0f || atmosphericEffectIntensity <= 0f || structuralEffectIntensity <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Effect intensities must be greater than 0!", this);
                isValid = false;
            }

            // Check distance settings
            if (lodDistance >= cullDistance)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "LOD distance must be less than cull distance!", this);
                isValid = false;
            }

            // Check performance settings
            if (maxParticleCount <= 0)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Max particle count must be greater than 0!", this);
                isValid = false;
            }

            // Warn about missing prefabs
            if (waterSplashPrefab == null)
                DebugManager.LogWarning(DebugCategory.Environment, "Water splash prefab not assigned!", this);

            if (ambientHarborSound == null)
                DebugManager.LogWarning(DebugCategory.Environment, "Ambient harbor sound not assigned!", this);

            return isValid;
        }

        private void OnValidate()
        {
            // Ensure effect counts are positive
            waterEffectCount = Mathf.Max(1, waterEffectCount);
            foamEffectCount = Mathf.Max(1, foamEffectCount);
            seagullCount = Mathf.Max(0, seagullCount);
            smokeSourceCount = Mathf.Max(0, smokeSourceCount);

            // Ensure intensities are positive
            waterEffectIntensity = Mathf.Max(0.1f, waterEffectIntensity);
            atmosphericEffectIntensity = Mathf.Max(0.1f, atmosphericEffectIntensity);
            structuralEffectIntensity = Mathf.Max(0.1f, structuralEffectIntensity);
            harborLightIntensity = Mathf.Max(0.1f, harborLightIntensity);

            // Ensure distances are valid
            lodDistance = Mathf.Max(50f, lodDistance);
            cullDistance = Mathf.Max(lodDistance + 50f, cullDistance);

            // Ensure performance settings are reasonable
            maxParticleCount = Mathf.Max(100, maxParticleCount);
            effectUpdateInterval = Mathf.Clamp(effectUpdateInterval, 0.1f, 5f);

            // Ensure speed settings are reasonable
            timeOfDaySpeed = Mathf.Max(0f, timeOfDaySpeed);
            weatherChangeFrequency = Mathf.Clamp(weatherChangeFrequency, 0.001f, 1f);
            flagAnimationSpeed = Mathf.Max(0.1f, flagAnimationSpeed);

            // Ensure volume and quality settings are valid
            maxAmbientVolume = Mathf.Clamp01(maxAmbientVolume);
            waterReflectionQuality = Mathf.Max(0.1f, waterReflectionQuality);
            shadowTransparency = Mathf.Clamp01(shadowTransparency);

            // Initialize color schemes if empty
            if (timeOfDayColorSchemes == null || timeOfDayColorSchemes.Length != 7)
            {
                InitializeDefaultTimeOfDayColors();
            }

            if (weatherColorSchemes == null || weatherColorSchemes.Length != 7)
            {
                InitializeDefaultWeatherColors();
            }
        }

        private void InitializeDefaultTimeOfDayColors()
        {
            timeOfDayColorSchemes = new TimeOfDayColors[7];

            timeOfDayColorSchemes[0] = new TimeOfDayColors
            {
                timeOfDay = TimeOfDay.Dawn,
                ambientColor = new Color(1f, 0.8f, 0.6f),
                lightColor = new Color(1f, 0.9f, 0.7f),
                fogColor = new Color(0.9f, 0.7f, 0.5f),
                waterColor = new Color(0.3f, 0.5f, 0.8f)
            };

            timeOfDayColorSchemes[1] = new TimeOfDayColors
            {
                timeOfDay = TimeOfDay.Morning,
                ambientColor = new Color(1f, 0.95f, 0.85f),
                lightColor = new Color(1f, 0.95f, 0.8f),
                fogColor = new Color(0.8f, 0.8f, 0.8f),
                waterColor = new Color(0.2f, 0.6f, 0.9f)
            };

            timeOfDayColorSchemes[2] = new TimeOfDayColors
            {
                timeOfDay = TimeOfDay.Noon,
                ambientColor = Color.white,
                lightColor = Color.white,
                fogColor = new Color(0.7f, 0.7f, 0.7f),
                waterColor = new Color(0.1f, 0.5f, 1f)
            };

            timeOfDayColorSchemes[3] = new TimeOfDayColors
            {
                timeOfDay = TimeOfDay.Afternoon,
                ambientColor = new Color(1f, 0.9f, 0.8f),
                lightColor = new Color(1f, 0.9f, 0.7f),
                fogColor = new Color(0.8f, 0.7f, 0.6f),
                waterColor = new Color(0.2f, 0.4f, 0.8f)
            };

            timeOfDayColorSchemes[4] = new TimeOfDayColors
            {
                timeOfDay = TimeOfDay.Dusk,
                ambientColor = new Color(0.9f, 0.6f, 0.4f),
                lightColor = new Color(1f, 0.7f, 0.4f),
                fogColor = new Color(0.7f, 0.5f, 0.3f),
                waterColor = new Color(0.3f, 0.3f, 0.6f)
            };

            timeOfDayColorSchemes[5] = new TimeOfDayColors
            {
                timeOfDay = TimeOfDay.Night,
                ambientColor = new Color(0.3f, 0.4f, 0.7f),
                lightColor = new Color(0.8f, 0.9f, 1f),
                fogColor = new Color(0.2f, 0.3f, 0.5f),
                waterColor = new Color(0.1f, 0.2f, 0.4f)
            };

            timeOfDayColorSchemes[6] = new TimeOfDayColors
            {
                timeOfDay = TimeOfDay.Midnight,
                ambientColor = new Color(0.2f, 0.3f, 0.6f),
                lightColor = new Color(0.7f, 0.8f, 1f),
                fogColor = new Color(0.1f, 0.2f, 0.4f),
                waterColor = new Color(0.05f, 0.1f, 0.3f)
            };
        }

        private void InitializeDefaultWeatherColors()
        {
            weatherColorSchemes = new WeatherColors[7];

            weatherColorSchemes[0] = new WeatherColors
            {
                weather = WeatherCondition.Clear,
                ambientModifier = Color.white,
                lightModifier = Color.white,
                fogTint = Color.clear,
                visibilityMultiplier = 1f
            };

            weatherColorSchemes[1] = new WeatherColors
            {
                weather = WeatherCondition.Overcast,
                ambientModifier = new Color(0.8f, 0.8f, 0.9f),
                lightModifier = new Color(0.7f, 0.7f, 0.8f),
                fogTint = new Color(0.6f, 0.6f, 0.7f),
                visibilityMultiplier = 0.8f
            };

            weatherColorSchemes[2] = new WeatherColors
            {
                weather = WeatherCondition.Light_Rain,
                ambientModifier = new Color(0.7f, 0.7f, 0.8f),
                lightModifier = new Color(0.6f, 0.6f, 0.7f),
                fogTint = new Color(0.5f, 0.5f, 0.6f),
                visibilityMultiplier = 0.7f
            };

            weatherColorSchemes[3] = new WeatherColors
            {
                weather = WeatherCondition.Heavy_Rain,
                ambientModifier = new Color(0.5f, 0.5f, 0.6f),
                lightModifier = new Color(0.4f, 0.4f, 0.5f),
                fogTint = new Color(0.4f, 0.4f, 0.5f),
                visibilityMultiplier = 0.5f
            };

            weatherColorSchemes[4] = new WeatherColors
            {
                weather = WeatherCondition.Storm,
                ambientModifier = new Color(0.4f, 0.4f, 0.5f),
                lightModifier = new Color(0.3f, 0.3f, 0.4f),
                fogTint = new Color(0.3f, 0.3f, 0.4f),
                visibilityMultiplier = 0.3f
            };

            weatherColorSchemes[5] = new WeatherColors
            {
                weather = WeatherCondition.Fog,
                ambientModifier = new Color(0.6f, 0.6f, 0.7f),
                lightModifier = new Color(0.5f, 0.5f, 0.6f),
                fogTint = new Color(0.7f, 0.7f, 0.8f),
                visibilityMultiplier = 0.4f
            };

            weatherColorSchemes[6] = new WeatherColors
            {
                weather = WeatherCondition.Snow,
                ambientModifier = new Color(0.9f, 0.9f, 1f),
                lightModifier = new Color(0.8f, 0.8f, 0.9f),
                fogTint = new Color(0.8f, 0.8f, 0.9f),
                visibilityMultiplier = 0.6f
            };
        }

        [System.Serializable]
        public struct LightingSettings
        {
            public Color ambientColor;
            public Color lightColor;
            public float lightIntensity;
            public Color fogColor;
            public float shadowIntensity;
        }
    }
}
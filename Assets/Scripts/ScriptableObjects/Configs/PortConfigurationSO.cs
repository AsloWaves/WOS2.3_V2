using UnityEngine;
using System.Collections.Generic;
using WOS.Debugging;

namespace WOS.ScriptableObjects
{
    /// <summary>
    /// Core configuration for port/harbor functionality.
    /// Defines basic port properties, services, and operational parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "PortConfig", menuName = "WOS/Environment/Port Configuration")]
    public class PortConfigurationSO : ScriptableObject
    {
        [System.Serializable]
        public enum PortType
        {
            TradingPort,    // Commercial trading hub
            MilitaryBase,   // Naval military installation
            FishingVillage, // Small fishing community
            IndustrialPort, // Manufacturing and industry
            CulturalHub     // Tourist and cultural center
        }

        [System.Serializable]
        public enum PortSize
        {
            Small,    // 1-3 docking zones
            Medium,   // 4-6 docking zones
            Large,    // 7-10 docking zones
            Massive   // 11+ docking zones
        }

        [System.Serializable]
        public class ServiceAvailability
        {
            [Header("Core Services")]
            [Tooltip("Can players repair ships here")]
            public bool hasRepairService = true;

            [Tooltip("Can players refuel ships here")]
            public bool hasRefuelService = true;

            [Tooltip("Can players buy/sell cargo here")]
            public bool hasTradingService = true;

            [Tooltip("Can players upgrade ships here")]
            public bool hasUpgradeService = false;

            [Header("Advanced Services")]
            [Tooltip("Can players store items here")]
            public bool hasStorageService = false;

            [Tooltip("Can players recruit crew here")]
            public bool hasCrewService = false;

            [Tooltip("Can players access missions here")]
            public bool hasMissionService = true;

            [Tooltip("Can players access bank services here")]
            public bool hasBankingService = false;

            [Header("Service Quality")]
            [Tooltip("Quality multiplier for services (affects speed/cost)")]
            [Range(0.5f, 2f)]
            public float serviceQuality = 1f;

            [Tooltip("Price multiplier for services")]
            [Range(0.5f, 2f)]
            public float priceMultiplier = 1f;
        }

        [Header("Port Identity")]
        [Tooltip("Display name of the port")]
        public string portName = "New Harbor";

        [Tooltip("Type of port determining available services")]
        public PortType portType = PortType.TradingPort;

        [Tooltip("Size category affecting capacity and features")]
        public PortSize portSize = PortSize.Medium;

        [Tooltip("Detailed description of the port")]
        [TextArea(3, 5)]
        public string portDescription = "A bustling harbor serving traders and travelers.";

        [Header("Scene Configuration")]
        [Tooltip("Name of the harbor scene to load when docking")]
        public string harborSceneName = "HarborScene";

        [Tooltip("Should we use async scene loading")]
        public bool useAsyncLoading = true;

        [Tooltip("Spawn point name in the harbor scene")]
        public string harborSpawnPointName = "PlayerSpawn";

        [Header("Protection Zone")]
        [Tooltip("Radius around port center where players are invincible")]
        [Range(200f, 1500f)]
        public float protectionRadius = 800f;

        [Tooltip("Show protection zone boundary to players")]
        public bool showProtectionZoneBoundary = true;

        [Tooltip("Warning distance before leaving protection zone")]
        [Range(50f, 300f)]
        public float protectionWarningDistance = 150f;

        [Header("Services Configuration")]
        [Tooltip("Available services and their configuration")]
        public ServiceAvailability services = new ServiceAvailability();

        [Header("Economic Settings")]
        [Tooltip("Base economic prosperity level (affects prices/availability)")]
        [Range(0f, 1f)]
        public float prosperityLevel = 0.6f;

        [Tooltip("Trading volume multiplier")]
        [Range(0.1f, 3f)]
        public float tradingVolume = 1f;

        [Tooltip("Security level (affects piracy/safety)")]
        [Range(0f, 1f)]
        public float securityLevel = 0.7f;

        [Header("Environmental Factors")]
        [Tooltip("How sheltered the port is from storms")]
        [Range(0f, 1f)]
        public float shelterLevel = 0.8f;

        [Tooltip("Water depth level (affects ship access)")]
        [Range(0f, 1f)]
        public float waterDepth = 0.7f;

        [Tooltip("Port infrastructure quality")]
        [Range(0f, 1f)]
        public float infrastructureQuality = 0.6f;

        [Header("Spawn Configuration")]
        [Tooltip("Where ships should spawn when leaving this port")]
        public Transform[] spawnPoints;

        [Tooltip("Patrol routes for NPC ships near this port")]
        public Transform[] patrolRoutes;

        [Header("Cross-References")]
        [Tooltip("Docking configuration for this port")]
        public DockingConfigurationSO dockingConfig;

        [Tooltip("Services configuration for this port")]
        public PortServicesConfigurationSO servicesConfig;

        [Tooltip("Visual effects configuration for this port")]
        public PortVisualConfigurationSO visualConfig;

        [Tooltip("UI configuration for this port")]
        public PortUIConfigurationSO uiConfig;

        [Tooltip("Integration settings for this port")]
        public PortIntegrationConfigurationSO integrationConfig;

        /// <summary>
        /// Check if a position is within the protection zone
        /// </summary>
        public bool IsWithinProtectionZone(Vector3 playerPosition, Vector3 portPosition)
        {
            float distance = Vector3.Distance(playerPosition, portPosition);
            return distance <= protectionRadius;
        }

        /// <summary>
        /// Check if player is approaching protection zone boundary
        /// </summary>
        public bool IsApproachingProtectionBoundary(Vector3 playerPosition, Vector3 portPosition)
        {
            float distance = Vector3.Distance(playerPosition, portPosition);
            return distance >= (protectionRadius - protectionWarningDistance) && distance <= protectionRadius;
        }

        /// <summary>
        /// Get suggested number of docking locations based on port size (for scene design)
        /// </summary>
        public int GetSuggestedDockingLocations()
        {
            return portSize switch
            {
                PortSize.Small => 3,
                PortSize.Medium => 6,
                PortSize.Large => 10,
                PortSize.Massive => 15,
                _ => 6
            };
        }

        /// <summary>
        /// Get service availability for a specific service type
        /// </summary>
        public bool IsServiceAvailable(string serviceType)
        {
            return serviceType.ToLower() switch
            {
                "repair" => services.hasRepairService,
                "refuel" => services.hasRefuelService,
                "trading" => services.hasTradingService,
                "upgrade" => services.hasUpgradeService,
                "storage" => services.hasStorageService,
                "crew" => services.hasCrewService,
                "mission" => services.hasMissionService,
                "banking" => services.hasBankingService,
                _ => false
            };
        }

        /// <summary>
        /// Get economic modifier for pricing
        /// </summary>
        public float GetPriceModifier()
        {
            float prosperityModifier = Mathf.Lerp(1.2f, 0.8f, prosperityLevel);
            float securityModifier = Mathf.Lerp(1.3f, 0.9f, securityLevel);
            float sizeModifier = portSize switch
            {
                PortSize.Small => 1.1f,
                PortSize.Medium => 1f,
                PortSize.Large => 0.95f,
                PortSize.Massive => 0.9f,
                _ => 1f
            };

            return prosperityModifier * securityModifier * sizeModifier * services.priceMultiplier;
        }

        /// <summary>
        /// Get service efficiency multiplier
        /// </summary>
        public float GetServiceEfficiency()
        {
            float prosperityBonus = prosperityLevel * 0.2f;
            float infrastructureBonus = infrastructureQuality * 0.3f;
            float sizeBonus = portSize switch
            {
                PortSize.Small => 0f,
                PortSize.Medium => 0.1f,
                PortSize.Large => 0.2f,
                PortSize.Massive => 0.3f,
                _ => 0f
            };

            return (1f + prosperityBonus + infrastructureBonus + sizeBonus) * services.serviceQuality;
        }

        /// <summary>
        /// Validate configuration and log warnings for issues
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(portName))
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Port name is empty!", this);
                isValid = false;
            }

            if (protectionRadius <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Protection radius must be greater than 0!", this);
                isValid = false;
            }

            if (protectionWarningDistance <= 0f)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Protection warning distance must be greater than 0!", this);
                isValid = false;
            }

            if (protectionWarningDistance >= protectionRadius)
            {
                DebugManager.LogWarning(DebugCategory.Environment, "Protection warning distance should be less than protection radius!", this);
            }

            // Check cross-references
            if (dockingConfig == null)
                DebugManager.LogWarning(DebugCategory.Environment, "Docking configuration not assigned!", this);

            if (servicesConfig == null)
                DebugManager.LogWarning(DebugCategory.Environment, "Services configuration not assigned!", this);

            if (visualConfig == null)
                DebugManager.LogWarning(DebugCategory.Environment, "Visual configuration not assigned!", this);

            if (uiConfig == null)
                DebugManager.LogWarning(DebugCategory.Environment, "UI configuration not assigned!", this);

            return isValid;
        }

        /// <summary>
        /// Get port statistics summary
        /// </summary>
        public PortStats GetPortStats()
        {
            return new PortStats
            {
                portName = this.portName,
                portType = this.portType,
                portSize = this.portSize,
                suggestedDockingLocations = GetSuggestedDockingLocations(),
                protectionRadius = this.protectionRadius,
                prosperityLevel = this.prosperityLevel,
                securityLevel = this.securityLevel,
                serviceEfficiency = GetServiceEfficiency(),
                priceModifier = GetPriceModifier(),
                shelterLevel = this.shelterLevel,
                infrastructureQuality = this.infrastructureQuality
            };
        }

        private void OnValidate()
        {
            // Ensure name is not empty
            if (string.IsNullOrEmpty(portName))
                portName = "New Harbor";

            // Clamp values to valid ranges
            protectionRadius = Mathf.Max(100f, protectionRadius);
            protectionWarningDistance = Mathf.Max(10f, Mathf.Min(protectionWarningDistance, protectionRadius - 10f));

            // Ensure prosperity and security are in valid range
            prosperityLevel = Mathf.Clamp01(prosperityLevel);
            securityLevel = Mathf.Clamp01(securityLevel);
            shelterLevel = Mathf.Clamp01(shelterLevel);
            waterDepth = Mathf.Clamp01(waterDepth);
            infrastructureQuality = Mathf.Clamp01(infrastructureQuality);

            // Ensure service multipliers are reasonable
            if (services != null)
            {
                services.serviceQuality = Mathf.Clamp(services.serviceQuality, 0.1f, 3f);
                services.priceMultiplier = Mathf.Clamp(services.priceMultiplier, 0.1f, 5f);
            }
        }
    }

    /// <summary>
    /// Port statistics for debugging and UI display
    /// </summary>
    [System.Serializable]
    public struct PortStats
    {
        public string portName;
        public PortConfigurationSO.PortType portType;
        public PortConfigurationSO.PortSize portSize;
        public int suggestedDockingLocations;
        public float protectionRadius;
        public float prosperityLevel;
        public float securityLevel;
        public float serviceEfficiency;
        public float priceModifier;
        public float shelterLevel;
        public float infrastructureQuality;

        public override string ToString()
        {
            return $"{portName} ({portType}, {portSize}): " +
                   $"Protection: {protectionRadius:F0}m, Docking: {suggestedDockingLocations}, Prosperity: {prosperityLevel:F1}, Security: {securityLevel:F1}";
        }
    }
}
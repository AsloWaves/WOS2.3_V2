using UnityEngine;
using System.Collections.Generic;
using WOS.Debugging;

namespace WOS.ScriptableObjects
{
    /// <summary>
    /// Configuration for port services including pricing, timing, and availability.
    /// Defines all the services available at ports and their parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "PortServicesConfig", menuName = "WOS/Environment/Port Services Configuration")]
    public class PortServicesConfigurationSO : ScriptableObject
    {
        [System.Serializable]
        public enum ServiceType
        {
            Repair,      // Ship hull and component repair
            Refuel,      // Fuel and supplies replenishment
            Trading,     // Buy/sell cargo and goods
            Upgrade,     // Ship modifications and improvements
            Storage,     // Warehouse and storage services
            Crew,        // Crew recruitment and management
            Mission,     // Quest and job board services
            Banking,     // Financial services
            Information, // Maps, weather, news
            Customs      // Port authority and documentation
        }

        [System.Serializable]
        public enum ServiceTier
        {
            Basic,       // Standard service quality
            Premium,     // Higher quality, faster service
            Luxury,      // Top tier service with bonuses
            Emergency    // 24/7 emergency service at premium cost
        }

        [System.Serializable]
        public class ServiceDefinition
        {
            [Header("Service Identity")]
            [Tooltip("Type of service")]
            public ServiceType serviceType = ServiceType.Repair;

            [Tooltip("Display name for the service")]
            public string serviceName = "Repair Service";

            [Tooltip("Detailed description of the service")]
            [TextArea(2, 4)]
            public string serviceDescription = "Professional ship repair and maintenance.";

            [Tooltip("Icon for UI display")]
            public Sprite serviceIcon;

            [Header("Availability")]
            [Tooltip("Service tier available")]
            public ServiceTier availableTier = ServiceTier.Basic;

            [Tooltip("Is this service currently available")]
            public bool isAvailable = true;

            [Tooltip("Hours service is available (24 = always)")]
            [Range(1, 24)]
            public int hoursAvailable = 24;

            [Tooltip("Service availability based on port prosperity")]
            [Range(0f, 1f)]
            public float minimumProsperityRequired = 0f;

            [Header("Pricing")]
            [Tooltip("Base cost for this service")]
            [Range(0f, 10000f)]
            public float baseCost = 100f;

            [Tooltip("Cost per unit (repair per %, fuel per unit, etc.)")]
            [Range(0f, 1000f)]
            public float costPerUnit = 10f;

            [Tooltip("Tier-based cost multipliers")]
            public TierPricing tierPricing = new TierPricing();

            [Header("Timing")]
            [Tooltip("Base time to complete service (seconds)")]
            [Range(1f, 3600f)]
            public float baseServiceTime = 30f;

            [Tooltip("Time per unit of service")]
            [Range(0.1f, 300f)]
            public float timePerUnit = 5f;

            [Tooltip("Tier-based time multipliers")]
            public TierTiming tierTiming = new TierTiming();

            [Header("Quality")]
            [Tooltip("Service quality multiplier (affects effectiveness)")]
            [Range(0.5f, 2f)]
            public float qualityMultiplier = 1f;

            [Tooltip("Tier-based quality bonuses")]
            public TierQuality tierQuality = new TierQuality();

            [Header("Requirements")]
            [Tooltip("Minimum ship level required")]
            [Range(0, 100)]
            public int minimumShipLevel = 0;

            [Tooltip("Required items or resources")]
            public List<string> requiredResources = new List<string>();

            [Tooltip("Special conditions for availability")]
            [TextArea(1, 3)]
            public string specialConditions = "";
        }

        [System.Serializable]
        public class TierPricing
        {
            [Tooltip("Basic tier cost multiplier")]
            [Range(0.5f, 2f)]
            public float basicMultiplier = 1f;

            [Tooltip("Premium tier cost multiplier")]
            [Range(1f, 3f)]
            public float premiumMultiplier = 1.5f;

            [Tooltip("Luxury tier cost multiplier")]
            [Range(1.5f, 5f)]
            public float luxuryMultiplier = 2.5f;

            [Tooltip("Emergency tier cost multiplier")]
            [Range(2f, 10f)]
            public float emergencyMultiplier = 4f;
        }

        [System.Serializable]
        public class TierTiming
        {
            [Tooltip("Basic tier time multiplier")]
            [Range(0.5f, 2f)]
            public float basicMultiplier = 1f;

            [Tooltip("Premium tier time multiplier")]
            [Range(0.3f, 1f)]
            public float premiumMultiplier = 0.7f;

            [Tooltip("Luxury tier time multiplier")]
            [Range(0.2f, 0.8f)]
            public float luxuryMultiplier = 0.5f;

            [Tooltip("Emergency tier time multiplier")]
            [Range(0.1f, 0.5f)]
            public float emergencyMultiplier = 0.3f;
        }

        [System.Serializable]
        public class TierQuality
        {
            [Tooltip("Basic tier quality bonus")]
            [Range(0f, 0.5f)]
            public float basicBonus = 0f;

            [Tooltip("Premium tier quality bonus")]
            [Range(0f, 1f)]
            public float premiumBonus = 0.2f;

            [Tooltip("Luxury tier quality bonus")]
            [Range(0f, 1.5f)]
            public float luxuryBonus = 0.5f;

            [Tooltip("Emergency tier quality bonus")]
            [Range(0f, 0.8f)]
            public float emergencyBonus = 0.1f; // Emergency is fast, not necessarily better
        }

        [System.Serializable]
        public class ServiceBundle
        {
            [Header("Bundle Definition")]
            [Tooltip("Bundle name")]
            public string bundleName = "Complete Service Package";

            [Tooltip("Services included in this bundle")]
            public List<ServiceType> includedServices = new List<ServiceType>();

            [Tooltip("Bundle discount percentage")]
            [Range(0f, 50f)]
            public float discountPercentage = 15f;

            [Tooltip("Bundle availability conditions")]
            [TextArea(1, 3)]
            public string availabilityConditions = "";

            [Header("Bundle Benefits")]
            [Tooltip("Additional benefits of choosing bundle")]
            [TextArea(2, 4)]
            public string bundleBenefits = "Save time and money with our complete service package.";

            [Tooltip("Priority service for bundle customers")]
            public bool priorityService = true;
        }

        [Header("Available Services")]
        [Tooltip("All services available at ports")]
        public List<ServiceDefinition> availableServices = new List<ServiceDefinition>();

        [Header("Service Bundles")]
        [Tooltip("Pre-configured service packages")]
        public List<ServiceBundle> serviceBundles = new List<ServiceBundle>();

        [Header("Global Settings")]
        [Tooltip("Global service cost multiplier")]
        [Range(0.1f, 5f)]
        public float globalCostMultiplier = 1f;

        [Tooltip("Global service time multiplier")]
        [Range(0.1f, 5f)]
        public float globalTimeMultiplier = 1f;

        [Tooltip("Global service quality multiplier")]
        [Range(0.5f, 2f)]
        public float globalQualityMultiplier = 1f;

        [Header("Economic Factors")]
        [Tooltip("How much port prosperity affects pricing")]
        [Range(0f, 2f)]
        public float prosperityPriceImpact = 0.3f;

        [Tooltip("How much port security affects availability")]
        [Range(0f, 1f)]
        public float securityAvailabilityImpact = 0.2f;

        [Tooltip("Dynamic pricing based on demand")]
        public bool enableDynamicPricing = true;

        [Tooltip("Maximum price increase due to demand")]
        [Range(0f, 2f)]
        public float maxDemandPriceIncrease = 0.5f;

        [Header("Queue Management")]
        [Tooltip("Maximum customers in service queue")]
        [Range(1, 20)]
        public int maxServiceQueue = 5;

        [Tooltip("Queue wait time affects pricing")]
        public bool queueAffectsPricing = true;

        [Tooltip("Queue time price reduction per minute")]
        [Range(0f, 0.1f)]
        public float queueDiscountPerMinute = 0.02f;

        /// <summary>
        /// Get service definition by type
        /// </summary>
        public ServiceDefinition GetService(ServiceType serviceType)
        {
            return availableServices.Find(s => s.serviceType == serviceType);
        }

        /// <summary>
        /// Get all services of a specific tier
        /// </summary>
        public List<ServiceDefinition> GetServicesByTier(ServiceTier tier)
        {
            return availableServices.FindAll(s => s.availableTier == tier && s.isAvailable);
        }

        /// <summary>
        /// Calculate service cost with all modifiers
        /// </summary>
        public float CalculateServiceCost(ServiceType serviceType, ServiceTier tier, float units,
            float prosperityLevel, float demandMultiplier = 1f)
        {
            var service = GetService(serviceType);
            if (service == null) return 0f;

            // Base cost calculation
            float baseCost = service.baseCost + (service.costPerUnit * units);

            // Apply tier multiplier
            float tierMultiplier = GetTierPriceMultiplier(tier, service.tierPricing);

            // Apply global multiplier
            baseCost *= globalCostMultiplier;

            // Apply tier multiplier
            baseCost *= tierMultiplier;

            // Apply prosperity modifier
            float prosperityModifier = 1f + (prosperityPriceImpact * (1f - prosperityLevel));
            baseCost *= prosperityModifier;

            // Apply demand multiplier
            if (enableDynamicPricing)
            {
                float demandModifier = Mathf.Clamp(demandMultiplier, 1f, 1f + maxDemandPriceIncrease);
                baseCost *= demandModifier;
            }

            return Mathf.Round(baseCost * 100f) / 100f; // Round to nearest cent
        }

        /// <summary>
        /// Calculate service time with all modifiers
        /// </summary>
        public float CalculateServiceTime(ServiceType serviceType, ServiceTier tier, float units,
            float qualityLevel = 1f)
        {
            var service = GetService(serviceType);
            if (service == null) return 0f;

            // Base time calculation
            float baseTime = service.baseServiceTime + (service.timePerUnit * units);

            // Apply tier multiplier
            float tierMultiplier = GetTierTimeMultiplier(tier, service.tierTiming);

            // Apply global multiplier
            baseTime *= globalTimeMultiplier;

            // Apply tier multiplier
            baseTime *= tierMultiplier;

            // Apply quality modifier (higher quality might take longer)
            baseTime *= Mathf.Lerp(0.8f, 1.2f, qualityLevel);

            return Mathf.Max(1f, baseTime); // Minimum 1 second
        }

        /// <summary>
        /// Calculate service quality with all modifiers
        /// </summary>
        public float CalculateServiceQuality(ServiceType serviceType, ServiceTier tier,
            float baseQuality = 1f)
        {
            var service = GetService(serviceType);
            if (service == null) return baseQuality;

            // Start with service base quality
            float quality = service.qualityMultiplier * baseQuality;

            // Apply tier bonus
            float tierBonus = GetTierQualityBonus(tier, service.tierQuality);
            quality += tierBonus;

            // Apply global multiplier
            quality *= globalQualityMultiplier;

            return Mathf.Clamp(quality, 0.1f, 3f); // Reasonable quality range
        }

        /// <summary>
        /// Check if service is available based on conditions
        /// </summary>
        public bool IsServiceAvailable(ServiceType serviceType, float prosperityLevel,
            float securityLevel, int currentHour = 12)
        {
            var service = GetService(serviceType);
            if (service == null || !service.isAvailable) return false;

            // Check prosperity requirement
            if (prosperityLevel < service.minimumProsperityRequired) return false;

            // Check operating hours
            if (service.hoursAvailable < 24)
            {
                int openHour = 12 - (service.hoursAvailable / 2);
                int closeHour = 12 + (service.hoursAvailable / 2);
                if (currentHour < openHour || currentHour >= closeHour) return false;
            }

            // Check security impact on availability
            float securityRequirement = service.serviceType switch
            {
                ServiceType.Banking => 0.7f,
                ServiceType.Upgrade => 0.5f,
                ServiceType.Trading => 0.3f,
                _ => 0f
            };

            if (securityLevel < securityRequirement) return false;

            return true;
        }

        /// <summary>
        /// Find suitable service bundle for given services
        /// </summary>
        public ServiceBundle FindBundleForServices(List<ServiceType> requestedServices)
        {
            ServiceBundle bestBundle = null;
            int bestMatch = 0;

            foreach (var bundle in serviceBundles)
            {
                int matchCount = 0;
                foreach (var requestedService in requestedServices)
                {
                    if (bundle.includedServices.Contains(requestedService))
                        matchCount++;
                }

                if (matchCount > bestMatch && matchCount >= requestedServices.Count * 0.7f)
                {
                    bestMatch = matchCount;
                    bestBundle = bundle;
                }
            }

            return bestBundle;
        }

        private float GetTierPriceMultiplier(ServiceTier tier, TierPricing pricing)
        {
            return tier switch
            {
                ServiceTier.Basic => pricing.basicMultiplier,
                ServiceTier.Premium => pricing.premiumMultiplier,
                ServiceTier.Luxury => pricing.luxuryMultiplier,
                ServiceTier.Emergency => pricing.emergencyMultiplier,
                _ => 1f
            };
        }

        private float GetTierTimeMultiplier(ServiceTier tier, TierTiming timing)
        {
            return tier switch
            {
                ServiceTier.Basic => timing.basicMultiplier,
                ServiceTier.Premium => timing.premiumMultiplier,
                ServiceTier.Luxury => timing.luxuryMultiplier,
                ServiceTier.Emergency => timing.emergencyMultiplier,
                _ => 1f
            };
        }

        private float GetTierQualityBonus(ServiceTier tier, TierQuality quality)
        {
            return tier switch
            {
                ServiceTier.Basic => quality.basicBonus,
                ServiceTier.Premium => quality.premiumBonus,
                ServiceTier.Luxury => quality.luxuryBonus,
                ServiceTier.Emergency => quality.emergencyBonus,
                _ => 0f
            };
        }

        /// <summary>
        /// Validate configuration and log warnings
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            foreach (var service in availableServices)
            {
                if (string.IsNullOrEmpty(service.serviceName))
                {
                    DebugManager.LogWarning(DebugCategory.Environment, $"Service {service.serviceType} has no name!", this);
                    isValid = false;
                }

                if (service.baseCost < 0f || service.costPerUnit < 0f)
                {
                    DebugManager.LogWarning(DebugCategory.Environment, $"Service {service.serviceType} has negative costs!", this);
                    isValid = false;
                }

                if (service.baseServiceTime <= 0f || service.timePerUnit <= 0f)
                {
                    DebugManager.LogWarning(DebugCategory.Environment, $"Service {service.serviceType} has invalid timing!", this);
                    isValid = false;
                }
            }

            return isValid;
        }

        private void OnValidate()
        {
            // Ensure global multipliers are reasonable
            globalCostMultiplier = Mathf.Max(0.1f, globalCostMultiplier);
            globalTimeMultiplier = Mathf.Max(0.1f, globalTimeMultiplier);
            globalQualityMultiplier = Mathf.Max(0.1f, globalQualityMultiplier);

            // Ensure impact values are reasonable
            prosperityPriceImpact = Mathf.Clamp(prosperityPriceImpact, 0f, 2f);
            securityAvailabilityImpact = Mathf.Clamp01(securityAvailabilityImpact);
            maxDemandPriceIncrease = Mathf.Max(0f, maxDemandPriceIncrease);

            // Ensure queue settings are valid
            maxServiceQueue = Mathf.Max(1, maxServiceQueue);
            queueDiscountPerMinute = Mathf.Max(0f, queueDiscountPerMinute);

            // Validate service definitions
            foreach (var service in availableServices)
            {
                if (service != null)
                {
                    service.baseCost = Mathf.Max(0f, service.baseCost);
                    service.costPerUnit = Mathf.Max(0f, service.costPerUnit);
                    service.baseServiceTime = Mathf.Max(1f, service.baseServiceTime);
                    service.timePerUnit = Mathf.Max(0.1f, service.timePerUnit);
                    service.qualityMultiplier = Mathf.Max(0.1f, service.qualityMultiplier);
                    service.minimumProsperityRequired = Mathf.Clamp01(service.minimumProsperityRequired);
                    service.hoursAvailable = Mathf.Clamp(service.hoursAvailable, 1, 24);
                }
            }

            // Validate service bundles
            foreach (var bundle in serviceBundles)
            {
                if (bundle != null)
                {
                    bundle.discountPercentage = Mathf.Clamp(bundle.discountPercentage, 0f, 80f);
                }
            }
        }
    }
}
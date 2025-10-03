using UnityEngine;
using Unity.Mathematics;

namespace WOS.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject configuration for ship physics and navigation parameters.
    /// Defines ship classes with authentic naval characteristics.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipConfiguration", menuName = "WOS/Ship Configuration")]
    public class ShipConfigurationSO : ScriptableObject
    {
        [Header("Ship Identity")]
        [Tooltip("Display name for this ship class")]
        public string shipName = "Frigate";

        [Tooltip("Ship classification (Destroyer, Frigate, Corvette, Patrol, Transport)")]
        public ShipClass shipClass = ShipClass.Frigate;

        [Tooltip("Brief description of ship capabilities")]
        [TextArea(2, 4)]
        public string description = "Fast patrol vessel with balanced speed and maneuverability";

        [Header("Physical Properties")]
        [Tooltip("Ship length in meters (affects turning radius)")]
        [Range(20f, 200f)]
        public float length = 85f;

        [Tooltip("Ship beam (width) in meters")]
        [Range(5f, 30f)]
        public float beam = 12f;

        [Tooltip("Ship displacement in tons (affects inertia)")]
        [Range(100f, 5000f)]
        public float displacement = 1200f;

        [Tooltip("Ship draft in meters (affects shallow water navigation)")]
        [Range(2f, 15f)]
        public float draft = 4.5f;

        [Header("Engine Performance")]
        [Tooltip("Maximum speed in knots")]
        [Range(15f, 45f)]
        public float maxSpeed = 28f;

        [Tooltip("Cruising speed in knots (most efficient)")]
        [Range(10f, 35f)]
        public float cruisingSpeed = 18f;

        [Tooltip("Engine acceleration factor")]
        [Range(0.5f, 3f)]
        public float acceleration = 1.2f;

        [Tooltip("Engine deceleration factor")]
        [Range(0.5f, 5f)]
        public float deceleration = 2.0f;

        [Header("Maneuverability")]
        [Tooltip("Maximum rudder angle in degrees")]
        [Range(15f, 45f)]
        public float maxRudderAngle = 35f;

        [Tooltip("Rudder response rate (degrees per second)")]
        [Range(5f, 30f)]
        public float rudderRate = 15f;

        [Tooltip("Turning circle diameter at full speed (in ship lengths)")]
        [Range(3f, 8f)]
        public float turningCircle = 5.2f;

        [Tooltip("How quickly ship responds to helm orders")]
        [Range(0.1f, 2f)]
        public float helmResponse = 0.8f;

        [Header("Naval Physics")]
        [Tooltip("Water resistance coefficient")]
        [Range(0.1f, 2f)]
        public float dragCoefficient = 0.6f;

        [Tooltip("Angular drag for turning resistance")]
        [Range(0.5f, 5f)]
        public float angularDrag = 2.5f;

        [Tooltip("How much ship drifts sideways during turns")]
        [Range(0f, 1f)]
        public float lateralDrift = 0.3f;

        [Tooltip("Minimum speed needed for effective steering")]
        [Range(2f, 8f)]
        public float steerageway = 4f;

        [Header("Cargo & Systems")]
        [Tooltip("Maximum cargo capacity in tons")]
        [Range(50f, 2000f)]
        public int cargoCapacity = 400;

        [Tooltip("Ship's empty weight in tons (affects performance calculations)")]
        [Range(100f, 3000f)]
        public float emptyWeight = 800f;

        [Tooltip("Crew complement")]
        [Range(10, 200)]
        public int crewSize = 45;

        [Tooltip("Fuel capacity in hours at cruising speed")]
        [Range(24f, 168f)]
        public float fuelEndurance = 96f;

        [Tooltip("Ship's operating range in nautical miles")]
        [Range(500f, 5000f)]
        public float operatingRange = 1800f;

        [Header("Weight Performance")]
        [Tooltip("How much speed is reduced per ton of cargo (percentage per ton)")]
        [Range(0.001f, 0.01f)]
        public float speedPenaltyPerTon = 0.003f;

        [Tooltip("How much acceleration is reduced per ton of cargo (percentage per ton)")]
        [Range(0.001f, 0.02f)]
        public float accelerationPenaltyPerTon = 0.005f;

        [Tooltip("How much maneuverability is reduced per ton of cargo (percentage per ton)")]
        [Range(0.001f, 0.015f)]
        public float maneuverabilityPenaltyPerTon = 0.004f;

        [Tooltip("Maximum performance penalty from weight (0-1 scale)")]
        [Range(0.1f, 0.7f)]
        public float maxWeightPenalty = 0.4f;

        [Header("Visual Configuration")]
        [Tooltip("Ship sprite/model prefab")]
        public GameObject shipPrefab;

        [Tooltip("Wake effect prefab")]
        public GameObject wakeEffect;

        [Tooltip("Ship icon for UI")]
        public Sprite shipIcon;

        [Header("Audio Configuration")]
        [Tooltip("Engine sound clips for different throttle settings")]
        public AudioClip[] engineSounds;

        [Tooltip("Horn/whistle sound")]
        public AudioClip hornSound;

        /// <summary>
        /// Calculate turning radius based on ship length and turning circle factor
        /// </summary>
        public float GetTurningRadius()
        {
            return length * turningCircle * 0.5f;
        }

        /// <summary>
        /// Get speed in Unity units per second (converted from knots)
        /// </summary>
        public float GetSpeedInUnitsPerSecond(float speedInKnots)
        {
            // 1 knot = 0.514444 m/s, adjust for Unity scale
            return speedInKnots * 0.514f;
        }

        /// <summary>
        /// Calculate ship's momentum based on current speed and displacement
        /// </summary>
        public float GetMomentum(float currentSpeed)
        {
            return displacement * currentSpeed;
        }

        /// <summary>
        /// Get the stopping distance for emergency stop at given speed
        /// </summary>
        public float GetStoppingDistance(float currentSpeed)
        {
            float momentum = GetMomentum(currentSpeed);
            return momentum / (deceleration * 1000f); // Simplified physics
        }

        /// <summary>
        /// Calculate total ship weight including cargo
        /// </summary>
        public float GetTotalWeight(float cargoWeight)
        {
            return emptyWeight + cargoWeight;
        }

        /// <summary>
        /// Get modified maximum speed based on current cargo weight
        /// </summary>
        public float GetModifiedMaxSpeed(float cargoWeight)
        {
            float totalWeight = GetTotalWeight(cargoWeight);
            float weightRatio = cargoWeight / cargoCapacity;
            float penalty = Mathf.Clamp01(weightRatio * speedPenaltyPerTon * cargoCapacity);
            penalty = Mathf.Min(penalty, maxWeightPenalty);

            return maxSpeed * (1f - penalty);
        }

        /// <summary>
        /// Get modified acceleration based on current cargo weight
        /// </summary>
        public float GetModifiedAcceleration(float cargoWeight)
        {
            float weightRatio = cargoWeight / cargoCapacity;
            float penalty = Mathf.Clamp01(weightRatio * accelerationPenaltyPerTon * cargoCapacity);
            penalty = Mathf.Min(penalty, maxWeightPenalty);

            return acceleration * (1f - penalty);
        }

        /// <summary>
        /// Get modified turning rate based on current cargo weight
        /// </summary>
        public float GetModifiedTurningRate(float cargoWeight)
        {
            float weightRatio = cargoWeight / cargoCapacity;
            float penalty = Mathf.Clamp01(weightRatio * maneuverabilityPenaltyPerTon * cargoCapacity);
            penalty = Mathf.Min(penalty, maxWeightPenalty);

            return rudderRate * (1f - penalty);
        }

        /// <summary>
        /// Get overall performance penalty from cargo weight (0-1 scale)
        /// </summary>
        public float GetWeightPerformancePenalty(float cargoWeight)
        {
            float weightRatio = cargoWeight / cargoCapacity;
            float averagePenalty = (speedPenaltyPerTon + accelerationPenaltyPerTon + maneuverabilityPenaltyPerTon) / 3f;
            float penalty = Mathf.Clamp01(weightRatio * averagePenalty * cargoCapacity);

            return Mathf.Min(penalty, maxWeightPenalty);
        }

        /// <summary>
        /// Check if ship is overloaded based on cargo weight
        /// </summary>
        public bool IsOverloaded(float cargoWeight)
        {
            return cargoWeight > cargoCapacity;
        }

        private void OnValidate()
        {
            // Ensure logical relationships between values
            cruisingSpeed = Mathf.Min(cruisingSpeed, maxSpeed * 0.8f);
            steerageway = Mathf.Min(steerageway, cruisingSpeed * 0.5f);

            // Ensure empty weight is less than total displacement
            emptyWeight = Mathf.Min(emptyWeight, displacement * 0.7f);

            // Ensure cargo capacity is reasonable for ship size
            cargoCapacity = Mathf.Min(cargoCapacity, (int)(displacement - emptyWeight));
        }
    }

    /// <summary>
    /// Ship classification enum for different vessel types
    /// </summary>
    public enum ShipClass
    {
        Destroyer,    // Fast, heavily armed warship
        Frigate,      // Medium-sized warship, balanced capabilities
        Corvette,     // Small, fast patrol vessel
        Patrol,       // Coastal patrol boat
        Transport     // Cargo and supply vessel
    }
}
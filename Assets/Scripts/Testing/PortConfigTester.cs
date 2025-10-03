using UnityEngine;
using WOS.Environment;
using WOS.ScriptableObjects;
using WOS.Debugging;

namespace WOS.Testing
{
    /// <summary>
    /// Simple test script to validate PortConfiguration functionality.
    /// Add this to a GameObject in your test scene to verify port config is working.
    /// </summary>
    public class PortConfigTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [Tooltip("Port configuration to test")]
        [SerializeField] private PortConfigurationSO portConfigToTest;

        [Tooltip("Test player position (will be moved around for testing)")]
        [SerializeField] private Transform testPlayerTransform;

        [Header("Test Results")]
        [SerializeField] private bool isInProtectionZone = false;
        [SerializeField] private bool isApproachingBoundary = false;
        [SerializeField] private float distanceFromPort = 0f;

        [Header("Automatic Testing")]
        [Tooltip("Automatically move test player in circle")]
        [SerializeField] private bool enableAutoTest = false;
        [SerializeField] private float testRadius = 1000f;
        [SerializeField] private float testSpeed = 50f;

        private float testTime = 0f;

        private void Start()
        {
            // Create test player if none assigned
            if (testPlayerTransform == null)
            {
                GameObject testPlayer = new GameObject("TestPlayer");
                testPlayerTransform = testPlayer.transform;

                // Add visual indicator
                var renderer = testPlayer.AddComponent<SpriteRenderer>();
                renderer.color = Color.red;

                // Start position outside protection zone
                testPlayerTransform.position = new Vector3(1200f, 0f, 0f);
            }

            // Validate port config
            if (portConfigToTest != null)
            {
                bool isValid = portConfigToTest.ValidateConfiguration();
                DebugManager.Log(DebugCategory.Environment, $"Port config validation: {(isValid ? "PASSED" : "FAILED")}", this);

                var stats = portConfigToTest.GetPortStats();
                DebugManager.Log(DebugCategory.Environment, $"Port stats: {stats}", this);
            }
            else
            {
                DebugManager.LogError(DebugCategory.Environment, "No port config assigned for testing!", this);
            }
        }

        private void Update()
        {
            if (portConfigToTest == null || testPlayerTransform == null) return;

            // Automatic testing movement
            if (enableAutoTest)
            {
                testTime += Time.deltaTime * testSpeed * 0.01f;
                float x = Mathf.Cos(testTime) * testRadius;
                float y = Mathf.Sin(testTime) * testRadius;
                testPlayerTransform.position = new Vector3(x, y, 0f);
            }

            // Update test results
            UpdateTestResults();
        }

        private void UpdateTestResults()
        {
            Vector3 portPosition = transform.position;
            Vector3 playerPosition = testPlayerTransform.position;

            // Calculate distance
            distanceFromPort = Vector3.Distance(playerPosition, portPosition);

            // Test protection zone
            bool wasInZone = isInProtectionZone;
            isInProtectionZone = portConfigToTest.IsWithinProtectionZone(playerPosition, portPosition);

            // Test boundary approach
            bool wasApproaching = isApproachingBoundary;
            isApproachingBoundary = portConfigToTest.IsApproachingProtectionBoundary(playerPosition, portPosition);

            // Log zone changes
            if (wasInZone != isInProtectionZone)
            {
                DebugManager.Log(DebugCategory.Environment,
                    $"Protection zone {(isInProtectionZone ? "ENTERED" : "EXITED")} at distance {distanceFromPort:F1}m", this);
            }

            if (wasApproaching != isApproachingBoundary && isApproachingBoundary)
            {
                DebugManager.Log(DebugCategory.Environment,
                    $"Approaching protection boundary at distance {distanceFromPort:F1}m", this);
            }
        }

        [ContextMenu("Test Protection Zone")]
        public void TestProtectionZone()
        {
            if (portConfigToTest == null)
            {
                Debug.LogError("No port config assigned!");
                return;
            }

            Vector3 portPos = transform.position;

            // Test various positions
            Vector3[] testPositions = {
                portPos, // Center
                portPos + Vector3.right * (portConfigToTest.protectionRadius * 0.5f), // Inside
                portPos + Vector3.right * (portConfigToTest.protectionRadius * 0.9f), // Near boundary
                portPos + Vector3.right * (portConfigToTest.protectionRadius * 1.1f), // Just outside
                portPos + Vector3.right * (portConfigToTest.protectionRadius * 2f)    // Far outside
            };

            string[] positionNames = { "Center", "Inside", "Near Boundary", "Just Outside", "Far Outside" };

            for (int i = 0; i < testPositions.Length; i++)
            {
                bool inZone = portConfigToTest.IsWithinProtectionZone(testPositions[i], portPos);
                bool approaching = portConfigToTest.IsApproachingProtectionBoundary(testPositions[i], portPos);
                float distance = Vector3.Distance(testPositions[i], portPos);

                Debug.Log($"{positionNames[i]}: Distance={distance:F1}m, InZone={inZone}, Approaching={approaching}");
            }
        }

        [ContextMenu("Move Test Player to Center")]
        public void MovePlayerToCenter()
        {
            if (testPlayerTransform != null)
                testPlayerTransform.position = transform.position;
        }

        [ContextMenu("Move Test Player Outside")]
        public void MovePlayerOutside()
        {
            if (testPlayerTransform != null && portConfigToTest != null)
            {
                float distance = portConfigToTest.protectionRadius * 1.5f;
                testPlayerTransform.position = transform.position + Vector3.right * distance;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (portConfigToTest == null) return;

            Vector3 center = transform.position;

            // Draw protection radius
            Gizmos.color = Color.green;
            DrawWireCircle(center, portConfigToTest.protectionRadius);

            // Draw warning boundary
            float warningRadius = portConfigToTest.protectionRadius - portConfigToTest.protectionWarningDistance;
            Gizmos.color = Color.yellow;
            DrawWireCircle(center, warningRadius);

            // Draw connection to test player
            if (testPlayerTransform != null)
            {
                Gizmos.color = isInProtectionZone ? Color.green : Color.red;
                Gizmos.DrawLine(center, testPlayerTransform.position);

                // Draw test player position
                Gizmos.DrawWireSphere(testPlayerTransform.position, 20f);
            }
        }

        private void DrawWireCircle(Vector3 center, float radius)
        {
            const int segments = 32;
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}
using UnityEngine;

namespace H8Games.Environment
{
    public class BackgroundShipController : MonoBehaviour
    {
        [SerializeField] 
        private BackgroundShipConfig config;
        
        private float currentSpeed;
        private Vector3 targetPosition;
        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
            
            if (config == null)
            {
                Debug.LogWarning($"BackgroundShipController on {gameObject.name} is missing a Config! The ship will not move.");
                return;
            }

            PickNewSpeed();
            PickNewWaypoint();
        }

        private void Update()
        {
            if (config == null) return;

            MoveTowardsTarget();
        }

        private void MoveTowardsTarget()
        {
            // Calculate distance ignoring the Y axis (height)
            Vector3 currentPosFlat = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 targetPosFlat = new Vector3(targetPosition.x, 0, targetPosition.z);
            float distanceToTarget = Vector3.Distance(currentPosFlat, targetPosFlat);
            
            // Pick a new target if we are close enough
            if (distanceToTarget <= config.waypointTolerance)
            {
                PickNewWaypoint();
                PickNewSpeed();
            }

            // Rotate towards the target smoothly
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            
            // Keep direction strictly horizontal to prevent the ship from pitching up/down
            directionToTarget.y = 0; 
            
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, config.rotationSpeed * Time.deltaTime);
            }

            // Move the ship forward in the direction it is facing
            transform.Translate(Vector3.forward * (currentSpeed * Time.deltaTime), Space.Self);
        }

        private void PickNewWaypoint()
        {
            // Pick a random point within a 2D circle around the initial spawn position
            Vector2 randomCircle = Random.insideUnitCircle * config.wanderRadius;
            
            // Apply it to the 3D space, keeping the ship's original Y position (water level)
            targetPosition = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
        }

        private void PickNewSpeed()
        {
            currentSpeed = Random.Range(config.minSpeed, config.maxSpeed);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (config != null)
            {
                // Draw the allowed wandering area
                Gizmos.color = new Color(0f, 1f, 1f, 0.3f); // Semi-transparent cyan
                Vector3 center = Application.isPlaying ? startPosition : transform.position;
                Gizmos.DrawWireSphere(center, config.wanderRadius);
                
                // Draw the current target and path in play mode
                if (Application.isPlaying)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(targetPosition, 0.5f);
                    Gizmos.DrawLine(transform.position, targetPosition);
                }
            }
        }
    }
}

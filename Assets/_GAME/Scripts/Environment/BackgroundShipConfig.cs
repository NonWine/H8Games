using UnityEngine;

namespace H8Games.Environment
{
    [CreateAssetMenu(fileName = "BackgroundShipConfig", menuName = "H8Games/Environment/Background Ship Config")]
    public class BackgroundShipConfig : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Minimum movement speed of the ship.")]
        public float minSpeed = 1f;
        [Tooltip("Maximum movement speed of the ship.")]
        public float maxSpeed = 3f;
        
        [Header("Rotation")]
        [Tooltip("How fast the ship turns to face its new target (degrees per second).")]
        public float rotationSpeed = 30f;
        
        [Header("Wandering Area")]
        [Tooltip("Radius around the initial position where the ship is allowed to wander.")]
        public float wanderRadius = 50f;
        
        [Tooltip("How close the ship needs to get to its target before picking a new one.")]
        public float waypointTolerance = 3f;
    }
}

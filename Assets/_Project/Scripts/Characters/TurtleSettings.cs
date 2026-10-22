using UnityEngine;

namespace TurtleLife.Characters
{
    [CreateAssetMenu(menuName = "Turtle Life/Turtle Settings")]
    public sealed class TurtleSettings : ScriptableObject
    {
        [Min(0.1f)] public float WalkSpeed = 2.8f;
        [Min(0.1f)] public float SwimSpeed = 3.2f;
        [Min(0.1f)] public float VerticalSwimSpeed = 2f;
        [Min(0.1f)] public float Acceleration = 12f;
        [Min(0.1f)] public float TurnSpeed = 540f;
        [Min(0.1f)] public float Gravity = 18f;
        [Min(0.1f)] public float WaterAcceleration = 5f;
    }
}

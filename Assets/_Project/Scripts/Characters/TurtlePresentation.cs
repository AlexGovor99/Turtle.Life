using UnityEngine;

namespace TurtleLife.Characters
{
    [RequireComponent(typeof(TurtleMotor))]
    public sealed class TurtlePresentation : MonoBehaviour
    {
        [SerializeField] private Transform[] flippers = new Transform[0];
        private Quaternion[] restRotations;
        private TurtleMotor motor;
        private float phase;
        public void Configure(Transform[] values) => flippers = values;

        private void Awake()
        {
            motor = GetComponent<TurtleMotor>();
            restRotations = new Quaternion[flippers.Length];
            for (int i = 0; i < flippers.Length; i++) restRotations[i] = flippers[i].localRotation;
        }

        private void LateUpdate()
        {
            float animationSpeed = motor.Mode == TurtleMovementMode.Water ? motor.ActualVelocity.magnitude : motor.Speed;
            float strength = Mathf.Clamp01(animationSpeed / 2f);
            phase += Time.deltaTime * (motor.Mode == TurtleMovementMode.Water ? 5f : 9f) * strength;
            for (int i = 0; i < flippers.Length; i++)
            {
                float angle = Mathf.Sin(phase + (i % 2) * Mathf.PI) * 25f * strength;
                flippers[i].localRotation = restRotations[i] * Quaternion.Euler(0f, 0f, angle);
            }
        }
    }
}

using UnityEngine;

namespace TurtleLife.Characters
{
    [RequireComponent(typeof(CharacterController), typeof(TurtleLocomotion))]
    public sealed class TurtleMotor : MonoBehaviour
    {
        [SerializeField] private TurtleSettings settings;
        private CharacterController controller;
        private TurtleLocomotion locomotion;
        private Vector3 velocity;
        public Vector3 ActualVelocity { get; private set; }
        public float Speed => new Vector2(ActualVelocity.x, ActualVelocity.z).magnitude;
        public TurtleMovementMode Mode => locomotion.Mode;
        public void Configure(TurtleSettings value) => settings = value;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            locomotion = GetComponent<TurtleLocomotion>();
            if (settings == null)
            {
                Debug.LogError("TurtleMotor requires TurtleSettings.", this);
                enabled = false;
            }
        }

        // The caller supplies intent and a world-space camera basis; no device or network APIs here.
        public void Tick(TurtleCommand command, Vector3 forward, float deltaTime)
        {
            if (!enabled || deltaTime <= 0f) return;
            TurtleMovementMode previousMode = locomotion.Mode;
            Vector3 bodyCenter = transform.TransformPoint(controller.center);
            locomotion.Evaluate(bodyCenter);
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector2 move = Vector2.ClampMagnitude(command.Move, 1f);
            Vector3 direction = forward * move.y + right * move.x;
            bool swimming = Mode == TurtleMovementMode.Water;
            if (swimming && previousMode != TurtleMovementMode.Water)
                velocity.y = Mathf.Clamp(velocity.y, -settings.VerticalSwimSpeed, settings.VerticalSwimSpeed);
            float speed = swimming ? settings.SwimSpeed : settings.WalkSpeed;
            float acceleration = swimming ? settings.WaterAcceleration : settings.Acceleration;
            Vector3 horizontal = Vector3.MoveTowards(new Vector3(velocity.x, 0f, velocity.z),
                direction * speed, acceleration * deltaTime);
            velocity.x = horizontal.x;
            velocity.z = horizontal.z;
            if (swimming)
                velocity.y = Mathf.MoveTowards(velocity.y,
                    Mathf.Clamp(command.Vertical, -1f, 1f) * settings.VerticalSwimSpeed,
                    settings.WaterAcceleration * deltaTime);
            else if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
            else velocity.y -= settings.Gravity * deltaTime;

            Vector3 displacement = velocity * deltaTime;
            if (swimming)
            {
                WaterVolume water = locomotion.ActiveWater;
                float ascent = Vector3.Dot(displacement, water.SurfaceNormal);
                float available = Mathf.Max(0f, -water.SurfaceDistance(bodyCenter) - 0.02f);
                if (ascent > available)
                {
                    displacement -= water.SurfaceNormal * (ascent - available);
                    if (velocity.y > 0f) velocity.y = 0f;
                }
            }
            Vector3 positionBeforeMove = transform.position;
            CollisionFlags collisions = controller.Move(displacement);
            ActualVelocity = (transform.position - positionBeforeMove) / deltaTime;
            if ((collisions & CollisionFlags.Above) != 0 && velocity.y > 0f) velocity.y = 0f;
            if ((collisions & CollisionFlags.Below) != 0 && velocity.y < 0f) velocity.y = 0f;
            locomotion.Evaluate(transform.TransformPoint(controller.center));
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(direction), settings.TurnSpeed * deltaTime);
        }

        public void Teleport(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
            velocity = Vector3.zero;
            ActualVelocity = Vector3.zero;
            locomotion.ResetState();
        }
    }
}

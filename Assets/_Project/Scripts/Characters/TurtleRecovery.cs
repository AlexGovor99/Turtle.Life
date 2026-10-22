using UnityEngine;

namespace TurtleLife.Characters
{
    [RequireComponent(typeof(TurtleMotor))]
    public sealed class TurtleRecovery : MonoBehaviour
    {
        [SerializeField] private float fallLimit = -8f;
        private TurtleMotor motor;
        private TurtleControls controls;
        private Vector3 checkpoint;
        private bool configured;

        public void Configure(Vector3 position)
        {
            checkpoint = position;
            configured = true;
        }

        private void Awake()
        {
            motor = GetComponent<TurtleMotor>();
            controls = GetComponent<TurtleControls>();
            if (!configured) checkpoint = transform.position;
        }

        private void Update()
        {
            if (transform.position.y < fallLimit || (controls != null && controls.isActiveAndEnabled
                && !controls.IsRebinding && Cursor.lockState == CursorLockMode.Locked
                && controls.Respawn != null && controls.Respawn.WasPressedThisFrame()))
                ReturnToCheckpoint();
        }

        public void ReturnToCheckpoint() => motor.Teleport(checkpoint);
    }
}

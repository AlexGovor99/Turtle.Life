using UnityEngine;

namespace TurtleLife.Characters
{
    [RequireComponent(typeof(TurtleMotor), typeof(TurtleControls))]
    public sealed class TurtleInput : MonoBehaviour
    {
        [SerializeField] private Transform view;
        private TurtleMotor motor;
        private TurtleControls controls;
        public void Configure(Transform cameraTransform)
        {
            view = cameraTransform;
        }

        private void Awake()
        {
            motor = GetComponent<TurtleMotor>();
            controls = GetComponent<TurtleControls>();
        }

        private void Update()
        {
            TurtleCommand command = default;
            if (controls.isActiveAndEnabled && !controls.IsRebinding && !controls.PointerCaptureBlocked && Cursor.lockState == CursorLockMode.Locked)
            {
                command.Move = controls.Move.ReadValue<Vector2>();
                command.Vertical = controls.Swim.ReadValue<float>();
            }
            motor.Tick(command, view != null ? view.forward : Vector3.forward, Time.deltaTime);
        }

    }
}

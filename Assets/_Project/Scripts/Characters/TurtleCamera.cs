using UnityEngine;

namespace TurtleLife.Characters
{
    public sealed class TurtleCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 5f;
        [SerializeField] private float sensitivity = 0.14f;
        [SerializeField] private LayerMask obstructionMask = 1;
        private float yaw;
        private float pitch = 28f;
        private int captureFrame;
        private TurtleControls controls;
        private readonly RaycastHit[] hits = new RaycastHit[16];
        public void Configure(Transform value) => target = value;

        private void OnEnable() => Capture();
        private void OnDisable() => Release();
        private void OnApplicationFocus(bool focused) { if (!focused) Release(); }
        private void Capture()
        {
            captureFrame = Time.frameCount;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        private static void Release() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        private void LateUpdate()
        {
            if (target == null) return;
            if (controls == null) controls = target.GetComponent<TurtleControls>();
            bool canRead = controls != null && controls.isActiveAndEnabled && !controls.IsRebinding;
            if (canRead && controls.ReleaseCursor.WasPressedThisFrame()) Release();
            else if (canRead && !controls.PointerCaptureBlocked && Cursor.lockState != CursorLockMode.Locked && controls.CaptureCursor.WasPressedThisFrame()) Capture();
            if (Cursor.lockState == CursorLockMode.Locked && canRead && Time.frameCount > captureFrame + 1)
            {
                Vector2 delta = controls.Look.ReadValue<Vector2>();
                Vector2 look = delta * sensitivity + controls.LookStick.ReadValue<Vector2>() * (150f * Time.deltaTime);
                yaw += look.x;
                pitch = Mathf.Clamp(pitch - look.y, -15f, 75f);
                distance = Mathf.Clamp(distance - controls.Zoom.ReadValue<float>() * 0.005f, 2f, 8f);
            }
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * 0.65f;
            Vector3 direction = rotation * Vector3.back;
            float actualDistance = distance;
            int count = Physics.SphereCastNonAlloc(pivot, 0.18f, direction, hits, distance,
                obstructionMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (hits[i].transform == target || hits[i].transform.IsChildOf(target)) continue;
                actualDistance = Mathf.Min(actualDistance, Mathf.Max(0.15f, hits[i].distance - 0.05f));
            }
            transform.SetPositionAndRotation(pivot + direction * actualDistance, rotation);
        }
    }
}

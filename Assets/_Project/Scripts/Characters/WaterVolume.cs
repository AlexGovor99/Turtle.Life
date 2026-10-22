using UnityEngine;

namespace TurtleLife.Characters
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class WaterVolume : MonoBehaviour
    {
        private BoxCollider volume;

        private void Awake() => volume = GetComponent<BoxCollider>();

        public Vector3 SurfaceNormal => transform.up;

        public float SurfaceDistance(Vector3 point)
        {
            if (volume == null) volume = GetComponent<BoxCollider>();
            Vector3 surface = transform.TransformPoint(volume.center + Vector3.up * (volume.size.y * 0.5f));
            return Vector3.Dot(point - surface, SurfaceNormal);
        }

        public bool Contains(Vector3 point) => Contains(point, 0f);

        public bool Contains(Vector3 point, float surfaceMargin)
        {
            if (volume == null) volume = GetComponent<BoxCollider>();
            if (!isActiveAndEnabled || !volume.enabled) return false;
            Vector3 local = transform.InverseTransformPoint(point) - volume.center;
            Vector3 half = volume.size * 0.5f;
            float localMargin = surfaceMargin / Mathf.Max(transform.lossyScale.y, 0.001f);
            return Mathf.Abs(local.x) < half.x && local.y > -half.y && local.y <= half.y + localMargin
                && Mathf.Abs(local.z) < half.z;
        }
    }
}

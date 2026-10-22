using UnityEngine;

namespace TurtleLife.Characters
{
    public enum TurtleMovementMode { Land, Water }

    public sealed class TurtleLocomotion : MonoBehaviour
    {
        [SerializeField] private WaterVolume[] waterVolumes = new WaterVolume[0];
        [SerializeField, Min(0f)] private float entryDepth = 0.04f;
        [SerializeField, Min(0f)] private float exitHeight = 0.12f;
        public TurtleMovementMode Mode { get; private set; }
        public WaterVolume ActiveWater { get; private set; }

        public void SetWaterVolumes(WaterVolume[] volumes) => waterVolumes = volumes;

        public void Evaluate(Vector3 bodyCenter)
        {
            // Retain the current volume near the surface, but require immersion to enter it.
            if (ActiveWater != null && ActiveWater.Contains(bodyCenter, exitHeight))
            {
                Mode = TurtleMovementMode.Water;
                return;
            }
            ActiveWater = null;
            Mode = TurtleMovementMode.Land;
            foreach (WaterVolume volume in waterVolumes)
            {
                if (volume != null && volume.Contains(bodyCenter, -entryDepth))
                {
                    ActiveWater = volume;
                    Mode = TurtleMovementMode.Water;
                    return;
                }
            }
        }

        public void ResetState()
        {
            ActiveWater = null;
            Mode = TurtleMovementMode.Land;
        }
    }
}

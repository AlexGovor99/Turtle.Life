using UnityEngine;
using UnityEngine.InputSystem;

namespace TurtleLife.Characters
{
    public sealed class CharacterDemoHud : MonoBehaviour
    {
        [SerializeField] private TurtleMotor turtle;
        private TurtleControls controls;
        private bool settingsOpen;
        public void Configure(TurtleMotor value) => turtle = value;

        private void Update()
        {
            if (turtle == null) return;
            if (controls == null) controls = turtle.GetComponent<TurtleControls>();
            if (controls != null && controls.Settings != null && !controls.IsRebinding
                && controls.Settings.WasPressedThisFrame())
                ToggleSettings();
        }

        private void ToggleSettings()
        {
            settingsOpen = !settingsOpen;
            controls.PointerCaptureBlocked = settingsOpen;
            if (settingsOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnDisable()
        {
            if (controls != null) controls.PointerCaptureBlocked = false;
        }
        private void OnGUI()
        {
            if (turtle == null) return;
            GUILayout.BeginArea(new Rect(16, 16, 440, settingsOpen ? 430 : 205), GUI.skin.box);
            GUILayout.Label("TURTLE.LIFE | Character prototype");
            GUILayout.Label("WASD / Arrows: move | Mouse: look | Wheel: zoom");
            GUILayout.Label("Water: Space to ascend, Ctrl / C to descend");
            GUILayout.Label("R: return to shore | Esc: release cursor | Click: capture");
            GUILayout.Label($"Mode: {turtle.Mode}   Speed: {turtle.Speed:0.0} m/s");
            GUILayout.Label("F1 / Gamepad Y: bindings | Sticks: move / look | LB/RB: dive / rise");
            GUI.enabled = controls != null && !controls.IsRebinding;
            if (GUILayout.Button(settingsOpen ? "Close bindings" : "Bindings (F1 / Y)")) ToggleSettings();
            GUI.enabled = true;
            if (settingsOpen && controls != null)
            {
                GUILayout.Label(controls.IsRebinding ? "Press a new key or button. Esc cancels." : "Select a binding to change:");
                GUI.enabled = !controls.IsRebinding;
                Binding("Forward", "Move", controls.Move, 1);
                Binding("Backward", "Move", controls.Move, 2);
                Binding("Left", "Move", controls.Move, 3);
                Binding("Right", "Move", controls.Move, 4);
                Binding("Ascend", "Swim", controls.Swim, 2);
                Binding("Descend", "Swim", controls.Swim, 1);
                Binding("Return to shore", "Respawn", controls.Respawn, 0);
                if (GUILayout.Button("Reset bindings")) controls.ResetBindings();
                GUI.enabled = true;
            }
            GUILayout.EndArea();
        }

        private void Binding(string label, string actionName, UnityEngine.InputSystem.InputAction action, int index)
        {
            if (GUILayout.Button($"{label}: {action.GetBindingDisplayString(index)}")) controls.Rebind(actionName, index);
        }
    }
}

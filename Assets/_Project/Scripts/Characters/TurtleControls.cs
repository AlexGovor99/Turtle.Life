using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TurtleLife.Characters
{
    // Each local player owns an independent action instance and binding overrides.
    public sealed class TurtleControls : MonoBehaviour
    {
        [SerializeField] private InputActionAsset bindings;
        private InputActionAsset actions;
        private InputActionRebindingExtensions.RebindingOperation rebinding;
        private const string OverridesKey = "TurtleLife.InputOverrides";
        public InputAction Move { get; private set; }
        public InputAction Swim { get; private set; }
        public InputAction Look { get; private set; }
        public InputAction LookStick { get; private set; }
        public InputAction Zoom { get; private set; }
        public InputAction Respawn { get; private set; }
        public InputAction ReleaseCursor { get; private set; }
        public InputAction CaptureCursor { get; private set; }
        public InputAction Settings { get; private set; }
        public bool PointerCaptureBlocked { get; set; }
        public bool IsRebinding => rebinding != null;

        public void Configure(InputActionAsset asset) => bindings = asset;

        private void Awake()
        {
            if (bindings == null)
            {
                Debug.LogError("TurtleControls requires an Input Actions asset.", this);
                enabled = false;
                return;
            }
            actions = Instantiate(bindings);
            if (PlayerPrefs.HasKey(OverridesKey))
                actions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(OverridesKey));
            Move = actions.FindAction("Turtle/Move", true);
            Swim = actions.FindAction("Turtle/Swim", true);
            Look = actions.FindAction("Turtle/Look", true);
            LookStick = actions.FindAction("Turtle/LookStick", true);
            Zoom = actions.FindAction("Turtle/Zoom", true);
            Respawn = actions.FindAction("Turtle/Respawn", true);
            ReleaseCursor = actions.FindAction("Turtle/ReleaseCursor", true);
            CaptureCursor = actions.FindAction("Turtle/CaptureCursor", true);
            Settings = actions.FindAction("Turtle/Settings", true);
        }

        private void OnEnable() => actions?.Enable();
        private void OnDisable()
        {
            rebinding?.Cancel();
            actions?.Disable();
        }

        public void Rebind(string actionName, int bindingIndex, Action completed = null)
        {
            if (actions == null || rebinding != null) return;
            InputAction action = actions.FindAction($"Turtle/{actionName}", true);
            if (bindingIndex < 0 || bindingIndex >= action.bindings.Count || action.bindings[bindingIndex].isComposite)
                throw new ArgumentOutOfRangeException(nameof(bindingIndex));
            actions.Disable();
            rebinding = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(operation => FinishRebind(operation, true, completed))
                .OnCancel(operation => FinishRebind(operation, false, completed));
            rebinding.Start();
        }

        private void FinishRebind(InputActionRebindingExtensions.RebindingOperation operation, bool save, Action completed)
        {
            operation.Dispose();
            rebinding = null;
            if (save)
            {
                PlayerPrefs.SetString(OverridesKey, actions.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();
            }
            if (isActiveAndEnabled) actions.Enable();
            completed?.Invoke();
        }

        public void ResetBindings()
        {
            if (actions == null || IsRebinding) return;
            actions.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey(OverridesKey);
            PlayerPrefs.Save();
        }

        public void CancelRebind() => rebinding?.Cancel();

        private void OnDestroy()
        {
            CancelRebind();
            if (actions != null) Destroy(actions);
        }
    }
}

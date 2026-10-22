using System.Collections;
using NUnit.Framework;
using TurtleLife.Characters;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TurtleLife.Tests
{
    public sealed class CharacterTests
    {
        private TurtleMotor motor;
        private TurtleControls controls;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("CharacterDemo");
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var camera = root.GetComponent<TurtleCamera>();
                if (camera != null) camera.enabled = false;
                var hud = root.GetComponent<CharacterDemoHud>();
                if (hud != null) hud.enabled = false;
                if (motor == null) motor = root.GetComponentInChildren<TurtleMotor>();
            }
            Assert.That(motor, Is.Not.Null);
            motor.GetComponent<TurtleInput>().enabled = false;
            motor.GetComponent<TurtleRecovery>().enabled = false;
            controls = motor.GetComponent<TurtleControls>();
            Physics.SyncTransforms();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            motor = null;
            controls = null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator RampCanBeTraversedIntoWaterAndBack()
        {
            motor.Teleport(new Vector3(0, 0.05f, -4));
            bool enteredWater = false;
            for (int i = 0; i < 200; i++)
            {
                motor.Tick(new TurtleCommand { Move = Vector2.up }, Vector3.forward, 1f / 60f);
                enteredWater |= motor.Mode == TurtleMovementMode.Water;
                yield return null;
            }
            Assert.That(enteredWater, Is.True, "Must enter water by walking down the ramp.");
            Assert.That(motor.transform.position.z, Is.GreaterThan(3f));
            for (int i = 0; i < 220; i++)
            {
                motor.Tick(new TurtleCommand { Move = Vector2.down }, Vector3.forward, 1f / 60f);
                yield return null;
            }
            Assert.That(motor.Mode, Is.EqualTo(TurtleMovementMode.Land));
            Assert.That(motor.transform.position.z, Is.LessThan(-1.5f));
            Assert.That(motor.transform.position.y, Is.GreaterThan(-0.1f));
        }

        [UnityTest]
        public IEnumerator HoldingAscendStaysAtSurfaceWithoutModeFlapping()
        {
            motor.Teleport(new Vector3(0, -1f, 5));
            for (int i = 0; i < 240; i++)
            {
                motor.Tick(new TurtleCommand { Vertical = 1 }, Vector3.forward, 1f / 60f);
                Assert.That(motor.Mode, Is.EqualTo(TurtleMovementMode.Water), $"Frame {i}");
                Assert.That(motor.transform.position.y + 0.45f, Is.LessThanOrEqualTo(0.501f));
                yield return null;
            }
            float surfaceY = motor.transform.position.y;
            for (int i = 0; i < 60; i++)
            {
                motor.Tick(default, Vector3.forward, 1f / 60f);
                yield return null;
            }
            Assert.That(motor.transform.position.y, Is.EqualTo(surfaceY).Within(0.03f));
            Step(new TurtleCommand { Vertical = -1 }, 60);
            Assert.That(motor.transform.position.y, Is.LessThan(surfaceY - 0.3f));
        }

        [Test]
        public void SpeedReflectsWallCollisionAndVerticalSwimming()
        {
            motor.Teleport(new Vector3(5, 0.05f, -6));
            Step(new TurtleCommand { Move = Vector2.right }, 120);
            Assert.That(motor.transform.position.x, Is.LessThan(6.5f));
            Assert.That(motor.Speed, Is.LessThan(0.05f));
            motor.Teleport(new Vector3(0, -1f, 5));
            Step(new TurtleCommand { Vertical = 1 }, 15);
            Assert.That(motor.ActualVelocity.y, Is.GreaterThan(0.5f));
            Assert.That(motor.Speed, Is.LessThan(0.05f));
        }

        [Test]
        public void DiagonalMovementDoesNotIncreaseSpeed()
        {
            Vector3 start = new Vector3(0, 0.05f, -6);
            motor.Teleport(start);
            Step(new TurtleCommand { Move = Vector2.up }, 60);
            float straight = Vector3.Distance(start, motor.transform.position);
            motor.Teleport(start);
            Step(new TurtleCommand { Move = Vector2.one }, 60);
            Assert.That(Vector3.Distance(start, motor.transform.position), Is.EqualTo(straight).Within(0.12f));
        }

        [UnityTest]
        public IEnumerator RecoveryWorksWithInputDisabled()
        {
            var recovery = motor.GetComponent<TurtleRecovery>();
            Vector3 checkpoint = new Vector3(0, 0.1f, -5);
            recovery.Configure(checkpoint);
            recovery.enabled = true;
            motor.Teleport(new Vector3(0, -10, -5));
            yield return null;
            yield return null;
            Assert.That(Vector3.Distance(motor.transform.position, checkpoint), Is.LessThan(0.01f));
        }

        [Test]
        public void GamepadActionsProvideMovementAndSwimming()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                InputSystem.QueueStateEvent(gamepad, new GamepadState
                {
                    leftStick = Vector2.up,
                    buttons = 1u << (int)GamepadButton.RightShoulder
                });
                InputSystem.Update();
                Assert.That(controls.Move.ReadValue<Vector2>().y, Is.GreaterThan(0.9f));
                Assert.That(controls.Swim.ReadValue<float>(), Is.GreaterThan(0.9f));
            }
            finally { InputSystem.RemoveDevice(gamepad); }
        }

        [UnityTest]
        public IEnumerator KeyboardBindingCanBeChangedAndRestored()
        {
            const string key = "TurtleLife.InputOverrides";
            bool hadOverrides = PlayerPrefs.HasKey(key);
            string previousOverrides = PlayerPrefs.GetString(key);
            var keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                controls.Rebind("Move", 1);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.I));
                InputSystem.Update();
                yield return new WaitForSecondsRealtime(0.15f);
                Assert.That(controls.IsRebinding, Is.False);
                Assert.That(controls.Move.bindings[1].effectivePath, Is.EqualTo("<Keyboard>/i"));
                Assert.That(PlayerPrefs.HasKey(key), Is.True);
                controls.ResetBindings();
                Assert.That(controls.Move.bindings[1].effectivePath, Is.EqualTo("<Keyboard>/w"));
            }
            finally
            {
                controls.CancelRebind();
                InputSystem.RemoveDevice(keyboard);
                if (hadOverrides) PlayerPrefs.SetString(key, previousOverrides);
                else PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }

        private void Step(TurtleCommand command, int count)
        {
            Physics.SyncTransforms();
            for (int i = 0; i < count; i++) motor.Tick(command, Vector3.forward, 1f / 60f);
        }
    }
}

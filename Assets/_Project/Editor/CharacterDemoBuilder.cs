using System.Collections.Generic;
using System.IO;
using TurtleLife.Characters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TurtleLife.Editor
{
    // One-time setup of reviewable assets; nothing is generated at runtime.
    public static class CharacterDemoBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/CharacterDemo.unity";
        private const string Root = "Assets/_Project";

        [MenuItem("Turtle Life/Create Character Demo (if missing)")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (File.Exists(ScenePath)) return;
            foreach (string folder in new[] { "Scenes", "Prefabs", "Configs", "Art/Materials" })
                Directory.CreateDirectory($"{Root}/{folder}");
            AssetDatabase.Refresh();
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                Material shell = Material("Shell", new Color(0.22f, 0.36f, 0.12f));
                Material skin = Material("Skin", new Color(0.48f, 0.66f, 0.26f));
                Material belly = Material("Belly", new Color(0.7f, 0.62f, 0.32f));
                Material eyes = Material("Eyes", new Color(0.035f, 0.045f, 0.03f));
                Material sand = Material("Sand", new Color(0.72f, 0.65f, 0.44f));
                Material stone = Material("Stone", new Color(0.37f, 0.45f, 0.43f));
                Material waterMaterial = Material("WaterSurface", new Color(0.08f, 0.53f, 0.68f, 0.22f), true);
                var settings = AssetDatabase.LoadAssetAtPath<TurtleSettings>($"{Root}/Configs/TurtleSettings.asset");
                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<TurtleSettings>();
                    AssetDatabase.CreateAsset(settings, $"{Root}/Configs/TurtleSettings.asset");
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/Prefabs/Turtle.prefab");
                if (prefab == null)
                {
                    var turtle = new GameObject("Turtle");
                    var controller = turtle.AddComponent<CharacterController>();
                    controller.center = new Vector3(0f, 0.45f, 0f);
                    controller.height = 0.85f;
                    controller.radius = 0.38f;
                    controller.stepOffset = 0.22f;
                    controller.slopeLimit = 50f;
                    controller.skinWidth = 0.035f;
                    turtle.AddComponent<TurtleLocomotion>();
                    var motor = turtle.AddComponent<TurtleMotor>();
                    motor.Configure(settings);
                    turtle.AddComponent<TurtleInput>();
                    turtle.GetComponent<TurtleControls>().Configure(AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>($"{Root}/Input/TurtleControls.inputactions"));
                    turtle.AddComponent<TurtleRecovery>();
                    var presentation = turtle.AddComponent<TurtlePresentation>();
                    Part(turtle.transform, "Belly", new Vector3(0, 0.35f, 0), new Vector3(0.78f, 0.28f, 1.02f), belly);
                    Part(turtle.transform, "Shell", new Vector3(0, 0.52f, -0.06f), new Vector3(0.92f, 0.52f, 1.16f), shell);
                    Part(turtle.transform, "Head", new Vector3(0, 0.46f, 0.66f), new Vector3(0.4f, 0.34f, 0.42f), skin);
                    for (int side = -1; side <= 1; side += 2)
                        Part(turtle.transform, "Eye", new Vector3(side * 0.16f, 0.55f, 0.78f), new Vector3(0.075f, 0.075f, 0.075f), eyes);
                    var flippers = new List<Transform>();
                    for (int i = 0; i < 4; i++)
                    {
                        float side = i % 2 == 0 ? -1 : 1;
                        Transform limb = Part(turtle.transform, "Flipper", new Vector3(side * 0.47f, 0.1f, i < 2 ? 0.32f : -0.39f),
                            new Vector3(0.38f, 0.2f, 0.32f), skin);
                        flippers.Add(limb);
                    }
                    Part(turtle.transform, "Tail", new Vector3(0, 0.28f, -0.66f), new Vector3(0.14f, 0.12f, 0.32f), skin);
                    presentation.Configure(flippers.ToArray());
                    prefab = PrefabUtility.SaveAsPrefabAsset(turtle, $"{Root}/Prefabs/Turtle.prefab");
                    Object.DestroyImmediate(turtle);
                }
                GameObject turtleInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                Vector3 spawn = new Vector3(0, 0.1f, -5f);
                turtleInstance.transform.position = spawn;

                Box("Shore", new Vector3(0, -0.5f, -5), new Vector3(14, 1, 8), sand);
                Box("PoolFloor", new Vector3(0, -2.5f, 4), new Vector3(14, 1, 10), sand);
                GameObject ramp = Box("Ramp", new Vector3(0, -1f, 0.8f), new Vector3(5, 0.3f, 5.7f), sand);
                ramp.transform.rotation = Quaternion.Euler(20, 0, 0);
                Box("LeftWall", new Vector3(-7, -0.25f, 0), new Vector3(0.4f, 4.5f, 18), stone);
                Box("RightWall", new Vector3(7, -0.25f, 0), new Vector3(0.4f, 4.5f, 18), stone);
                Box("BackWall", new Vector3(0, -0.25f, 9), new Vector3(14, 4.5f, 0.4f), stone);
                Box("FrontWall", new Vector3(0, -0.25f, -9), new Vector3(14, 4.5f, 0.4f), stone);
                Box("LowStep", new Vector3(-3, 0.08f, -4), new Vector3(1.8f, 0.16f, 1.2f), stone);
                Box("Obstacle", new Vector3(3, 0.55f, -4), new Vector3(1.2f, 1.1f, 1.2f), stone);
                Box("UnderwaterRock", new Vector3(3, -1.5f, 5), new Vector3(2, 1, 2), stone);

                var water = new GameObject("WaterVolume");
                water.transform.position = new Vector3(0, -0.85f, 4);
                var volumeCollider = water.AddComponent<BoxCollider>();
                volumeCollider.size = new Vector3(13.6f, 2.7f, 10);
                volumeCollider.isTrigger = true;
                var volume = water.AddComponent<WaterVolume>();
                turtleInstance.GetComponent<TurtleLocomotion>().SetWaterVolumes(new[] { volume });
                GameObject surface = Box("WaterSurface", new Vector3(0, 0.5f, 4), new Vector3(13.6f, 0.015f, 10), waterMaterial);
                Object.DestroyImmediate(surface.GetComponent<Collider>());
                surface.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                surface.GetComponent<Renderer>().receiveShadows = false;

                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                var camera = cameraObject.AddComponent<Camera>();
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 150f;
                cameraObject.AddComponent<AudioListener>();
                cameraObject.AddComponent<TurtleCamera>().Configure(turtleInstance.transform);
                cameraObject.transform.SetPositionAndRotation(spawn + new Vector3(0, 3, -5), Quaternion.Euler(28, 0, 0));
                turtleInstance.GetComponent<TurtleInput>().Configure(cameraObject.transform);
                var hud = new GameObject("Demo HUD").AddComponent<CharacterDemoHud>();
                hud.Configure(turtleInstance.GetComponent<TurtleMotor>());
                var light = new GameObject("Sun").AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 2f;
                light.shadows = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(50, -30, 0);
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.55f, 0.65f, 0.7f);
                EditorSceneManager.SaveScene(scene, ScenePath);
                var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                scenes.RemoveAll(s => s.path == ScenePath);
                scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
                AssetDatabase.SaveAssets();
                Debug.Log($"Character prototype ready: {ScenePath}");
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private static Material Material(string name, Color color, bool transparent = false)
        {
            string path = $"{Root}/Art/Materials/{name}.mat";
            var result = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (result != null) return result;
            result = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            result.SetColor("_BaseColor", color);
            result.SetFloat("_Smoothness", transparent ? 0.7f : 0.2f);
            if (transparent)
            {
                result.SetFloat("_Surface", 1f);
                result.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                result.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                result.SetFloat("_ZWrite", 0f);
                result.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                result.renderQueue = 3000;
            }
            AssetDatabase.CreateAsset(result, path);
            return result;
        }

        private static Transform Part(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            return part.transform;
        }

        private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box;
        }
    }
}

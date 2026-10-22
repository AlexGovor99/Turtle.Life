using System.IO;
using NUnit.Framework;
using TurtleLife.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TurtleLife.Tests
{
    public sealed class DemoBuilderTests
    {
        [Test]
        public void MissingSceneDoesNotCausePrefabOverwrite()
        {
            const string prefab = "Assets/_Project/Prefabs/Turtle.prefab";
            const string originalScene = CharacterDemoBuilder.ScenePath;
            const string temporaryScene = "Assets/_Project/Scenes/CharacterDemoPreservationTest.unity";
            Assert.That(File.Exists(temporaryScene), Is.False);
            byte[] originalPrefab = File.ReadAllBytes(prefab);
            var buildScenes = EditorBuildSettings.scenes;
            // EditMode's runner starts in an untitled scene; generation requires a saved scene context.
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
            Assert.That(AssetDatabase.MoveAsset(originalScene, temporaryScene), Is.Empty);
            try
            {
                CharacterDemoBuilder.Create();
                Assert.That(File.Exists(originalScene), Is.True);
                Assert.That(File.ReadAllBytes(prefab), Is.EqualTo(originalPrefab), "Existing prefab must be reused verbatim.");
            }
            finally
            {
                File.WriteAllBytes(prefab, originalPrefab);
                AssetDatabase.DeleteAsset(originalScene);
                Assert.That(AssetDatabase.MoveAsset(temporaryScene, originalScene), Is.Empty);
                EditorBuildSettings.scenes = buildScenes;
                AssetDatabase.Refresh();
            }
        }

        [Test]
        public void GeneratorDoesNotRunOnEditorLoad()
        {
            Assert.That(System.Attribute.IsDefined(typeof(CharacterDemoBuilder), typeof(InitializeOnLoadAttribute)), Is.False);
        }
    }
}

using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace TurtleLife.Editor
{
    [InitializeOnLoad]
    public static class CharacterTestRunner
    {
        private static TestRunnerApi api;
        private const string RunningKey = "TurtleLife.CharacterTestsRunning";
        private const string ResultsKey = "TurtleLife.CharacterTestResultsPath";

        static CharacterTestRunner()
        {
            api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new ResultCallbacks());
        }

        [MenuItem("Turtle Life/Run Character PlayMode Tests")]
        public static void Run()
        {
            if (SessionState.GetBool(RunningKey, false)) return;
            SessionState.SetBool(RunningKey, true);
            SessionState.SetString(ResultsKey, "TestResults/CharacterTests.xml");
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.PlayMode,
                assemblyNames = new[] { "TurtleLife.PlayModeTests" }
            }));
        }

        [MenuItem("Turtle Life/Run Character EditMode Tests")]
        public static void RunEditorTests()
        {
            if (SessionState.GetBool(RunningKey, false)) return;
            SessionState.SetBool(RunningKey, true);
            SessionState.SetString(ResultsKey, "TestResults/CharacterEditorTests.xml");
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "TurtleLife.EditModeTests" }
            }) { runSynchronously = true });
        }

        private sealed class ResultCallbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                if (!SessionState.GetBool(RunningKey, false)) return;
                SessionState.SetBool(RunningKey, false);
                Directory.CreateDirectory("TestResults");
                TestRunnerApi.SaveResultToFile(result, SessionState.GetString(ResultsKey, "TestResults/CharacterTests.xml"));
                Debug.Log($"CHARACTER TESTS: passed={result.PassCount}, failed={result.FailCount}, skipped={result.SkipCount}");
            }
        }
    }
}

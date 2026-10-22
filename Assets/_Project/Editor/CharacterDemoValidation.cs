using UnityEditor;

namespace TurtleLife.Editor
{
    public static class CharacterDemoValidation
    {
        [MenuItem("Turtle Life/Validate Character Demo (PlayMode Tests)")]
        public static void Run() => CharacterTestRunner.Run();
    }
}

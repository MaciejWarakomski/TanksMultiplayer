using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Editor
{
    [InitializeOnLoad]
    public static class StartupSceneLoader
    {
        static StartupSceneLoader()
        {
            EditorApplication.playModeStateChanged += LoadStartupScene;
        }

        private static void LoadStartupScene(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                {
                    if (SceneManager.GetActiveScene().buildIndex != 0)
                    {
                        SceneManager.LoadScene(0);
                    }

                    break;
                }
            }
        }
    }
}
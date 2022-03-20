using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com.MorganHouston.Imprecision
{

    public static class SceneLoader
    {

        public static int levelToLoad = 0;

        public static void LoadThisScene(int sceneToLoad)
        {
#if UNITY_WSA
            ResetLightingData();
#endif
            levelToLoad = sceneToLoad;
            SceneManager.LoadScene(levelToLoad);
        }

        public static Scene GetCurrentScene()
        {
            return SceneManager.GetActiveScene();
        }

        private static void ResetLightingData()
        {
            LightmapSettings.lightmaps = new LightmapData[0];
            Resources.UnloadUnusedAssets();
        }

    }

}
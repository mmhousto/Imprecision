using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com.MorganHouston.Imprecision
{

    public static class SceneLoader
    {

        public static int levelToLoad = 1;

        public static void LoadThisScene(int sceneToLoad)
        {
            levelToLoad = sceneToLoad;
            SceneManager.LoadSceneAsync(0);
        }

        public static Scene GetCurrentScene()
        {
            return SceneManager.GetActiveScene();
        }

        public static void ResetLightingData()
        {
            LightmapSettings.lightmaps = new LightmapData[0];
            Resources.UnloadUnusedAssets();
        }

    }

}
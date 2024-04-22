using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using static UnityEngine.Rendering.HDROutputUtils;

namespace Com.MorganHouston.Imprecision
{
    public class LoadLevel : MonoBehaviour
    {
        public Slider slider;
        public TextMeshProUGUI progressText;
        public TextMeshProUGUI loadingText;
        public GameObject loadingCanvas;
        public GameObject loadingCamera;
        private int dots = 1;
        private float progress;
        private Scene sceneToUnload;
        public static List<AsyncOperation> operations;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
                
        }

        void Start()
        {
            /*#if UNITY_WSA
                        SceneLoader.ResetLightingData();
            #endif*/
            sceneToUnload = SceneLoader.GetCurrentScene();
            StartCoroutine(LoadAsynchronously(SceneLoader.levelToLoad));

            //InvokeRepeating(nameof(UpdateLoadingText), 0.0f, 1f);
            //SceneManager.LoadScene(SceneLoader.levelToLoad);
        }

        /*void Update()
        {
            progress += Time.deltaTime / 10;
            progress = Mathf.Clamp(progress, 0f, 0.9999f);

            slider.value = progress * 100f;
            progressText.text = progress * 100f + "%";
        }*/

        IEnumerator LoadAsynchronously(int index)
        {
            yield return null;

            AsyncOperation load = SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);

            while (!load.isDone)
            {
                float progress = Mathf.Clamp(load.progress / .9f, 0f, 0.9999f);

                slider.value = progress * 100f;
                progressText.text = progress * 100f + "%";

                yield return null;
            }

            //SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(index));

            yield return load;

            AsyncOperation unload = SceneManager.UnloadSceneAsync(sceneToUnload);

            yield return unload;

            /*AsyncOperation unloadAssets = Resources.UnloadUnusedAssets();

            while (!unloadAssets.isDone)
            {
                float progress = Mathf.Clamp(unloadAssets.progress / .9f, 0f, 0.9999f);

                slider.value = progress * 100f;
                progressText.text = progress * 100f + "%";

                yield return null;
            }

            yield return unloadAssets;*/
            if(unload.isDone)
                Destroy(this.gameObject);
        }

        IEnumerator EndLoadAsync(int index, AsyncOperation loadingScene)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            while (!operation.isDone)
            {
                yield return null;
            }

            // Activate the new scene
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(index));
        }

        IEnumerator EndLoadAsync(Scene sceneToUnLoad)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneToUnLoad);

            while (!operation.isDone)
            {

                yield return null;
            }


            // Activate the new scene
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(0));
            StartCoroutine(LoadAsynchronously(SceneLoader.levelToLoad));
        }

        void UpdateLoadingText()
        {
            dots++;
            if (dots > 3)
            {
                dots = 1;
            }

            switch (dots)
            {
                case 1:
                    loadingText.text = "Patience young Örvar.";
                    break;
                case 2:
                    loadingText.text = "Patience young Örvar..";
                    break;
                case 3:
                    loadingText.text = "Patience young Örvar...";
                    break;
                default:
                    loadingText.text = "Patience young Örvar...";
                    break;
            }

        }
    }
}
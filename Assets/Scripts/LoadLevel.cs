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

        private void Awake()
        {
            //StartCoroutine(LoadAsynchronously(SceneLoader.levelToLoad));
                
        }

        void Start()
        {
            //InvokeRepeating(nameof(UpdateLoadingText), 0.0f, 1f);
            SceneLoader.ResetLightingData();
            SceneManager.LoadScene(SceneLoader.levelToLoad);
        }

        void Update()
        {
            progress += Time.deltaTime / 10;
            progress = Mathf.Clamp(progress, 0f, 0.9999f);

            slider.value = progress * 100f;
            progressText.text = progress * 100f + "%";
        }

        IEnumerator LoadAsynchronously(int index)
        {

            AsyncOperation operation = SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp(operation.progress / .9f, 0f, 0.9999f);

                slider.value = progress * 100f;
                progressText.text = progress * 100f + "%";

                yield return null;
            }

            DestroyImmediate(loadingCanvas);
            DestroyImmediate(loadingCamera);
            StartCoroutine(EndLoadAsync(index));
            
        }

        IEnumerator EndLoadAsync(int index)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            while (!operation.isDone)
            {
                yield return null;
            }
            operation.allowSceneActivation = true;

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
                    loadingText.text = "Loading.";
                    break;
                case 2:
                    loadingText.text = "Loading..";
                    break;
                case 3:
                    loadingText.text = "Loading...";
                    break;
                default:
                    loadingText.text = "Loading...";
                    break;
            }

        }
    }
}
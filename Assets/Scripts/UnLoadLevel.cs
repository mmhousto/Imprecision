using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Com.MorganHouston.Imprecision
{
    public class UnLoadLevel : MonoBehaviour
    {
        private static UnLoadLevel instance;

        public static UnLoadLevel Instance { get { return instance; } }

        private Scene currentSceneToUnload;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(Instance.gameObject);
            }
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void LoadUnLoad(int level)
        {
            SceneLoader.levelToLoad = level;
            //EventSystem.current.enabled = false;
            StartCoroutine(LoadAndUnload());
            //SceneManager.LoadScene(0);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentSceneToUnload = scene;
            if (currentSceneToUnload.isLoaded)
                SceneManager.SetActiveScene(currentSceneToUnload);
            Debug.Log("Scene loaded: " +  scene.name);
        }

        IEnumerator LoadAndUnload()
        {
            Scene curScene = SceneManager.GetActiveScene();

            yield return null;

            AsyncOperation load = SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);

            while (!load.isDone)
            {
                /*float progress = Mathf.Clamp(operation.progress / .9f, 0f, 0.9999f);

                slider.value = progress * 100f;
                progressText.text = progress * 100f + "%";*/

                yield return null;
            }

            yield return load;

            AsyncOperation unload = SceneManager.UnloadSceneAsync(curScene);

            yield return unload;

            Debug.Log("Unloaded last level");

            //AsyncOperation unloadAssets = Resources.UnloadUnusedAssets();

            //yield return unloadAssets;

            //SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(0));

        }

        IEnumerator LoadAsynchronously(int index)
        {

            AsyncOperation operation = SceneManager.LoadSceneAsync(index);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                /*float progress = Mathf.Clamp(operation.progress / .9f, 0f, 0.9999f);

                slider.value = progress * 100f;
                progressText.text = progress * 100f + "%";*/

                yield return null;
            }
            yield return operation;
            StartCoroutine(EndLoadAsync(index, operation));
        }

        IEnumerator EndLoadAsync(int index, AsyncOperation loadingScene)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(currentSceneToUnload);

            while (!operation.isDone)
            {
                yield return null;
            }
            loadingScene.allowSceneActivation = true;

            // Activate the new scene
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(index));
        }
    }
}

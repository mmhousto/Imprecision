using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.MorganHouston.Imprecision
{
    public class LoadLevel : MonoBehaviour
    {
        public Slider slider;
        public TextMeshProUGUI progressText;
        public TextMeshProUGUI loadingText;

        private int dots = 1;

        private void Awake()
        {
#if UNITY_WSA
            SceneLoader.ResetLightingData();
#endif
        }

        void Start()
        {
            InvokeRepeating(nameof(UpdateLoadingText), 0.0f, 1f);
            StartCoroutine(LoadAsynchronously(SceneLoader.levelToLoad));
        }

        IEnumerator LoadAsynchronously(int index)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(index);

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / .9f);

                slider.value = progress * 100f;
                progressText.text = progress * 100f + "%";

                yield return null;
            }
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Com.MorganHouston.Imprecision
{
    public class PauseManager : MonoBehaviour
    {
        public GameObject masterSlider;
        public GameObject pauseScreen;
        public EventSystem eventSystem;
        public GameObject onScreenButtons;

        public void OnPause(InputValue value)
        {
            pauseScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Time.timeScale = 0;
            eventSystem.SetSelectedGameObject(masterSlider);
        }

        public void OnPause(bool value)
        {
#if (UNITY_IOS || UNITY_ANDROID)
            onScreenButtons.SetActive(false);
#endif
            pauseScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Time.timeScale = 0;
            eventSystem.SetSelectedGameObject(masterSlider);
        }

        public void UnPause()
        {
#if (UNITY_IOS || UNITY_ANDROID)
            onScreenButtons.SetActive(true);
#endif
            pauseScreen.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;
        }

        public void ReturnHome()
        {
            Time.timeScale = 1;
            SceneLoader.LoadThisScene(1);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1;
            SceneLoader.LoadThisScene(2);
        }

    }
}

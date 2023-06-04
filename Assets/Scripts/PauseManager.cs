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
        public bool isPaused;

        public void OnPause(InputValue value)
        {
            isPaused = true;
            pauseScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Time.timeScale = 0;
            eventSystem.SetSelectedGameObject(masterSlider);
            RumbleManager.instance?.PauseRumble();
        }

        public void OnPause(bool value)
        {
            isPaused = true;
#if (UNITY_IOS || UNITY_ANDROID)
            onScreenButtons.SetActive(false);
#endif
            pauseScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Time.timeScale = 0;
            eventSystem.SetSelectedGameObject(masterSlider);
            RumbleManager.instance?.PauseRumble();
        }

        public void UnPause()
        {
            isPaused = false;
#if (UNITY_IOS || UNITY_ANDROID)
            onScreenButtons.SetActive(true);
#endif
            pauseScreen.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;
            RumbleManager.instance?.ResumeRumble();
        }

        public void ReturnHome()
        {
            Time.timeScale = 1;
            SceneLoader.LoadThisScene(1);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1;
            SceneLoader.LoadThisScene(SceneLoader.GetCurrentScene().buildIndex);
        }

    }
}

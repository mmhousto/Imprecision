#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace Com.MorganHouston.Imprecision
{
    public class PauseManager : MonoBehaviour
    {
        public GameObject audioButton;
        public GameObject pauseScreen;
        public EventSystem eventSystem;
        public GameObject onScreenButtons;
        public bool isPaused;
        private bool isControllerConnected = false;
        private bool isSteamOverlayActive = false;

#if !DISABLESTEAMWORKS
        protected Callback<GameOverlayActivated_t> overlayIsOn;
#endif

        private void Start()
        {
#if !DISABLESTEAMWORKS
            overlayIsOn = Callback<GameOverlayActivated_t>.Create(PauseGameIfSteamOverlayOn);
#endif

            // Check if a controller is initially connected
            if (Gamepad.current != null)
            {
                isControllerConnected = true;
            }
        }

        private void Update()
        {
            // Check if a controller was connected and gets disconnected
            if (isControllerConnected && Gamepad.all.Count <= 0)
            {
                // Controller was just unplugged
                isControllerConnected = false;
                OnPause(true);
            }
            else if (!isControllerConnected && Gamepad.all.Count > 0)
            {
                // Controller was just plugged in
                isControllerConnected = true;
            }
        }

        public void OnPause(InputValue value)
        {
            isPaused = true;
            pauseScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Time.timeScale = 0;
            eventSystem.SetSelectedGameObject(audioButton);
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
            eventSystem.SetSelectedGameObject(audioButton);
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

        void PauseGameIfSteamOverlayOn(GameOverlayActivated_t callback)
        {
            if (!isPaused && !GameManager.Instance.isGameOver)
            {
                OnPause(true);
            }
        }

    }
}

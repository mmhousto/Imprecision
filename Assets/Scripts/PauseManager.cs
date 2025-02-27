#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace Com.MorganHouston.Imprecision
{
    public class PauseManager : MonoBehaviour
    {
        public Image backgroundUI;
        public GameObject audioButton;
        public GameObject pauseScreen;
        public EventSystem eventSystem;
        public GameObject onScreenButtons;
        public GameObject tutorialScreen;
        public GameObject hud;
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
                if(GameManager.Instance.gameStarted)
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
            backgroundUI.enabled = true;
            pauseScreen.SetActive(true);
            hud.SetActive(false);
            Cursor.lockState = CursorLockMode.Confined;

            if(tutorialScreen != null)
                tutorialScreen.SetActive(false);

            Time.timeScale = 0;
            eventSystem.SetSelectedGameObject(audioButton);
            RumbleManager.instance?.PauseRumble();
        }

        public void OnPause(bool value)
        {
            isPaused = true;
            backgroundUI.enabled = true;
#if (UNITY_IOS || UNITY_ANDROID)
            onScreenButtons.SetActive(false);
#endif
            pauseScreen.SetActive(true);
            hud.SetActive(false);
            Cursor.lockState = CursorLockMode.Confined;

            if (tutorialScreen != null)
                tutorialScreen.SetActive(false);

            Time.timeScale = 0;
            eventSystem.SetSelectedGameObject(audioButton);
            RumbleManager.instance?.PauseRumble();
        }

        public void UnPause()
        {
            Invoke(nameof(DelayUnPause), 0.5f);
            backgroundUI.enabled = false;
#if (UNITY_IOS || UNITY_ANDROID)
            onScreenButtons.SetActive(true);
#endif
            pauseScreen.SetActive(false);
            hud.SetActive(true);
            if (TutorialManager.playedTutorial == false && tutorialScreen != null)
                tutorialScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;
            RumbleManager.instance?.ResumeRumble();
        }

        private void DelayUnPause()
        {
            isPaused = false;
        }

        public void ReturnHome()
        {
            Time.timeScale = 1;
            //SceneLoader.LoadThisScene(1);
            UnLoadLevel.Instance.LoadUnLoad(1);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1;
            //SceneLoader.LoadThisScene(SceneLoader.GetCurrentScene().buildIndex);
            UnLoadLevel.Instance.LoadUnLoad(SceneLoader.GetCurrentScene().buildIndex);
        }

#if !DISABLESTEAMWORKS
        void PauseGameIfSteamOverlayOn(GameOverlayActivated_t callback)
        {
            if (!isPaused && !GameManager.Instance.isGameOver)
            {
                OnPause(true);
            }
        }
#endif

    }
}

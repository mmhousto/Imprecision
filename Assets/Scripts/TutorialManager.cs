using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using StarterAssets;

namespace Com.MorganHouston.Imprecision
{
    public class TutorialManager : MonoBehaviour
    {
        public static bool playedTutorial;
        public PauseManager pauseManager;
        public PlayerInput playerInput;
        public StarterAssetsInputs starterAssetsInputs;
        public TextMeshProUGUI tutorialText;
        public GameObject tutorialPanel;
        public GameOverManager gameOverManager;
        public string[] tutorialPCInstructions;
        public string[] tutorialConsoleInstructions;
        public string[] tutorialMobileInstructions;
        public AudioClip[] pcAudio;
        private int currentInstruction;
        private bool pauseStarted;
        private bool targetSet;
        private GameObject target;
        private AudioSource audioSource;

        // Start is called before the first frame update
        void Start()
        {
            playedTutorial = PreferenceManager.Instance.PlayedTutorial;
            if (playedTutorial)
            {
                tutorialPanel.SetActive(false);
            }
            else
            {
                tutorialPanel.SetActive(true);
            }

            audioSource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
            //UpdateInstructions()'
            if (playedTutorial != PreferenceManager.Instance.PlayedTutorial) playedTutorial = PreferenceManager.Instance.PlayedTutorial;

            if (playedTutorial == false && pauseManager.isPaused == false)
                HandleInstructions();

            if (playedTutorial == false && !tutorialPanel.activeInHierarchy && pauseManager.isPaused == false)
            {
                tutorialPanel.SetActive(true);
            }
        }

        void HandleInstructions()
        {
            switch (currentInstruction)
            {
                case 0:
                    if (pauseStarted == false)
                    {
                        pauseStarted = true;
                        audioSource.PlayOneShot(pcAudio[currentInstruction]);
                        StartCoroutine(WaitThenGoToNextInstruction(pcAudio[currentInstruction].length + 1f));
                    }
                    break;
                case 1:
                    if (pauseStarted == false)
                    {
                        pauseStarted = true;
                        audioSource.Stop();
                        audioSource.PlayOneShot(pcAudio[currentInstruction]);
                        StartCoroutine(WaitThenGoToNextInstruction(pcAudio[currentInstruction].length + 1f));
                    }
                    break;
                case 2:
                    if (starterAssetsInputs.move != Vector2.zero && pauseStarted == false)
                    {
                        pauseStarted = true;
                        audioSource.Stop();
                        audioSource.PlayOneShot(pcAudio[currentInstruction]);
                        UpdateInstructions(currentInstruction+1);
                        StartCoroutine(GoToNextInstruction(pcAudio[currentInstruction].length + 1f));
                    }
                    break;
                case 3:
                    if (starterAssetsInputs.look != Vector2.zero && pauseStarted == false)
                    {
                        pauseStarted = true;
                        audioSource.Stop();
                        audioSource.PlayOneShot(pcAudio[currentInstruction]);
                        UpdateInstructions(currentInstruction + 1);
                        StartCoroutine(GoToNextInstruction(pcAudio[currentInstruction].length + 1f));
                    }
                    break;
                case 4:
                    if (starterAssetsInputs.sprint == true && pauseStarted == false)
                    {
                        pauseStarted = true;
                        audioSource.Stop();
                        audioSource.PlayOneShot(pcAudio[currentInstruction]);
                        UpdateInstructions(currentInstruction + 1);
                        StartCoroutine(GoToNextInstruction(pcAudio[currentInstruction].length + 1f));
                    }
                    break;
                case 5:
                    if (starterAssetsInputs.jump == true && pauseStarted == false) {
                        pauseStarted = true;
                        audioSource.Stop();
                        audioSource.PlayOneShot(pcAudio[currentInstruction]);
                        UpdateInstructions(currentInstruction + 1);
                        StartCoroutine(GoToNextInstruction(pcAudio[currentInstruction].length + 1f));
                    }
                    break;
                case 6:
                    if (starterAssetsInputs.sprint == true && starterAssetsInputs.jump == true && pauseStarted == false) {
                        pauseStarted = true;
                        audioSource.Stop();
                        audioSource.PlayOneShot(pcAudio[currentInstruction]);
                        UpdateInstructions(currentInstruction + 1);
                        StartCoroutine(GoToNextInstruction(pcAudio[currentInstruction].length + 1f));
                    }
                    break;
                case 7:
                    if (starterAssetsInputs.aiming == true && pauseStarted == false) {
                        pauseStarted = true;
                        audioSource.Stop();
                        audioSource.PlayOneShot(pcAudio[currentInstruction]);
                        UpdateInstructions(currentInstruction + 1);
                        StartCoroutine(GoToNextInstruction(pcAudio[currentInstruction].length + 1f));
                    }
                    break;
                case 8:
                    if (starterAssetsInputs.isPullingBack == true && pauseStarted == false) {
                        pauseStarted = true;
                        audioSource.Stop();
                        audioSource.PlayOneShot(pcAudio[currentInstruction]);
                        UpdateInstructions(currentInstruction + 1);
                        StartCoroutine(GoToNextInstruction(pcAudio[currentInstruction].length + 1f));
                    }
                    break;
                case 9:
                    if (target == null && GameObject.FindWithTag("target") && targetSet == false)
                    {
                        targetSet = true;
                        target = GameObject.FindWithTag("target");
                    }

                    if (target == null && pauseStarted == false)
                    {
                        pauseStarted = true;
                        audioSource.Stop();
                        audioSource.PlayOneShot(pcAudio[currentInstruction]);
                        UpdateInstructions(currentInstruction + 1);
                        StartCoroutine(GoToNextInstruction(pcAudio[currentInstruction].length + 1f));
                    }
                    break;
                case 10:
                    if (pauseStarted == false)
                    {
                        pauseStarted = true;
                        audioSource.Stop();
                        audioSource.PlayOneShot(pcAudio[currentInstruction]);
                        StartCoroutine(CloseTutorial(pcAudio[currentInstruction].length + 1f));
                    }
                    break;
                default: 
                    break;

            }
        }

        void UpdateInstructions(int _currentInstruction)
        {
            if (Gamepad.current != null && tutorialText.text != tutorialConsoleInstructions[_currentInstruction])
            {
                tutorialText.text = tutorialConsoleInstructions[_currentInstruction];
            }
            else if (Gamepad.current == null && playerInput.currentControlScheme == "KeyboardMouse" && tutorialText.text != tutorialPCInstructions[_currentInstruction])
            {
                tutorialText.text = tutorialPCInstructions[_currentInstruction];
            }
            else if (playerInput.currentControlScheme == "Touch" && tutorialText.text != tutorialMobileInstructions[_currentInstruction])
            {
                tutorialText.text = tutorialMobileInstructions[_currentInstruction];
            }
        }

        IEnumerator GoToNextInstruction(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            currentInstruction++;
            pauseStarted = false;
        }

        IEnumerator WaitThenGoToNextInstruction(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            currentInstruction++;
            UpdateInstructions(currentInstruction);
            pauseStarted = false;
        }

        IEnumerator CloseTutorial(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            tutorialPanel.SetActive(false);
            playedTutorial = true;
            PreferenceManager.Instance.SetTutorial(true);
            gameOverManager.RestartGame();
        }
    }
}

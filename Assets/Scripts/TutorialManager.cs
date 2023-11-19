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
        public PlayerInput playerInput;
        public StarterAssetsInputs starterAssetsInputs;
        public TextMeshProUGUI tutorialText;
        public GameObject tutorialPanel;
        public string[] tutorialPCInstructions;
        public string[] tutorialConsoleInstructions;
        public string[] tutorialMobileInstructions;
        private int currentInstruction;

        // Start is called before the first frame update
        void Start()
        {
            if (playedTutorial)
            {
                tutorialPanel.SetActive(false);
            }
            else
            {
                tutorialPanel.SetActive(true);
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdateInstructions();
            HandleInstructions();
        }

        void HandleInstructions()
        {
            switch (currentInstruction)
            {
                case 0:
                    if (starterAssetsInputs.move != Vector2.zero) GoToNextInstruction();
                    break;
                case 1:
                    if (starterAssetsInputs.look != Vector2.zero) GoToNextInstruction();
                    break;
                case 2:
                    if (starterAssetsInputs.sprint == true) GoToNextInstruction();
                    break;
                case 3:
                    if (starterAssetsInputs.jump == true) GoToNextInstruction();
                    break;
                case 4:
                    if (starterAssetsInputs.sprint == true && starterAssetsInputs.jump == true) GoToNextInstruction();
                    break;
                case 5:
                    if (starterAssetsInputs.aiming == true) GoToNextInstruction();
                    break;
                case 6:
                    if (starterAssetsInputs.isPullingBack == true) GoToNextInstruction();
                    break;
                case 7:
                    break;
                default: 
                    break;

            }
        }

        void UpdateInstructions()
        {
            if (Gamepad.current != null && tutorialText.text != tutorialConsoleInstructions[currentInstruction])
            {
                tutorialText.text = tutorialConsoleInstructions[currentInstruction];
            }
            else if (Gamepad.current == null && playerInput.currentControlScheme == "KeyboardMouse" && tutorialText.text != tutorialPCInstructions[currentInstruction])
            {
                tutorialText.text = tutorialPCInstructions[currentInstruction];
            }
            else if (playerInput.currentControlScheme == "Touch" && tutorialText.text != tutorialMobileInstructions[currentInstruction])
            {
                tutorialText.text = tutorialMobileInstructions[currentInstruction];
            }
        }

        public void GoToNextInstruction()
        {
            currentInstruction++;

        }
    }
}

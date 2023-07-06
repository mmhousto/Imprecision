using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace Com.MorganHouston.Imprecision
{
    public class SkipTextManager : MonoBehaviour
    {
        public GameObject mobileSkip, pcConsoleSkip;
        public PlayerInput playerInput;
        public Sprite[] sprites;
        private Image pcConsoleImage;

        // Start is called before the first frame update
        void Start()
        {
            pcConsoleImage = pcConsoleSkip.GetComponent<Image>();

#if UNITY_IOS || UNITY_ANDROID
            pcConsoleSkip.SetActive(false);
            mobileSkip.SetActive(true);
#else
            pcConsoleSkip.SetActive(true);
            mobileSkip.SetActive(false);
#endif

            SetLabel();
        }

        // Update is called once per frame
        void Update()
        {
            SetLabel();
        }

        private void SetLabel()
        {
            if (Gamepad.current != null && pcConsoleImage.sprite != sprites[1])
            {
                pcConsoleSkip.SetActive(true);
                pcConsoleImage.sprite = sprites[1];
                mobileSkip.SetActive(false);
            }
            else if (Gamepad.current == null && playerInput.currentControlScheme == "KeyboardMouse" && pcConsoleImage.sprite != sprites[0])
            {
                pcConsoleSkip.SetActive(true);
                pcConsoleImage.sprite = sprites[0];
                mobileSkip.SetActive(false);
            }
            else if (playerInput.currentControlScheme == "Touch" && !mobileSkip.activeInHierarchy)
            {
                pcConsoleSkip.SetActive(false);
                mobileSkip.SetActive(true);
            }
                
        }
    }
}

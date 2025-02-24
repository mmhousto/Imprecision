using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Com.GCTC.ZombCube
{
    public class ContextPrompt : MonoBehaviour
    {
        public Sprite pc, xbox, ps;
        private Image image;
        // Start is called before the first frame update
        void Start()
        {
            image = GetComponent<Image>();

            CheckControllerType();
        }

        // Update is called once per frame
        void Update()
        {
            CheckControllerType();

        }

        void CheckControllerType()
        {
            if (Gamepad.current != null)
            {
                string controllerName = Gamepad.current.displayName.ToLower();

                if (IsXboxController(controllerName) && image.sprite != xbox)
                {
                    image.sprite = xbox;
                }
                else if (IsPlayStationController(controllerName) && (image.sprite != ps))
                {
                    image.sprite = ps;
                }
                else if (!IsXboxController(controllerName) && !IsPlayStationController(controllerName))
                {

#if UNITY_PS5
                    if (image.sprite != ps)
                        image.sprite = ps;
#else
                    if (image.sprite != xbox)
                        image.sprite = xbox;
#endif
                }
            }
            else
            {
#if UNITY_PS5
                if (image.sprite != ps)
                    image.sprite = ps;
#else
                if (image.sprite != pc)
                    image.sprite = pc;
#endif

#if (UNITY_IOS || UNITY_ANDROID)
                if (image.enabled == true)
                    image.enabled = false;
#endif
            }

        }

        bool IsXboxController(string controllerName)
        {
            return controllerName.Contains("xbox");
        }

        bool IsPlayStationController(string controllerName)
        {
            return controllerName.Contains("dualshock") || controllerName.Contains("dual sense") || controllerName.Contains("ps4") || controllerName.Contains("ps5");
        }
    }
}

/*
The PlayerInput component has an auto-switch control scheme action that allows automatic changing of connected devices.
IE: Switching from Keyboard to Gamepad in-game.
When built to a mobile phone; in most cases, there is no concept of switching connected devices as controls are typically driven through what is on the device's hardware (Screen, Tilt, etc)
In Input System 1.0.2, if the PlayerInput component has Auto Switch enabled, it will search the mobile device for connected devices; which is very costly and results in bad performance.
This is fixed in Input System 1.1.
For the time-being; this script will disable a PlayerInput's auto switch control schemes; when project is built to mobile.
*/

using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

public class MobileDisableAutoSwitchControls : MonoBehaviour
{
    
#if (UNITY_IOS || UNITY_ANDROID)

    [Header("Target")]
    public PlayerInput playerInput;

    public GameObject touchZone, lookStick;

    void Awake()
    {
        SetSwipeOrStickLook();

        DisableScreenControls();
    }

    private void Update()
    {
        //DisableScreenControls();
    }

    public void SetSwipeOrStickLook()
    {
        if (PlayerPrefs.GetInt("Swipe", 0) == 0)
        {
            EnableStickLook();
        }
        else
        {
            EnableSwipe();
        }
    }

    public void EnableSwipe()
    {
        touchZone.SetActive(true);
        lookStick.SetActive(false);
    }

    public void EnableStickLook()
    {
        touchZone.SetActive(false);
        lookStick.SetActive(true);
    }

    public void DisableScreenControls()
    {
        /*if (this.gameObject.activeInHierarchy && (playerInput.currentControlScheme == "KeyboardMouse" || playerInput.currentControlScheme == "Gamepad" || playerInput.currentControlScheme == "Xbox Controller" || playerInput.currentControlScheme == "PS4 Controller"))
        {
            this.gameObject.SetActive(false);
        }
        else if (!this.gameObject.activeInHierarchy && playerInput.currentControlScheme == "Touch")
        {
            this.gameObject.SetActive(true);
        }*/
        this.gameObject.SetActive(false);
    }



#else

    void Start()
    {
        Destroy(this.gameObject);
    }

#endif

}

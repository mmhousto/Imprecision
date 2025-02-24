#if UNITY_PS5
using UnityEngine;
using UnityEngine.PS5;
using System;
using System.Collections;

public class GamePad : MonoBehaviour
{
    public static GamePad activeGamePad = null;

    private static bool enableInput = true;
    private static float timeout = 0;

    public static void EnableInput(bool enable)
    {
        if (enable != enableInput)
        {
            enableInput = enable;

            if (enable == true)
            {
                timeout = 1.0f;
            }
        }
    }

    public static bool IsInputEnabled { get { return enableInput; } }

    // Custom class for holding all the gamepad sprites
	public struct PS5GamePad
	{
		public Vector2 thumbstick_left;
		public Vector2 thumbstick_right;

		public bool cross;
		public bool circle;
		public bool triangle;
		public bool square;

		public bool dpad_down;
		public bool dpad_right;
		public bool dpad_up;
		public bool dpad_left;

		public bool L1;
		public bool L2;
        public bool L3;
		public bool R1;
		public bool R2;
        public bool R3;

        public bool options;
        public bool touchpad;
	}

    public PS5GamePad previousFrame;
    public PS5GamePad currentFrame;

    public bool IsSquarePressed { get { return previousFrame.square == false && currentFrame.square == true; } }
    public bool IsCirclePressed { get { return previousFrame.circle == false && currentFrame.circle == true; } }
    public bool IsTrianglePressed { get { return previousFrame.triangle == false && currentFrame.triangle == true; } }
    public bool IsCrossPressed { get { return previousFrame.cross == false && currentFrame.cross == true; } }

    public bool IsDpadDownPressed { get { return previousFrame.dpad_down == false && currentFrame.dpad_down == true; } }
    public bool IsDpadRightPressed { get { return previousFrame.dpad_right == false && currentFrame.dpad_right == true; } }
    public bool IsDpadUpPressed { get { return previousFrame.dpad_up == false && currentFrame.dpad_up == true; } }
    public bool IsDpadLeftPressed { get { return previousFrame.dpad_left == false && currentFrame.dpad_left == true; } }

    public bool IsR3Pressed { get { return previousFrame.R3 == false && currentFrame.R3 == true; } }

    public Vector2 GetThumbstickLeft { get { return currentFrame.thumbstick_left; } }
    public Vector2 GetThumbstickRight { get { return currentFrame.thumbstick_right; } }

    bool AnyInput
    {
        get
        {
            if (currentFrame.cross || currentFrame.circle || currentFrame.triangle || currentFrame.square ||
                currentFrame.dpad_down || currentFrame.dpad_right || currentFrame.dpad_up || currentFrame.dpad_left ||
                currentFrame.L1 || currentFrame.L2 || currentFrame.L3 || currentFrame.R1 || currentFrame.R2 || currentFrame.R3 )
            {
                return true;
            }

            if ( Vector2.SqrMagnitude(currentFrame.thumbstick_left) > 0.0f )
            {
                return true;
            }

            if (Vector2.SqrMagnitude(currentFrame.thumbstick_right) > 0.0f)
            {
                return true;
            }

            return false;
        }
    }

    public bool IsConnected
    {
        get { return PS5Input.PadIsConnected(playerId); }
    }

    public int playerId = -1;

	private int stickID;
	private bool hasSetupGamepad = false;
	public PS5Input.LoggedInUser loggedInUser;

    private KeyCode optionsBtnKeyCode;

    private string leftStickHorizontalAxis;
    private string leftStickVerticalAxis;

    private string rightStickHorizontalAxis;
    private string rightStickVerticalAxis;

    private KeyCode L1BtnKeyCode;
    private KeyCode R1BtnKeyCode;
    private string L2Axis;
    private string R2Axis;
    private KeyCode L3BtnKeyCode;
    private KeyCode R3BtnKeyCode;

    private KeyCode CrossBtnKeyCode;
    private KeyCode CircleBtnKeyCode;
    private KeyCode SquareBtnKeyCode;
    private KeyCode TriangleBtnKeyCode;

    private string DPadRightAxis;
    private string DPadLeftAxis;
    private string DPadUpAxis;
    private string DPadDownAxis;

    void Start()
	{
		// Stick ID is the player ID + 1
		stickID = playerId + 1;

		ToggleGamePad(false);

        optionsBtnKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + stickID + "Button7", true);
        leftStickHorizontalAxis = "leftstick" + stickID + "horizontal";
        leftStickVerticalAxis = "leftstick" + stickID + "vertical";

        rightStickHorizontalAxis = "rightstick" + stickID + "horizontal";
        rightStickVerticalAxis = "rightstick" + stickID + "vertical";

        CrossBtnKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + stickID + "Button0", true);
        CircleBtnKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + stickID + "Button1", true);
        SquareBtnKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + stickID + "Button2", true);
        TriangleBtnKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + stickID + "Button3", true);

        L1BtnKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + stickID + "Button4", true);
        R1BtnKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + stickID + "Button5", true);

        L3BtnKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + stickID + "Button8", true);
        R3BtnKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + stickID + "Button9", true);

        DPadRightAxis = "dpad" + stickID + "_horizontal";
        DPadLeftAxis = "dpad" + stickID + "_horizontal";
        DPadUpAxis = "dpad" + stickID + "_vertical";
        DPadDownAxis = "dpad" + stickID + "_vertical";

        L2Axis = "joystick" + stickID + "_left_trigger";
        R2Axis = "joystick" + stickID + "_right_trigger";

    }

	void Update()
	{
        if (timeout > 0.0f )
        {
            timeout -= Time.deltaTime;
        }

        if (enableInput == false || timeout > 0.0f)
        {
            previousFrame = new PS5GamePad();
            currentFrame = new PS5GamePad();
            return;
        }

        if (PS5Input.PadIsConnected(playerId))
        {
            previousFrame = currentFrame;

            // Set the gamepad to the start values for the player
            if (!hasSetupGamepad)
                ToggleGamePad(true);

            //All of the below functions assume Gamepad.current to not be null in the new Input System
            #if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Gamepad.current == null)
            {
                Debug.LogError("Failed to find a gamepad with the New Input System. " +
                    "Have you installed the PS5 Input System extension and have a gamepad connected?");
                return;
            }
            #endif

            // Handle each part individually
            Thumbsticks();
            InputButtons();
            DPadButtons();
            TriggerShoulderButtons();

            // Options button is on its own, so we'll do it here
#if ENABLE_INPUT_SYSTEM
            var pad = UnityEngine.InputSystem.Gamepad.current;
            currentFrame.options = pad.selectButton.wasPressedThisFrame;
#else
            currentFrame.options = Input.GetKey(optionsBtnKeyCode);
#endif

            if (activeGamePad == null || AnyInput == true)
            {
                activeGamePad = this;
            }
        }
        else if (hasSetupGamepad)
        {
            ToggleGamePad(false);
        }
	}

	// Toggle the gamepad between connected and disconnected states
	void ToggleGamePad(bool active)
	{
		if(active)
		{
			// Set 3D Text to whoever's using the pad
#if UNITY_2017_2_OR_NEWER
            loggedInUser = PS5Input.RefreshUsersDetails(playerId);
#else
            loggedInUser = PS5Input.PadRefreshUsersDetails(playerId);
#endif


			hasSetupGamepad = true;
		}
		else
		{
			hasSetupGamepad = false;
		}
	}

    void Thumbsticks()
	{
#if ENABLE_INPUT_SYSTEM
        var pad = UnityEngine.InputSystem.Gamepad.current;

        currentFrame.L3 = pad.leftStickButton.isPressed;
        currentFrame.R3 = pad.rightStickButton.isPressed;
        currentFrame.thumbstick_left = pad.leftStick.ReadValue();
        currentFrame.thumbstick_right = pad.rightStick.ReadValue();
#else
        currentFrame.thumbstick_left = new Vector2(Input.GetAxis(leftStickHorizontalAxis), Input.GetAxis(leftStickVerticalAxis));
		currentFrame.thumbstick_right = new Vector2(Input.GetAxis(rightStickHorizontalAxis), Input.GetAxis(rightStickVerticalAxis));

        currentFrame.L3 = Input.GetKey(L3BtnKeyCode);
        currentFrame.R3 = Input.GetKey(R3BtnKeyCode);
#endif
	}

	// Make the Cross, Circle, Triangle and Square buttons light up when pressed
	void InputButtons()
	{
#if ENABLE_INPUT_SYSTEM
        var pad = UnityEngine.InputSystem.Gamepad.current;
        currentFrame.cross = pad.crossButton.isPressed;
        currentFrame.circle = pad.circleButton.isPressed;
        currentFrame.square = pad.squareButton.isPressed;
        currentFrame.triangle = pad.triangleButton.isPressed;
#else
        currentFrame.cross = Input.GetKey(CrossBtnKeyCode);
        currentFrame.circle = Input.GetKey(CircleBtnKeyCode);
        currentFrame.square = Input.GetKey(SquareBtnKeyCode);
        currentFrame.triangle = Input.GetKey(TriangleBtnKeyCode);
#endif
	}

    // Make the DPad directions light up when pressed
    void DPadButtons()
	{
#if ENABLE_INPUT_SYSTEM
        var pad = UnityEngine.InputSystem.Gamepad.current;
        currentFrame.dpad_right= pad.dpad.right.isPressed;
        currentFrame.dpad_left= pad.dpad.left.isPressed;
        currentFrame.dpad_up= pad.dpad.up.isPressed;
        currentFrame.dpad_down= pad.dpad.down.isPressed;
#else
        currentFrame.dpad_right = Input.GetAxis(DPadRightAxis) > 0;
        currentFrame.dpad_left = Input.GetAxis(DPadLeftAxis) < 0;
        currentFrame.dpad_up = Input.GetAxis(DPadUpAxis) > 0;
        currentFrame.dpad_down = Input.GetAxis(DPadDownAxis) < 0;
#endif
	}

	void TriggerShoulderButtons()
	{
#if ENABLE_INPUT_SYSTEM
        var pad = UnityEngine.InputSystem.Gamepad.current;
        currentFrame.L2 = pad.leftTrigger.isPressed;
        currentFrame.R2 = pad.rightTrigger.isPressed;
        currentFrame.L1 = pad.leftShoulder.isPressed;
        currentFrame.R1 = pad.rightShoulder.isPressed;
#else
        // Make the triggers light up based on how "pulled" they are
        currentFrame.L2 = Input.GetAxis(L2Axis) != 0;
        currentFrame.R2 = Input.GetAxis(R2Axis) != 0;

        // Make the shoulders light up when pressed
        currentFrame.L1 = Input.GetKey(L1BtnKeyCode);
        currentFrame.R1 = Input.GetKey(R1BtnKeyCode);
#endif
	}

}
#endif

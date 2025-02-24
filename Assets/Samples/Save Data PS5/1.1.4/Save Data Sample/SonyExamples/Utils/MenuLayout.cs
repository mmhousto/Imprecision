using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_PS5
public class MenuLayout
{
	int width;
	int height;
	int ySpace;
	int x;
	int y;
	GUIStyle style;
	GUIStyle styleSelected;
	int selectedItemIndex = 0;
	bool buttonPressed = false;
	bool backButtonPressed = false;
	int numItems = 0;
	int fontSize = 16;

	static float timeOfLastChange = 0.0f;
	
	int currCount = 0;
	IScreen owner = null;

    //static bool inputEnabled = true;

    string[] tooltips = new string[30];

    //public static void EnableInput(bool enable)
    //{
    //    inputEnabled = enable;       
    //}

	public IScreen GetOwner()
	{
		return owner;
	}

	public MenuLayout(IScreen screen, int itemWidth, int itemFontSize)
	{
		owner = screen;
		numItems = 0;	// itemCount;
		width = itemWidth;
		fontSize = itemFontSize;
	}

	public void DoLayout()
	{
		numItems = currCount;

		style = new GUIStyle(GUI.skin.GetStyle("Button"));
		style.fontSize = fontSize;
		style.alignment = TextAnchor.MiddleCenter;

		styleSelected = new GUIStyle(GUI.skin.GetStyle("Button"));
		styleSelected.fontSize = fontSize + 8;
		styleSelected.alignment = TextAnchor.MiddleCenter;

        height = style.fontSize + 16;
		ySpace = 8;
		x = (int)((Screen.width*0.30f) - width) / 2;
        y = (int)(Screen.height * 0.12f); // (int)((Screen.height*0.75f) - (height + ySpace) * numItems) / 2;

		currCount = 0;

        if (tooltips[selectedItemIndex] != null && tooltips[selectedItemIndex].Length > 0)
        {
            //GUIStyle tooltipStyle = new GUIStyle(GUI.skin.GetStyle("TextArea"));
            GUIStyle tooltipStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            tooltipStyle.fontSize = fontSize + 2;
            tooltipStyle.alignment = TextAnchor.UpperCenter;
            tooltipStyle.wordWrap = true;

            int bottomY = y + ((height + ySpace) * numItems);
            GUI.Label(new Rect(Screen.width * 0.01f, bottomY, Screen.width * 0.28f, Screen.height - 1), tooltips[selectedItemIndex], tooltipStyle);
        }
    }

	public void SetSelectedItem(int index)
	{
		if (index < 0)
		{
			selectedItemIndex = 0;
		}

		if (index > numItems - 1)
		{
			selectedItemIndex = numItems - 1;
		}
	}

	public void ItemNext()
	{
		if (numItems > 0)
		{
			selectedItemIndex++;
			if (selectedItemIndex >= numItems)
			{
				selectedItemIndex = 0;
			}
		}
	}

	public void ItemPrev()
	{
		if (numItems > 0)
		{
			selectedItemIndex--;
			if (selectedItemIndex < 0)
			{
				selectedItemIndex = numItems - 1;
			}
		}
	}

	public void Update()
	{
		DoLayout();

        if (GamePad.IsInputEnabled == true)
        {
            HandleInput();
        }
	}

    public void HandleInput()
	{
        if (GamePad.activeGamePad == null)
        {
            return;
        }

        buttonPressed = false;
		backButtonPressed = false;
		float repeatTime = 0.3f;		// time before key repeat is allowed

		float MenuEventDeltaTime = Time.timeSinceLevelLoad - timeOfLastChange;
        bool advance = GamePad.activeGamePad.IsCrossPressed; // fire1Btn.IsPressed;
		bool back = GamePad.activeGamePad.IsCirclePressed; // fire2Btn.IsPressed;

        if (advance && (MenuEventDeltaTime > repeatTime))   // (X)
		{
			buttonPressed = true;
			timeOfLastChange = Time.timeSinceLevelLoad;
			return;
		}

		if (back && (MenuEventDeltaTime > repeatTime))   // (O)
		{
			backButtonPressed = true;
			timeOfLastChange = Time.timeSinceLevelLoad;
			return;
		}

		// up and down navigation
		float direction = GamePad.activeGamePad.GetThumbstickLeft.y;      // Left stick
		bool down = (direction > 0.1f) || GamePad.activeGamePad.IsDpadDownPressed;
		bool up = (direction < -0.1f) || GamePad.activeGamePad.IsDpadUpPressed;
        if (down && (MenuEventDeltaTime > repeatTime))
		{
			ItemNext();
			timeOfLastChange = Time.timeSinceLevelLoad;
		}
		if (up && (MenuEventDeltaTime > repeatTime))
		{
			ItemPrev();
			timeOfLastChange = Time.timeSinceLevelLoad;
		}

		if (!down && !up && !advance && !back) { timeOfLastChange = 0; }	// reset timer if no movement, to speed change of direction
	}
	
	private bool AddButton(string text, bool enabled = true, bool selected = false)
	{
		GUI.enabled = enabled;
		bool ret = GUI.Button(GetRect(), text, selected ? styleSelected : style);
		y += height + ySpace;
		GUI.enabled = true;
		return ret;
	}

    public bool AddItem(string name, string tooltip, bool enabled = true)
    {
        bool result = AddItem(name, enabled);
        tooltips[currCount-1] = tooltip;
        return result;
    }

    public bool AddItem(string name, bool enabled = true)
	{
        tooltips[currCount] = "";
        bool clicked = false;
		if (numItems > 0)
		{
			if (AddButton(name, enabled, selectedItemIndex == currCount))
			{
				selectedItemIndex = currCount;
				clicked = true;
			}
			else if (buttonPressed && enabled && selectedItemIndex == currCount)
			{
				clicked = true;
				buttonPressed = false;
			}
		}

		currCount++;

		return clicked;
	}

	public bool AddBackIndex(string name, bool enabled = true)
	{
		bool clicked = false;

		if (numItems > 0)
		{
			if (AddButton(name, enabled, selectedItemIndex == currCount))
			{
				selectedItemIndex = currCount;
				clicked = true;
			}
			else if (buttonPressed && enabled && selectedItemIndex == currCount)
			{
				clicked = true;
				buttonPressed = false;
			}
			else if (backButtonPressed && enabled)
			{
				clicked = true;
				backButtonPressed = false;
			}
		}

		currCount++;

		return clicked;
	}
	
	public Rect GetRect()
	{
		return new Rect(x, y, width, height);
	}
};
#endif

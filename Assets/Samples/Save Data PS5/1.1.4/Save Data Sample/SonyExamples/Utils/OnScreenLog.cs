using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

#if UNITY_PS5
public class OnScreenLog : MonoBehaviour
{
    static int msgCount = 0;

    struct Message
    {
        public string text;
        public Color color;

        public Message(string text, Color color)
        {
            this.text = text;
            this.color = color;
        }
    }

    static private List<Message> log = new List<Message>();

    static int maxLines = 16;
    static int fontSize = 18;
    static int frameCount = 0;

    public static int FrameCount { get { return frameCount; } } 

    static System.Object syncObject = new System.Object();

    // Use this for initialization
    void Start()
    {
        if (Application.platform == RuntimePlatform.PS4)
        {
            maxLines = 45;
        }
    }

    // Update is called once per frame
    void Update()
    {
        frameCount++;
    }

    static int numLinesSinceLastFrame = 0;

    void OnGUI()
    {
        lock (syncObject)
        {
            GUIStyle style = GUI.skin.GetStyle("Label");
            style.fontSize = fontSize;
            style.alignment = TextAnchor.UpperLeft;
            style.wordWrap = false;

            float screenLogHeight = (Screen.height * 0.73f) - (style.lineHeight + 4); // Space on the screen to show the log
            float totalScreenSize = log.Count * style.lineHeight; // Total space the log requires. This could be much larger than the screen space.

            float y = 0.0f;

            if (numLinesSinceLastFrame > 0 && scrollOffset < 0.0f)
            {
                // Force the offset to move as more lines have been added but the display shouldn't scroll.
                scrollOffset -= numLinesSinceLastFrame * style.lineHeight;
            }

            if (totalScreenSize <= screenLogHeight)
            {
                scrollOffset = 0.0f;
            }
            else
            {
                y = screenLogHeight - totalScreenSize;
            }

            if ( scrollOffset > 0 )
            {
                scrollOffset = 0.0f;
            }

            y -= scrollOffset;

            if (y > 0)
            {
                y = 0;
                scrollOffset = screenLogHeight - totalScreenSize;
            }

            string logText = "";
            foreach (Message msg in log)
            {
                if (y > 0 && y < (screenLogHeight))
                {
                    logText = msg.text;
                    logText += "\n";

                    GUI.color = msg.color;
                    GUI.Label(new Rect(Screen.width * 0.31f, y, Screen.width * 0.74f, style.lineHeight + 4), logText, style);
                }

                y += style.lineHeight;
            }

            GUI.color = Color.white;
            GUI.Label(new Rect(Screen.width - 100, Screen.height - 100, Screen.width - 1, style.lineHeight + 4), frameCount.ToString());

            numLinesSinceLastFrame = 0;
        }
    }

    static float scrollOffset = 0.0f;

    static public void ScrollUp(float scale)
    {
        scrollOffset += (1000.0f * scale) * Time.deltaTime;
    }

    static public void ScrollDown(float scale)
    {
        scrollOffset -= (1000.0f * scale) * Time.deltaTime;
    }

    static public void ScrollReset()
    {
        scrollOffset = 0.0f;
    }

    static private void Add(string msg, Color color, bool keepNewlines = false, bool outputToConsole = false)
    {
        lock (syncObject)
        {
            if (keepNewlines == false)
            {
                string cleaned = msg.Replace("\r", " ");
                cleaned = cleaned.Replace("\n", " ");

                log.Add(new Message(cleaned, color));

                if (outputToConsole)
                {
                    Console.WriteLine(cleaned);
                }

                msgCount++;

                numLinesSinceLastFrame++;

                if (msgCount > maxLines)
                {
                    //log.RemoveAt(0);
                }
            }
            else
            {
                string[] lines = msg.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                if (lines != null)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        log.Add(new Message(lines[i], color));

                        if (outputToConsole)
                        {
                            Console.WriteLine(lines[i]);
                        }

                        msgCount++;
                        numLinesSinceLastFrame++;

                        if (msgCount > maxLines)
                        {
                            //log.RemoveAt(0);
                        }
                    }
                }
            }
        }
    }

    static public void Add(string msg, bool keepNewlines = false)
    {
        Add(msg, Color.white, keepNewlines, true);
    }

    static public void Add(string msg, Color color)
    {
        Add(msg, color, false, true);
    }

    static public void AddWarning(string msg)
    {
        Add(msg, Color.yellow, false, false);

        Console.Error.WriteLine(msg);
    }

    static public void AddError(string msg)
    {
        Add(msg, Color.red, false, false);

        Console.Error.WriteLine(msg);
    }

    static public void AddNewLine()
    {
        Add("");
    }

    static public void Clear()
    {
        lock (syncObject)
        {
            if (log != null)
            {
                log.Clear();
                msgCount = 0;
                scrollOffset = 0.0f;
            }
        }
    }
}
#endif

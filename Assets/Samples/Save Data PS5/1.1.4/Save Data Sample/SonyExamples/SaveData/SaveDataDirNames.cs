
using System.Collections.Generic;
using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Core;

public static class SaveDataDirNames
{
#if UNITY_PS5
    static List<string> dirNames = new List<string>();
    static int currentIndex = -1;

    static public List<string> GetDirNames()
    {
        return dirNames;
    }

    static public void GenerateNewDirName(int frameCount)
    {
        string name = "Example" + OnScreenLog.FrameCount;

        AddDirName(name);
        currentIndex = dirNames.Count-1;
    }

    static public void AddDirName(DirName name)
    {
        AddDirName(name.Data);
    }

    static public void ClearAllNames()
    {
        dirNames.Clear();
    }

    static public void AddDirName(string name)
    {
        int index = FindIndex(name);

        if ( index == -1 )
        {
            // Name doesn't exist, so add it to the list
            dirNames.Add(name);
            if ( currentIndex == -1)
            {
                currentIndex = 0;
            }
        }
    }

    static public int FindIndex(string name)
    {
        for (int i = 0; i < dirNames.Count; i++)
        {
            if (dirNames[i] == name)
            {
                return i;
            }
        }

        return -1;
    }

    static public void RemoveDirName(string name)
    {
        for(int i = 0; i < dirNames.Count; i++)
        {
            if ( dirNames[i] == name)
            {
                dirNames.RemoveAt(i);
                if ( currentIndex == i )
                {
                    if ( currentIndex >= dirNames.Count)
                    {
                        currentIndex = 0;
                    }
                }
            }
        }
    }

    static public bool HasCurrentDirName()
    {
        return currentIndex >= 0 && currentIndex < dirNames.Count;
    }

    static public int GetCurrentDirNameIndex()
    {
        return currentIndex;
    }

    static public string GetCurrentDirName()
    {
        if ( HasCurrentDirName() == false)
        {
            return "";
        }

        return dirNames[currentIndex];
    }

    static public void IncrementCurrentIndex()
    {
        if (dirNames.Count == 0)
        {
            currentIndex = -1;
            return;
        }

        currentIndex++;

        if ( currentIndex >= dirNames.Count )
        {
            currentIndex = 0;
        }
    }

    static public void DecrementCurrentIndex()
    {
        if (dirNames.Count == 0)
        {
            currentIndex = -1;
            return;
        }

        currentIndex--;

        if (currentIndex < 0 )
        {
            currentIndex = dirNames.Count-1;
        }
    }
#endif

}

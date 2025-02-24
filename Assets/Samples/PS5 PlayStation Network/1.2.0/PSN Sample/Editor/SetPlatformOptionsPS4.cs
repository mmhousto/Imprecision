using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class SetPlatformOptionsPS4
{
#if UNITY_PS4
    //Method to do work
    static string SearchForFile(string path, string findFilename, bool returnAssetRelativePath = false)
    {
        string findFilenameNoExt = Path.GetFileNameWithoutExtension(findFilename);

        var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (Path.GetFileNameWithoutExtension(file) == findFilenameNoExt)
            {
                if (returnAssetRelativePath)
                {
                    return file.Replace(Application.dataPath, "Assets");
                }
                return file;
            }
        };

        return "";
    }

    static string SearchForDirectory(string path, string findDirectory, bool returnAssetRelativePath = false)
    {
        var directories = Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories);
        foreach (var directory in directories)
        {
            string fullPath = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar);
            string lastDirName = Path.GetFileName(fullPath);

            if (lastDirName.ToLower() == findDirectory.ToLower())
            {
                if (returnAssetRelativePath)
                {
                    return directory.Replace(Application.dataPath, "Assets");
                }
                return directory;
            }
        };

        return "";
    }


    [MenuItem("SCE Publishing Utils/Set Publish Settings For PS4")]
    // Use this for initialization
    static void SetOptions()
    {
        // Param file settings.
        PlayerSettings.PS4.category = PlayerSettings.PS4.PS4AppCategory.Application;
        PlayerSettings.PS4.appVersion = "01.00";
        PlayerSettings.PS4.masterVersion = "01.00";
        // This is the Unity PS4 PSN title content ID
        PlayerSettings.PS4.contentID = "IV0002-NPXX53794_00-UNITYSAMPLEAPP01";
        // The title ID of NPXX53794_00 uses a NP Communication ID of NPWR05690_00 ...

        PlayerSettings.productName = "Unity PSN Example";
        PlayerSettings.PS4.parentalLevel = 1;
        PlayerSettings.PS4.enterButtonAssignment = PlayerSettings.PS4.PS4EnterButtonAssignment.CrossButton;
        PlayerSettings.PS4.paramSfxPath = "";

        PlayerSettings.PS4.npTrophyPackPath = "";
        PlayerSettings.PS4.npAgeRating = 12;

        // Replace the old NpToolkit module with the NpToolkit2 version
        string[] modules = PlayerSettings.PS4.includedModules;

        if (modules.Length == 0)
        {
            Debug.Log("The player settings modules list is empty. Please open the player settings to initialise the list and try again.");
            return;
        }

        bool alreadySet = false;

        for (int i = modules.Length - 1; i >= 0; i--)
        {
            if (modules[i].IndexOf("libSceNpCppWebApi.prx", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                alreadySet = true;
            }
        }
        PlayerSettings.PS4.includedModules = modules;

        if ( alreadySet == false)
        {
            Debug.LogError("Unable to find libSceNpCppWebApi.prx in modules list.");
        }

        AssetDatabase.Refresh();

        // PSN Settings.
        string fullSettingsPath = SearchForDirectory(Application.dataPath, "settingsPS4~");

        string titleDatPath = SearchForFile(fullSettingsPath, "nptitle", true);
        PlayerSettings.PS4.NPtitleDatPath = titleDatPath;
    }

    // Replace whatever Input Manager you currently have with one to work with the Nptoolkit Sample
    [MenuItem("SCE Publishing Utils/Set Input Manager")]
    static void ReplaceInputManager()
    {
        // This is the InputManager asset that comes with the example project. Note that to avoid an import error, the '.asset' file extension has been removed
        string sourceFile = SearchForFile(Application.dataPath, "InputManager");

        // This is the InputManager in your ProjectSettings folder
        string targetFile = Application.dataPath;
        targetFile = targetFile.Replace("/Assets", "/ProjectSettings/InputManager.asset");

        // Replace the ProjectSettings file with the new one, and trigger a refresh so the Editor sees it
        FileUtil.ReplaceFile(sourceFile, targetFile);
        AssetDatabase.Refresh();

        Debug.Log("InputManager replaced!");
    }
#endif
}

using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;


public class SetPSNPlatformOptionsPS5
{
#if UNITY_PS5
    //Method to do work
    static string SearchForFile(string path, string findFilename, bool returnAssetRelativePath = false)
    {
        string findFilenameNoExt = Path.GetFileNameWithoutExtension(findFilename);

        var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (Path.GetFileNameWithoutExtension(file) == findFilenameNoExt)
            {
                if(returnAssetRelativePath)
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

    [MenuItem("SCE Publishing Utils/PSN/Set Publish Settings For PS5")]
    // Use this for initialization
    static void SetOptions()
    {
        PlayerSettings.productName = "Unity PSN Example";

        string fullSettingsPath = SearchForDirectory(Application.dataPath, "settings~");

        string configZipFile = SearchForFile(fullSettingsPath, "npconfig", true);
        string paramFile = SearchForFile(fullSettingsPath, "param", true);

        string pic0 = SearchForFile(fullSettingsPath, "pic0.png", true);
        string pic1 = SearchForFile(fullSettingsPath, "pic1.png", true);
    //    string snd0 = SearchForFile(fullSettingsPath, "snd0.png", true);
        string startupImages = SearchForDirectory(fullSettingsPath, "StartupImages", true);
        string titleIcons = SearchForDirectory(fullSettingsPath, "TitleIcons", true);

#if UNITY_2020_1_OR_NEWER
        UnityEditor.PS5.PlayerSettings.paramFilePath = paramFile;
        Debug.LogWarning("Set 'Select param file' to " + paramFile);
        UnityEditor.PS5.PlayerSettings.npConfigZipPath = configZipFile;
        Debug.LogWarning("Set 'Package metadata file' path to " + configZipFile);
        //     Debug.LogWarning("Set 'Background Audio' to " + snd0);
        UnityEditor.PS5.PlayerSettings.iconImagesFolder = titleIcons;
        Debug.LogWarning("Set 'Multi-language Icon Images' to " + titleIcons);
        UnityEditor.PS5.PlayerSettings.backgroundImagePath = pic0;
        Debug.LogWarning("Set 'Background Image' to " + pic0);
        UnityEditor.PS5.PlayerSettings.startupBackgroundImagePath = pic1;
        Debug.LogWarning("Set 'Default Startup Image (Background)' to " + pic1);
        UnityEditor.PS5.PlayerSettings.startupImagesFolder = startupImages;
        Debug.LogWarning("Set 'Multi-language Start-up Images' to " + startupImages);

#else
        //   PlayerSettings.
        PlayerSettings.PS5.npConfigZipPath = configZipFile;

        PlayerSettings.PS5.paramFilePath = paramFile;

        PlayerSettings.PS5.BackgroundImagePath = pic0;

        PlayerSettings.PS5.StartupImagePath = pic1;

      //  PlayerSettings.PS5.BGMPath = snd0;

        PlayerSettings.PS5.startupImagesFolder = startupImages;

        PlayerSettings.PS5.iconImagesFolder = titleIcons;
#endif

    }

    // Replace whatever Input Manager you currently have with one to work with the Nptoolkit Sample
    [MenuItem("SCE Publishing Utils/PSN/Set Input Manager")]
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



        Debug.Log("InputManager replaced! " + sourceFile + " -> " + targetFile);
    }
#endif
    }

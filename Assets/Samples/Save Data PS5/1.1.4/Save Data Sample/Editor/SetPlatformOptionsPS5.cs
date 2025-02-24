using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEditor.PS5;

public class SetSaveDataPlatformOptionsPS5
{
    //Method to do work
    static string SearchForFile(string path, string findFilename)
    {
        var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (Path.GetFileNameWithoutExtension(file) == findFilename)
                return file;
        };

        return "";
    }

    [MenuItem("SCE Publishing Utils/SaveData/Set Publish Settings For PS5")]
	// Use this for initialization
	static void SetOptions()
	{
		UnityEditor.PlayerSettings.productName = "Unity Save Data Example";

        // Copy the SampleStreamingAssets folder to the Assets root.
        string sourceFile = SearchForFile(Application.dataPath, "SaveIcon");

        // Remove up to the assets folder
        sourceFile = sourceFile.Replace(Application.dataPath, "Assets");

       // string sampleStreamingFolder = Path.GetDirectoryName(sourceFile);

        string streamingDir = Application.dataPath + "/StreamingAssets";
        string targetFile = streamingDir + "/SaveIcon.png";

        targetFile = targetFile.Replace(Application.dataPath, "Assets");

        if ( Directory.Exists(streamingDir) == false)
        {
            Directory.CreateDirectory(streamingDir);
        }

        AssetDatabase.CopyAsset(sourceFile, targetFile);
        AssetDatabase.Refresh();

        //Set for loading PS4 Saves
        SetParamJson();
    }

    static void SetParamJson()
    {
        string sourceFilePath = SearchForFile(Application.dataPath, "param");
        if (sourceFilePath == null)
        {
            Debug.LogWarning("Didn't find param json");
        }

        UnityEditor.PS5.PlayerSettings.paramFilePath = sourceFilePath;
    }

    // Replace whatever Input Manager you currently have with one to work with the SaveData Sample
    [MenuItem("SCE Publishing Utils/SaveData/Set Input Manager")]
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
}

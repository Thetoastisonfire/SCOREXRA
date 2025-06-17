using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class SoundImport : MonoBehaviour
{
    //if not already installed
    [MenuItem("Tools/Install Sound Files")]
    public static void Install() //if stuff isnt installed yet
    {
        string targetPath = "Assets/Resources/Sound";
        string sourcePath = "Assets/AidanAssets/SoundAssets"; // Change this to your actual source

        //check if test file exists; if not, start importing
        if (!File.Exists(Path.Combine(targetPath, "test.wav")))
        {
            copyDirectory(sourcePath, targetPath); //will add directories and files
            AssetDatabase.Refresh(); //refresh database so unity sees the files
            Debug.Log("Sound files installed recursively.");
        }
        else
        {
            Debug.Log("Sound files already installed. Skipping.");
        }
    }
    private static void copyDirectory(string sourceDir, string targetDir)
    {
        //create if doesnt exist
        Directory.CreateDirectory(targetDir);

        //copy all files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            if (!file.EndsWith(".meta"))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetDir, fileName);
                File.Copy(file, destFile, true);
            }
        }

        //recursive copy subdirectories
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(dir);
            string newTargetDir = Path.Combine(targetDir, dirName);
            copyDirectory(dir, newTargetDir);
        }
    }//endof method
}

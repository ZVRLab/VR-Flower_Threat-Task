using UnityEngine;
using System.IO;
using UnityEngine;
using System;

public class FolderManager : MonoBehaviour
{
    public static FolderManager Instance;
    public string SessionFolderPath { get; private set; }

    void Awake()
    {
        Instance = this;
        CreateSessionFolder();
    }

    void CreateSessionFolder()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string dateString = DateTime.Now.ToString("MM-dd-yyyy");
        string folderName = $"PMA_{dateString}";

        SessionFolderPath = Path.Combine(desktopPath, folderName);
        Directory.CreateDirectory(SessionFolderPath);
    }
}
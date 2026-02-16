using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class RecordPlayerMovement : MonoBehaviour
{
    [Header("Recording Settings")]
    public Transform player;        // Assign player or camera transform
    public float recordInterval = 0.5f;  // Seconds between samples

    private string filePath;
    private bool isRecording = false;
    private int trialNumber = 0;
    private int stormFlag = 0; 

    void Awake()
    {
        // File path: PMAmovementLog_10_17_2025.txt (saves in persistentDataPath)
        string fileName = $"MovementLog.txt";
        string folderPath = FolderManager.Instance.SessionFolderPath;
        filePath = Path.Combine(folderPath, fileName);

        // Write header 
        File.AppendAllText(filePath, "Trial,Storm,Time,PosX,PosZ,RotY,ForwardX,ForwardZ\n");
    }

    public void StartRecording(int trial, bool isStorm)
    {
        trialNumber = trial;
        stormFlag = isStorm ? 1 : 0; // convert bool to 1/0
        if (!isRecording)
        {
            isRecording = true;
            StartCoroutine(RecordMovement());
        }
    }

    public void StopRecording()
    {
        isRecording = false;
    }

    private IEnumerator RecordMovement()
    {
        while (isRecording)
        {
            Vector3 pos = player.position;
            float rotY = player.eulerAngles.y;
            
            // Get the forward direction (normalized vector)
            Vector3 forward = player.forward;
            
            // Record X and Z position, rotation Y (yaw rotation; rotating left or right), and forward direction X and Z (where player is facing)
            string line = $"{trialNumber},{stormFlag},{Time.time:F2},{pos.x:F3},{pos.z:F3},{rotY:F1},{forward.x:F3},{forward.z:F3}\n";
            File.AppendAllText(filePath, line);

            yield return new WaitForSeconds(recordInterval);
        }
    }
}
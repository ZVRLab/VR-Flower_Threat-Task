using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class RecordPlayerMovement : MonoBehaviour
{
    [Header("Recording Settings")]
    public Transform player;        // Assign player or camera transform
    public BoxCollider shelterCollider; // * Added: Assign the House's BoxCollider in Inspector CEAV
    public BoxCollider miningCollider;  // * Added: Assign the Crystal's BoxCollider in Inspector CEAV
    public float recordInterval = 0.5f;  // Seconds between samples

    // * Added: Threshold for whether an object is visible on screen (based on camera FOV) CEAV
    //*Calculate the cosine of half xFOV to determine if an object is within the player's field of view. CEAV*
    //*Aspect ratio is 1.78 (16:9) and yFOV is 60 degrees, the xFOV is approximately 90 degrees. CEAV*
    private const float viewThreshold = 0.7f;

    private string filePath;
    private string facingFilePath; // * Added: path for the new facing log file
    private bool isRecording = false;
    private int trialNumber = 0;
    private int stormFlag = 0; 

    private int facingShelterCount = 0;
    private int facingOreCount = 0;

    public bool countFacingTime = false; //This will be used to tell when to record facing time
    public float TimeFacingHouse => facingShelterCount * recordInterval;
    public float TimeFacingMine => facingOreCount * recordInterval;

    void Awake()
    {
        // File path: PMAmovementLog_10_17_2025.txt (saves in persistentDataPath)
        string fileName = $"MovementLog.txt";
        string folderPath = FolderManager.Instance.SessionFolderPath;
        filePath = Path.Combine(folderPath, fileName);

        // Write header
        File.AppendAllText(filePath, "Trial,Storm,Time,PosX,PosZ,RotY,ForwardX,ForwardZ\n");

        // * Added: Facing shelter log file CEAV
        string facingFileName = "FacingShelterLog.txt";
        facingFilePath = Path.Combine(folderPath, facingFileName);
        File.AppendAllText(facingFilePath, "Trial,Storm,Time,PosX,PosZ,InShelter,ShelterDot,FacingShelter,OreDot,FacingOre\n");
    }

    public void StartRecording(int trial, bool isStorm)
    {
        trialNumber = trial;
        stormFlag = isStorm ? 1 : 0; // convert bool to 1/0

         // Reset counts for new trial
    facingShelterCount = 0;
    facingOreCount = 0;

    // Ensure previous coroutine is stopped
    StopAllCoroutines();

    isRecording = true;
    StartCoroutine(RecordMovement());
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

            // * Added: Log facing data with shelter status and ore status CEAV
            {
                // * Check if player is inside the shelter using the collider's bounds CEAV
                int inShelter = shelterCollider.bounds.Contains(pos) ? 1 : 0;
                Vector3 forwardFlat = new Vector3(forward.x, 0f, forward.z).normalized; 
                // *flattening the player's directionremoves the vertical tilt of the dot product calculation CEAV*

                // * Calculate direction to ore and dot product (how directly player faces it) CEAV
                Vector3 oreCenter = miningCollider.transform.TransformPoint(miningCollider.center);
                Vector3 dirToOre = (oreCenter - pos).normalized;
                dirToOre.y = 0f; // used to normalize the direction to ore on the horizontal plane, ignoring vertical differences CEAV
                dirToOre.Normalize(); //normalized the direction to ore vector to ensure the dot product is accurate CEAV
                float oreDot = Vector3.Dot(forwardFlat, dirToOre);
                int facingOre = oreDot > viewThreshold ? 1 : 0; // 1 if facing ore, 0 if not (based on view threshold) CEAV
                if (countFacingTime && facingOre == 1) {
                facingOreCount++;
                }

                string facingLine;
                if (inShelter == 1)
                {
                    // * If inside shelter, leave shelter-facing columns blank CEAV
                    facingLine = $"{trialNumber},{stormFlag},{Time.time:F2},{pos.x:F3},{pos.z:F3},{inShelter},,,,{oreDot:F3},{facingOre}\n";
                }
                else
                {
                    // * If outside shelter, also calculate direction to shelter and dot product CEAV
                    Vector3 shelterCenter = shelterCollider.transform.TransformPoint(shelterCollider.center);
                    Vector3 dirToShelter = (shelterCenter - pos).normalized;
                    dirToShelter.y = 0f; // used to normalize the direction to shelter on the horizontal plane, ignoring vertical differences CEAV
                    dirToShelter.Normalize(); //normalized the direction to shelter vector to ensure the dot product is accurate CEAV
                    float shelterDot = Vector3.Dot(forwardFlat, dirToShelter);
                    int facingShelter = shelterDot > viewThreshold ? 1 : 0; // 1 if facing shelter, 0 if not (based on view threshold) CEAV
                    if (countFacingTime && facingShelter == 1) {
                    facingShelterCount++;
                    }

                    facingLine = $"{trialNumber},{stormFlag},{Time.time:F2},{pos.x:F3},{pos.z:F3},{inShelter},{shelterDot:F3},{facingShelter},{oreDot:F3},{facingOre}\n";
                }
                File.AppendAllText(facingFilePath, facingLine);
            }

            yield return new WaitForSeconds(recordInterval);
        }
    }
}
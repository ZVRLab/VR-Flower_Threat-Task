using UnityEngine;
using System;
using System.IO;

public class OreMiner : MonoBehaviour
{
    [Header("Mining Settings")]
    public float mineCooldown = 1f;       // 1 second between mines
    private float lastMineTime = -1f;     

    public int pointsThisTrial = 0;       // Points earned this trial

    [Header("References")]
    public ScoreUI scoreUI;               // Assign in Inspector
    private bool canMine = false;         // Is player inside mining zone?
      //UDPSender comunicator
	public UDPSender U;
    public TrialManager trialManager;
    public bool isExpectancyActive = false; //Can't accidentally mine when making expectancy selection
    private string folderPath;

    void Awake()
    {
        folderPath = FolderManager.Instance.SessionFolderPath;
    }

    void Update()
    {
        // Mining action: press 4 to mine
        if (canMine && Input.GetKeyDown(KeyCode.Alpha4) && !isExpectancyActive)
        {
            if (Time.time - lastMineTime >= mineCooldown)
            {
                MineOre(); //Calls upon MineOre to get points and update the points UI
            }
        }
    }

    void MineOre()
    {
        pointsThisTrial++;
        lastMineTime = Time.time;
        //UDP sender code G for storm cue
        UDPSender.sendString("M");
        //Document when the point was added
         string pointPath = Path.Combine(folderPath, "PointAddedFile.txt");
         int trialNum = trialManager.CurrentTrial;
    using (StreamWriter sw = new StreamWriter(pointPath, true))
{
    sw.WriteLine("{0}, {1}, Points {2}, Trial {3}", Time.time, DateTime.Now, pointsThisTrial, trialNum + 1); //(Added a point to trial because it started at 0)
}

        // Update the UI
        if (scoreUI != null)
            scoreUI.UpdateScore(pointsThisTrial);

        Debug.Log("Ore mined! Total: " + pointsThisTrial);
    }

    // Detect when player enters/exits mining area
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            canMine = true; //Player is in mining area 
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canMine = false; //Player left mining area
        }
    }

    // Reset points and cooldown at the start of each trial
    public void ResetMiner()
    {
        pointsThisTrial = 0;
        lastMineTime = -1f;

        if (scoreUI != null)
            scoreUI.UpdateScore(pointsThisTrial);
    }
}

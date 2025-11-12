using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using System;
using System.IO;

public class TrialManager : MonoBehaviour
{
    [Header("Trial Settings")]
    public int totalTrials = 80;
    public float trialDuration = 30f; // seconds per trial
    public int stormCount = 40;

    [Header("References")]
    public GameObject expectancyPanel;
    public FirstPersonController playerController; // disable/enable movement script
    public AudioSource warningSound;
    public ParticleSystem stormClouds;
    public OreMiner oreMiner;
    public PlayerDataLogging dataLogger;
    public RecordPlayerMovement movementRecorder;
    //UDPSender comunicator
	public UDPSender U;
   

    [Header("Environment")]
    public Collider miningArea;
    public Collider shelterArea;
    public Light directionalLight;

    private int currentTrial = 0;
    private int expectancyValue = 5;
    private bool[] stormTrials;
    private List<string> dataLog = new List<string>();
    private float timeSpentMining = 0f;
    private float timeInShelter = 0f;
    private float timeNotInZones = 0f;

    private bool playerInShelter = false;
    private bool playerInMining = false;

    void Start()
    { 
        //UDP sender code G for start cue
        UDPSender.sendString("G");
        
        // Randomly assign 40 trials as storm trials
        stormTrials = new bool[totalTrials];
        List<int> stormIndices = new List<int>();
        while (stormIndices.Count < stormCount)
        {
            int idx = UnityEngine.Random.Range(0, totalTrials);
            if (!stormIndices.Contains(idx))
                stormIndices.Add(idx);
        }
        foreach (int idx in stormIndices)
            stormTrials[idx] = true;

        StartCoroutine(RunTrials());
    }

    IEnumerator RunTrials()
    {
        for (currentTrial = 0; currentTrial < totalTrials; currentTrial++)
        {
            // ---- Phase 1: Expectancy ----
            yield return StartCoroutine(DoExpectancy());

            // ---- Phase 2: Mining (with possible storm) ----
            yield return StartCoroutine(DoTrial(currentTrial));
        }

        Debug.Log("All trials complete!");
        foreach (var line in dataLog)
            Debug.Log(line);
    }

    IEnumerator DoExpectancy() //Have the participant input their expectancy rating 
{
        // Disable movement
        playerController.enabled = false;

        //Disable mining
        oreMiner.isExpectancyActive = true;

    // Reset UI state
    expectancyPanel.SetActive(true);
    ExpectancyUI expectancyUI = expectancyPanel.GetComponent<ExpectancyUI>();
    expectancyUI.ResetExpectancy();
    
    // Lock expectancy panel for 1 second so player doesn't accidentally skip when actively mining
    expectancyUI.enabled = false;   // temporarily disable script so it can't process input
    yield return new WaitForSeconds(1f);
    expectancyUI.enabled = true;    // re-enable after lockout

    // Wait until expectancy is confirmed
        while (!expectancyUI.expectancySelected)
        {
            yield return null;
        }

    // Store the value for logging
    expectancyValue = expectancyUI.expectancyValue;
    StreamWriter sw = new StreamWriter("ExpectancyRatingFile", true);
    sw.WriteLine("{0}, {1}, Expectancy {2}", Time.time, DateTime.Now, expectancyValue);
    sw.Close();

    // Hide the panel and re-enable movement and mining
    expectancyPanel.SetActive(false);
        playerController.enabled = true;
        oreMiner.isExpectancyActive = false;
}

    IEnumerator DoTrial(int trialNum)
    {
        float timer = 0f;
        bool stormActive = stormTrials[trialNum];
        bool shocked = false;
        timeSpentMining = 0f; //Reset time spent mining at start of each trial
        timeInShelter = 0f; //Reset time in shelter at start of each trial

       //Start movement recording 
        movementRecorder.StartRecording(trialNum + 1, stormActive);   

    // Initial Behavior Code (where they were when the trial started)
    // 0 = other, 1 = house, 2 = mining
    int initialBehavior = 0;
        if (playerInShelter)
        {
            initialBehavior = 1;
        }
        else if (playerInMining)
        {
            initialBehavior = 2;
        }
    
    //Reset miner points at the start of trial
        if(oreMiner !=null) 
        {
            oreMiner.ResetMiner();
        }

    
    // Play warning immediately if this is a storm trial and have storm clouds roll in
    if (stormActive)
    {
        //UDP sender code G for storm cue
        UDPSender.sendString("G");
        warningSound.Play();
        stormClouds.Play();
        StartCoroutine(FadeLightIntensity(directionalLight, 1.4f, 1.0f, trialDuration));
    }

    while (timer < trialDuration)
    {
        timer += Time.deltaTime;

        //Track how long participant mines during the trial
        if (playerInMining) 
        {
            timeSpentMining += Time.deltaTime;
        }
        //Track how long participant is in the shelter during the trial
        if (playerInShelter)
        {
            timeInShelter += Time.deltaTime;
        }
        //Track how long participant is not in the shelter or mines
         if (!playerInMining && !playerInShelter)
        {
            timeNotInZones += Time.deltaTime;
        }
           

        yield return null;
    }
    // Stop recording when trial ends
    movementRecorder.StopRecording();

    // Now that the trial has truly ended, check for shock
if (stormActive)
{
    // Wait one physics frame to ensure trigger updates have processed
    yield return new WaitForFixedUpdate();

    if (!playerInShelter) {
 //sends message to UDPServer script for shock
 UDPSender.sendString("S");	        
yield return StartCoroutine(ApplyShocks());
    shocked = true;
    }
    else {
        shocked = false;
        //sends message to UDPServer script for safe (neutral)
        UDPSender.sendString("N");	
    }
        
}

    //Stop particle effect (storm clouds) when trial ends
    if(stormActive && stormClouds !=null) {
        stormClouds.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
         // Restore brightness after storm ends
            if (directionalLight != null)
                StartCoroutine(FadeLightIntensity(directionalLight, 1.0f, 1.4f, 2f));
    }

    // Final behavior code 
    // 0 = other, 1 = house, 2 = mining
    int finalBehavior = 0;
    if (playerInShelter) {
        finalBehavior = 1;
    }
    else if (playerInMining) {
        finalBehavior = 2;
    }

    // --- Log trial ---
    dataLogger.LogTrial(
        trialNum + 1,                // trial #
        stormActive,                 // storm present (Y/N)
        shocked,                      //Whether they got shocked
        expectancyValue,             // expectancy (0–9)
        initialBehavior,             //Where they were when trial started
        finalBehavior,               // Where they were when trial ended
        oreMiner.pointsThisTrial,     // points earned
        timeSpentMining,             // Tracked above in the trial loop
        timeInShelter,                //Tracked in the trial loop
        timeNotInZones              //Time not in shelter or mine. Tracked in trial loop
    );

    // Save immediately so progress is never lost
    dataLogger.SaveLog();
    }

    IEnumerator ApplyShocks()
    {
        //Freeze walking/rotation
        playerController.canMove = false;
        
        for (int i = 0; i < 5; i++) //Once per second for 5 seconds
        {
            //Have some way to connect to biopac 
            Debug.Log("Shock!");
            yield return new WaitForSeconds(1f);
        }

            //Restore movement after shocks
            playerController.canMove = true;
        }

//Detect whether player is in shelter or mining area. This uses ZoneDetector.cs
public void SetPlayerInShelter(bool inside)
    {
        playerInShelter = inside;
        Debug.Log("Shelter status: " + inside);
    }

    public void SetPlayerInMining(bool inside)
    {
        playerInMining = inside;
        Debug.Log("Mining status: " + inside);
    }
  // Smoothly fade the directional light intensity during a storm trial
    IEnumerator FadeLightIntensity(Light light, float start, float end, float duration)
    {
        float elapsed = 0f;
        light.intensity = start;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            light.intensity = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        light.intensity = end;
    }

}

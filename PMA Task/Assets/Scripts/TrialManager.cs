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
    public int totalTrials = 40;
        /**************************************************************************
     * CHANGE: Add ranges for free movement and decision movement times
     * Reason: Jittered time ranges
     * Modified: 12/12/25 by SD
     **************************************************************************/
    //15-20 for free movement
    public float freeMovementMin = 15f;
    public float freeMovementMax = 20f;
    //12-15 sec for decision movement
    public float stormDurationMin = 12f;
    public float stormDurationMax= 15f;

    [Header("References")]    public GameObject expectancyPanel;
    public FirstPersonController playerController; // disable/enable movement script
    public AudioSource warningSound;
    public ParticleSystem stormClouds;
    public ParticleSystem rainStorm;
    public OreMiner oreMiner;
    public PlayerDataLogging dataLogger;
    public RecordPlayerMovement movementRecorder;
    public int CurrentTrial => currentTrial;
    public Vector3 playerPosition;
    private string folderPath;
    //UDPSender comunicator
	public UDPSender U;


    [Header("Environment")]
    public Collider miningArea;
    public Collider shelterArea;
    public Light directionalLight;

    [Header("Setup")]
    private int currentTrial = 0;
    private int expectancyValue = 5;
    private bool[] stormTrials;
    private List<string> dataLog = new List<string>();
    private float timeSpentMining = 0f;
    private float timeInShelter = 0f;
    private float timeNotInZones = 0f;

    private bool playerInShelter = false;
    private bool playerInMining = false;
    
void Awake() 
{
    
}
    void Start()
    { 
        folderPath = FolderManager.Instance.SessionFolderPath;
        
        //UDP sender code G for start cue
        UDPSender.sendString("G");
        
    /**************************************************************************
     * CHANGE: Assign all trials as storm trials
     * Reason: No reason for non-storm trials
     * Modified: 12/12/25 by SD
     **************************************************************************/
    // All trials are storm trials
    stormTrials = new bool[totalTrials];
    for (int i = 0; i < totalTrials; i++)
        stormTrials[i] = true;

        StartCoroutine(RunTrials());
    }

 /**************************************************************************
     * CHANGE: Added Update to get player's location
     * Reason: To put in StreamWriter statements when events happen
     * Modified: 12/12/25 by SD
     **************************************************************************/
    void Update()
    {
        playerPosition = playerController.transform.position;
    }

    IEnumerator RunTrials()
    {
        for (currentTrial = 0; currentTrial < totalTrials; currentTrial++)
        {
          
            // ---- Run the trial (expectancy will show during storm if it happens) ----
            yield return StartCoroutine(DoTrial(currentTrial));
        }

        Debug.Log("All trials complete!");
        foreach (var line in dataLog)
            Debug.Log(line);
    }

    IEnumerator DoTrial(int trialNum) //Each trial: Free movement -> Storm sound -> Expectancy -> Decision movement -> Possible shock
    {
        float timer = 0f;
        bool stormActive = stormTrials[trialNum];
        bool shocked = false;
        timeSpentMining = 0f;
        timeInShelter = 0f;
       float stormDuration = UnityEngine.Random.Range(stormDurationMin, stormDurationMax);
    float freeMovementDuration = UnityEngine.Random.Range(freeMovementMin, freeMovementMax);

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
    
    /**************************************************************************
     * CHANGE: Free movement added to start of each trial before storm sound and expectancy
     MODIFIED: 12/12/2025
     **************************************************************************/
     //PHASE 1 - FREE MOVEMENT ******************************************************
    yield return StartCoroutine(DoFreeMovement(freeMovementDuration));
    Debug.Log("Free movement duration = " + freeMovementDuration);


    //PHASE 2 - STORM WARNING ******************************************************
    // Play warning immediately if this is a storm trial and have storm clouds roll in
    if (stormActive)
    {
        //UDP sender code G for storm cue
        UDPSender.sendString("G");
        warningSound.Play();

        //Document when the storm sound happened
    string stormPath = Path.Combine(folderPath, "StormFile.txt");
    using (StreamWriter sw = new StreamWriter(stormPath, true))
    {
        sw.WriteLine("{0}, {1}, Position ({2:F2}, {3:F2}, {4:F2}", Time.time, DateTime.Now, playerPosition.x, playerPosition.y, playerPosition.z);
    }
    }

    // PHASE 3 - EXPECTANCY RATING ******************************************************
    yield return StartCoroutine(DoExpectancy());

    /**************************************************************************
     * CHANGE: Decision movement made as a Coroutine method
     MODIFIED: 12/12/2025 by SD
     **************************************************************************/
    //PHASE 4 - DECISION MOVEMENT ******************************************************
   stormClouds.Play();
   rainStorm.Play();
    StartCoroutine(FadeLightIntensity(directionalLight, 1.4f, 1.0f, stormDuration));
   yield return StartCoroutine(DoDecisionMovement(stormDuration));
   Debug.Log("Decision movement duration = " + stormDuration);


    //PHASE 5 - SHOCK EVALUATION ******************************************************
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
    //Document when the shock happened
    string shockPath = Path.Combine(folderPath, "ShockFile.txt");
    using (StreamWriter sw = new StreamWriter(shockPath, true))
    {
        sw.WriteLine("{0}, {1}, Position ({2:F2}, {3:F2}, {4:F2})", Time.time, DateTime.Now, playerPosition.x, playerPosition.y, playerPosition.z);
    }
    }
        else {
            shocked = false;
            //sends message to UDPServer script for safe (neutral)
            UDPSender.sendString("N");	
        }
    }

    //PHASE 6 - END TRIAL AND RECORD ******************************************************
    // Stop recording movement when trial ends
    movementRecorder.StopRecording();

    //Stop particle effect (storm clouds) when trial ends
    if(stormActive && stormClouds !=null) {
        stormClouds.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        rainStorm.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
    string expectancyPath = Path.Combine(folderPath, "ExpectancyRatingFile.txt");
    using (StreamWriter sw = new StreamWriter(expectancyPath, true))
{
    sw.WriteLine("{0}, {1}, Expectancy {2}, Position ({2:F2}, {3:F2}, {4:F2})", Time.time, DateTime.Now, expectancyValue, playerPosition.x, playerPosition.y, playerPosition.z);
}

    // Hide the panel and re-enable movement and mining
    expectancyPanel.SetActive(false);
        playerController.enabled = true;
        oreMiner.isExpectancyActive = false;
}

    /**************************************************************************
         * MODIFIED: 12/12/2025
         * CHANGE: Added free movement method
         * REASON: Participants should have time to roam and mine freely before storm sound
         **************************************************************************/

    IEnumerator DoFreeMovement(float duration) //Have participants roam and mine freely before storm sound
    {
        float timer = 0f;
        
    while (timer < duration)
    {
        timer += Time.deltaTime;

        // Track free-movement behaviors 
        if (playerInMining)
            timeSpentMining += Time.deltaTime;

        if (playerInShelter)
            timeInShelter += Time.deltaTime;

        if (!playerInMining && !playerInShelter)
            timeNotInZones += Time.deltaTime;

        yield return null;
    }
    }
  /**************************************************************************
         * MODIFIED: 12/12/2025
         * CHANGE: Added decision movement method
         * REASON: Participants should have time to roam and mine freely before storm sound
         **************************************************************************/
    IEnumerator DoDecisionMovement(float duration) //Participants decide how they want to move following storm sound
    {

    float timer = 0f;
    while (timer < duration)
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

        //Record when player enters or exits the shelter
        string enterexitPath = Path.Combine(folderPath, "EnterExit.txt"); 
        string shelterState = inside ? "ENTER" : "EXIT";
    using (StreamWriter sw = new StreamWriter(enterexitPath, true))
    {
        sw.WriteLine("{0}, {1}, {2} Shelter, Position ({3:F2}, {4:F2}, {5:F2})", Time.time, DateTime.Now, shelterState, playerPosition.x, playerPosition.y, playerPosition.z);
    }
    }

    public void SetPlayerInMining(bool inside)
    {
        playerInMining = inside;
        Debug.Log("Mining status: " + inside);

      //Record when player enters or exits the mine
      string enterexitPath = Path.Combine(folderPath, "EnterExit.txt"); 
        string mineState = inside ? "ENTER" : "EXIT";
    using (StreamWriter sw = new StreamWriter(enterexitPath, true))
    {
        sw.WriteLine("{0}, {1}, {2} Mine, Position ({3:F2}, {4:F2}, {5:F2})", Time.time, DateTime.Now, mineState, playerPosition.x, playerPosition.y, playerPosition.z);
    }
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

using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

public class PlayerDataLogging : MonoBehaviour
{
    private List<string> logLines = new List<string>();
    private string filePath;
    private string folderPath;


    void Start()
    {
        // File name: pmaDataLog_MM_DD_YYYY.txt
        string fileName = "TrialsSummary.txt";
        // Save to desktop
        folderPath = FolderManager.Instance.SessionFolderPath;
        filePath = Path.Combine(folderPath, fileName);
        // Write header row
        logLines.Add("Trial\tStorm\tShock\tExpectancy\tPreStormLocation(0=inbetween;1=house;2=mine)\tStormEndLocation(0=inbetween;1=house;2=mine)\tPoints\tTotalMiningTime\tTotalShelterTime\tTotalTimeNotInShelterMining\tMiningTime_Baseline\tMiningTime_Decision\tShelterTime_Baseline\tShelterTime_Decision\tTimeNotInShelterMining_Baseline\tTimeNotInShelterMining_Decision\tTimeFacingShelter\tTimeFacingMiningArea\tTimeToShelter(-77=Started in shelter; -88=Did not reach shelter; -99=Stayed mining)");
    }

    //Add row of trial data
      public void LogTrial(int trialNum, bool storm, bool shock, int expectancy, int initialBehavior, int finalBehavior, int points,
    float miningTime, float shelterTime, float noZoneTime,
    float miningTime_Baseline, float miningTime_Decision,
    float shelterTime_Baseline, float shelterTime_Decision,
    float noZoneTime_Baseline, float noZoneTime_Decision,
    float timeFacingHouse, float timeFacingMine, float timeToSafeHouse)
    {
        // storm = 0/1
        int stormVal = storm ? 1 : 0; //1 if there was a storm trial

        //shock = 0/1
        int shockVal = shock ? 1 : 0; //1 if there was a shock given 

        string line = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7:F2}\t{8:F2}\t{9:F2}\t{10:F2}\t{11:F2}\t{12:F2}\t{13:F2}\t{14:F2}\t{15:F2}\t{16:F2}\t{17:F2}\t{18:F2}",
        trialNum, stormVal, shockVal, expectancy, initialBehavior, finalBehavior, points,
        miningTime, shelterTime, noZoneTime,
        miningTime_Baseline, miningTime_Decision,
        shelterTime_Baseline, shelterTime_Decision,
        noZoneTime_Baseline, noZoneTime_Decision,
        timeFacingHouse, timeFacingMine, timeToSafeHouse);

        logLines.Add(line);
    }

    //Save the file at the end of the experiment
    public void SaveLog()
    {
    File.AppendAllLines(filePath, logLines); //Each trial gets added to the file immediately
    logLines.Clear(); // clear buffer after writing
    Debug.Log("Data log saved to desktop: " + filePath);
    }
}

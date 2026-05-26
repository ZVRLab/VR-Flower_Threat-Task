using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AcclimationEnd : MonoBehaviour
{
    public string optionsPageName = "OptionsPage";

    // Update is called once per frame
    void Update()
    {
		//Leaves the acclimation scene
		if(Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.T))
	    {
	    	if (SceneLoader.needsAcclimation) {
                SceneManager.LoadScene(SceneLoader.nextScene);
            }
	    }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(optionsPageName);
        }
    }
}
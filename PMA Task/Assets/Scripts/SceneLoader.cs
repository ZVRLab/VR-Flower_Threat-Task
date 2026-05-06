using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    
    [Header("Optional Escape Behavior")]
    public string escapeScene; // set per scene in Inspector
    public bool quitOnEscape = false; //This is set as true for the Main Menu scene, as this will quit the game entirely. 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (quitOnEscape) //If on Main Menu (because this is marked true in the inspector)
            {
                QuitGame();
            }
            else if (!string.IsNullOrEmpty(escapeScene)) //Otherwise, it will take you to the scene set in the inspector. 
            {
                LoadScene(escapeScene);
            }
        }
    }

    //This works through buttons on the panels with the OnClick() behavior
    //Had to add scenes to build settings. 0: Main Menu; 1: Option Page; 2: Practice; 3: End Scene; 4: Platform; 5: Crossing
    //This script is added to the scene manager object which is dragged into each panels OnClick() event to individually choose LoadScene(String) and then type the scene name EXACTLY as it appears in Unity
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

        public void QuitGame() //Also assigned to the Quit Button in the inspector for the Main Menu scene OnClick()
    {
        Debug.Log("Quitting game...");

        Application.Quit();
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    //This works through buttons on the panels with the OnClick() behavior
    //Had to add scenes to build settings. 0: Main Menu; 1: Option Page; 2: Practice; 3: End Scene; 4: Platform; 5: Crossing
    //This script is added to the scene manager object which is dragged into each panels OnClick() event to individually choose LoadScene(String) and then type the scene name EXACTLY as it appears in Unity
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

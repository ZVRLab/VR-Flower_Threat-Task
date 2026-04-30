using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    private int score = 0;

    public bool isPracticeMode = false;

    void Start()
    {
            if (isPracticeMode)
        scoreText.text = "";   // start blank
    else
        UpdateScore(0);
    }

    public void UpdateScore(int newScore) //Called upon when mining
    {
        score = newScore;
        scoreText.text = "Points: " + score.ToString();
    }

    public void ShowMining(bool isMining)
    {
         scoreText.text = isMining ? "Mining..." : "";
    }
}

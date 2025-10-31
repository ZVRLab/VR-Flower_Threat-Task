using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    private int score = 0;

    void Start()
    {
        UpdateScore(0);
    }

    public void UpdateScore(int newScore) //Called upon when mining
    {
        score = newScore;
        scoreText.text = "Points: " + score.ToString();
    }
}

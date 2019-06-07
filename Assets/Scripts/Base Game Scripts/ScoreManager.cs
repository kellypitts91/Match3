using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private Board board;
    public Text scoreText;
    public int score;
    public Image scoreBar;
    private GameData gameData;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        gameData = FindObjectOfType<GameData>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = score.ToString();
    }

    public void IncreaseScore(int amountToIncrease) {
        score += amountToIncrease;
        if(gameData != null) {
            int highScore = gameData.saveData.highScores[board.level];
            //todo - only want to do this if the level was won
            if(score > highScore) {
                gameData.saveData.highScores[board.level] = score;
            }
            gameData.Save();
        }
        UpdateBar();
    }

    private void UpdateBar() {
        if(board != null && scoreBar != null) {
            int length = board.scoreGoals.Length;
            scoreBar.fillAmount = (float)score / (float)board.scoreGoals[length-1];
        }
    }
}

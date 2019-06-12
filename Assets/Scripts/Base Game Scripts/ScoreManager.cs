﻿using System.Collections;
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
    private int numberStars;

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
        for(int i = 0; i < board.scoreGoals.Length; i++) {
            if(score > board.scoreGoals[i] && numberStars < i+1) {
                numberStars++;
            }
        }
        if(gameData != null) {
            int highScore = gameData.saveData.highScores[board.level];
            //todo - only want to do this if the level was won
            if(score > highScore) {
                gameData.saveData.highScores[board.level] = score;
            }
            int currentStars = gameData.saveData.stars[board.level];
            if(numberStars > currentStars) {
                gameData.saveData.stars[board.level] = numberStars;
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

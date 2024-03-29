﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameType {
    Moves,
    Time
}

[System.Serializable]
public class EndGameRequirements {
    public GameType gameType;
    public int counterValue;
}

public class EndGameManager : MonoBehaviour
{
    public EndGameRequirements requirements;
    public GameObject movesLabel;
    public GameObject timeLabel;
    public GameObject youWinPanel;
    public GameObject tryAgainPanel;
    public Text counter;
    public int currentCounterValue;
    private float timerSeconds;
    private Board board;

    // Start is called before the first frame update
    void Start() {
        board = FindObjectOfType<Board>();
        SetGameType();
        SetupGame();
    }

    void SetGameType() {
        if(board.world != null) {
            if(board.level < board.world.levels.Length) {
                Level tempLevel = board.world.levels[board.level];
                if(tempLevel != null) {
                    requirements = tempLevel.requirements;
                }
            }
        }
    }

    void SetupGame() {
        currentCounterValue = requirements.counterValue;
        if(requirements.gameType == GameType.Moves) {
            movesLabel.SetActive(true);
            timeLabel.SetActive(false);
        } else {
            timerSeconds = 1;
            movesLabel.SetActive(false);
            timeLabel.SetActive(true);
        }
        counter.text = currentCounterValue.ToString();
    }

    public void DecreaseCounterValue() {
        if(board.currentState != GameState.pause) {
            currentCounterValue--;
            counter.text = currentCounterValue.ToString();
            if(currentCounterValue <= 0) {
                LoseGame();
            }
        }
    }

    public void WinGame() {
        youWinPanel.SetActive(true);
        board.currentState = GameState.win;
        currentCounterValue = 0;
        counter.text = currentCounterValue.ToString();
        FadePanelController fade = FindObjectOfType<FadePanelController>();
        fade.GameOver();
    }

    public void LoseGame() {
        tryAgainPanel.SetActive(true);
        board.currentState = GameState.lose;
        Debug.Log("Try again");
        currentCounterValue = 0;
        counter.text = currentCounterValue.ToString();
        FadePanelController fade = FindObjectOfType<FadePanelController>();
        fade.GameOver();
    }

    // Update is called once per frame
    void Update() {
        if(requirements.gameType == GameType.Time && currentCounterValue > 0) {
            timerSeconds -= Time.deltaTime;
            if(timerSeconds <= 0) {
                DecreaseCounterValue();
                timerSeconds = 1;
            }
        }
    }
}

using System.Collections;
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
    public Text counter;
    public int currentCounterValue;
    private float timerSeconds;

    // Start is called before the first frame update
    void Start() {
        SetupGame();
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
        currentCounterValue--;
        counter.text = currentCounterValue.ToString();
        if(currentCounterValue <= 0) {
            Debug.Log("Try again");
            currentCounterValue = 0;
            counter.text = currentCounterValue.ToString();
        }
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

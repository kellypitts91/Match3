﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BlankGoal {
    public int numberNeeded;
    public int numberColected;
    public Sprite goalSprite;
    public string matchValue;
}

public class GoalPanel {

    public Image image;
    public Sprite sprite;
    public Text text;
    public string textToDisplay;

    override
    public string ToString() {
        return "image = " + image + 
                ", sprite = " + sprite + 
                ", text = " + text +
                ", textToDisplay = " + textToDisplay;
    }
}

public class GoalManager : MonoBehaviour
{
    public BlankGoal[] levelGoals;
    public List<GoalPanel> currentGoals = new List<GoalPanel>();
    public GameObject goalPrefab;
    public GameObject goalIntroParent;
    public GameObject goalGameParent;
    private EndGameManager endGameManager;
    private Board board;
    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        endGameManager = FindObjectOfType<EndGameManager>();
        GetGoals();
        SetupGoals();
    }

    void GetGoals() {
        if(board != null) {
            if(board.world != null) {
                if(board.level < board.world.levels.Length) {
                    Level tempLevel = board.world.levels[board.level];
                    if(tempLevel != null) {
                        levelGoals = tempLevel.levelGoals;
                        for(int i = 0; i < levelGoals.Length; i++) {
                            levelGoals[i].numberColected = 0;
                        }
                    }
                }
            }
        }
    }

    void SetupGoals() {
        for(int i = 0; i < levelGoals.Length; i++) {
            //Create new goal panel at the goalIntroParent position
            GameObject goal = Instantiate(goalPrefab, goalIntroParent.transform.position, Quaternion.identity);
            goal.transform.SetParent(goalIntroParent.transform, false);
            //Set the image and text of the goal
            GoalPanel panel = setupGoalsPanelWithChildren(goal, levelGoals[i]);
            
            //Create new goal panel at the goalGameParent position
            GameObject gameGoal = Instantiate(goalPrefab, goalGameParent.transform.position, Quaternion.identity);
            gameGoal.transform.SetParent(goalGameParent.transform, false);

            panel = setupGoalsPanelWithChildren(gameGoal, levelGoals[i]);
            currentGoals.Add(panel);
        }
    }

    public void UpdateGoals() {
        int goalsCompleted = 0;
        for(int i = 0; i < levelGoals.Length; i++) {
            currentGoals[i].text.text = levelGoals[i].numberColected + "/" + levelGoals[i].numberNeeded; 
            if(levelGoals[i].numberColected >= levelGoals[i].numberNeeded) {
                goalsCompleted++;
                currentGoals[i].text.text = levelGoals[i].numberNeeded + "/" + levelGoals[i].numberNeeded;
            }
        }
        if(goalsCompleted >= levelGoals.Length) {
            if(endGameManager != null) {
                endGameManager.WinGame();
                FadePanelController fade = FindObjectOfType<FadePanelController>();
                fade.GameOver();
                Debug.Log("You win!!");
            }
        }
    }

    public void CompareGoal(string goalToCompare) {
        for(int i = 0; i < levelGoals.Length; i++) {
            if(goalToCompare == levelGoals[i].matchValue) {
                levelGoals[i].numberColected++;
            }
        }
    }

    private GoalPanel setupGoalsPanelWithChildren(GameObject goal, BlankGoal levelGoal) {
        Image image = null;
        Text text = null;
        for(int i = 0; i < goal.transform.childCount; i++) {
            GameObject currentItem = goal.transform.GetChild(i).gameObject;
            if(currentItem.GetComponentInChildren<Image>() != null) {
                image = currentItem.GetComponentInChildren<Image>();
                image.sprite = levelGoal.goalSprite;
            }
            if(currentItem.GetComponentInChildren<Text>() != null) {
                text = currentItem.GetComponentInChildren<Text>();
                text.text = "0/"+ levelGoal.numberNeeded;
            }
        }
        if(image != null && text != null) {
            GoalPanel goalPanel = new GoalPanel();
            goalPanel.image = image;
            goalPanel.sprite = image.sprite;
            goalPanel.text = text;
            goalPanel.textToDisplay = text.text;
            return goalPanel; 
        }
        return null;
    }
}

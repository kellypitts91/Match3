using System.Collections;
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

public class GoalManager : MonoBehaviour
{
    public BlankGoal[] levelGoals;
    public List<GoalPanel> currentGoals = new List<GoalPanel>();
    public GameObject goalPrefab;
    public GameObject goalIntroParent;
    public GameObject goalGameParent;
    private EndGameManager endGameManager;
    // Start is called before the first frame update
    void Start()
    {
        endGameManager = FindObjectOfType<EndGameManager>();
        SetupGoals();
    }

    void SetupGoals() {
        for(int i = 0; i < levelGoals.Length; i++) {
            //Create new goal panel at the goalIntroParent position
            GameObject goal = Instantiate(goalPrefab, goalIntroParent.transform.position, Quaternion.identity);
            goal.transform.SetParent(goalIntroParent.transform, false);
            //Set the image and text of the goal
            GoalPanel panel = goal.GetComponent<GoalPanel>();

            Debug.Log(levelGoals[i].goalSprite);
            Debug.Log(levelGoals[i].numberNeeded);
            // panel.thisSprite = levelGoals[i].goalSprite;
            // panel.thisString = "0/" + levelGoals[i].numberNeeded;

            //Create new goal panel at the goalGameParent position
            GameObject gameGoal = Instantiate(goalPrefab, goalGameParent.transform.position, Quaternion.identity);
            gameGoal.transform.SetParent(goalGameParent.transform, false);

            panel = gameGoal.GetComponent<GoalPanel>();
            currentGoals.Add(panel);
            // panel.thisSprite = levelGoals[i].goalSprite;
            // panel.thisString = "0/" + levelGoals[i].numberNeeded;
        }
    }

    public void UpdateGoals() {
        int goalsCompleted = 0;
        for(int i = 0; i <levelGoals.Length; i++) {
            currentGoals[i].thisText.text = levelGoals[i].numberColected + "/" + levelGoals[i].numberNeeded; 
            if(levelGoals[i].numberColected >= levelGoals[i].numberNeeded) {
                goalsCompleted++;
                currentGoals[i].thisText.text = levelGoals[i].numberNeeded + "/" + levelGoals[i].numberNeeded;
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
}

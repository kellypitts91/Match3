using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SaveData {
    public bool[] isActive;
    public int[] highScores;
    public int[] stars;
}

public class GameData : MonoBehaviour
{
    public static GameData gameData;
    public SaveData saveData;
    private string fileName;

    // Start is called before the first frame update
    void Awake() {
        if(gameData == null) {
            DontDestroyOnLoad(this.gameObject);
            gameData = this;
        } else {
            Destroy(this.gameObject);
        }
        fileName = Application.persistentDataPath + "/player.dat";
        Load();
    }

    private void Start() {
        
    }

    public void Save() {
        //Create a binary formatter which can read binary files
        BinaryFormatter formatter = new BinaryFormatter();
        //Create a route from the program to the file
        FileStream file = File.Open(fileName, FileMode.Create);
        //Create a copy of save data
        SaveData data = new SaveData();
        data = saveData;
        //Actually save the data in the file
        formatter.Serialize(file, data);
        file.Close();
        Debug.Log("Saved");
    }

    public void Load() {
        //Check if the save game file exist
        if(File.Exists(fileName)) {
            //Create a binary formatter
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(fileName, FileMode.Open);
            saveData = formatter.Deserialize(file) as SaveData;
            file.Close();
            Debug.Log("Loaded");
        } else {
            Debug.Log("File does not exist");
        }
    }

    private void OnDisable() {
        Save();
    }

    // Update is called once per frame
    void Update() {
        
    }
}

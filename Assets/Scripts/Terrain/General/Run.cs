using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Run : MonoBehaviour
{
    [System.Serializable]
    public struct Level
    {
        [System.Serializable]
        public struct Exit
        {
            public int stagesEarly;
            public string exitTo;
        }

        public string name;
        public int subStages;
        public float difficultyBonusPerStage;
        public float levelDifficultyBonus;
        public GameObject levelPrefab;
        public GameObject managerOverride;
        public Exit[] nextLevels;
    }
    public static Run current;
    public GameObject managerPrefab;
    GameObject player;
    int difficulty;
    Level currentLevel;
    int currentSubStage;
    public Level[] levels;

    public static bool StartRun(GameObject container, Level[] levelList)
    {
        if (StartRun(container, levelList, 0))
        {
            return true;
        }
        return false;
    }


    private Level? GetLevel(string targetString)
    {
        foreach(Level level in levels)
        {
            if(level.name == targetString)
            {
                return level;
            }
        }
        Debug.LogError("Could not find a level named " + targetString + " in Level List.");
        return null;
    }

    public void LevelUp(int exitNumber = 0)
    {
        SceneManager.sceneLoaded += OnSceneLoaded; //Add our "OnSceneLoaded" function to the list of functions to be called when a new scene is loaded. This is scary, so this function removes itself from the list when it's done
        if (currentSubStage >= currentLevel.subStages)
        {
            currentLevel = (Level)GetLevel(currentLevel.nextLevels[exitNumber].exitTo);
            currentSubStage = 1;
        }
        else
        {
            currentSubStage++;
        }
        print("Entered " + currentLevel.name + " - " + currentSubStage);
        SceneManager.LoadScene("Level");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Instantiate(currentLevel.levelPrefab);
        player.GetComponent<PlayerController>().gameOverAnimator = GameObject.Find("UICanvas").GetComponent<Animator>();
        if (currentLevel.managerOverride)
        {
            Instantiate(currentLevel.managerOverride, new Vector3(0, 0, -4), Quaternion.identity);
        }
        else
        {
            Instantiate(managerPrefab, new Vector3(0, 0, -4), Quaternion.identity);
        }
        SceneManager.sceneLoaded -= OnSceneLoaded; 
    }

    private void Setup(int difficulty)
    {
        player = GameObject.Find("Player");
        DontDestroyOnLoad(player);
        currentLevel = levels[0];
        currentSubStage = 1;
        this.difficulty = difficulty;
    }

    public static bool StartRun(GameObject container, Level[] levelList, int difficulty)
    {
        if (current)
        {
            Debug.LogWarning("Cannot start a new run while there is a run in existance");
            return false;
        }


        Run newRun = container.AddComponent<Run>();
        newRun.levels = levelList;

        if (difficulty < 0)
        {
            Debug.LogWarning("Tried to start a run with negative difficulty (" + difficulty + ")");
            newRun.Setup(0);
        }
        else
        {
            if (difficulty > 0)
            {
                print("Started a run with custom difficulty of " + difficulty + ".");
            }
            newRun.Setup(difficulty);
        }


        current = newRun;
        DontDestroyOnLoad(container);
        return true;
    }

    public int GetSubStage()
    {
        return currentSubStage;
    }

    public int GetDifficulty()
    {
        return difficulty;
    }
}

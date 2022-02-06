using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int level = 0;
    [SerializeField] private PackageGenerator pg;
    [SerializeField] private AIContainer ai;

    [SerializeField] private int packagesGeneratedBeforeNewLevel = 100;
    [SerializeField] private Text levelDisplay;

    [SerializeField] private float cooldownTime;

    [SerializeField] private GameMode gamemode;

    public UnityEvent resetEvent;
    public UnityEvent pauseEvent;
    public UnityEvent unPauseEvent;



    private bool paused = false;

    private int leftScore;
    private int rightScore;
    private int totalScore;

    public void Awake() {
        resetEvent = new UnityEvent();
        pauseEvent = new UnityEvent();
        unPauseEvent = new UnityEvent();
    }

    // Start is called before the first frame update
    void Start()
    {
        //pause the package generator for a second to give player some breathing room before starting.      
        gamemode = FindObjectOfType<GameMode>();
        if (gamemode == null) {
            Debug.LogError("Error: GameMode object is not in the scene.");
        }
        else {
            level = gamemode.GetStartingLevel();
            if (gamemode.GetMode() == GameModeEnum.VS_AI) {
                ai.gameObject.SetActive(true);
                ai.ApplyDifficulty(gamemode.GetAIDifficulty());
            }
        }   
        LevelValueAssigner.AssignLevelValues(pg, level);


        pg.CoolDown(2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause")) {
            if (paused) {
                UnPause();
            }
            else {
                Pause();
            }
        }

        if (paused && Input.GetKeyDown(KeyCode.Tab)) {
            Debug.Log("Quit");
            Application.Quit();
        }

        //Check if enough packages have been generated to advance to a new level. 
        if (pg.packagesGenerated >= packagesGeneratedBeforeNewLevel) {
            NewLevel();
        }

        //Update display Text
        levelDisplay.text = UpdateDisplayText();

        //Check for losing condition. 
        if (gamemode.LosingConditionSatisfied()) {
            Restart();
        }

        //Check if the player pressed a number key.
        int key = PressedANumberKey();
        if (key != -1) {
            SetGameSpeed(KeyToNumber(key));
        }
    }

    public void NewLevel() {
        level++;
        pg.CoolDown(cooldownTime);
        LevelValueAssigner.AssignLevelValues(pg, level);
        pg.packagesGenerated = 0;
    }

    public void Restart() {
        leftScore = 0;
        rightScore = 0;
        totalScore = 0;
        level = gamemode.GetStartingLevel();
        LevelValueAssigner.AssignLevelValues(pg, level);
        resetEvent.Invoke();
    }

    public void Pause() {
        Time.timeScale = 0;
        paused = true;
        pauseEvent.Invoke();
    }

    public void UnPause() {
        Time.timeScale = 1;
        paused = false;
        unPauseEvent.Invoke();
    }

    private string UpdateDisplayText() {
        StringBuilder sb = new StringBuilder();

        sb.Append("Level: " + (level + 1) + "\n");
        if (gamemode.GetMode() == GameModeEnum.CO_OP || gamemode.GetMode() == GameModeEnum.VERSUS || gamemode.GetMode() == GameModeEnum.VS_AI) {
            sb.Append("1P Score: " + leftScore + "\n");
            sb.Append("2P Score: " + rightScore + "\n");
        }

        sb.Append("Total Score: " + totalScore);

        return sb.ToString();
    }

    //Converts a number to its ASCII code equivalent
    private int NumberToKey(int num) {
        if (num < 0 || num > 9) return 0;
        return num + 48;
    }

    private int KeyToNumber(KeyCode key) {
        return (int)key - 48;
    }

    private int KeyToNumber(int key) {
        return (int)key - 48;
    }

    private int PressedANumberKey() {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKeyDown(key) && ((int) key >= 48 && (int) key <= 57)) {
                return (int) key;
            }
        }

        return -1;
    }

    private void SetGameSpeed(int value) {
        if (value <= 0) return;
        Time.timeScale = (float) 1 / (float) value;
    }

    public void IncLeftScore(int value) {
        leftScore += value;
        IncTotalScore(value);
    }

    public void IncRightScore(int value) {
        rightScore += value;
        IncTotalScore(value);
    }

    public void IncTotalScore(int value) {
        totalScore += value;
    }

}

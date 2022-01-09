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
        resetEvent.AddListener(Restart);
    }

    // Start is called before the first frame update
    void Start()
    {
        //pause the package generator for a second to give player some breathing room before starting. 
        pg.CoolDown(2f);
        LevelValueAssigner.AssignLevelValues(pg, level);
        gamemode = FindObjectOfType<GameMode>();
        if (gamemode == null) {
            Debug.LogError("Error: GameMode object is not in the scene.");
        }

        level = gamemode.GetStartingLevel();
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

        //Check if enough packages have been generated to advance to a new level. 
        if (pg.packagesGenerated >= packagesGeneratedBeforeNewLevel) {
            NewLevel();
        }

        //Update display Text
        levelDisplay.text = UpdateDisplayText();

        //Check for losing condition. 
        if (gamemode.LosingConditionSatisfied()) {
            resetEvent.Invoke();
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
        level = 0;
        LevelValueAssigner.AssignLevelValues(pg, level);
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

        sb.Append("Level: " + level + "\n");
        if (gamemode.GetMode() == GameModeEnum.CO_OP || gamemode.GetMode() == GameModeEnum.VERSUS) {
            sb.Append("1P Score: " + leftScore + "\n");
            sb.Append("2P Score: " + rightScore + "\n");
        }

        sb.Append("Total Score: " + totalScore);

        return sb.ToString();
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

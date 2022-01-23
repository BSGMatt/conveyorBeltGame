using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMode : MonoBehaviour
{

    [SerializeField] private GameModeEnum mode;
    [SerializeField] private int startingLevel;

    public void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public void SetMode(GameModeEnum mode) {
        this.mode = mode;
    }

    public void SetModeByValue(int mode) {
        switch(mode) {
            default:
                this.mode = GameModeEnum.SINGLE_PLAYER;
                break;
            case 1:
                this.mode = GameModeEnum.CO_OP;
                break;
            case 2:
                this.mode = GameModeEnum.VERSUS;
                break;
            case 3:
                this.mode = GameModeEnum.VS_AI;
                break;
        }
    }

    public void SetStartingLevel(int level) {
        startingLevel = level;
    }

    public GameModeEnum GetMode() {
        return mode;
    }

    public int GetStartingLevel() {
        return startingLevel;
    }

    public bool LosingConditionSatisfied() {
        MissCounter mc = FindObjectOfType<MissCounter>();
        switch(mode) {
            default: //Versus or Vs. AI
                return mc.LeftExceededMaxMisses() || mc.RightExceededMaxMisses();
            case GameModeEnum.SINGLE_PLAYER:
                return mc.ExceededMaxNumberOfMisses();
            case GameModeEnum.CO_OP:
                return mc.ExceededMaxNumberOfMisses();
        }


    }
}

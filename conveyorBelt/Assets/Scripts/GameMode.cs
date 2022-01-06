using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMode : MonoBehaviour
{

    [SerializeField] private GameModeEnum mode;

    public void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public void SetMode(GameModeEnum mode) {
        this.mode = mode;
    }

    public GameModeEnum GetMode() {
        return mode;
    }

    public bool LosingConditionSatisfied() {
        MissCounter mc = FindObjectOfType<MissCounter>();
        switch(mode) {
            case GameModeEnum.SINGLE_PLAYER:
                return mc.ExceededMaxNumberOfMisses();
            case GameModeEnum.CO_OP:
                return mc.ExceededMaxNumberOfMisses();
            case GameModeEnum.VERSUS:
                return mc.LeftExceededMaxMisses() || mc.RightExceededMaxMisses();
        }

        return false;
    }
}

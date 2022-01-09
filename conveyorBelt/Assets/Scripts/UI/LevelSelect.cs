using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    private const int MAX_LEVEL = 20;

    [SerializeField] private Text LevelDisplayText;
    [SerializeField] private GameMode gm;

    private int level = 1;

    public void Start() {
        UpdateValues();
    }

    public void DecLevelSelect() {

        if (level > 1) {
            level--;
            UpdateValues();
        }

    }
    public void IncLevelSelect() {

        if (level < MAX_LEVEL) {
            level++;
            UpdateValues();
        }

    }

    public void UpdateValues() {
        LevelDisplayText.text = level.ToString();
        gm.SetStartingLevel(level - 1);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


/// <summary>
/// This script keeps track of the amount of misses. 
/// </summary>
public class MissCounter : MonoBehaviour
{

    public UnityEvent leftMissEvent;
    public UnityEvent rightMissEvent;
    [SerializeField] private int maxAmountOfMisses;

    private int leftMisses;
    private int rightMisses;
    private int numberOfMisses;


    [SerializeField] private Text missesDisplay;

    public void Awake() {
        leftMissEvent = new UnityEvent();
        rightMissEvent = new UnityEvent();
        leftMissEvent.AddListener(IncLeftMissCount);
        rightMissEvent.AddListener(IncRightMissCount);
    }

    // Start is called before the first frame update
    void Start()
    {
        numberOfMisses = 0;
        leftMisses = 0;
        rightMisses = 0;

        FindObjectOfType<GameManager>().resetEvent.AddListener(Restart);
    }

    // Update is called once per frame
    void Update()
    {
        missesDisplay.text = DisplayText();
    }

    private string DisplayText() {
        switch (FindObjectOfType<GameMode>().GetMode()) {
            //For 1P and Co-Op:
            default:
                return "Misses: " + numberOfMisses + "/" + maxAmountOfMisses;
            case GameModeEnum.VERSUS:
                return "1P Misses: " + leftMisses + "/" + maxAmountOfMisses + 
                    "\n" + "2P Misses: " + rightMisses + "/" + maxAmountOfMisses;
        }
    }

    public void Restart() {
        numberOfMisses = 0;
        leftMisses = 0;
        rightMisses = 0;
    }

    /// <summary>
    /// Increase the miss counter by 1. 
    /// </summary>
    private void IncMissCount() {
        numberOfMisses++;
    }

    private void IncLeftMissCount() {
        leftMisses++;
        IncMissCount();
    }

    private void IncRightMissCount() {
        rightMisses++;
        IncMissCount();
    }

    /// <summary>
    /// The number of misses. 
    /// </summary>
    /// <returns>The number of misses</returns>
    public int GetNumberOfMisses() {
        return numberOfMisses;
    }

    /// <summary>
    /// Checks whether the number of misses is greater than or equal to the maximum amount of misses allowed. 
    /// </summary>
    /// <returns>true if the number of misses exceeds the maximum amount of misses, false otherwise.</returns>
    public bool ExceededMaxNumberOfMisses() {
        return numberOfMisses >= maxAmountOfMisses;
    }

    public bool LeftExceededMaxMisses() {
        return leftMisses >= maxAmountOfMisses;
    }

    public bool RightExceededMaxMisses() {
        return rightMisses >= maxAmountOfMisses;
    }

}

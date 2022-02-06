using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AIContainer : Container
{
    public enum Difficulty {
        VeryEasy = 1,
        Easy = 2,
        Medium = 3,
        Hard = 4,
        VeryHard = 5
    }

    [SerializeField] private float reactionTime = 0.1f;
    [SerializeField] private float accuracy = 0.5f;
    [SerializeField] private float slapAccuracy = 0.5f;
    [SerializeField] private float moveSpeed = 0.1f;

    //Coroutine for determining where to go. 
    private Coroutine aIcoroutine = null;

    //Coroutine for moving around. 
    private bool moving = false;
    private Coroutine moveCoroutine = null;

    //Queue for packages that have passed the near trigger
    private List<Package> nearTriggerQueue;

    //List of packages that had to be temporarily ignored to catch
    public List<Package> priorityList;

    private int targetPosition = 1;

    public Text aiText;
    private string targetInfoText;

    private Package targetPackage = null;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        nearTriggerQueue = new List<Package>();

    }

    public void Update() {

        if (!controlEnabled) {
            StopAllCoroutines();
            aIcoroutine = null;
            moveCoroutine = null;
        }
        else {
            if (aIcoroutine == null) aIcoroutine = StartCoroutine(AICoroutine());
        }

        //aiText.text = AIQueueString();
    }

    public IEnumerator AICoroutine() {
        bool react = false;

        while (!react) {
            MakeChoice();

            yield return new WaitForSeconds(reactionTime);
        }

        aIcoroutine = null;

        yield return 0;
    }

    public void MakeChoice() {
        if (Random.value < accuracy) {
            Debug.Log("Evaluate");
            EvalulateCurrentGameState();
        }
        else {
            Debug.Log("Don't Evaluate");
        }
    }

    public void MakeChoiceToSlap() {
        if (Random.value < slapAccuracy) {
            Debug.Log("Slap");
            StartSlapCoroutine();
        }
        else {
            Debug.Log("Don't Slap");
        }
    }

    /// <summary>
    /// Check the current state of the game and decided whether or not to pick a different target. 
    /// </summary>
    public void EvalulateCurrentGameState() {

        Debug.Log("Evaluating game state.");

        /*Check if the target package is null (either because a target hasn't been picked yet or the previous target was destroyed)
        The target is going the opposite direction. If so, pick a target. 
        */
        if (targetPackage == null || targetPackage.GetDirection() < 0) {

            //First check if there is any packages near the miss trigger. 
            if (nearTriggerQueue.Count > 0) {
                targetInfoText = "Target is from the near queue.";
                targetPackage = nearTriggerQueue[0];
                nearTriggerQueue.RemoveAt(0);
            }
            else if (priorityList.Count > 0) {//If not, check for packages that had to be ignored to catch ones near the miss trigger. 
                targetInfoText = "Target is from the priority list.";
                targetPackage = priorityList[0];
                priorityList.RemoveAt(0);
            }
            else if (FindObjectOfType<PackageGenerator>().GetFastestPackage() != null) {//If not, go to the fastest package. 
                targetInfoText = "Target is fastest package.";
                targetPackage = FindObjectOfType<PackageGenerator>().GetFastestPackage();
            }

            //Extra debug stuff. Colors the target package red to make testing a bit easier. 
            if (targetPackage != null) {
                //targetPackage.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0);
                targetPosition = targetPackage.GetPosition();
                Debug.Log("Target Package at  Position: " + targetPackage.GetPosition());
            }
        }

        Debug.Log("Staying at the same position: " + targetPosition);
        /*If there is no target, let the console know about it. 
        *This usually happens if there is no packages on screen 
        */
        if (targetPackage == null) {
            targetInfoText = "There is no target picked.";
            Debug.LogWarning("No target package was selected.");
        }

        /*
         * Move to the target position. If the target has changed, then the AI will move to the target package's
         * position. Otherwise, it will move to the previously set target position. 
         */
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(Move(targetPosition));
    }

    /// <summary>
    /// Force the AI to change its target to another package. If the AI already has a target package selected
    /// when the method is called, then that target package is added to the priority list. 
    /// </summary>
    /// <param name="package"></param>
    public void OverrideTargetPackage(Package package) {
        targetPackage = package;
        targetPosition = targetPackage.GetPosition();
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(Move(targetPosition));

        if (targetPosition == FindObjectOfType<PackageGenerator>().PositionWithMostPackages()) {
            MakeChoiceToSlap();
        }
    }

    public IEnumerator Move(int position) {

        while (currentPosition != position) {
            if (currentPosition < position) {
                MoveUp();
            }
            else {
                MoveDown();
            }

            yield return new WaitForSeconds(moveSpeed);
        }

        moveCoroutine = null;

        yield return 0;
    }

    public List<Package> GetNearTriggerQueue() {
        return nearTriggerQueue;
    }

    public void SetTargetPackage(Package package) {
        targetPackage = package;
    }

    public Package GetTargetPackage() {
        return targetPackage;
    }

    public string AIQueueString() {
        StringBuilder sb = new StringBuilder();

        sb.Append("Near Trigger List: [ ");
        foreach (Package i in nearTriggerQueue.ToArray()) {
            sb.Append("Speed: " + i.GetSpeed() + ", Position: " + i.GetPosition() + " ");
        }
        sb.Append("]");

        sb.AppendLine();
        sb.Append("Priority List: [ ");
        foreach (Package i in priorityList.ToArray()) {
            sb.Append("Speed: " + i.GetSpeed() + ", Position: " + i.GetPosition() + " ");
        }
        sb.Append("]");


        sb.AppendLine();

        if (FindObjectOfType<PackageGenerator>().GetFastestPackage() != null) {
            sb.Append("Fastest Package Speed: " + FindObjectOfType<PackageGenerator>().GetFastestPackage().GetSpeed() +
                ", Position: " + FindObjectOfType<PackageGenerator>().GetFastestPackage().GetPosition());
        }

        sb.AppendLine();
        sb.Append(targetInfoText);

        return sb.ToString();
    }

    public void ApplyDifficulty(Difficulty diff) {
        switch(diff) {
            default:
                accuracy = 0.5f;
                slapAccuracy = 0.375f;
                return;
            case Difficulty.Easy:
                accuracy = 0.75f;
                slapAccuracy = 0.5f;
                return;
            case Difficulty.Medium:
                accuracy = 0.875f;
                slapAccuracy = 0.625f;
                return;
            case Difficulty.Hard:
                accuracy = 1;
                slapAccuracy = 0.75f;
                return;
            case Difficulty.VeryHard:
                accuracy = 1;
                slapAccuracy = 1;
                return;

        }
    }
}

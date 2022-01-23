using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AIContainer : Container
{
    [SerializeField] private float reactionTime = 0.1f;
    [SerializeField] private float accuracy = 0.5f;
    [SerializeField] private float slapAccuracy = 0.5f;
    [SerializeField] private float moveSpeed = 0.1f;

    //Coroutine for determining where to go. 
    private Coroutine aIcoroutine = null;

    //Coroutine for moving around. 
    private bool moving = false;
    private Coroutine moveCoroutine = null;

    private Queue<int> farTriggerQueue;

    //Queue for packages that have passed the near trigger
    private Queue<Package> nearTriggerQueue;

    //List of packages that had to be temporarily ignored to catch
    public List<Package> priorityList;

    private int targetPosition = 1;

    public UnityEvent aboutToMissEvent;
    public UnityEvent movingTowardsEvent;

    public Text aiText;

    private Package targetPackage = null;

    public void Awake() {
        aboutToMissEvent = new UnityEvent();
        movingTowardsEvent = new UnityEvent();
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        nearTriggerQueue = new Queue<Package>();
        farTriggerQueue = new Queue<int>();

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

        aiText.text = AIQueueString();
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

    public void EvalulateCurrentGameState() {

        Debug.Log("Evaluating game state.");

        if (targetPackage == null) {
            if (nearTriggerQueue.Count > 0) {
                Debug.Log("Grabbing Target from near Queue");
                targetPackage = nearTriggerQueue.Dequeue();
                
                    
            }
            else if (priorityList.Count > 0) {
                targetPackage = priorityList[0];
                priorityList.RemoveAt(0);
            }
            else if (FindObjectOfType<PackageGenerator>().GetFastestPackagePositions().Count > 0) {
                Debug.Log("Grabbing Target from fast Queue");
                targetPackage = FindObjectOfType<PackageGenerator>().GetFastestPackage();
            }

            if (targetPackage != null) {
                targetPackage.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0);
                targetPosition = targetPackage.GetPosition();
                Debug.Log("Target Package at  Position: " + targetPackage.GetPosition());
            }
        }

        Debug.Log("Staying at the same position: " + targetPosition);

        if (targetPackage == null) {
            Debug.LogWarning("No target package was selected.");
        }

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(Move(targetPosition));
    }

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

    public Queue<Package> GetNearTriggerQueue() {
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

        sb.Append("[ ");
        foreach (Package i in nearTriggerQueue.ToArray()) {
            sb.Append("Speed: " + i.GetSpeed() + ", Position: " + i.GetPosition() + " ");
        }
        sb.Append("]");

        return sb.ToString();
    }

}

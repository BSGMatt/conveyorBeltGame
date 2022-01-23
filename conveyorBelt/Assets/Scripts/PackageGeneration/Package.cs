using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Package : MonoBehaviour
{

    private float speed;
    private float direction;
    private float deflect = 1.2f;

    private int score;
    private int position;

    private Rigidbody2D rb;
    private Collider2D coll;

    //A reference to the last slapBox the package hit. 
    private Collider2D lastSlapBox;

    //A flag checking whether this package passed the "aboutToMissTrigger"
    private bool aiAboutToMissMe = false;
    private bool isFastest;
    public bool isOnPriorityList = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        coll = gameObject.GetComponent<Collider2D>();
        FindObjectOfType<GameManager>().resetEvent.AddListener(Remove);
    }

    // Update is called once per frame
    void Update()
    {
        score = RoundToTwoSigFigs(100 * speed);
    }

    public void FixedUpdate() {
        rb.position += new Vector2(speed * direction * Time.deltaTime, 0);
    }

    public void SetSpeed(float speed) {
        this.speed = speed;
    }

    public float GetSpeed() {
        return speed;
    }

    public void SetDirection(int direction) {
        this.direction = direction;
    }

    public void SetPosition(int position) {
        this.position = position;
    }

    public void SetIsFastest(bool val) {
        isFastest = val;
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("LeftMissTrigger")){        
            FindObjectOfType<MissCounter>().leftMissEvent.Invoke();
            Remove();
        }
        else if (collision.gameObject.CompareTag("RightMissTrigger")) {
            FindObjectOfType<MissCounter>().rightMissEvent.Invoke();
            Remove();
        }
        else if (collision.gameObject.CompareTag("LeftContainer")) {
            FindObjectOfType<PackageGenerator>().DecAveragePackageSpeedAtPosition(position, speed);
            FindObjectOfType<GameManager>().IncLeftScore(score);
            Remove();
        }
        else if (collision.gameObject.CompareTag("RightContainer")) {
            FindObjectOfType<PackageGenerator>().DecAveragePackageSpeedAtPosition(position, speed);
            FindObjectOfType<GameManager>().IncRightScore(score);
            Remove();
        }
        else if (collision.gameObject.CompareTag("AI")) {
            FindObjectOfType<PackageGenerator>().DecAveragePackageSpeedAtPosition(position, speed);
            FindObjectOfType<GameManager>().IncRightScore(score);
            Remove();
        }
        else if (collision.gameObject.CompareTag("AboutToMissTrigger")) {
            PassedNearTrigger();
            //We only need to invoke this event once, so we will ingore all future triggers. 
            Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), collision);
        }
        else if (collision.gameObject.CompareTag("GettingCloserTrigger")) {
            PassedFarTrigger();
            //We only need to invoke this event once, so we will ingore all future triggers. 
            Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), collision);
        }
        else if (collision.gameObject.CompareTag("Slap")) {
            //Reverse direction. 
            direction *= -1;
            speed *= deflect;
            //Ignore collision with the new collider so this event isn't called again. 
            Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), collision);
            //Stop Ignoring the last slapBox the package hit, and set the last slapBox to this new collision. 
            if (lastSlapBox != null) Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), lastSlapBox, false);
            lastSlapBox = collision;
        }
    }

    private void PassedNearTrigger() {
        Debug.Log("PassedNearTrigger");
        Debug.LogFormat("Speed: {0}, Position: {1}", speed, position);

        //Reason why the package's package is being compared to 1/3 the target's speed is 
        //since the near trigger is 2/3's the way to the miss trigger, this package will almost always reach
        //the miss trigger first unless the target is triple the speed, since at worst, the target package
        //will have to travel 3x the distance of the this package to reach the miss trigger. 
        if (FindObjectOfType<AIContainer>().GetTargetPackage() != null && 
            speed > (FindObjectOfType<AIContainer>().GetTargetPackage().GetSpeed() * 0.333f)) {
            //Add the target to the priority list to go to it later, flag this package so we know to remove it once it's destroyed. 
            FindObjectOfType<AIContainer>().priorityList.Add(FindObjectOfType<AIContainer>().GetTargetPackage());
            isOnPriorityList = true;

            //Make the AI move to this package. 
            FindObjectOfType<AIContainer>().OverrideTargetPackage(this);
        }
        else {
            FindObjectOfType<AIContainer>().GetNearTriggerQueue().Enqueue(this);
        }

    }
    private void PassedFarTrigger() {
        Debug.Log("PassedFarTrigger");
    }

    public float GetDirection() {
        return direction;
    }

    public int GetPosition() {
        return position;
    }

    public void Remove() {
        FindObjectOfType<GameManager>().resetEvent.RemoveListener(Remove);
        FindObjectOfType<PackageGenerator>().DecPackagesAtPosition(position);
        if (FindObjectOfType<GameMode>().GetMode() == GameModeEnum.VS_AI) {
            FindObjectOfType<AIContainer>().aboutToMissEvent.RemoveListener(PassedNearTrigger);
            FindObjectOfType<AIContainer>().movingTowardsEvent.RemoveListener(PassedFarTrigger);
            if (isOnPriorityList) {
                FindObjectOfType<AIContainer>().priorityList.Remove(this);
                isOnPriorityList = false;
            }
        }
        if (isFastest) {
            PackageGenerator p =  FindObjectOfType<PackageGenerator>();
            p.GetFastestPackagePositions().Pop();
            p.SetFastestPackage(null);
        }
        Destroy(gameObject);
    }

    private int RoundToTwoSigFigs(float value) {

        int place = 1;

        while (value >= 100) {
            value /= 10;
            place *= 10;
        }

        return Mathf.RoundToInt(value) * place;
    }

}

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
    private bool isNearMiss = false;
    private bool isFastest;
    public bool isOnPriorityList = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        coll = gameObject.GetComponent<Collider2D>();
        FindObjectOfType<GameManager>().resetEvent.AddListener(Remove);
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
        else if (collision.gameObject.CompareTag("Slap")) {
            //Reverse direction. 
            direction *= -1;
            speed *= deflect;
            score = RoundToTwoSigFigs(100 * speed);
            //Ignore collision with the new collider so this event isn't called again. 
            Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), collision);
            //Stop Ignoring the last slapBox the package hit, and set the last slapBox to this new collision. 
            if (lastSlapBox != null) Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), lastSlapBox, false);
            lastSlapBox = collision;

            //Revaluate fastest packages. 
            /*if (direction > 0 && (FindObjectOfType<PackageGenerator>().GetFastestPackage() == null 
                || speed > FindObjectOfType<PackageGenerator>().GetFastestPackage().GetSpeed())) {

                FindObjectOfType<PackageGenerator>().SetFastestPackage(this);
                FindObjectOfType<PackageGenerator>().GetFastestPackagePositions().Push(position);
            }*/
        }
    }

    private void PassedNearTrigger() {
        Debug.Log("PassedNearTrigger");
        Debug.LogFormat("Speed: {0}, Position: {1}", speed, position);

        /**
         * I may change this later to add an extra condition, but that would require calculating the 
         * package's distance from the miss trigger as well as their speeds and predicting which package would hit the miss trigger first. 
         * */
        if (FindObjectOfType<AIContainer>().GetTargetPackage() != null) {

            //If this package will reach the miss trigger before the target. 
            if (WillReachMissTriggerFirst()) {

                //Add the target to the priority list to go to it later, flag this package so we know to remove it once it's destroyed. 
                FindObjectOfType<AIContainer>().priorityList.Add(FindObjectOfType<AIContainer>().GetTargetPackage());
                isOnPriorityList = true;

                //Make the AI move to this package. 
                FindObjectOfType<AIContainer>().OverrideTargetPackage(this);
            }
            else {
                //Add this package to the miss trigger list to grab it later. 
                FindObjectOfType<AIContainer>().GetNearTriggerQueue().Insert(0, this);
                isNearMiss = true;
            }

        }
        else {
            //Make the AI move to this package. 
            FindObjectOfType<AIContainer>().OverrideTargetPackage(this);
        }

    }

    /// <summary>
    /// Removes the package from play, removing it from any list if necessary. 
    /// </summary>
    public void Remove() {
        FindObjectOfType<GameManager>().resetEvent.RemoveListener(Remove);
        FindObjectOfType<PackageGenerator>().DecPackagesAtPosition(position);
        if (FindObjectOfType<GameMode>().GetMode() == GameModeEnum.VS_AI) {
            if (isOnPriorityList) {
                FindObjectOfType<AIContainer>().priorityList.Remove(this);
                isOnPriorityList = false;
            }
            if (isNearMiss) {
                FindObjectOfType<AIContainer>().GetNearTriggerQueue().Remove(this);
                isNearMiss = false;
            }
        }
        if (isFastest) {
            PackageGenerator p = FindObjectOfType<PackageGenerator>();
            p.GetFastestPackagePositions().Pop();
            p.SetFastestPackage(null);
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Checks if given package will reach the miss trigger before the target package does. 
    /// </summary>
    /// <returns></returns>
    private bool WillReachMissTriggerFirst() {
        Package target = FindObjectOfType<AIContainer>().GetTargetPackage();
        GameObject rightMTrig = GameObject.FindGameObjectWithTag("RightMissTrigger");

        //Compare the time it will take for this package to reach the miss trigger to the time
        //it will take for the target to reach the miss trigger. 
        float timeForPackage = (rightMTrig.transform.position.x - transform.position.x) / speed;
        float timeForTarget = (rightMTrig.transform.position.x - target.transform.position.x) / target.speed;

        return timeForPackage < timeForTarget;
    }

    /// <summary>
    /// Rounds the given value to 2 significant figures. 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private int RoundToTwoSigFigs(float value) {

        int place = 1;

        while (value >= 100) {
            value /= 10;
            place *= 10;
        }

        return Mathf.RoundToInt(value) * place;
    }

    /// <summary>
    /// The direction the package is moving. -1 is left, 1 is right. 
    /// </summary>
    /// <returns></returns>
    public float GetDirection() {
        return direction;
    }

    /// <summary>
    /// Where on the playfield the package is. A lower value means the package is
    /// toward the bottom, while a higher means the package is toward the top. 
    /// </summary>
    /// <returns></returns>
    public int GetPosition() {
        return position;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Package : MonoBehaviour
{

    private float speed;
    private float direction;
    private float deflect = 1.2f;

    private int score;

    private Rigidbody2D rb;
    private Collider2D coll;

    //A reference to the last slapBox the package hit. 
    private Collider2D lastSlapBox;

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

    public void SetDirection(int direction) {
        this.direction = direction;
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("LeftMissTrigger")){
            FindObjectOfType<MissCounter>().leftMissEvent.Invoke();
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("RightMissTrigger")) {
            FindObjectOfType<MissCounter>().rightMissEvent.Invoke();
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("LeftContainer")) {
            FindObjectOfType<GameManager>().IncLeftScore(score);
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("RightContainer")) {
            FindObjectOfType<GameManager>().IncRightScore(score);
            Destroy(gameObject);
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

    public void Remove() {
        FindObjectOfType<GameManager>().resetEvent.RemoveListener(Remove);
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

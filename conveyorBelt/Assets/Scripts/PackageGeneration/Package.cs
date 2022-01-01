using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Package : MonoBehaviour
{

    private float speed;
    private float direction;

    private Rigidbody2D rb;
    private Collider2D coll;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        coll = gameObject.GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        if (collision.gameObject.CompareTag("MissTrigger")){
            FindObjectOfType<MissCounter>().packageMissedEvent.Invoke();
            Destroy(gameObject);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    //The position of each row.
    [SerializeField] private Transform[] positionNodes;

    //The GameObject the container will get its slap hurt box from.
    [SerializeField] private GameObject slapBox;
    [SerializeField] private float slapDuration = 0.5f;
    private Coroutine slap = null;

    //The inputs this container needs to check for movement. 
    [SerializeField] private string myUpButton;
    [SerializeField] private string myDownButton;
    [SerializeField] private string mySlapButton;


    private int currentPosition;

    private bool controlEnabled = true;
    
    public void Start() {
        //Set the current position to be in the middle. 
        currentPosition = (positionNodes.Length - 1) / 2;

        transform.position = positionNodes[currentPosition].position;

        //Disable SlapBox
        slapBox.SetActive(false);

        FindObjectOfType<GameManager>().resetEvent.AddListener(Start);
        FindObjectOfType<GameManager>().pauseEvent.AddListener(Pause);
        FindObjectOfType<GameManager>().unPauseEvent.AddListener(UnPause);
    }

    public void Update() {

        if (controlEnabled) CheckInput();

    }

    private void Pause() {
        controlEnabled = false;
    }

    private void UnPause() {
        controlEnabled = true;
    }

    private void CheckInput() {

        if (Input.GetButtonDown(myUpButton)) {
            MoveUp();
        }
        else if (Input.GetButtonDown(myDownButton)) {
            MoveDown();
        }
        else if (Input.GetButtonDown(mySlapButton)) {
            StartSlapCoroutine();
        }
    }

    private void MoveUp() {

        currentPosition++;
        
        //Is position out of bounds?
        if (currentPosition >= positionNodes.Length) {
            currentPosition = 0;
        }


        transform.position = positionNodes[currentPosition].position;
    }

    private void MoveDown() {

        currentPosition--;

        //Is position out of bounds?
        if (currentPosition < 0) {
            currentPosition = positionNodes.Length - 1;
        }


        transform.position = positionNodes[currentPosition].position;
    }

    private void StartSlapCoroutine() {
        if (slap == null) slap = StartCoroutine(Slap());
    }

    private IEnumerator Slap() {
        slapBox.SetActive(true);
        while (slapBox.activeInHierarchy) {
            yield return new WaitForSeconds(slapDuration);

            slapBox.SetActive(false);
        }

        slap = null;
    }
}

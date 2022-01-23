using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public GameObject ai;
    public GameObject aiAboutToMissTrigger;

    //The position of each row.
    public Transform[] positionNodes;

    //The GameObject the container will get its slap hurt box from.
    public GameObject slapBox;
    [SerializeField] private float slapDuration = 0.5f;
    private Coroutine slap = null;

    //The inputs this container needs to check for movement. 
    [SerializeField] private string myUpButton;
    [SerializeField] private string myDownButton;
    [SerializeField] private string mySlapButton;


    protected int currentPosition;

    protected bool controlEnabled = true;

    //Check if the game is in "Vs. AI" mode and that this is the container on the right.
    //If so, then activate the AI Container and deactivate this one. 
    public void Awake() {
        if (FindObjectOfType<GameMode>().GetMode() == GameModeEnum.VS_AI && 
            gameObject.CompareTag("RightContainer")) {

            ai.SetActive(true);
            aiAboutToMissTrigger.SetActive(true);
            gameObject.SetActive(false);
        }
        else {
            if (ai != null) {
                ai.SetActive(false);
                aiAboutToMissTrigger.SetActive(false);
            }
        }
    }

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

    protected void Pause() {
        controlEnabled = false;
    }

    protected void UnPause() {
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

    protected void MoveUp() {

        currentPosition++;
        
        //Is position out of bounds?
        if (currentPosition >= positionNodes.Length) {
            currentPosition = 0;
        }


        transform.position = positionNodes[currentPosition].position;
    }

    protected void MoveDown() {

        currentPosition--;

        //Is position out of bounds?
        if (currentPosition < 0) {
            currentPosition = positionNodes.Length - 1;
        }


        transform.position = positionNodes[currentPosition].position;
    }

    protected void StartSlapCoroutine() {
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

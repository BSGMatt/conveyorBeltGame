using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    //The position of each row.
    [SerializeField] private Transform[] positionNodes;

    //The input this container needs to check for movement. 
    [SerializeField] private string myUpButton;
    [SerializeField] private string myDownButton;

    private int currentPosition;

    public void Start() {
        //Set the current position to be in the middle. 
        currentPosition = (positionNodes.Length - 1) / 2;

        transform.position = positionNodes[currentPosition].position;
    }

    public void Update() {

        CheckInput();

    }

    private void CheckInput() {

        if (Input.GetButtonDown(myUpButton)) {
            MoveUp();
        }
        else if (Input.GetButtonDown(myDownButton)) {
            MoveDown();
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
}

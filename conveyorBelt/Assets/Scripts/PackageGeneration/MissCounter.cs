using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MissCounter : MonoBehaviour
{

    public UnityEvent packageMissedEvent;
    private int numberOfMisses;

    [SerializeField] private Text missesDisplay;

    public void Awake() {
        packageMissedEvent = new UnityEvent();
        packageMissedEvent.AddListener(IncMissCount);
    }

    // Start is called before the first frame update
    void Start()
    {
        numberOfMisses = 0;
    }

    // Update is called once per frame
    void Update()
    {
        missesDisplay.text = "Misses: " + numberOfMisses;
    }

    private void IncMissCount() {
        numberOfMisses++;
    }
}

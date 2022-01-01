using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PackageGeneratorTest : MonoBehaviour
{

    [SerializeField] private PackageGenerator pg;
    [SerializeField] private Text displayText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        displayText.text = pg.InfoString();
    }
}

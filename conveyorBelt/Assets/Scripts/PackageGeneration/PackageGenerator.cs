using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PackageGenerator : MonoBehaviour
{

    /// <summary>
    /// The prefab to create instances of when generating packages. 
    /// </summary>
    [SerializeField] private GameObject packagePrefab;

    [SerializeField] private Transform[] rows;

    /// <summary>
    /// <para>The rate at which the packages are spawned in. The formula for 
    /// the package rate over time is initPackageRate + (kA^x)/kB, where
    /// kA, kB are constants and x is the number of packages generated. </para>
    /// </summary>
    [SerializeField] private float initPackageRate;
    [SerializeField] private float kA;
    [SerializeField] private float kB;

    /// <summary>
    /// The lowest package rate that is allowed. 
    /// </summary>
    [SerializeField] private float minPackageRate;

    private float currentPackageRate;
    

    /// <summary>
    /// How fast the package moves across the conveyor belt. 
    /// </summary>
    [SerializeField] private float minPackageSpeed;

    /// <summary>
    /// How fast the package moves across the conveyor belt. 
    /// </summary>
    [SerializeField] private float maxPackageSpeed;

    private int packagesGenerated;
    private bool pauseGenerator = false;//Controlled by a Unity Event to be added. 
    private Coroutine generatePackages;

    //For testing purposes. 
    private float mostRecentPackageSpeed;
    private int mostRecentPackageDirection;
    private int mostRecentPackagePosIndex;

    // Start is called before the first frame update
    void Start()
    {
        packagesGenerated = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!pauseGenerator && generatePackages == null) {
            generatePackages = StartCoroutine(GeneratePackages());
        }
    }

    private IEnumerator GeneratePackages() {

        while (!pauseGenerator) {
            CreatePackage();

            currentPackageRate = initPackageRate - (Mathf.Pow(kA, packagesGenerated / kB) * 0.1f);
            if (currentPackageRate <= minPackageRate) currentPackageRate = minPackageRate;
            yield return new WaitForSeconds(currentPackageRate);
        }

        generatePackages = null;

        yield return 0;
    }

    private void CreatePackage() {

        int rowIndex = (int)Random.Range(0, rows.Length);
        float packageSpeed = Random.Range(minPackageSpeed, maxPackageSpeed);
        int leftOrRight = (int)(Random.value * 2);
        int direction = -1;
        if (leftOrRight >= 1) {
            direction = 1;
        }

        mostRecentPackageSpeed = packageSpeed;
        mostRecentPackageDirection = direction;
        mostRecentPackagePosIndex = rowIndex;

        GameObject package = Instantiate<GameObject>(packagePrefab, rows[rowIndex].position, Quaternion.identity);
        package.GetComponent<Package>().SetDirection(direction);
        package.GetComponent<Package>().SetSpeed(packageSpeed);

        packagesGenerated++;
    }

    public string InfoString() {
        StringBuilder sb = new StringBuilder();
        sb.Append("Package Generation Rate: " + currentPackageRate + "\n");
        sb.Append("Minimum Package Speed: " + minPackageSpeed + "\n");
        sb.Append("Maximum Package Speed: " + maxPackageSpeed + "\n");
        sb.Append("Package Speed: " + mostRecentPackageSpeed + "\n");
        sb.Append("Package Direction: " + mostRecentPackageDirection + "\n");
        sb.Append("Package Position Index: " + mostRecentPackagePosIndex + "\n");
        sb.Append("Total Packages Generated: " + packagesGenerated + "\n");

        return sb.ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField] private float initPackageRate = 1.5f;
    [SerializeField] private float kA = 1.1f;
    [SerializeField] private float kB = 8;

    [SerializeField] private float cooldownTime;

    /// <summary>
    /// The lowest package rate that is allowed. 
    /// </summary>
    [SerializeField] private float minPackageRate = 0.67f;

    private float currentPackageRate;

    public int packagesGenerated;

    /// <summary>
    /// How fast the package moves across the conveyor belt. 
    /// </summary>
    [SerializeField] private float minPackageSpeed = 0.75f;

    /// <summary>
    /// How fast the package moves across the conveyor belt. 
    /// </summary>
    [SerializeField] private float maxPackageSpeed = 3f;

    private bool pauseGenerator = false;//Controlled by a Unity Event to be added. 
    private Coroutine generatePackages;

    //For testing purposes. 
    private float mostRecentPackageSpeed;
    private int mostRecentPackageDirection;
    private int mostRecentPackagePosIndex;

    //tracks the average speed of packages and the number packages at a position; used to help the AI determine which position to move to. 
    private int[] packagesPerPosition;
    private float[] averagePackageSpeedPerPosition;
    private Package fastestPackage;
    private Stack<int> fastPackagePos;


    // Start is called before the first frame update
    void Start()
    {
        packagesGenerated = 0;
        FindObjectOfType<GameManager>().resetEvent.AddListener(Restart);

        packagesPerPosition = new int[rows.Length];
        averagePackageSpeedPerPosition = new float[rows.Length];

        for(int i = 0; i < rows.Length; i++) {
            packagesPerPosition[i] = 0;
            averagePackageSpeedPerPosition[i] = 0;
        }

        fastestPackage = null;
        fastPackagePos = new Stack<int>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!pauseGenerator && generatePackages == null) {
            generatePackages = StartCoroutine(GeneratePackages());
        }

        for (int i = 0; i < rows.Length; i++) {
            if(packagesPerPosition[i] == 0) {
                averagePackageSpeedPerPosition[i] = 0;
            }
        }

    }

    public void Stop() {
        pauseGenerator = true;
        generatePackages = null;
    }

    public void Continue() {
        pauseGenerator = false;
    }

    public void Restart() {
        CoolDown(cooldownTime);
    }

    public void CoolDown(float seconds) {
        if (generatePackages != null) StopCoroutine(generatePackages);
        generatePackages = null;
        StartCoroutine(RunCooldown(seconds));
    }

    private IEnumerator RunCooldown(float seconds) {
        pauseGenerator = true;

        while (pauseGenerator) {        
            yield return new WaitForSeconds(seconds);

            pauseGenerator = false;
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
        package.GetComponent<Package>().SetPosition(rowIndex);

        if (direction > 0 && (fastestPackage == null || packageSpeed > fastestPackage.GetSpeed())) {
           
            fastestPackage = package.GetComponent<Package>();
            package.GetComponent<Package>().SetIsFastest(true);
            fastPackagePos.Push(rowIndex);
        }
        else {
            package.GetComponent<Package>().SetIsFastest(false);
        }
        

        packagesPerPosition[rowIndex]++;
        averagePackageSpeedPerPosition[rowIndex] = (averagePackageSpeedPerPosition[rowIndex] + packageSpeed) / packagesPerPosition[rowIndex];
        packagesGenerated++;
    }

    public void SetPackageSpeedRange(float min, float max) {
        minPackageSpeed = min;
        maxPackageSpeed = max;
    }

    public int PositionWithMostPackages() {
        int max = packagesPerPosition[0];
        int pos = 0;
        for(int i = 0; i < packagesPerPosition.Length; i++) {
            if (packagesPerPosition[i] > max) {
                max = packagesPerPosition[i];
                pos = i;
            }
        }

        Debug.Log("Position with most packages: " + pos);

        return pos;
    }

    public int PositionWithGreatestAverageSpeed() {
        float max = averagePackageSpeedPerPosition[0];
        int pos = 0;
        for (int i = 0; i < averagePackageSpeedPerPosition.Length; i++) {
            if (averagePackageSpeedPerPosition[i] > max) {
                max = averagePackageSpeedPerPosition[i];
                pos = i;
            }
            
        }

        return pos;
    }

    public Stack<int> GetFastestPackagePositions() {
        return fastPackagePos;
    }

    public void SetFastestPackage(Package pack) {
        fastestPackage = pack;
    }

    public Package GetFastestPackage() {
        return fastestPackage;
    }

    public void DecPackagesAtPosition(int rowIndex) {
        packagesPerPosition[rowIndex]--;
    }

    //Decreases the average package speed at a position. 
    public void DecAveragePackageSpeedAtPosition(int rowIndex, float speedToRemove) {
        averagePackageSpeedPerPosition[rowIndex] = 
            (averagePackageSpeedPerPosition[rowIndex] - speedToRemove) / packagesPerPosition[rowIndex];
    }

    public int[] GetPackagesPerPosition() {
        return packagesPerPosition;
    }

    public float GetMinPackageSpeed() {
        return minPackageSpeed;
    }

    public float GetMaxPackageSpeed() {
        return maxPackageSpeed;
    }

    public void SetInitPackageRate(float val) {
        initPackageRate = val;
    }

    public void SetMinPackageRate(float val) {
        minPackageRate = val;
    }

    public float GetMinPackageRate() {
        return minPackageRate;
    }

    public float GetInitPackageRate() {
        return initPackageRate;
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
        sb.Append("No. Packages Per Position: " + packagesPerPosition[0] + "," +  packagesPerPosition[1] + "," + packagesPerPosition[2] + "\n");
        sb.Append("Position with most packages: " + PositionWithMostPackages() + "\n");
        return sb.ToString();
    }
}

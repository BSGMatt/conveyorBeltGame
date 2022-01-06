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


    // Start is called before the first frame update
    void Start()
    {
        packagesGenerated = 0;
        FindObjectOfType<GameManager>().resetEvent.AddListener(Restart);
    }

    // Update is called once per frame
    void Update()
    {
        if (!pauseGenerator && generatePackages == null) {
            generatePackages = StartCoroutine(GeneratePackages());
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

        packagesGenerated++;
    }

    public void SetPackageSpeedRange(float min, float max) {
        minPackageSpeed = min;
        maxPackageSpeed = max;
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

        return sb.ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField]
    private float timeToFlyToStar;
    public float maxFuel;
    private float fuel;

    private float fuelUsage;
    [Tooltip("The time in seconds to use up one unit of fuel")]
    [SerializeField]
    private float fuelConsumption;
    private float timeTravelling; // Time the ship has spent flying

    [Tooltip("The maximum amount of stars ship can travel to in one path")]
    public int maxJumpDistance;

    private int currentPosition = 0; // Current star in the path the ship has reached
    private GameObject targetPoint; // The next star in the path the ship is trying to get to

    private List<GameObject> pathPoints;
    public bool isInMotion;

    [SerializeField]
    private ParticleSystem leftFire, rightFire;

    [SerializeField]
    private AudioSource engineSound, fireSound, takeOffSound, attackedSound;

    [SerializeField]
    private PathFinder pathFinder;
    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private LerpManager lerpLibrary;

    [SerializeField]
    private Animator pirateAttackTextAnim;

    public StarInformation currentStar;

    void Start()
    {
        fuel = maxFuel;
        pathPoints = new List<GameObject>();
    }

    void Update()
    {
        if (isInMotion) {
            timeTravelling += Time.deltaTime;
            fuelUsage += Time.deltaTime;

            // If the ship has reached it's current target destination
            if (transform.position == targetPoint.transform.position) {
                // Ensure that an end destination exists
                if (pathFinder.EndStar() != null) {
                    // If the point is the Destination star, but not the end star
                    if (targetPoint.name != pathFinder.EndStar().name) {
                        // If the player has arrived at a dangerous star, generate the chance of being attacked by pirates
                        StarInformation hitStar = targetPoint.GetComponent<StarInformation>();
                        if (hitStar.isDangerousStar && !hitStar.starColonised) {
                            PirateAttack();
                        }
                        GetNextPosition();
                    }
                }
            }
        }

        // As fuelUsage is increased by delta time, every fuelConsumption amount of seconds we remove a unit of fuel
        if(fuelUsage >= fuelConsumption) {
            fuel--;
            fuelUsage = 0;
        }

        if (fuel <= 0) {
            GameOver();
        }
    }

    public void PrepareForTakeOff() {
        // Initialise the path and make the ship look towards it's first target
        currentPosition = 0;
        pathPoints = pathFinder.GetPath();
        transform.position = pathPoints[0].transform.position;
        targetPoint = pathPoints[1];
        transform.LookAt(targetPoint.transform.position);
        // Start engine particles
        leftFire.Play();
        rightFire.Play();
        isInMotion = true;
        currentPosition++;
        PlayEngineSounds();
        takeOffSound.PlayDelayed(0.3f);
        // Start the ship lerping from the start star to the next point
        lerpLibrary.LerpBetweenVector3(this.gameObject, LerpManager.eases.easeInAndOut, LerpManager.effectTypes.position, pathPoints[0].transform.position, targetPoint.transform.position, timeToFlyToStar);
        Spin360();
    }

    // Look towards target star and move forwards in that direction at a constant speed
    // (Used as a placeholder until the lerp library was implemented)
    /*void FlyToDestination() {
        transform.Translate(Vector3.forward * timeToFlyToStar * Time.deltaTime);
        Vector3 LookDirection = (targetPoint.transform.position - transform.position);
        Quaternion targetRotation = Quaternion.LookRotation(LookDirection, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 270 * Time.deltaTime);
    }*/

    void GetNextPosition() {
        // Set the target point to the next star in the list, and make the ship look towards it
        targetPoint = pathPoints[currentPosition + 1];
        transform.LookAt(targetPoint.transform.position);
        takeOffSound.PlayDelayed(0.3f);
        // Lerp the ship between it's current star and it's target star
        lerpLibrary.LerpBetweenVector3(this.gameObject, LerpManager.eases.easeInAndOut, LerpManager.effectTypes.position, pathPoints[currentPosition].transform.position, targetPoint.transform.position, timeToFlyToStar);
        Spin360();
        // Increment the current position the ship is at in the path
        currentPosition++;
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.name == targetPoint.name) {
            // Ensure that an end destination exists
            if(pathFinder.EndStar() != null) {
                // If the point is the Destination star
                if (targetPoint.name == pathFinder.EndStar().name) {
                    // Stop the flight as we've reached the end of the route
                    isInMotion = false;
                    leftFire.Stop();
                    rightFire.Stop();
                    FinishedFlight();
                } 
            }
        }
    }

    // Make the ship roll 360 degrees
    private void Spin360() {
        Vector3 startRotation = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + 1);
        Vector3 targetRotation = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z - 359);
        lerpLibrary.LerpBetweenVector3(this.gameObject, LerpManager.eases.easeInAndOut, LerpManager.effectTypes.rotation, startRotation, targetRotation, 2f);
    }

    // When the ship has reached it's destination, set the new start to star to the current star, and clear the path ready for a new route to be calculated
    public void FinishedFlight() {
        pathFinder.UpdateStartStar(pathFinder.EndStar());
        currentStar = pathFinder.EndStar().GetComponent<StarInformation>();
        pathPoints.Clear();
        pathFinder.CancelRoute();
        StartCoroutine(StopEngineSounds(1f));
    }

    // End the game
    // Called if the ship runs out of fuel
    void GameOver() {
        isInMotion = false;
        leftFire.Stop();
        rightFire.Stop();
        pathPoints.Clear();
        pathFinder.CancelRoute();
        pathFinder.state = PathFinder.pathFindingStates.gameOver;
        StopEngineSounds(1f);
        uiManager.isGamePaused = true;
    }


    // Test that we have enough fuel for the journey
    public void TestFuelUsage(float distanceToTravel) {
        float fuelRequired = distanceToTravel * timeToFlyToStar / fuelConsumption;
        if(fuelRequired >= fuel) {
            pathFinder.state = PathFinder.pathFindingStates.notEnoughFuel;
        }
    }

    // Make sure the paths length isn't greaterthan the max jump distance
    public void TestPathLength(float amountOfStars) {
        if (amountOfStars > maxJumpDistance) {
            pathFinder.state = PathFinder.pathFindingStates.pathTooLong;
        }
    }

    // Increment the fuel by a given amount
    public void RefillFuel(float amount) {
        fuel += amount;
        if(fuel > maxFuel) {
            fuel = maxFuel;
        }
    }

    public float GetFuel() {
        return fuel;
    }

    private void PirateAttack() {
        float randomValue = Random.Range(0f, 100f);
        // 35% chance of being attacked by pirates
        if(randomValue <= 35) {
            pirateAttackTextAnim.SetTrigger("EnablePirateText");
            // Reduce fuel by 10%
            fuel -= maxFuel * 0.1f;
            attackedSound.Play();
        }
    }

    public void PlayEngineSounds() {
        engineSound.Play();
        fireSound.Play();
        engineSound.volume = 1;
        fireSound.volume = 1;
    }

    public void PauseEngineSoundsSounds() {
        engineSound.Pause();
        fireSound.Pause();
    }

    public void ResumeEngineSounds() {
        engineSound.UnPause();
        fireSound.UnPause();
    }

    public IEnumerator StopEngineSounds(float fadeOutTime) {
        while (engineSound.volume > 0) {
            yield return null;
            if (fadeOutTime > 0) {
                engineSound.volume -= Time.deltaTime / fadeOutTime;
                fireSound.volume -= Time.deltaTime / fadeOutTime;
            } else {
                engineSound.volume = fadeOutTime;
                fireSound.volume = fadeOutTime;
            }
        }
        engineSound.Stop();
        fireSound.Stop();
        yield return null;
    }
}

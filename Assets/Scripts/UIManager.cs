using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private PathFinder pathFinder;
    private SpawnIslands starSpawner;
    private ShipController ship;

    [Header("UI Screens")]
    public GameObject mainUIScreen;
    public GameObject selectDestinationUI;
    public GameObject startJourneyUI;
    public GameObject travellingRouteUI;
    public GameObject GameOverScreen;
    public GameObject PauseMenu;
    public GameObject QuitMenu;
    public GameObject ColoniseMenu;

    [Header("Individual UI Elements")]
    public TextMeshProUGUI errorText;
    private Color errorColor;
    public Slider fuelBar;

    public Toggle mapAToggle;
    public Toggle mapBToggle;
    public Toggle canReachEveryStarToggle;
    public Button coloniseStarButton;
    public TMP_InputField renameStarField;
    public GameObject starNamingErrorText;
    public GameObject coloniseFlagObject;
    public Animator fuelGainTxtAnim;
    public Animator cannotColoniseTxtAnim;

    public Image blackOverlay;

    public TextMeshProUGUI startStarText;
    public TextMeshProUGUI endStarText;
    public TextMeshProUGUI pathOfStarsText;
    public TextMeshProUGUI pathLengthText;

    [Header("Other Options")]
    public bool showStarConnections = true;
    [SerializeField]
    private AudioSource foundPathSound, errorSound, uiClick, pauseSound,
        resumeSound, uiBackSound, coloniseSound;
    [SerializeField]
    private AudioListener camAudioListener;

    private bool errorDisplayed = false;

    public bool isGamePaused;

    private void Start() {
        blackOverlay.color = Color.black;
        RetrievePreferences();
        // Initialise scripts
        pathFinder = FindObjectOfType<PathFinder>();
        starSpawner = FindObjectOfType<SpawnIslands>();
        ship = FindObjectOfType<ShipController>();
        // Initialise UI elements
        errorColor = errorText.color;
        if (mapAToggle.isOn) {
            starSpawner.makeMapB = false;
        } else {
            starSpawner.makeMapB = true;
        }
        starSpawner.allStarsConnect = canReachEveryStarToggle.isOn;
        blackOverlay.CrossFadeColor(Color.clear, 1.5f, false, true);
        fuelBar.maxValue = ship.maxFuel;
    }

    void Update()
    {
        HandleUIDisplay();
        
        if(errorText.color.a > 0) {
            errorColor.a -= (Time.deltaTime * 25);
            errorText.color = errorColor;
        }

        if(pathFinder.GetPath().Count > 0) {
            pathOfStarsText.text = "";
            foreach (GameObject star in pathFinder.GetPath()) {
                string starName = star.name;
                pathOfStarsText.text += starName + "\n";
            }
            endStarText.text = pathFinder.EndStar().name;
            pathLengthText.text = pathFinder.GetPathLength().ToString() + "Ly";
        } else {
            pathOfStarsText.text = "";
            pathLengthText.text = "";

        }

        if(pathFinder.state == PathFinder.pathFindingStates.selectDestination) {
            coloniseStarButton.interactable = true;
        } else {
            coloniseStarButton.interactable = false;
        }

        startStarText.text = pathFinder.StartStar().name;

        fuelBar.value = ship.GetFuel();
    }

    void HandleUIDisplay() {
        // Depending on what state the game is in, display the right UI
        switch (pathFinder.state) {
            case (PathFinder.pathFindingStates.selectDestination):
                DisplayUI(true, true, false, false, false);
                break;
            case (PathFinder.pathFindingStates.noRouteFound):
                // Set all UI to invisible and display an error on screen
                SetAllUIVisible(false);
                if(errorDisplayed == false) {
                    StartCoroutine(DisplayError("ERROR: COULD NOT DETECT ROUTE", 1f, PathFinder.pathFindingStates.selectDestination));
                    errorDisplayed = true;
                }
                break;
            case (PathFinder.pathFindingStates.notEnoughFuel):
                // Set all UI to invisible and display an error on screen
                SetAllUIVisible(false);
                if(errorDisplayed == false) {
                    StartCoroutine(DisplayError("WARNING: INSUFFICIENT FUEL TO REACH TARGET", 1f, PathFinder.pathFindingStates.selectDestination));
                    errorDisplayed = true;
                }
                break;
            case (PathFinder.pathFindingStates.pathTooLong):
                // Set all UI to invisible and display an error on screen
                SetAllUIVisible(false);
                if (errorDisplayed == false) {
                    StartCoroutine(DisplayError("WARNING: PATH TOO LONG", 1f, PathFinder.pathFindingStates.selectDestination));
                    errorDisplayed = true;
                }
                break;
            case (PathFinder.pathFindingStates.startJourney):
                DisplayUI(true, false, true, false, false);
                break;
            case (PathFinder.pathFindingStates.travelingPath):
                DisplayUI(true, false, false, true, false);
                break;
            case (PathFinder.pathFindingStates.gameOver):
                DisplayUI(false, false, false, false, true);
                break;
        }
    }

    void DisplayUI(bool mainUI, bool selectDestination, bool startJourney, bool travellingRoute, bool gameOver) {
        // Set the corresponding UI's according to the boolean parameters provided
        mainUIScreen.SetActive(mainUI);
        selectDestinationUI.SetActive(selectDestination);
        startJourneyUI.SetActive(startJourney);
        travellingRouteUI.SetActive(travellingRoute);
        GameOverScreen.SetActive(gameOver);
    }

    public void StartRoute() {
        foundPathSound.Play();
        pathFinder.StartRoute();
    }

    public void CancelandSelectNewRoute() {
        uiBackSound.Play();
        pathFinder.CancelRoute();
    }

    // Displays red warning text to the player when there was an error with pathinding
    IEnumerator DisplayError(string textToDisplay, float length, PathFinder.pathFindingStates stateToReturnTo) {
        errorSound.Play();
        // Set the text on screen
        errorText.text = textToDisplay;
        errorColor.a = 255;
        errorText.color = errorColor;
        yield return new WaitForSeconds(length);
        // Hide the error after waiting time
        errorColor.a = 0;
        errorText.color = errorColor;
        // Return to the specified state, cancel the route, and continue the game
        pathFinder.state = stateToReturnTo;
        pathFinder.CancelRoute();
        errorDisplayed = false;
        mainUIScreen.SetActive(true);
    }

    void SetAllUIVisible(bool visible) {
        // Set all UI visible according to the value of the boolean specified
        mainUIScreen.SetActive(visible);
        selectDestinationUI.SetActive(visible);
        startJourneyUI.SetActive(visible);
        travellingRouteUI.SetActive(visible);
        GameOverScreen.SetActive(visible);
    }

    public void ToggleStarConnections() {
        // Allows the players to toggle the visibility of the connections between all stars
        showStarConnections = !showStarConnections;
        uiClick.Play();
        foreach (GameObject star in starSpawner.stars) {
            star.GetComponent<StarInformation>().showConnections = showStarConnections;
        }
    }

    public void TogglePath() {
        // Allows the player to toggle the visibilty of the path
        uiClick.Play();
        pathFinder.showPath = !pathFinder.showPath;
    }

    public void RegenerateMap() {
        // Fade the scene out, save the players preferences, and reload the scene after a delay
        uiClick.Play();
        blackOverlay.CrossFadeColor(Color.black, 1, false, true);
        SavePreferences(mapAToggle.isOn, mapBToggle.isOn, canReachEveryStarToggle.isOn);
        StartCoroutine(ReloadScene(1.1f));
    }

    // Open a menu when the player wants to colonise a star
    public void OpenColonialismMenu() {
        // As long as the player is currently stationary at a star they haven't already colonised
        if (ship.currentStar != null && ship.currentStar.starColonised == false) {
            // If the star is a pirate star, don't let them colonise it
            if (ship.currentStar.isDangerousStar) {
                // Only play the animation clip if it isn't already playing
                // This avoids the animation loop stacking up and played multiple times
                foreach(AnimatorClipInfo animClip in cannotColoniseTxtAnim.GetCurrentAnimatorClipInfo(0)) {
                    if(animClip.clip.name != "FadePirateAttackText") {
                        cannotColoniseTxtAnim.SetTrigger("EnablePirateText");
                    }
                }
                // Play a sound and exit the function
                errorSound.Play();
                return;
            }
            uiClick.Play();
            // Bring up the menu where a player can rename the star
            ColoniseMenu.SetActive(true);
            starNamingErrorText.SetActive(false);
            // Reset the text from previous inputs and pause the game
            renameStarField.text = "";
            Time.timeScale = 0;
        }
    }

    public void ColoniseStar() {
        // Check that the length of the name is within bounds
        if (renameStarField.text.Length < 3 || renameStarField.text.Length > 16) {
            starNamingErrorText.SetActive(true);
            return;
        }
        // Resume the game, and set the stars name to the one chosen by the player
        Time.timeScale = 1;
        ship.currentStar.name = renameStarField.text;
        ship.currentStar.starName = renameStarField.text;
        ship.currentStar.starColonised = true;
        ColoniseMenu.SetActive(false);
        coloniseSound.Play();

        // Spawn a flag on top of the star
        Vector3 flagSpawnPosition = new Vector3(ship.currentStar.transform.position.x, ship.currentStar.transform.position.y + ship.currentStar.transform.localScale.y / 2, ship.currentStar.transform.position.z);
        GameObject settlingFlag = Instantiate(coloniseFlagObject, flagSpawnPosition, Quaternion.identity);

        int chanceOfGainingFuel = Random.Range(0, 100);
        // 50% chance of gaining fuel from colonising a star
        if(chanceOfGainingFuel >= 50) {
            // Refill fuel by 5%, and show a message to the player
            ship.RefillFuel(ship.maxFuel * 0.05f);
            fuelGainTxtAnim.SetTrigger("EnableFuelText");
        }
    }

    // Resume the game and close the menu
    public void CancelColonialism() {
        Time.timeScale = 1;
        uiBackSound.Play();
        ColoniseMenu.SetActive(false);
    }

    // Pause the game and set the pause UI to visible
    public void PauseGame() {
        pauseSound.Play();
        PauseMenu.SetActive(true);
        isGamePaused = true;
        ship.PauseEngineSoundsSounds();
        Time.timeScale = 0;
    }

    // Continue the game and hide the pause UI
    public void ResumeGame() {
        Time.timeScale = 1;
        resumeSound.Play();
        PauseMenu.SetActive(false);
        isGamePaused = false;
        ship.ResumeEngineSounds();
    }

    // Bring up a menu ensuring the player wants to quit the game
    public void DoubleCheckQuit() {
        Time.timeScale = 1;
        uiClick.Play();
        QuitMenu.SetActive(true);
        Time.timeScale = 0;
    }

    // Close the quit menu, and return back to the pause menu
    public void CancelQuit() {
        Time.timeScale = 1;
        uiBackSound.Play();
        QuitMenu.SetActive(false);
        Time.timeScale = 0;
    }

    public void QuitGame() {
        Application.Quit();
    }

    // Reloads this scene after a specified delay
    private IEnumerator ReloadScene(float delay) {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(0);
    }

    // Save the preferences saved by the player on the UI 
    private void SavePreferences(bool mapA, bool mapB, bool connectAllStars) {
        if (mapA) {
            PlayerPrefs.SetFloat("MapA", 1);
            PlayerPrefs.SetFloat("MapB", 0);
        } 
        if(mapB){
            PlayerPrefs.SetFloat("MapA", 0);
            PlayerPrefs.SetFloat("MapB", 1);
        }
        if (connectAllStars) {
            PlayerPrefs.SetFloat("ConnectAllStars", 1);
        } else {
            PlayerPrefs.SetFloat("ConnectAllStars", 0);
        }
    }

    // Retrieve the preferences set by the player from the previous simulation
    private void RetrievePreferences() {
        if(PlayerPrefs.GetFloat("MapA") == 1) {
            mapAToggle.isOn = true;
            mapBToggle.isOn = false;
        } 
        if(PlayerPrefs.GetFloat("MapB") == 1) {
            mapAToggle.isOn = false;
            mapBToggle.isOn = true;
        }
        if(PlayerPrefs.GetFloat("ConnectAllStars") == 1) {
            canReachEveryStarToggle.isOn = true;
        } else {
            canReachEveryStarToggle.isOn = false;
        }
    }
}

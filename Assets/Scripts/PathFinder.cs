using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public enum pathFindingStates {
        selectDestination,
        calculatingRoute,
        noRouteFound,
        notEnoughFuel,
        pathTooLong,
        startJourney,
        travelingPath,
        gameOver,
    }

    public pathFindingStates state;

    [SerializeField]
    private SpawnIslands starGenerator;

    public LayerMask whatIsAStar;

    private GameObject startStar;
    private GameObject endStar;
    [SerializeField]
    private GameObject startStarPulse; // Creates an effect to signify which star is the start of the path

    [Header("Star Materials")]
    [SerializeField]
    private Color startStarColor; // The current starting star material

    public GameObject selectedStar;
    private MeshRenderer startStarRenderer;
    private MeshRenderer selectedStarRenderer;

    // Lists to hold the stars during pathfinding
    List<GameObject> openList = new List<GameObject>();
    List<GameObject> closedList = new List<GameObject>();
    List<GameObject> path = new List<GameObject>();

    private LineRenderer pathLine;
    private float pathLength;
    public bool showPath = true;

    [SerializeField]
    private AudioSource selectSound;

    [SerializeField]
    private ShipController ship;

    void Start()
    {
        state = pathFindingStates.selectDestination;
        pathLine = GetComponent<LineRenderer>();
    }

    public void InitialisePathfindingInfo() {
        startStar = GetStarWithMostConnections();
        startStarRenderer = startStar.GetComponent<MeshRenderer>();
        startStarRenderer.material.color = startStarColor;
        startStarPulse.transform.position = startStar.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) {
            SelectDestination();
        }

        if(startStarRenderer != null) {
            startStarRenderer.material.color = startStarColor;
        } else {
            if(startStar != null) {
                startStarRenderer = startStar.GetComponent<MeshRenderer>();
            }
        }

        if (selectedStar != null) {
            selectedStarRenderer.material.color = Color.green;
        }

        if (Input.GetKeyDown(KeyCode.G)) {
            CancelRoute();
        }

        // Enable or disable the line renderer according to the value of the 'showPath' bool
        pathLine.enabled = showPath;
    }

    // Returns the star which has the most connections (this is where the player will start)
    GameObject GetStarWithMostConnections() {
        int numberOfConnections = 0;
        GameObject starWithMostConnections = null;
        // Look through all the stars in the map
        for(int i = 0; i < starGenerator.numberOfStars; i++){
            StarInformation starInfo = starGenerator.stars[i].GetComponent<StarInformation>();
            // If the number of connected stars is greater than our current number of connections
            // Then set this star to be the one with the most connections
            if(starInfo.connectedStarsList.Count > numberOfConnections) {
                numberOfConnections = starInfo.connectedStarsList.Count;
                starWithMostConnections = starGenerator.stars[i].gameObject;
            }
        }
        // After completing loop, return the star with the most connections that was found
        return starWithMostConnections;
    }

    public void SelectDestination() {
        // Do a check to see if the game is paused, and return if so
        if (Time.timeScale == 0) {
            return;
        }
        // If the game is in the correct state
        if (state == pathFindingStates.selectDestination) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // When a star is clicked on, set that game object to the selected star, update it's material
            // and start calculating the route between the start and end star
            if (Physics.Raycast(ray, out hit, 20000, whatIsAStar)) {
                selectedStar = hit.transform.gameObject;
                if(selectedStar.name == startStar.name) {
                    selectedStar = null;
                    return;
                }
                selectedStarRenderer = selectedStar.GetComponent<MeshRenderer>();
                state = pathFindingStates.calculatingRoute;
                endStar = selectedStar;
                selectSound.Play();
                GeneratePath();
            }
        }
    }

    void GeneratePath() {
        // Initialise a game object to hold the current star
        GameObject currentNode;
        currentNode = startStar;

        // Reset the costs for all stars in the map, so they can be recalculated with the new route
        foreach(GameObject star in starGenerator.stars) {
            StarInformation starInfo = star.GetComponent<StarInformation>();
            starInfo.ResetFGH();
        }

        // Set the g cost of the current star to 0, as this will be the starting star
        StarInformation currentStarInfo = currentNode.GetComponent<StarInformation>();
        currentStarInfo.g = 0;

        // Clear the lists ready for the next calculation
        openList.Clear();
        closedList.Clear();
        path.Clear();

        openList.Add(startStar);

        while (openList.Count > 0) {

            float currentF = float.MaxValue;

            // Look for the star with the lowest F cost in the open list
            foreach (GameObject potentialStar in openList) {
                StarInformation potentialStarInfo = potentialStar.GetComponent<StarInformation>();
                if(potentialStarInfo.f < currentF) {
                    currentNode = potentialStar;
                    currentF = potentialStarInfo.f;
                }
            }
            
            // If the star we've reached is the destination, calculate the path to get there
            if(currentNode.name == endStar.name) {
                CalculatePath();
                return;
            }

            currentStarInfo = currentNode.GetComponent<StarInformation>();

            foreach (GameObject childStar in currentStarInfo.connectedStarsList) {
                // First check if this star is in the closed list, if so ignore it
                foreach(GameObject closedStar in closedList) {
                    if(childStar.name == closedStar.name) {
                        // Jump to the end of the loop to start the next iteration with the next star
                        goto startNextIteration;
                    }
                }

                StarInformation childStarInfo = childStar.GetComponent<StarInformation>();
                childStarInfo.h = Vector3.Distance(childStar.transform.position, endStar.transform.position);

                // Check if the child star is in the open list
                bool isInOpenList = false;
                foreach (GameObject openStar in openList) {
                    if (childStar.name == openStar.name) {
                        isInOpenList = true;
                    }
                }
                // If this star isn't on the open list, add it to the open list and set it's
                // parent to the current star
                if (!isInOpenList) {
                    openList.Add(childStar);
                    childStarInfo.parentStar = currentNode;
                    childStarInfo.g = currentStarInfo.g + Vector3.Distance(childStar.transform.position, currentNode.transform.position);
                    childStarInfo.f = childStarInfo.g + childStarInfo.h;
                }
                // If it is on the open list, check if the G score is better than its current G score
                // when going through the current star to get there
                else {
                    float newGScore = currentStarInfo.g + Vector3.Distance(childStar.transform.position, currentNode.transform.position);
                    if (newGScore < childStarInfo.g) {
                        childStarInfo.parentStar = currentNode;
                        childStarInfo.g = newGScore;
                            childStarInfo.f = childStarInfo.g + childStarInfo.h;                       
                    }
                }

                // If the star isn't a dangerous one, give it calculate its g cost normally, other wise multiply
                // it to increase the cost to get through
                if (!childStarInfo.isDangerousStar) {
                    childStarInfo.g = currentStarInfo.g + Vector3.Distance(currentNode.transform.position, childStar.transform.position);
                } else {
                    childStarInfo.g = currentStarInfo.g + Vector3.Distance(currentNode.transform.position, childStar.transform.position) * childStarInfo.dangerousPathMultiplyer;
                }

                startNextIteration:
                continue;
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);
        }

        // If all stars wre checked and no path was found, send an error message to the player
        NoRouteExists();
    }

    // Reverse back through the found stars parent stars to generate the list of points that make up the path
    void CalculatePath() {
        StarInformation currentStar = endStar.GetComponent<StarInformation>();
        int iterations = 0;
        while(currentStar.name != startStar.name) {
            iterations++;
            path.Add(currentStar.gameObject);
            if (currentStar)
                if (currentStar.parentStar != null) {
                    currentStar = currentStar.parentStar.GetComponent<StarInformation>();
                }
            // If there are too many iterations, cancel the loop
            if (iterations > 150) { // 150 is a number greater than the amount of stars in the level
                NoRouteExists();
                return;
            }
        }
        // Add the final star (starting star) as loop exits before it is added
        path.Add(currentStar.gameObject);
        path.Reverse(); // Reverse the path to get the right order from start to Finish
        state = pathFindingStates.startJourney;
        pathLength = GetPathLength();
        ship.TestFuelUsage(path.Count - 1);
        ship.TestPathLength(path.Count);
        DrawPath();
    }

    // Returns the exact length between all stars in the path
    public float GetPathLength() {
        float currentLength = 0;
        for(int i = 0; i < path.Count; i++) {
            if(i != path.Count - 1) {
                 currentLength += Vector3.Distance(path[i].transform.position, path[i + 1].transform.position);
            }
        }
        return currentLength;
    }

    public GameObject StartStar() {
        return startStar;
    }

    public GameObject EndStar() {
        return endStar;
    }

    // Draw a line between each star in the path, showing the route
    void DrawPath() {
        pathLine.positionCount = path.Count;
        pathLine.startWidth = 6;
        pathLine.endWidth = 6;
        for (int i = 0; i < path.Count; i++) {
            pathLine.SetPosition(i, path[i].transform.position);
        }
    }

    // Empty the path and remove the line showing the route
    public void ResetPath() {
        path.Clear();
        pathLine.positionCount = 0;
        pathLine.startWidth = 0;
        pathLine.endWidth = 0;
    }

    public void NoRouteExists() {
        CancelRoute();
        state = pathFindingStates.noRouteFound;
    }

    // Start the ship on it's journey
    public void StartRoute() {
        startStarPulse.SetActive(false);
        state = pathFindingStates.travelingPath;
        ship.PrepareForTakeOff();
    }

    // Clear path and reset the end star that was selected
    public void CancelRoute() {
        state = pathFindingStates.selectDestination;
        ResetPath();
        if (selectedStar != null) {
            selectedStar.GetComponent<StarInformation>().ResetStarVisual();
        }
        selectedStar = null;
        selectedStarRenderer = null;
        endStar = null;
    }

    public List<GameObject> GetPath() {
        return path;
    }

    public void UpdateStartStar(GameObject newStar) {
        startStar = newStar;
    }
}

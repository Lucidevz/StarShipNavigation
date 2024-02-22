using System.Collections.Generic;
using UnityEngine;

public class SpawnIslands : MonoBehaviour {
    [Header("General Variables")]
    public GameObject galaxyObject;
    public int numberOfStars; // The desired number of stars to spawn
    [Tooltip("The minimum and maximum size a star can be")]
    public Vector2 starSizes;
    public GameObject[] islandsToSpawn;
    public GameObject starObject; // Star object to be instantiated
    public Vector3 mapSize;
    [SerializeField]
    private GetStarNames starNames;

    public LayerMask islandsLayer; 
    // List of all stars in the map
    public List<GameObject> stars;

    [Tooltip("Enable if you want all stars to be able to be reached")]
    public bool allStarsConnect;

    [Header("Star Connection Variables")]
    [Tooltip("The minimum and maximum distance a star needs to be in order to make a connection")]
    public Vector2 connectionRange;

    private List<GameObject> starsInPath;
    [SerializeField]
    private GameObject closestStar = null;

    private GameObject[,] starPairs; // To contain a list of stars and a the best connection to them

    // The scripts of the islands we're checking for connection (island moving from, and an island moving to)
    private StarInformation starCheckingForConnectionFrom = null;
    private StarInformation closestStarInfo = null;

    [Header("Extra Options")]
    public bool makeMapB; // Different shaped map
    public Vector2 connectionRangeMapB;

    public GameObject currentStar;

    public Vector2 distanceRange;

    private int starNumbers;

    private PathFinder pathFinder;

    [SerializeField]
    private GameObject pirateSpaceVolume;

    void Start() {
        starsInPath = new List<GameObject>();
        starPairs = new GameObject[numberOfStars + 1, 2];
        pathFinder = GameObject.FindGameObjectWithTag("Path Finder").GetComponent<PathFinder>();

        pirateSpaceVolume.transform.position = Random.insideUnitSphere * 500;

        GenerateMap();

    }

    void CreateStars() {
        for (int i = 0; i < numberOfStars; i++) {
            // Initialise a new star and add it to the stars list
            GameObject newStar = Instantiate(starObject, Vector3.zero, Quaternion.identity);
            stars.Add(newStar);
            float starRadius = Random.Range(starSizes.x, starSizes.y);
            Vector3 starSize = new Vector3(starRadius, starRadius, starRadius);
            newStar.transform.localScale = starSize;

            Vector3 spawnPosition = Vector3.zero;
            bool isLocationFree = false;

            // Keep checking for a free spawn position in the map
            do {
                spawnPosition = GetNewSpawnCoords(mapSize.x, mapSize.y, mapSize.z);
                isLocationFree = IsSpawnLocationAvailable(spawnPosition, starRadius * 3);
                // If the position isn't available, run the loop again until one is found
            } while (isLocationFree == false);

            // Once the loop has returned true, set the star to the position that was found
            newStar.transform.position = spawnPosition;

            AssignStarRandomColor(newStar);
            newStar.transform.SetParent(galaxyObject.transform);
            newStar.SetActive(true);

            // Add the very first star created to the path so the connection making has somewhere to start 
            if (starNumbers == 0) {
                starsInPath.Add(newStar);
            }

            // Give the stars a temporary name while they are initialised
            newStar.name = ("Star " + starNumbers);
            starNumbers++;
        }
    }

    Vector3 GetNewSpawnCoords(float xBounds, float yBounds, float zBounds) {
        // Return a random position within the bounds of the map size
        return new Vector3(Random.Range(-xBounds, xBounds), Random.Range(-yBounds, yBounds), Random.Range(-zBounds, zBounds));
    }

    bool IsSpawnLocationAvailable(Vector3 currentSpawnPosition, float radiusDetection) {
        // If there is a star close to the current one being spawned, choose another spawn location
        if (Physics.CheckSphere(currentSpawnPosition, radiusDetection, islandsLayer)) {
            return false;
        } else {
            // If the space is free, use this location
            return true;
        }
    }

    void GiveStarNames() {
        // Give each star a name from the star names text file in the project
        for (int i = 0; i < stars.Count; i++) {
            StarInformation currentIslandInfo = stars[i].GetComponent<StarInformation>();
            currentIslandInfo.starName = starNames.GetStarName(i);
            stars[i].name = currentIslandInfo.starName;
        }
    }

    void GeneratePathToStars(Vector2 rangeToConnect) {
        List<GameObject> starsInRange = new List<GameObject>(); // Holds all stars that can be connected to
        // For each star in the map
        for (int i = 0; i < stars.Count; i++) {
            starsInRange.Clear(); // Clear the list ready for the next star
            // Get all the stars within the max range from the current stars position
            Collider[] discoveredStars = Physics.OverlapSphere(stars[i].transform.position, rangeToConnect.y, islandsLayer);
            // Add all of the found stars to the stars in range list
            foreach(Collider c in discoveredStars) {
                starsInRange.Add(c.gameObject);
            }

            // Get all stars within the min range from the current stars position
            Collider[] starsTooClose = Physics.OverlapSphere(stars[i].transform.position, rangeToConnect.x, islandsLayer);

             //Loop through the stars too close and remove them from the stars in Range list
            foreach(Collider c in starsTooClose) {
                for (int j = 0; j < starsInRange.Count; j++) {
                    if (c.gameObject.name == starsInRange[j].gameObject.name) {
                        starsInRange.RemoveAt(j);
                    }
                    if (j > starsInRange.Count) {
                        break;
                    }
                }
            }
            // 'StarInformation' script holds all the connections to each star
            StarInformation starInfo = stars[i].GetComponent<StarInformation>();
            if(starInfo != null) {
                // Add each star within range and their distances to the current stars information script
                if (starsInRange.Count > 0) {
                    bool alreadyInList = false;
                    // Check that the star isn't already connecting to this one
                    foreach (GameObject starObj in starsInRange) {
                        alreadyInList = false;
                        for (int x = 0; x < starInfo.connectedStarsList.Count; x++) {
                            if(starObj.name == starInfo.connectedStarsList[x].name) {
                                alreadyInList = true;
                                break;
                            }
                        }
                        if (!alreadyInList) {
                            starInfo.connectedStarsList.Add(starObj);
                        }
                    }
                    // If there was no stars within range, connect to the nearest one
                } else {
                        GetNearestStar(stars[i].transform);
                }
            }
        }
    }

    // Get the closest star object to any given star in the map and make a connection between them
    void GetNearestStar(Transform star) {
        GameObject closestStar = null; // Stores the current closest star
        float nearestStarDistance = mapSize.x * mapSize.y; 
        Collider[] nearbyStars = Physics.OverlapSphere(star.position, nearestStarDistance, islandsLayer);

        StarInformation starInfo = star.GetComponent<StarInformation>();
        // Only check to connect to this star if we aren't already connected to it
        foreach(GameObject currentConnections in starInfo.connectedStarsList) {
            foreach (Collider c in nearbyStars) {
                if (c.gameObject.name != currentConnections.name) {
                    // Get the distance between the current island and the target island to connecting to
                    float distance = Vector3.Distance(star.transform.position, c.transform.position);
                    // Check if this distance is less than the current lowest distance, 
                    if (distance < nearestStarDistance && c.gameObject.name != star.name) {
                        closestStar = c.gameObject;
                        nearestStarDistance = distance;
                    }
                }
            }
        }
        if(closestStar != null) {
            // Make connections betwen the 2 found stars
            StarInformation connectingStarInfo = closestStar.GetComponent<StarInformation>();
            starInfo.connectedStarsList.Add(closestStar);
            connectingStarInfo.connectedStarsList.Add(star.gameObject);
        }
    }

    
    // Adds a pair of stars to the star pairs array. The first element is a star we're moving from, and the
    // second element is the closest star to it that we can connect to. (The array is evaluated to find the star
    // pair with the best / shortest connection)
    void CreateConnectionPairs(Transform star) {
        GameObject closestStar = null; // Stores the current closest star
        float nearestStarDistance = mapSize.x * mapSize.y;
        Collider[] nearbyStars = Physics.OverlapSphere(star.position, nearestStarDistance, islandsLayer);

        foreach (Collider c in nearbyStars) {
            StarInformation connectingStarInfo = c.GetComponent<StarInformation>();
            // Get the distance between the current star and the target star to connecting to
            float distance = Vector3.Distance(star.transform.position, c.transform.position);
            // Check if this distance is less than the current lowest distance, 
            if (distance < nearestStarDistance && c.gameObject.name != star.name) {
                if (!connectingStarInfo.hasConnection) {
                    // If this star isn't already part of the path, then set this star to be the closest island, and the nearest distance to the lengthbetween them
                    closestStar = c.gameObject;
                    nearestStarDistance = distance;
                
                }
            }
        }

        // Add the stars to the star pairs array
        for (int i = 0; i < starPairs.GetLength(0); i++) {
            // Check for the next available element in the star pairs array
            if (starPairs[i, 0] == null) {
                // The first element will be the star we are make a connection from
                starPairs[i, 0] = star.gameObject;
                // The second element will be the star we are making a connection to
                starPairs[i, 1] = closestStar.gameObject;
                return;
            }
        }
    }
    
    // Recursive function to generate connections between all islands (uses prims algorithm)
    public void MakePathBetweenAllStars() {
        // Reset island connetion script variables
        starCheckingForConnectionFrom = null; 
        closestStarInfo = null; 

        // Loop through all currently visited islands to find a new island to connect to
        foreach (GameObject i in starsInPath) {
            GameObject currentStar = i;
            currentStar.GetComponent<StarInformation>().hasConnection = true;

            // Get the closest Island to the one we're looking from
            CreateConnectionPairs(currentStar.transform);            
        }

        // Will loop through all possible connections found from the visisted stars list, and find the shortest one available
        CheckStarsForClosest();

        // Add the connection found from function above to the list of stars in the path
        starCheckingForConnectionFrom.connectedStarsList.Add(closestStar);
        closestStarInfo.connectedStarsList.Add(starCheckingForConnectionFrom.gameObject);
        closestStarInfo.hasConnection = true;
        starsInPath.Add(closestStar);

        // Clear the array of connection pairs ready for the next iteration
        ClearStarPairs();

        // Loop through all islands, if there is one that still doesn't have a connection, run the process again
        foreach (GameObject starObj in stars) {
            if (starObj.GetComponent<StarInformation>().hasConnection == false) {
                MakePathBetweenAllStars();
            }    
        }
    }

    void CheckStarsForClosest() {
        // Variables to check which island has the closest connection
        float distanceTest = mapSize.x * mapSize.y; // large enough number to cover size of whole map
        float distBetweenStars = mapSize.x * mapSize.y;
        GameObject starMovingFrom = null;
        // Loop through all elements in the star pairs array
        for (int i = 0; i < starPairs.GetLength(0); i++) {
            if (starPairs[i, 0] != null && starPairs[i, 1] != null) {
                // Calculate the distance between each island and their corresponding connection
                distBetweenStars = Vector3.Distance(starPairs[i, 0].transform.position,
                starPairs[i, 1].transform.position);
                // Check if the distance between is less than our current least distance
                if (distBetweenStars < distanceTest) {
                    // This is the closest known island connection pair, so assign it to the corresponding variables
                    distanceTest = distBetweenStars;
                    starMovingFrom = starPairs[i, 0];
                    closestStar = starPairs[i, 1];

                }
            }
        }
        starCheckingForConnectionFrom = starMovingFrom.GetComponent<StarInformation>();
        closestStarInfo = closestStar.GetComponent<StarInformation>();
    }

    void ClearStarPairs() {
        // Set all elements in the array to null
        for (int i = 0; i < starPairs.GetLength(0); i++) {
            starPairs[i, 0] = null;
            starPairs[i, 1] = null;
        }
    }

    void AssignStarRandomColor(GameObject star) {
        // Give the star a random color
        Color starColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        star.GetComponent<MeshRenderer>().material.color = starColor;
    }

    void GenerateMapB() {
        for (int i = 0; i < numberOfStars / 2; i++) {
            SpawnStarInMapB(currentStar.transform.forward + currentStar.transform.up, i);
            SpawnStarInMapB(-currentStar.transform.forward + -currentStar.transform.up, i);
        }
    }

    // Create a differently shaped map of stars
    void SpawnStarInMapB(Vector3 direction, int index) {
        // Spawn a star in a specified direction from the current star
        float randomDistance = Random.Range(distanceRange.x, distanceRange.y);
        Vector3 randomRotation = new Vector3(0, Random.Range(0, 359), 0);
        GameObject nextStar = Instantiate(starObject, direction * randomDistance, Quaternion.Euler(randomRotation));
        currentStar = nextStar;
        // Gradually increase the range of the distance which the stars can spawn at
        distanceRange.y += 0.05f;
        distanceRange.x += 0.01f;

        // Initialise regular star information
        AssignStarRandomColor(currentStar);

        float starRadius = Random.Range(starSizes.x, starSizes.y);
        Vector3 starSize = new Vector3(starRadius, starRadius, starRadius);
        currentStar.transform.localScale = starSize;

        AssignStarRandomColor(currentStar);
        currentStar.transform.SetParent(galaxyObject.transform);
        currentStar.SetActive(true);

        // Add the very first star created to the path so the connection making has somewhere to start 
        if (starNumbers == 0) {
            starsInPath.Add(currentStar);
        }

        // Temporary name before real names are added
        currentStar.name = ("Star " + starNumbers);
        starNumbers++;

        stars.Add(currentStar);
        currentStar.transform.SetParent(galaxyObject.transform);
    }

    public void GenerateMap() {
        // Generate the correct map
        if (!makeMapB) {
            CreateStars();
        } else {
            GenerateMapB();
        }

        if (allStarsConnect) {
            // Create a minimum spanning tree first before making extra connections
            MakePathBetweenAllStars();
        }

        // Call the correct path generator depending on the type of map
        if (!makeMapB) {
            GeneratePathToStars(connectionRange);
        } else {
            GeneratePathToStars(connectionRangeMapB);
        }

        // Assign stars proper names from a text file
        GiveStarNames();

        // Initalise the pathfinder once all stars have been created
        pathFinder.InitialisePathfindingInfo();
    }
}

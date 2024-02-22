using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarInformation : MonoBehaviour
{
    public string starName;
    public float starSize;

    // F, G and H costs that will be updated when a new route is calculated
    public float f = float.MaxValue;
    public float g = float.MaxValue;
    public float h = 0;

    // The star that connects to this star when a path is found
    public GameObject parentStar;

    // All stars this star is connected to
    public List<GameObject> connectedStarsList = new List<GameObject>();

    private SpawnIslands starSpanwer;

    public bool hasConnection; // Is the star part of a connection to another star

    private LineRenderer lineRenderer;
    public bool showConnections = true;
    public float connectionPathWidth;

    private MeshRenderer mRenderer;
    private Color originalColor;

    private PathFinder pathFinder;

    // Determines if this is a dangerous star to travel through
    public bool isDangerousStar;
    public float dangerousPathMultiplyer;
    public LayerMask pirateSpace;

    public bool starColonised;

    private void Start() {
        starSpanwer = FindObjectOfType<SpawnIslands>();
        // As a star is a sphere, it's scale on all axis will be the same, so use one to find it's size
        starSize = transform.localScale.x;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = connectionPathWidth;
        lineRenderer.endWidth = connectionPathWidth;

        mRenderer = GetComponent<MeshRenderer>();
        originalColor = mRenderer.material.color;

        pathFinder = FindObjectOfType<PathFinder>();

        starColonised = false;

        // If this star is overlapping pirate space, set this star to be a dangerous one and generate a
        // multiplyer that will be used to increase it's cost during pathfinding
        if (Physics.CheckSphere(transform.position, transform.localScale.x / 4, pirateSpace)) {
            isDangerousStar = true;
            dangerousPathMultiplyer = Random.Range(25, 50);
            mRenderer.material.color = Color.black;
            originalColor = mRenderer.material.color;
        }
    }

    private void Update() {
        if (connectedStarsList.Count > 0) {
            StartCoroutine(CreateLineConnections());
        }

        // Enable or disable the line renderer according to the 'showConnections' bool value
        lineRenderer.enabled = showConnections;
    }

    private void OnDrawGizmosSelected() {
        // Selecting a star will show its connection boundaries
        if(starSpanwer != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, starSpanwer.connectionRange.x);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, starSpanwer.connectionRange.y);
        }
    }

    private IEnumerator CreateLineConnections() {
        // Wait a small delay before creating connections
        yield return new WaitForSeconds(0.15f);

        if (connectedStarsList.Count != 0) {
            int starCount = 0;
            lineRenderer.positionCount = connectedStarsList.Count * 2;
            for (int i = 0; i < lineRenderer.positionCount; i++) {
                // Create a line renderer between the stars position, then to one of it's connected stars position, then back to the stars position again, and repeat through all connected stars
                if (i % 2 == 1) {
                    lineRenderer.SetPosition(i, transform.position);
                    starCount++;
                } else {
                    lineRenderer.SetPosition(i, connectedStarsList[starCount].transform.position);           
                }
            }
        }
    }

    // Mouse Enter and Exit functions allow players to see which star is under the mouse so they can select it
    void OnMouseEnter() {
        // As this function isn't affected by time scale, do a check to return if the game is paused
        if (Time.timeScale == 0) {
            return;
        }
        // Visually show this star is under the cursor by changing it's color and scale
        if(this.gameObject != pathFinder.StartStar() && this.gameObject != pathFinder.EndStar()) {
            SelectStar(Color.red, starSize * 1.3f);
        }
    }

    private void OnMouseExit() {
        // As this function isn't affected by time scale, do a check to return if the game is paused
        if (Time.timeScale == 0) {
            return;
        }
        // Reset the star if it no longer has the mouse hovering over it
        ResetStarVisual();
    }

    void SelectStar(Color targetColor, float newSize) {
        // Change a stars material and size when the mouse hovers or un-hovers over it
        if (pathFinder != null) {
            if (pathFinder.state == PathFinder.pathFindingStates.selectDestination) {
                mRenderer.material.color = targetColor;
                transform.localScale = new Vector3(newSize, newSize, newSize);
            }
        }
    }

    public void ResetStarVisual() {
        // Change a stars material and size back to it's original version
        SelectStar(originalColor, starSize);
    }

    public void ResetFGH() {
        f = 0;
        g = float.MaxValue;
        h = 0;
    }
}

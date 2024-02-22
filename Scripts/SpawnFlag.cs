using UnityEngine;

public class SpawnFlag : MonoBehaviour
{
    [SerializeField]
    private float flagScale; // Scale of the flag on all axis
    [SerializeField]
    private float scaleUpSpeed; // How long it takes for it to scale up when it spawns in seconds
    private LerpManager lerpLibrary;

    void Start()
    {
        lerpLibrary = GameObject.FindGameObjectWithTag("LerpLibrary").GetComponent<LerpManager>();

        // Get the flag to look towards the camera on the Y axis
        transform.LookAt(Camera.main.transform.position);
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        // Create a lerping motion where it's scale is bounced into space
        lerpLibrary.LerpBetweenVector3(this.gameObject, LerpManager.eases.EaseOutBounce, LerpManager.effectTypes.scale, Vector3.zero, Vector3.one * flagScale, scaleUpSpeed);
    }
}

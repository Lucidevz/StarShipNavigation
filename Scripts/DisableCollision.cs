using System.Collections;
using UnityEngine;

public class DisableCollision : MonoBehaviour
{
    public float waitTime;
    public SphereCollider collisionToDisable;

    void Start()
    {
        StartCoroutine(DisableObjectCollision(waitTime));
    }

    // Disables the collision after a short delay
    private IEnumerator DisableObjectCollision(float delay) {
        yield return new WaitForSeconds(delay);
        collisionToDisable.enabled = false;
    }
}

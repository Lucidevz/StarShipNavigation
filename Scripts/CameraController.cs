using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public PlayerInput camInput;
    [SerializeField]
    private float rotateSpeed;

    [Tooltip("The minimum and maximum angle the camera can be on the vertical axis")]
    public Vector2 verticalAxisClamp;

    private float horizontalMove;
    private float verticalMove;

    private float verticalAngle;

    // When a horizontal button is pressed, retrieve a positive or negative value to rotate the camera left and right
    public void MoveHorizontal(InputAction.CallbackContext context) {
        if (context.performed) {
            horizontalMove = context.ReadValue<float>();
        }
        if (context.canceled) {
            horizontalMove = 0;
        }
    }

    // When a vertical button is pressed, retrieve a positive or negative value to rotate the camera up and down
    public void MoveVertical(InputAction.CallbackContext context) {
        if (context.performed) {
            verticalMove = context.ReadValue<float>();
        }
        if (context.canceled) {
            verticalMove = 0;
        }
    }

    void Update()
    {
        transform.Rotate(0, horizontalMove * rotateSpeed * Time.deltaTime, 0, Space.World);
        // Convert the angle of the camera on the vertical axis to a number in degrees
        verticalAngle = ((transform.rotation.eulerAngles.x + 540) % 360) - 180;
        // Only rotate down on the vertical axis if the angle is more than the minimum bound
        if (verticalAngle >= verticalAxisClamp.x && verticalMove < 0) {
            transform.Rotate(verticalMove * rotateSpeed * Time.deltaTime, 0, 0, Space.Self);
        }
        // Only rotate up on the vertical axis if the angle is less than the maximum bound
        if (verticalAngle <= verticalAxisClamp.y && verticalMove > 0) {
            transform.Rotate(verticalMove * rotateSpeed * Time.deltaTime, 0, 0, Space.Self);

        }
    }
}

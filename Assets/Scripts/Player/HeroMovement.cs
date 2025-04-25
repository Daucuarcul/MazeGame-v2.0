using UnityEngine;

public class HeroMovement : MonoBehaviour
{
    public float speed = 3.0f;
    [HideInInspector] public FixedJoystick joystick;

    private void FixedUpdate()
    {
        // Luăm direcția din joystick
        Vector3 direction = new Vector3(joystick.Horizontal, 0, joystick.Vertical).normalized;

        // Dacă joystickul are input, rotim eroul în acea direcție
        if (direction.magnitude >= 0.1f)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10 * Time.fixedDeltaTime);
        }

        // Mergem constant înainte
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }
}

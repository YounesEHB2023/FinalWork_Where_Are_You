using UnityEngine;

public class PickUpMovement : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        transform.Translate(x * speed * Time.deltaTime, 0, z * speed * Time.deltaTime);
    }
}
using UnityEngine;

public class ItemRespawn : MonoBehaviour
{
    public float minY = -10f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (transform.position.y < minY)
            Respawn();
    }

    public void Respawn()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.SetParent(null);
        gameObject.SetActive(true);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }
}
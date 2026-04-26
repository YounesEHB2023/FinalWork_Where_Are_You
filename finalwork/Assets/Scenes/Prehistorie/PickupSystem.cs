using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    public Transform holdPoint;
    public float pickupRange = 3f;

    private GameObject heldObject;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
            {
                TryPickup();
            }
            else
            {
                DropObject();
            }
        }
    }

    void TryPickup()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, pickupRange))
        {
            if (hit.collider.CompareTag("Pickup"))
            {
                heldObject = hit.collider.gameObject;

                heldObject.transform.SetParent(holdPoint);
                heldObject.transform.localPosition = Vector3.zero;

                Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;
            }
        }
    }

    void DropObject()
    {
        heldObject.transform.SetParent(null);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;

        heldObject = null;
    }
}
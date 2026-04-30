using UnityEngine;

public class OutlineProximity : MonoBehaviour
{
    public Transform player;
    public float activationDistance = 3f;

    private Outline outline;

    void Start()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        outline.enabled = distance <= activationDistance;
    }
}
using System.Collections;
using UnityEngine;

public class MedievalChestOpen : InteractableObject
{
    [Header("Chest")]
    public Transform chestTop;
    public Vector3 closedRotation = new Vector3(45f, 110.31f, 0f);
    public Vector3 openRotation = new Vector3(0f, 110.31f, 0f);

    [Header("Pest Particle")]
    public GameObject pestParticle;

    [Header("Settings")]
    public float openDuration = 0.5f;

    private bool isOpen = false;
    private bool isAnimating = false;

    void Start()
    {
        if (chestTop != null)
            chestTop.localRotation = Quaternion.Euler(closedRotation);

        if (pestParticle != null)
            pestParticle.SetActive(false);
    }

    public override void Interact(PickupSystem player)
    {
        if (isOpen || isAnimating) return;

        StartCoroutine(OpenChest());
    }

    IEnumerator OpenChest()
    {
        isAnimating = true;

        Quaternion startRot = chestTop.localRotation;
        Quaternion endRot = Quaternion.Euler(openRotation);

        float t = 0f;

        while (t < openDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / openDuration);

            chestTop.localRotation = Quaternion.Lerp(startRot, endRot, p);

            yield return null;
        }

        chestTop.localRotation = endRot;

        if (pestParticle != null)
            pestParticle.SetActive(true);

        isOpen = true;
        isAnimating = false;
    }
}
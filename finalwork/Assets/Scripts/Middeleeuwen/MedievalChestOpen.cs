using System.Collections;
using TMPro;
using UnityEngine;

public class MedievalChestOpen : InteractableObject
{
    [Header("Chest")]
    public Transform chestTop;
    public Vector3 closedRotation = new Vector3(45f, 110.31f, 0f);
    public Vector3 openRotation = new Vector3(0f, 110.31f, 0f);

    [Header("Pest Particle")]
    public GameObject pestParticle;

    [Header("UI")]
public GameObject pressXUI;

    [Header("Settings")]
    public float openDuration = 0.5f;

    private bool isOpen = false;
    private bool isAnimating = false;
    private bool playerInside = false;

    void Start()
    {
        if (chestTop != null)
            chestTop.localRotation = Quaternion.Euler(closedRotation);

        if (pestParticle != null)
            pestParticle.SetActive(false);

        HidePrompt();
    }

    void Update()
    {
        if (playerInside && !isOpen && !isAnimating)
            ShowPrompt();
        else
            HidePrompt();
    }

    public override void Interact(PickupSystem player)
    {
        if (isOpen || isAnimating) return;
        if (player.playerIndex != ownerPlayerIndex) return;

        StartCoroutine(OpenChest());
    }

    IEnumerator OpenChest()
    {
        isAnimating = true;
        HidePrompt();

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

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;
        if (pickup.playerIndex != ownerPlayerIndex) return;

        playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;
        if (pickup.playerIndex != ownerPlayerIndex) return;

        playerInside = false;
        HidePrompt();
    }

    void ShowPrompt()
{
    if (pressXUI != null)
        pressXUI.SetActive(true);
}

    void HidePrompt()
    {
        if (pressXUI != null)
            pressXUI.SetActive(false);
    }
}
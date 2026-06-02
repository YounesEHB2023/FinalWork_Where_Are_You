using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MedievalLeverDoor : InteractableObject
{
    [Header("Lever")]
    public Transform leverStick;
    public Vector3 leverOpenRotation;
    public Vector3 leverClosedRotation = new Vector3(60f, 0f, 0f);

    [Header("Door")]
    public Transform door;
    public Vector3 doorOpenRotation;
    public Vector3 doorClosedRotation;

    [Header("UI")]
    public GameObject pressXUI;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip leverSound;

    [Header("Settings")]
    public bool startsOpen = true;
    public float animationDuration = 0.4f;

    private bool isOpen;
    private bool isAnimating;
    private bool locked = false;
    private bool playerInside = false;

    void Start()
    {
        isOpen = startsOpen;
        ApplyInstant();
        HidePrompt();
    }

    void Update()
    {
        if (locked || isAnimating || !playerInside)
        {
            HidePrompt();
            return;
        }

        ShowPrompt();

        Gamepad pad = GetGamepad();

        if (pad != null && pad.buttonSouth.wasPressedThisFrame)
            ToggleLever();
    }

    public override void Interact(PickupSystem player)
    {
        // Lever uses only Box Collider Trigger + Update input.
    }

    void ToggleLever()
    {
        if (audioSource != null && leverSound != null)
            audioSource.PlayOneShot(leverSound);

        isOpen = !isOpen;
        StartCoroutine(AnimateLeverAndDoor());
    }

    IEnumerator AnimateLeverAndDoor()
    {
        isAnimating = true;
        HidePrompt();

        Quaternion leverStart = leverStick.localRotation;
        Quaternion leverEnd = Quaternion.Euler(isOpen ? leverOpenRotation : leverClosedRotation);

        Quaternion doorStart = door.localRotation;
        Quaternion doorEnd = Quaternion.Euler(isOpen ? doorOpenRotation : doorClosedRotation);

        float t = 0f;

        while (t < animationDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / animationDuration);

            if (leverStick != null)
                leverStick.localRotation = Quaternion.Lerp(leverStart, leverEnd, p);

            if (door != null)
                door.localRotation = Quaternion.Lerp(doorStart, doorEnd, p);

            yield return null;
        }

        if (leverStick != null)
            leverStick.localRotation = leverEnd;

        if (door != null)
            door.localRotation = doorEnd;

        isAnimating = false;
    }

    void ApplyInstant()
    {
        if (leverStick != null)
            leverStick.localRotation = Quaternion.Euler(isOpen ? leverOpenRotation : leverClosedRotation);

        if (door != null)
            door.localRotation = Quaternion.Euler(isOpen ? doorOpenRotation : doorClosedRotation);
    }

    Gamepad GetGamepad()
    {
        if (Gamepad.all.Count <= ownerPlayerIndex)
            return null;

        return Gamepad.all[ownerPlayerIndex];
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

    public bool IsClosed()
    {
        return !isOpen;
    }

    public void LockLever()
    {
        locked = true;
        HidePrompt();
    }
}
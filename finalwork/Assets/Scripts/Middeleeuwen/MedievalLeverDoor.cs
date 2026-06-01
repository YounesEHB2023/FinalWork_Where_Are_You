using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MedievalLeverDoor : MonoBehaviour
{
    [Header("Owner")]
    public int ownerPlayerIndex = 1;

    [Header("Lever")]
    public Transform leverStick;
    public Vector3 leverOpenRotation;
    public Vector3 leverClosedRotation = new Vector3(60f, 0f, 0f);

    [Header("Door")]
    public Transform door;
    public Vector3 doorOpenRotation;
    public Vector3 doorClosedRotation;

    [Header("Settings")]
    public bool startsOpen = true;
    public float animationDuration = 0.4f;

    private bool isOpen;
    private bool playerInside;
    private bool isAnimating;
    private bool locked = false;

public void LockLever()
{
    locked = true;
}

    void Start()
    {
        isOpen = startsOpen;
        ApplyInstant();
    }

    void Update()
    {
if (locked || !playerInside || isAnimating) return;

        Gamepad pad = GetGamepad();

        bool pressed =
            pad != null &&
            pad.buttonSouth.wasPressedThisFrame;

        if (pressed)
            ToggleLever();
    }

    void ToggleLever()
    {
        isOpen = !isOpen;
        StartCoroutine(AnimateLeverAndDoor());
    }

    IEnumerator AnimateLeverAndDoor()
    {
        isAnimating = true;

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
    }

    public bool IsClosed()
{
    return !isOpen;
}
}
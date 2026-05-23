using System.Collections;
using UnityEngine;

public class OudheidDoorOpener : MonoBehaviour
{
    [Header("Movement")]
    public float openHeight = 4f;
    public float openSpeed = 1.5f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip doorSound;

    private bool isOpening = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * openHeight;
    }

    public void OpenDoor()
    {
        if (isOpening) return;

        isOpening = true;

        if (audioSource != null && doorSound != null)
            audioSource.PlayOneShot(doorSound);

        StartCoroutine(OpenDoorRoutine());
    }

    IEnumerator OpenDoorRoutine()
    {
        while (Vector3.Distance(transform.position, openPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                openPosition,
                openSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = openPosition;
    }
}
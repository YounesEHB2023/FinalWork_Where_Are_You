using UnityEngine;
using System.Collections;

public class Barricade : MonoBehaviour
{
    [Header("Barricade Settings")]
    public int hitsToBreak = 3;

    [Header("Player")]
    public Transform holdPoint;

    [Header("Weapon Name")]
    public string requiredWeaponName = "PrehistoricAxe";

    [Header("Effects")]
    public AudioSource hitSound;

    [Header("Camera Shake")]
    public Transform playerCamera;
    public float shakeDuration = 0.1f;
    public float shakeAmount = 0.08f;

    [Header("Interaction")]
    public float hitDistance = 3f;

    private int currentHits = 0;
    private bool canHit = true;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canHit)
        {
            TryHitBarricade();
        }
    }

    void TryHitBarricade()
    {
        // Check of speler axe vasthoudt
        bool hasAxe = false;

        foreach (Transform child in holdPoint)
        {
            if (child.name.Contains(requiredWeaponName))
            {
                hasAxe = true;
                break;
            }
        }

        if (!hasAxe)
        {
            Debug.Log("Je moet de axe vasthouden!");
            return;
        }

        // Raycast
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, hitDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                StartCoroutine(HitBarricade());
            }
        }
    }

    IEnumerator HitBarricade()
    {
        canHit = false;

        currentHits++;

        Debug.Log("Barricade geraakt: " + currentHits);

        // Sound
        if (hitSound != null)
        {
            hitSound.Play();
        }

        // Kleine shake
        if (playerCamera != null)
        {
            yield return StartCoroutine(CameraShake());
        }

        // Destroy barricade
        if (currentHits >= hitsToBreak)
        {
            Destroy(gameObject);
        }

        yield return new WaitForSeconds(0.4f);

        canHit = true;
    }

    IEnumerator CameraShake()
    {
        Vector3 originalPos = playerCamera.localPosition;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;

            playerCamera.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        playerCamera.localPosition = originalPos;
    }
}
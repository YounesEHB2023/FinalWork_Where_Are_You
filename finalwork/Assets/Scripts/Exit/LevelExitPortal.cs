using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExitPortal : NetworkBehaviour
{
    [Header("Scene")]
    public string nextSceneName = "PrehistoricPuzzle2";

    [Header("Portal")]
    public Transform portalCenter;
    public int requiredPlayers = 1;

    [Header("Camera Animation")]
    public float animationDuration = 2f;
    public float moveStrength = 0.65f;
    public float spinSpeed = 220f;
    public float maxFov = 120f;

    [Header("Fade")]
    public CanvasGroup blackFadeCanvasGroup;

    private HashSet<ulong> playersInside = new HashSet<ulong>();
    private bool transitionStarted = false;

    void Start()
    {
        if (blackFadeCanvasGroup != null)
            blackFadeCanvasGroup.alpha = 0f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!IsServer) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj == null) return;

        playersInside.Add(playerNetObj.OwnerClientId);

        if (!transitionStarted && playersInside.Count >= requiredPlayers)
        {
            transitionStarted = true;

            StartTransitionClientRpc();

            StartCoroutine(LoadNextSceneAfterDelay());
        }
    }

    [ClientRpc]
    void StartTransitionClientRpc()
    {
        StartCoroutine(TransitionAnimation());
    }

    IEnumerator TransitionAnimation()
{
    Camera cam = FindPlayerCamera();

    if (cam == null)
        yield break;

    Transform camTransform = cam.transform;

    Quaternion startRot = camTransform.localRotation;
    float startFov = cam.fieldOfView;

    float t = 0f;

    while (t < animationDuration)
    {
        t += Time.deltaTime;
        float progress = Mathf.SmoothStep(0f, 1f, t / animationDuration);

        cam.fieldOfView = Mathf.Lerp(startFov, maxFov, progress);

        float spin = Mathf.Sin(progress * Mathf.PI) * 12f;
        camTransform.localRotation = startRot * Quaternion.Euler(0f, 0f, spin);

        if (blackFadeCanvasGroup != null)
        {
            float fadeProgress = Mathf.InverseLerp(0.55f, 1f, progress);
            blackFadeCanvasGroup.alpha = fadeProgress;
        }

        yield return null;
    }

    if (blackFadeCanvasGroup != null)
        blackFadeCanvasGroup.alpha = 1f;

    cam.fieldOfView = startFov;
    camTransform.localRotation = startRot;
}

    Camera FindPlayerCamera()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach (Camera cam in cameras)
        {
            if (cam.enabled && cam.name.Contains("PlayerCamera"))
                return cam;
        }

        foreach (Camera cam in cameras)
        {
            if (cam.enabled)
                return cam;
        }

        return null;
    }

    IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(animationDuration + 0.2f);

        NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}
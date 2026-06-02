using System.Collections;
using UnityEngine;

public class LevelExitPortal : MonoBehaviour
{
    [Header("Scene")]
    public string nextSceneName = "MultiplayerOudheidPuzzle1";

    [Header("Owner")]
    public int triggerPlayerIndex = 0;

    [Header("Camera Animation")]
    public float animationDuration = 2f;
    public float maxFov = 115f;

    [Header("Fade")]
    public CanvasGroup blackFadePlayer1;
    public CanvasGroup blackFadePlayer2;

    [Header("Complete Popup")]
    public LevelCompletePopupManager completePopupManager;

    private bool transitionStarted = false;

    void Start()
    {
        ResetFade(blackFadePlayer1);
        ResetFade(blackFadePlayer2);
    }

    void OnTriggerEnter(Collider other)
    {
        if (transitionStarted) return;
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;

        if (pickup.playerIndex != triggerPlayerIndex) return;

        transitionStarted = true;
        StartCoroutine(TransitionAndShowPopup());
    }

    IEnumerator TransitionAndShowPopup()
    {
        PrepareFade(blackFadePlayer1);
        PrepareFade(blackFadePlayer2);

        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        float[] startFovs = new float[cameras.Length];
        Quaternion[] startRots = new Quaternion[cameras.Length];

        for (int i = 0; i < cameras.Length; i++)
        {
            startFovs[i] = cameras[i].fieldOfView;
            startRots[i] = cameras[i].transform.localRotation;
        }

        float t = 0f;

        while (t < animationDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / animationDuration);

            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].fieldOfView = Mathf.Lerp(startFovs[i], maxFov, progress);

                float spin = Mathf.Sin(progress * Mathf.PI) * 12f;
                cameras[i].transform.localRotation =
                    startRots[i] * Quaternion.Euler(0f, 0f, spin);
            }

            float fadeProgress = Mathf.InverseLerp(0.55f, 1f, progress);

            SetFade(blackFadePlayer1, fadeProgress);
            SetFade(blackFadePlayer2, fadeProgress);

            yield return null;
        }

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].fieldOfView = startFovs[i];
            cameras[i].transform.localRotation = startRots[i];
        }

        SetFade(blackFadePlayer1, 1f);
        SetFade(blackFadePlayer2, 1f);

        if (completePopupManager != null)
        {
            completePopupManager.nextSceneName = nextSceneName;
            completePopupManager.ShowPopup();
        }
        else
        {
            Debug.LogWarning("Complete Popup Manager is missing.");
        }
    }

    void PrepareFade(CanvasGroup fade)
    {
        if (fade == null) return;

        fade.gameObject.SetActive(true);
        fade.transform.SetAsLastSibling();
        fade.blocksRaycasts = true;
        fade.alpha = 0f;
    }

    void SetFade(CanvasGroup fade, float alpha)
    {
        if (fade == null) return;
        fade.alpha = alpha;
    }

    void ResetFade(CanvasGroup fade)
    {
        if (fade == null) return;

        fade.alpha = 0f;
        fade.blocksRaycasts = false;
        fade.gameObject.SetActive(false);
    }
}
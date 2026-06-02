using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class FinalCreditsSequence : MonoBehaviour
{
    [Header("Player Count")]
    public TextMeshProUGUI playerCountTextPlayer1;
    public TextMeshProUGUI playerCountTextPlayer2;

    [Header("Fade")]
    public CanvasGroup blackFadePlayer1;
    public CanvasGroup blackFadePlayer2;

    [Header("Popup")]
    public CanvasGroup popupPlayer1;
    public CanvasGroup popupPlayer2;
    public GameObject waitingTextPlayer1;
    public GameObject waitingTextPlayer2;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public GameObject videoImagePlayer1;
    public GameObject videoImagePlayer2;

    [Header("Skip")]
    public CanvasGroup skipTextPlayer1;
    public CanvasGroup skipTextPlayer2;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public float waitBeforeFade = 2f;
    public float fadeDuration = 1f;
    public float popupAnimDuration = 0.35f;

    private bool player1Inside;
    private bool player2Inside;
    private bool sequenceStarted;

    private bool player1PopupReady;
    private bool player2PopupReady;
    private bool videoStarted;
    private bool skipVisible;

    void Start()
    {
        HideAll();
        UpdateCountText();
    }

    void Update()
    {
        if (!sequenceStarted) return;

        if (!videoStarted)
        {
            HandlePopupInput();
            return;
        }

        HandleVideoSkip();
    }

    void OnTriggerEnter(Collider other)
    {
        if (sequenceStarted) return;
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;

        if (pickup.playerIndex == 0) player1Inside = true;
        if (pickup.playerIndex == 1) player2Inside = true;

        UpdateCountText();

        if (player1Inside && player2Inside)
            StartCoroutine(StartFinalSequence());
    }

    void OnTriggerExit(Collider other)
    {
        if (sequenceStarted) return;
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;

        if (pickup.playerIndex == 0) player1Inside = false;
        if (pickup.playerIndex == 1) player2Inside = false;

        UpdateCountText();
    }

    void UpdateCountText()
    {
        int count = 0;
        if (player1Inside) count++;
        if (player2Inside) count++;

        bool show = count > 0;
        string text = count + " / 2 players";

        if (playerCountTextPlayer1 != null)
        {
            playerCountTextPlayer1.gameObject.SetActive(show);
            playerCountTextPlayer1.text = text;
        }

        if (playerCountTextPlayer2 != null)
        {
            playerCountTextPlayer2.gameObject.SetActive(show);
            playerCountTextPlayer2.text = text;
        }
    }

    IEnumerator StartFinalSequence()
    {
        sequenceStarted = true;

        yield return new WaitForSeconds(waitBeforeFade);

        yield return FadeIn(blackFadePlayer1);
        yield return FadeIn(blackFadePlayer2);

        StartCoroutine(OpenPopup(popupPlayer1));
        StartCoroutine(OpenPopup(popupPlayer2));
    }

    void HandlePopupInput()
    {
        Gamepad pad1 = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;
        Gamepad pad2 = Gamepad.all.Count > 1 ? Gamepad.all[1] : null;

        if (!player1PopupReady && pad1 != null && pad1.buttonSouth.wasPressedThisFrame)
        {
            player1PopupReady = true;
            StartCoroutine(ConfirmPopup(popupPlayer1, waitingTextPlayer1));
        }

        if (!player2PopupReady && pad2 != null && pad2.buttonSouth.wasPressedThisFrame)
        {
            player2PopupReady = true;
            StartCoroutine(ConfirmPopup(popupPlayer2, waitingTextPlayer2));
        }

        if (player1PopupReady && player2PopupReady)
            StartCoroutine(StartVideo());
    }

    IEnumerator StartVideo()
{
    videoStarted = true;

    if (waitingTextPlayer1 != null) waitingTextPlayer1.SetActive(false);
    if (waitingTextPlayer2 != null) waitingTextPlayer2.SetActive(false);

    if (videoImagePlayer1 != null) videoImagePlayer1.SetActive(true);
    if (videoImagePlayer2 != null) videoImagePlayer2.SetActive(true);

    if (videoPlayer != null)
    {
        videoPlayer.Stop();
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();

        yield return new WaitForSeconds(0.2f);

        while (videoPlayer.isPlaying)
            yield return null;
    }

    SceneManager.LoadScene(mainMenuSceneName);
}

    void HandleVideoSkip()
    {
        Gamepad pad1 = Gamepad.all.Count > 0 ? Gamepad.all[0] : null;
        Gamepad pad2 = Gamepad.all.Count > 1 ? Gamepad.all[1] : null;

        bool anyButton =
            (pad1 != null && AnyButtonPressed(pad1)) ||
            (pad2 != null && AnyButtonPressed(pad2));

        if (anyButton && !skipVisible)
        {
            skipVisible = true;
            StartCoroutine(FadeSkipText());
        }

        bool skipPressed =
            (pad1 != null && pad1.buttonEast.wasPressedThisFrame) ||
            (pad2 != null && pad2.buttonEast.wasPressedThisFrame);

        if (skipPressed)
            SceneManager.LoadScene(mainMenuSceneName);
    }

    bool AnyButtonPressed(Gamepad pad)
    {
        return pad.buttonSouth.wasPressedThisFrame ||
               pad.buttonNorth.wasPressedThisFrame ||
               pad.buttonEast.wasPressedThisFrame ||
               pad.buttonWest.wasPressedThisFrame ||
               pad.leftShoulder.wasPressedThisFrame ||
               pad.rightShoulder.wasPressedThisFrame ||
               pad.startButton.wasPressedThisFrame ||
               pad.selectButton.wasPressedThisFrame;
    }

    IEnumerator OpenPopup(CanvasGroup popup)
    {
        if (popup == null) yield break;

        popup.gameObject.SetActive(true);
        popup.alpha = 0f;
        popup.transform.localScale = Vector3.one * 0.85f;

        float t = 0f;

        while (t < popupAnimDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / popupAnimDuration);

            popup.alpha = p;
            popup.transform.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, p);

            yield return null;
        }

        popup.alpha = 1f;
        popup.transform.localScale = Vector3.one;
    }

    IEnumerator ConfirmPopup(CanvasGroup popup, GameObject waitingText)
    {
        if (popup != null)
        {
            float t = 0f;

            while (t < popupAnimDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / popupAnimDuration);

                popup.alpha = Mathf.Lerp(1f, 0f, p);
                popup.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.85f, p);

                yield return null;
            }

            popup.alpha = 0f;
            popup.gameObject.SetActive(false);
        }

        if (waitingText != null)
            waitingText.SetActive(true);
    }

    IEnumerator FadeIn(CanvasGroup fade)
    {
        if (fade == null) yield break;

        fade.gameObject.SetActive(true);
        fade.alpha = 0f;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fade.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        fade.alpha = 1f;
    }

    IEnumerator FadeSkipText()
    {
        if (skipTextPlayer1 != null) skipTextPlayer1.gameObject.SetActive(true);
        if (skipTextPlayer2 != null) skipTextPlayer2.gameObject.SetActive(true);

        float t = 0f;

        while (t < 0.4f)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / 0.4f);

            if (skipTextPlayer1 != null) skipTextPlayer1.alpha = a;
            if (skipTextPlayer2 != null) skipTextPlayer2.alpha = a;

            yield return null;
        }
    }

    void HideAll()
    {
        if (playerCountTextPlayer1 != null) playerCountTextPlayer1.gameObject.SetActive(false);
        if (playerCountTextPlayer2 != null) playerCountTextPlayer2.gameObject.SetActive(false);

        if (blackFadePlayer1 != null) blackFadePlayer1.gameObject.SetActive(false);
        if (blackFadePlayer2 != null) blackFadePlayer2.gameObject.SetActive(false);

        if (popupPlayer1 != null) popupPlayer1.gameObject.SetActive(false);
        if (popupPlayer2 != null) popupPlayer2.gameObject.SetActive(false);

        if (waitingTextPlayer1 != null) waitingTextPlayer1.SetActive(false);
        if (waitingTextPlayer2 != null) waitingTextPlayer2.SetActive(false);

        if (videoImagePlayer1 != null) videoImagePlayer1.SetActive(false);
        if (videoImagePlayer2 != null) videoImagePlayer2.SetActive(false);

        if (skipTextPlayer1 != null)
        {
            skipTextPlayer1.alpha = 0f;
            skipTextPlayer1.gameObject.SetActive(false);
        }

        if (skipTextPlayer2 != null)
        {
            skipTextPlayer2.alpha = 0f;
            skipTextPlayer2.gameObject.SetActive(false);
        }
    }
}
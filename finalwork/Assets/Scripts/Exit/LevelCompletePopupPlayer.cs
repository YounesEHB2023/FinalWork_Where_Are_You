using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelCompletePopupPlayer : MonoBehaviour
{
    public int playerIndex = 0;

    [Header("Popup")]
    public CanvasGroup popupGroup;
    public RectTransform popupPanel;

    [Header("UI")]
    public GameObject buttonVerder;
    public GameObject waitingText;

    [Header("Animation")]
    public float animationDuration = 0.35f;

    public bool IsReady { get; private set; }

    private bool isOpen = false;
    private bool isAnimating = false;

    void Start()
    {
        HideAll();
    }

    void Update()
    {
        if (!isOpen) return;
        if (IsReady) return;
        if (isAnimating) return;

        Gamepad pad = GetGamepad();

        if (pad != null && pad.buttonSouth.wasPressedThisFrame)
            StartCoroutine(ConfirmRoutine());
    }

    public void Open()
    {
        IsReady = false;
        isOpen = true;

        if (waitingText != null)
            waitingText.SetActive(false);

        if (buttonVerder != null)
            buttonVerder.SetActive(true);

        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        isAnimating = true;

        if (popupGroup != null)
        {
            popupGroup.gameObject.SetActive(true);
            popupGroup.alpha = 0f;
            popupGroup.blocksRaycasts = true;
        }

        if (popupPanel != null)
            popupPanel.localScale = Vector3.one * 0.85f;

        float t = 0f;

        while (t < animationDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / animationDuration);

            if (popupGroup != null)
                popupGroup.alpha = p;

            if (popupPanel != null)
                popupPanel.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, p);

            yield return null;
        }

        if (popupGroup != null)
        {
            popupGroup.alpha = 1f;
            popupGroup.blocksRaycasts = true;
        }

        if (popupPanel != null)
            popupPanel.localScale = Vector3.one;

        isAnimating = false;
    }

    IEnumerator ConfirmRoutine()
    {
        isAnimating = true;
        IsReady = true;

        if (buttonVerder != null)
            buttonVerder.SetActive(false);

        float t = 0f;

        while (t < animationDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / animationDuration);

            if (popupGroup != null)
                popupGroup.alpha = Mathf.Lerp(1f, 0f, p);

            if (popupPanel != null)
                popupPanel.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.85f, p);

            yield return null;
        }

        if (popupGroup != null)
        {
            popupGroup.alpha = 0f;
            popupGroup.blocksRaycasts = false;
        }

        if (waitingText != null)
            waitingText.SetActive(true);

        isAnimating = false;
    }

    Gamepad GetGamepad()
    {
        if (Gamepad.all.Count <= playerIndex)
            return null;

        return Gamepad.all[playerIndex];
    }

    void HideAll()
    {
        if (popupGroup != null)
        {
            popupGroup.gameObject.SetActive(true);
            popupGroup.alpha = 0f;
            popupGroup.blocksRaycasts = false;
        }

        if (buttonVerder != null)
            buttonVerder.SetActive(false);

        if (waitingText != null)
            waitingText.SetActive(false);
    }
}
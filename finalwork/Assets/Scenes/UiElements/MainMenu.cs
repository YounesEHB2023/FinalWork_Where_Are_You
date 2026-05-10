using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    public RectTransform startButton;
    public RectTransform exitButton;

    [Header("Selection Pattern")]
    public RectTransform selectorPattern;
    public CanvasGroup selectorGroup;

    [Header("Fade")]
    public CanvasGroup fadePanel;

    [Header("Settings")]
    public float startFadeDuration = 1.5f;
    public float selectFadeDuration = 0.15f;
    public float sceneFadeDuration = 0.8f;

    [Header("Click Feedback")]
    public float clickPulseDuration = 0.2f;
    public float clickPulseScale = 1.12f;

    private int selectedIndex = 0;
    private bool isBusy = false;
    private bool joystickReady = true;

    void Start()
    {
        SelectOption(0, true);
        StartCoroutine(IntroFade());
    }

    void Update()
    {
        if (isBusy) return;

        HandleKeyboardInput();
        HandleControllerInput();
    }

    void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
            SelectOption(0, false);

        if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
            SelectOption(1, false);

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            ConfirmSelection();
    }

    void HandleControllerInput()
    {
        if (Gamepad.current == null) return;

        Vector2 stick = Gamepad.current.leftStick.ReadValue();
        Vector2 dpad = Gamepad.current.dpad.ReadValue();

        if (joystickReady)
        {
            if (stick.y > 0.5f || dpad.y > 0.5f)
            {
                SelectOption(0, false);
                joystickReady = false;
            }

            if (stick.y < -0.5f || dpad.y < -0.5f)
            {
                SelectOption(1, false);
                joystickReady = false;
            }
        }

        if (Mathf.Abs(stick.y) < 0.2f && Mathf.Abs(dpad.y) < 0.2f)
            joystickReady = true;

        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            ConfirmSelection();
    }

    public void SelectStart()
    {
        SelectOption(0, false);
    }

    public void SelectExit()
    {
        SelectOption(1, false);
    }

    public void ClickStart()
    {
        SelectOption(0, true);
        ConfirmSelection();
    }

    public void ClickExit()
    {
        SelectOption(1, true);
        ConfirmSelection();
    }

    void SelectOption(int index, bool instant)
    {
        if (selectedIndex == index && !instant) return;

        selectedIndex = index;

        RectTransform target = selectedIndex == 0 ? startButton : exitButton;

        if (instant)
        {
            if (selectorPattern != null)
                selectorPattern.position = target.position;

            if (selectorGroup != null)
                selectorGroup.alpha = 1f;
        }
        else
        {
            StopCoroutine(nameof(AnimateSelector));
            StartCoroutine(AnimateSelector(target));
        }
    }

    IEnumerator AnimateSelector(RectTransform target)
    {
        if (selectorGroup != null)
        {
            float t = 0f;

            while (t < selectFadeDuration)
            {
                t += Time.deltaTime;
                selectorGroup.alpha = Mathf.Lerp(1f, 0f, t / selectFadeDuration);
                yield return null;
            }

            selectorGroup.alpha = 0f;
        }

        if (selectorPattern != null)
            selectorPattern.position = target.position;

        if (selectorGroup != null)
        {
            float t = 0f;

            while (t < selectFadeDuration)
            {
                t += Time.deltaTime;
                selectorGroup.alpha = Mathf.Lerp(0f, 1f, t / selectFadeDuration);
                yield return null;
            }

            selectorGroup.alpha = 1f;
        }
    }

    void ConfirmSelection()
    {
        if (isBusy) return;

        if (selectedIndex == 0)
            StartCoroutine(ClickFeedbackThenPlay());
        else
            StartCoroutine(ClickFeedbackThenQuit());
    }

    IEnumerator ClickFeedbackThenPlay()
    {
        isBusy = true;

        RectTransform target = selectedIndex == 0 ? startButton : exitButton;
        yield return StartCoroutine(ClickPulse(target));

        yield return StartCoroutine(FadeToBlack());

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    IEnumerator ClickFeedbackThenQuit()
    {
        isBusy = true;

        RectTransform target = selectedIndex == 0 ? startButton : exitButton;
        yield return StartCoroutine(ClickPulse(target));

        yield return StartCoroutine(FadeToBlack());

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    IEnumerator ClickPulse(RectTransform target)
    {
        if (target == null) yield break;

        Vector3 startScale = target.localScale;
        Vector3 bigScale = startScale * clickPulseScale;

        float half = clickPulseDuration / 2f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / half);

            target.localScale = Vector3.Lerp(startScale, bigScale, p);

            if (selectorGroup != null)
                selectorGroup.alpha = Mathf.Lerp(1f, 0.4f, p);

            yield return null;
        }

        t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / half);

            target.localScale = Vector3.Lerp(bigScale, startScale, p);

            if (selectorGroup != null)
                selectorGroup.alpha = Mathf.Lerp(0.4f, 1f, p);

            yield return null;
        }

        target.localScale = startScale;

        if (selectorGroup != null)
            selectorGroup.alpha = 1f;
    }

    IEnumerator IntroFade()
    {
        isBusy = true;

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.alpha = 1f;
            fadePanel.blocksRaycasts = true;
        }

        yield return new WaitForSeconds(0.4f);

        float t = 0f;

        while (t < startFadeDuration)
        {
            t += Time.deltaTime;

            if (fadePanel != null)
                fadePanel.alpha = Mathf.Lerp(1f, 0f, t / startFadeDuration);

            yield return null;
        }

        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
            fadePanel.gameObject.SetActive(false);
        }

        isBusy = false;
    }

    IEnumerator FadeToBlack()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = true;

        float t = 0f;

        while (t < sceneFadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(0f, 1f, t / sceneFadeDuration);
            yield return null;
        }

        fadePanel.alpha = 1f;
    }
}
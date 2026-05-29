using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LobbyManager : MonoBehaviour
{
    public TextMeshProUGUI player1StatusText;
    public TextMeshProUGUI player2StatusText;
    public TextMeshProUGUI countText;

    public Button startButton;
    public Button backButton;

    public CanvasGroup backButtonGroup;
    public CanvasGroup startButtonGroup;

    public GameObject player1JoinHint;
    public GameObject player2JoinHint;

    public CanvasGroup fadePanel;

    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "MultiplayerPhrehistoricPuzzle1";

    private Color readyColor = new Color32(107, 217, 61, 255);
    private Color waitingColor = new Color32(218, 126, 25, 255);

    private int selectedButtonIndex = 0; // 0 = Back, 1 = Start
    private bool joystickReady = true;
    private bool isStarting = false;
    private bool autoSelectedStart = false;

    void Start()
    {
        LocalMultiplayerData.Reset();

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (backButton != null)
            backButton.onClick.AddListener(BackToMenu);

        UpdateLobbyUI();
        UpdateButtonVisuals();
        StartCoroutine(IntroFade());
    }

    void Update()
    {
        if (!Application.isFocused || isStarting) return;

        HandleGamepadJoin();
        HandleMenuControl();

        UpdateLobbyUI();
        UpdateButtonVisuals();
    }

    void HandleGamepadJoin()
    {
        foreach (Gamepad pad in Gamepad.all)
        {
            if (!pad.buttonNorth.wasPressedThisFrame) continue;

            if (LocalMultiplayerData.player1Gamepad == null)
            {
                LocalMultiplayerData.player1Gamepad = pad;
                return;
            }

            if (LocalMultiplayerData.player2Gamepad == null && pad != LocalMultiplayerData.player1Gamepad)
            {
                LocalMultiplayerData.player2Gamepad = pad;
                return;
            }
        }
    }

    void UpdateLobbyUI()
    {
        bool p1Ready = LocalMultiplayerData.HasPlayer1;
        bool p2Ready = LocalMultiplayerData.HasPlayer2;
        bool bothReady = p1Ready && p2Ready;

        if (player1JoinHint != null) player1JoinHint.SetActive(!p1Ready);
        if (player2JoinHint != null) player2JoinHint.SetActive(!p2Ready);

        if (player1StatusText != null)
        {
            player1StatusText.text = p1Ready ? "READY" : "WAITING...";
            player1StatusText.color = p1Ready ? readyColor : waitingColor;
        }

        if (player2StatusText != null)
        {
            player2StatusText.text = p2Ready ? "READY" : "WAITING...";
            player2StatusText.color = p2Ready ? readyColor : waitingColor;
        }

        if (countText != null)
        {
            int count = 0;
            if (p1Ready) count++;
            if (p2Ready) count++;

            countText.text = count + " / 2 PLAYERS";
        }

        if (startButton != null)
            startButton.interactable = bothReady && !isStarting;

        if (bothReady && !autoSelectedStart)
        {
            selectedButtonIndex = 1;
            autoSelectedStart = true;
        }

        if (!bothReady)
        {
            selectedButtonIndex = 0;
            autoSelectedStart = false;
        }
    }

    void HandleMenuControl()
    {
        Gamepad pad = LocalMultiplayerData.player1Gamepad;

        if (pad == null && Gamepad.all.Count > 0)
            pad = Gamepad.all[0];

        if (pad == null) return;

        Vector2 dpad = pad.dpad.ReadValue();
        Vector2 stick = pad.leftStick.ReadValue();

        if (joystickReady)
        {
            if (dpad.x > 0.5f || stick.x > 0.5f)
            {
                if (startButton != null && startButton.interactable)
                    selectedButtonIndex = 1;

                joystickReady = false;
            }

            if (dpad.x < -0.5f || stick.x < -0.5f)
            {
                selectedButtonIndex = 0;
                joystickReady = false;
            }
        }

        if (Mathf.Abs(dpad.x) < 0.2f && Mathf.Abs(stick.x) < 0.2f)
            joystickReady = true;

        if (pad.buttonSouth.wasPressedThisFrame)
        {
            if (selectedButtonIndex == 0)
                BackToMenu();

            if (selectedButtonIndex == 1 && startButton != null && startButton.interactable)
                StartGame();
        }
    }

    public void HoverBackButton()
    {
        selectedButtonIndex = 0;
    }

    public void HoverStartButton()
    {
        if (startButton != null && startButton.interactable)
            selectedButtonIndex = 1;
    }

    void UpdateButtonVisuals()
    {
        SetButtonOpacity(backButtonGroup, selectedButtonIndex == 0 ? 1f : 0.5f);
        SetButtonOpacity(startButtonGroup, selectedButtonIndex == 1 ? 1f : 0.5f);
    }

    void SetButtonOpacity(CanvasGroup group, float alpha)
    {
        if (group == null) return;
        group.alpha = alpha;
    }

    void StartGame()
    {
        if (isStarting) return;
        if (!LocalMultiplayerData.HasPlayer1 || !LocalMultiplayerData.HasPlayer2) return;

        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        isStarting = true;
        yield return FadeToBlack();
        SceneManager.LoadScene(gameSceneName);
    }

    void BackToMenu()
    {
        if (isStarting) return;
        StartCoroutine(BackRoutine());
    }

    IEnumerator BackRoutine()
    {
        isStarting = true;
        yield return FadeToBlack();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    IEnumerator IntroFade()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 1f;

        float t = 0f;
        while (t < 0.8f)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(1f, 0f, t / 0.8f);
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.gameObject.SetActive(false);
    }

    IEnumerator FadeToBlack()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 0f;

        float t = 0f;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(0f, 1f, t / 0.6f);
            yield return null;
        }

        fadePanel.alpha = 1f;
    }
}
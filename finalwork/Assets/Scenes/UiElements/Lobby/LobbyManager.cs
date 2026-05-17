using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("Texts")]
    public TextMeshProUGUI player1StatusText;
    public TextMeshProUGUI player2StatusText;
    public TextMeshProUGUI countText;

    [Header("Buttons")]
    public Button startButton;
    public Button backButton;

    [Header("Fade")]
    public CanvasGroup fadePanel;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "MultiplayerPhrehistoricPuzzle1";

    private Color readyColor = new Color32(107, 217, 61, 255);
    private Color waitingColor = new Color32(218, 126, 25, 255);

    private bool isStarting = false;

    void Start()
    {
        ForceLobbyMode();

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (backButton != null)
            backButton.onClick.AddListener(BackToMenu);

        StartCoroutine(IntroFade());
    }

    void Update()
    {
        ForceLobbyMode();
        UpdateLobbyUI();
    }

    void UpdateLobbyUI()
    {
        if (NetworkManager.Singleton == null) return;

        int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
        bool isHost = NetworkManager.Singleton.IsHost;

        if (player1StatusText != null)
        {
            player1StatusText.text = "READY";
            player1StatusText.color = readyColor;
        }

        if (player2StatusText != null)
        {
            if (playerCount >= 2)
            {
                player2StatusText.text = "READY";
                player2StatusText.color = readyColor;
            }
            else
            {
                player2StatusText.text = "WAITING...";
                player2StatusText.color = waitingColor;
            }
        }

        if (countText != null)
            countText.text = playerCount + " / 2 PLAYERS";

        if (startButton != null)
        {
            bool canStart = isHost && playerCount >= 2 && !isStarting;
            startButton.interactable = canStart;
        }
    }

void ForceLobbyMode()
{
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

    foreach (GameObject obj in allObjects)
    {
        if (obj.name.Contains("InventoryCanvas"))
            obj.SetActive(false);

        if (obj.name.Contains("InventoryUI"))
            obj.SetActive(false);
    }
}

    void StartGame()
    {
        if (isStarting) return;
        if (NetworkManager.Singleton == null) return;
        if (!NetworkManager.Singleton.IsHost) return;
        if (NetworkManager.Singleton.ConnectedClientsList.Count < 2) return;

        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        isStarting = true;

        if (startButton != null)
            yield return StartCoroutine(ButtonClickAnimation(startButton.transform));

        yield return StartCoroutine(FadeToBlack());

        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    void BackToMenu()
    {
        if (isStarting) return;
        StartCoroutine(BackToMenuRoutine());
    }

    IEnumerator BackToMenuRoutine()
    {
        isStarting = true;

        if (backButton != null)
            yield return StartCoroutine(ButtonClickAnimation(backButton.transform));

        yield return StartCoroutine(FadeToBlack());

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    IEnumerator IntroFade()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 1f;
        fadePanel.blocksRaycasts = true;

        float duration = 0.8f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
        fadePanel.gameObject.SetActive(false);
    }

    IEnumerator FadeToBlack()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = true;

        float duration = 0.6f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        fadePanel.alpha = 1f;
    }

    IEnumerator ButtonClickAnimation(Transform buttonTransform)
    {
        Vector3 startScale = buttonTransform.localScale;
        Vector3 bigScale = startScale * 1.08f;

        float duration = 0.18f;
        float half = duration / 2f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / half);
            buttonTransform.localScale = Vector3.Lerp(startScale, bigScale, p);
            yield return null;
        }

        t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / half);
            buttonTransform.localScale = Vector3.Lerp(bigScale, startScale, p);
            yield return null;
        }

        buttonTransform.localScale = startScale;
    }
}
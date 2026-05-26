using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OudheidExitPortal : NetworkBehaviour
{
    public bool isPlayer1Exit = true;
    public string nextSceneName = "MulitplayerMiddeleeuwenPuzzle1";

    public CanvasGroup blackFadeCanvasGroup;
    public GameObject waitingTextObject;

    public float fadeDuration = 1f;

    private static bool player1Ready = false;
    private static bool player2Ready = false;

    private bool triggered = false;

    void Start()
    {
        if (blackFadeCanvasGroup != null)
            blackFadeCanvasGroup.alpha = 0f;

        if (waitingTextObject != null)
            waitingTextObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj == null || !playerNetObj.IsOwner) return;

        triggered = true;

        HideInventoryUI();
        StartCoroutine(FadeToBlack());

        ReportPlayerExitedServerRpc(isPlayer1Exit);
    }

    IEnumerator FadeToBlack()
    {
        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.gameObject.SetActive(true);
            blackFadeCanvasGroup.blocksRaycasts = true;

            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                blackFadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                yield return null;
            }

            blackFadeCanvasGroup.alpha = 1f;
        }

        if (waitingTextObject != null)
        {
            waitingTextObject.SetActive(true);
            waitingTextObject.transform.SetAsLastSibling();
        }
    }

    void HideInventoryUI()
    {
        GameObject[] objects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (GameObject obj in objects)
        {
            if (obj.name.Contains("InventoryCanvas") || obj.name.Contains("InventoryUI"))
                obj.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ReportPlayerExitedServerRpc(bool player1Exit)
    {
        if (player1Exit)
            player1Ready = true;
        else
            player2Ready = true;

        if (player1Ready && player2Ready)
        {
            player1Ready = false;
            player2Ready = false;

            NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OudheidExitPortal : MonoBehaviour
{
    public int ownerPlayerIndex = 0;
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
        {
            blackFadeCanvasGroup.alpha = 0f;
            blackFadeCanvasGroup.blocksRaycasts = false;
            blackFadeCanvasGroup.gameObject.SetActive(false);
        }

        if (waitingTextObject != null)
            waitingTextObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;

        if (pickup.playerIndex != ownerPlayerIndex) return;

        triggered = true;

        HideInventoryUI(ownerPlayerIndex);
        StartCoroutine(FadeToBlack());

        if (ownerPlayerIndex == 0)
            player1Ready = true;
        else
            player2Ready = true;

        if (player1Ready && player2Ready)
        {
            player1Ready = false;
            player2Ready = false;

            StartCoroutine(LoadSceneAfterSmallDelay());
        }
    }

    IEnumerator FadeToBlack()
    {
        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.gameObject.SetActive(true);
            blackFadeCanvasGroup.transform.SetAsLastSibling();
            blackFadeCanvasGroup.blocksRaycasts = true;
            blackFadeCanvasGroup.alpha = 0f;

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

    IEnumerator LoadSceneAfterSmallDelay()
    {
        yield return new WaitForSeconds(fadeDuration + 0.2f);
        SceneManager.LoadScene(nextSceneName);
    }

    void HideInventoryUI(int playerIndex)
    {
        GameObject[] objects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (GameObject obj in objects)
        {
            if (playerIndex == 0 && obj.name.Contains("Inventory_Player1"))
                obj.SetActive(false);

            if (playerIndex == 1 && obj.name.Contains("Inventory_Player2"))
                obj.SetActive(false);
        }
    }
}
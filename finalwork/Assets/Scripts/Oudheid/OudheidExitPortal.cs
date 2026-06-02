using System.Collections;
using UnityEngine;

public class OudheidExitPortal : MonoBehaviour
{
    public int ownerPlayerIndex = 0;
    public string nextSceneName = "MiddeleeuwenPuzzle1";

    public CanvasGroup blackFadeCanvasGroup;
    public LevelCompletePopupPlayer playerPopup;
    public LevelCompletePopupManager popupManager;

    public float fadeDuration = 1f;

    private bool triggered = false;

    void Start()
    {
        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.alpha = 0f;
            blackFadeCanvasGroup.blocksRaycasts = false;
            blackFadeCanvasGroup.gameObject.SetActive(false);
        }
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
        StartCoroutine(FadeThenShowPopup());
    }

    IEnumerator FadeThenShowPopup()
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

        if (popupManager != null)
            popupManager.nextSceneName = nextSceneName;

if (popupManager != null)
{
    popupManager.nextSceneName = nextSceneName;
    popupManager.StartCheckingPlayers();
}

        if (playerPopup != null)
            playerPopup.Open();
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
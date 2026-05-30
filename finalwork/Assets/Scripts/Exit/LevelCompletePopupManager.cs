using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompletePopupManager : MonoBehaviour
{
    public LevelCompletePopupPlayer player1Popup;
    public LevelCompletePopupPlayer player2Popup;

    public string nextSceneName = "MultiplayerOudheidPuzzle1";

    private bool popupActive = false;

    void Update()
    {
        if (!popupActive) return;

        if (player1Popup != null && player2Popup != null)
        {
            if (player1Popup.IsReady && player2Popup.IsReady)
                StartCoroutine(LoadSceneRoutine());
        }
    }

    public void ShowPopup()
    {
        popupActive = true;

        if (player1Popup != null)
            player1Popup.Open();

        if (player2Popup != null)
            player2Popup.Open();
    }

    IEnumerator LoadSceneRoutine()
    {
        popupActive = false;
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene(nextSceneName);
    }

    public void StartCheckingPlayers()
{
    popupActive = true;
}
}
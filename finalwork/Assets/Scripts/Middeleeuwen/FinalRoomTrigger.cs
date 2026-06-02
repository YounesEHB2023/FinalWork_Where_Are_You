using System.Collections;
using TMPro;
using UnityEngine;

public class FinalRoomTrigger : MonoBehaviour
{
    public TextMeshProUGUI playerCountTextPlayer1;
    public TextMeshProUGUI playerCountTextPlayer2;

    public float waitBeforeFade = 2f;

    private bool player1Inside = false;
    private bool player2Inside = false;
    private bool sequenceStarted = false;

    void Start()
    {
        HideText();
    }

    void OnTriggerEnter(Collider other)
    {
        if (sequenceStarted) return;
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;

        if (pickup.playerIndex == 0) player1Inside = true;
        if (pickup.playerIndex == 1) player2Inside = true;

        UpdateText();

        if (player1Inside && player2Inside)
            StartCoroutine(StartEndSequence());
    }

    void OnTriggerExit(Collider other)
    {
        if (sequenceStarted) return;
        if (!other.CompareTag("Player")) return;

        PickupSystem pickup = other.GetComponentInChildren<PickupSystem>(true);
        if (pickup == null) return;

        if (pickup.playerIndex == 0) player1Inside = false;
        if (pickup.playerIndex == 1) player2Inside = false;

        UpdateText();
    }

    void UpdateText()
    {
        int count = 0;
        if (player1Inside) count++;
        if (player2Inside) count++;

        if (count == 0)
        {
            HideText();
            return;
        }

        string text = count + " / 2 SPELERS";

        if (playerCountTextPlayer1 != null)
        {
            playerCountTextPlayer1.gameObject.SetActive(true);
            playerCountTextPlayer1.text = text;
        }

        if (playerCountTextPlayer2 != null)
        {
            playerCountTextPlayer2.gameObject.SetActive(true);
            playerCountTextPlayer2.text = text;
        }
    }

    void HideText()
    {
        if (playerCountTextPlayer1 != null)
            playerCountTextPlayer1.gameObject.SetActive(false);

        if (playerCountTextPlayer2 != null)
            playerCountTextPlayer2.gameObject.SetActive(false);
    }

    IEnumerator StartEndSequence()
    {
        sequenceStarted = true;

        yield return new WaitForSeconds(waitBeforeFade);

        Debug.Log("Both players are in final room. Start fade + popup next.");
    }
}
using System.Collections;
using UnityEngine;

public class MedievalPlaguePuzzleManager : MonoBehaviour
{
    [Header("Levers")]
    public MedievalLeverDoor diningLever;
    public MedievalLeverDoor prisonLever;
    public MedievalLeverDoor weaponsLever;
    public MedievalLeverDoor chestLever;

    [Header("Final Door")]
    public Transform finalDoor;
    public Vector3 finalDoorOpenRotation = new Vector3(-90f, 0f, 255f);
    public float doorOpenDuration = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip puzzleSolvedSound;
    public AudioClip doorOpenSound;

    [Header("After Puzzle")]
public Transform player1TeleportPoint;

    private bool solved = false;

    void Update()
    {
        if (solved) return;

        CheckPuzzle();
    }

    void CheckPuzzle()
    {
        if (diningLever == null || prisonLever == null || weaponsLever == null || chestLever == null)
            return;

        bool diningCorrect = diningLever.IsClosed();    
        bool prisonCorrect = prisonLever.IsClosed();     
        bool chestCorrect = chestLever.IsClosed();       
        bool weaponsCorrect = !weaponsLever.IsClosed();  

        if (diningCorrect && prisonCorrect && chestCorrect && weaponsCorrect)
        {
            solved = true;
            StartCoroutine(SolvePuzzle());
        }
    }

    IEnumerator SolvePuzzle()
    {
        if (audioSource != null && puzzleSolvedSound != null)
            audioSource.PlayOneShot(puzzleSolvedSound);

        yield return new WaitForSeconds(0.6f);

        if (audioSource != null && doorOpenSound != null)
            audioSource.PlayOneShot(doorOpenSound);

        if (finalDoor != null)
        {
            Quaternion startRot = finalDoor.localRotation;
            Quaternion endRot = Quaternion.Euler(finalDoorOpenRotation);

            float t = 0f;

            while (t < doorOpenDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / doorOpenDuration);

                finalDoor.localRotation = Quaternion.Lerp(startRot, endRot, p);

                yield return null;
            }

            finalDoor.localRotation = endRot;
        }

        TeleportPlayer1();
LockLevers();
    }

    void LockLevers()
{
    if (diningLever != null) diningLever.LockLever();
    if (prisonLever != null) prisonLever.LockLever();
    if (weaponsLever != null) weaponsLever.LockLever();
    if (chestLever != null) chestLever.LockLever();
}

void TeleportPlayer1()
{
    FirstPersonController[] players = FindObjectsByType<FirstPersonController>(FindObjectsSortMode.None);

    foreach (FirstPersonController player in players)
    {
        if (player.playerIndex != 0) continue;

        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null) cc.enabled = false;

        player.transform.position = player1TeleportPoint.position;
        player.transform.rotation = player1TeleportPoint.rotation;

        if (cc != null) cc.enabled = true;

        break;
    }
}
}
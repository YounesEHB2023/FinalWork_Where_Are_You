using UnityEngine;

public class OudheidWeaponPuzzleManager : MonoBehaviour
{
    public WeaponPedestalSocket[] sockets;
    public AudioSource audioSource;
    public AudioClip correctSound;

    [Header("Final Puzzle")]
    public OudheidFinalPuzzleManager finalPuzzleManager;
    public bool isPlayer1Puzzle = true;

    private bool puzzleSolved = false;

    public bool IsSolved()
    {
        return puzzleSolved;
    }

    public void CheckPuzzle()
    {
        if (puzzleSolved) return;

        foreach (WeaponPedestalSocket socket in sockets)
        {
            if (socket == null || !socket.HasWeaponPlaced())
                return;
        }

        foreach (WeaponPedestalSocket socket in sockets)
        {
            if (!socket.IsCorrectWeaponPlaced())
                return;
        }

        puzzleSolved = true;

        if (audioSource != null && correctSound != null)
            audioSource.PlayOneShot(correctSound);

        foreach (WeaponPedestalSocket socket in sockets)
            socket.LockPlacedWeapon();

        if (finalPuzzleManager != null)
    finalPuzzleManager.CheckFinalPuzzle();

        Debug.Log(gameObject.name + " CORRECT");
    }
}
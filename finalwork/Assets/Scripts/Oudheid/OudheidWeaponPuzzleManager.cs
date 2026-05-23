using UnityEngine;

public class OudheidWeaponPuzzleManager : MonoBehaviour
{
    public WeaponPedestalSocket[] sockets;
    public AudioSource audioSource;
    public AudioClip correctSound;

    private bool puzzleSolved = false;

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
        {
            socket.LockPlacedWeapon();
        }

        Debug.Log("OUDHEID WEAPON PUZZLE CORRECT");
    }
}
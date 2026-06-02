using UnityEngine;

public class OudheidFinalPuzzleManager : MonoBehaviour
{
    public OudheidWeaponPuzzleManager player1Puzzle;
    public OudheidWeaponPuzzleManager player2Puzzle;

    public OudheidDoorOpener[] doors;

    private bool finalSolved = false;

    public void CheckFinalPuzzle()
    {
        if (finalSolved) return;

        if (player1Puzzle == null || player2Puzzle == null) return;

        if (!player1Puzzle.IsSolved()) return;
        if (!player2Puzzle.IsSolved()) return;

        finalSolved = true;

        foreach (OudheidDoorOpener door in doors)
        {
            if (door != null)
                door.OpenDoor();
        }

        Debug.Log("OUDHEID FULL PUZZLE COMPLETE");
    }
}
using Unity.Netcode;
using UnityEngine;

public class OudheidFinalPuzzleManager : NetworkBehaviour
{
    public OudheidDoorOpener[] doors;

    private bool player1Solved = false;
    private bool player2Solved = false;
    private bool finalSolved = false;

    public void ReportPuzzleSolved(bool isPlayer1Puzzle)
    {
        if (!IsSpawned)
        {
            Debug.LogWarning("OudheidFinalPuzzleManager is not spawned yet.");
            return;
        }

        ReportPuzzleSolvedServerRpc(isPlayer1Puzzle);
    }

    [ServerRpc(RequireOwnership = false)]
    void ReportPuzzleSolvedServerRpc(bool isPlayer1Puzzle)
    {
        if (finalSolved) return;

        if (isPlayer1Puzzle)
            player1Solved = true;
        else
            player2Solved = true;

        if (player1Solved && player2Solved)
        {
            finalSolved = true;
            OpenDoorsClientRpc();
        }
    }

    [ClientRpc]
    void OpenDoorsClientRpc()
    {
        foreach (OudheidDoorOpener door in doors)
        {
            if (door != null)
                door.OpenDoor();
        }

        Debug.Log("OUDHEID FULL PUZZLE COMPLETE");
    }
}
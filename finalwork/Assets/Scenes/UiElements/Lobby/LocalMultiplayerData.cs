using UnityEngine.InputSystem;

public static class LocalMultiplayerData
{
    public static Gamepad player1Gamepad;
    public static Gamepad player2Gamepad;

    public static bool HasPlayer1 => player1Gamepad != null;
    public static bool HasPlayer2 => player2Gamepad != null;

    public static void Reset()
    {
        player1Gamepad = null;
        player2Gamepad = null;
    }
}
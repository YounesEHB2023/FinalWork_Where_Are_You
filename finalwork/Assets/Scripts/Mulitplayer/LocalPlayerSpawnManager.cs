using UnityEngine;

public class LocalPlayerSpawnManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;

    void Start()
    {
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        GameObject player1 = Instantiate(playerPrefab, spawnPoint1.position, spawnPoint1.rotation);
        GameObject player2 = Instantiate(playerPrefab, spawnPoint2.position, spawnPoint2.rotation);

        SetupPlayer(player1, 0, 0);
        SetupPlayer(player2, 1, 1);
    }

    void SetupPlayer(GameObject player, int playerIndex, int displayIndex)
    {
        FirstPersonController controller = player.GetComponent<FirstPersonController>();

        if (controller != null)
        {
            controller.playerIndex = playerIndex;
            controller.useKeyboardAndMouse = false;
        }

        Camera cam = player.GetComponentInChildren<Camera>(true);

        if (cam != null)
        {
            cam.enabled = true;
            cam.targetDisplay = displayIndex;
            cam.rect = new Rect(0, 0, 1, 1);
        }

        AudioListener[] listeners = player.GetComponentsInChildren<AudioListener>(true);
        foreach (AudioListener listener in listeners)
        {
            listener.enabled = playerIndex == 0;
        }
        Canvas[] canvases = player.GetComponentsInChildren<Canvas>(true);

        foreach (Canvas canvas in canvases)
        {
           canvas.targetDisplay = displayIndex;
        }
    }
}
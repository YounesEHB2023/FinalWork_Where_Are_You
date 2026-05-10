using UnityEngine;
using Unity.Netcode;

public class CustomSpawnManager : MonoBehaviour
{
    public Transform forestSpawn;
    public Transform caveSpawn;

    public GameObject playerPrefab;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
    }

    private void SpawnPlayer(ulong clientId)
    {
        // Alleen server mag spelers spawnen
        if (!NetworkManager.Singleton.IsServer) return;

        GameObject player = Instantiate(playerPrefab);

        // Host speler
        if (clientId == 0)
        {
            player.transform.position = forestSpawn.position;
        }
        // Client speler
        else
        {
            player.transform.position = caveSpawn.position;
        }

        player.GetComponent<NetworkObject>()
            .SpawnAsPlayerObject(clientId, true);
    }
}
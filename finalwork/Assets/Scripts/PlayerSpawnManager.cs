using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : NetworkBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;

        StartCoroutine(SpawnAllPlayersAfterSceneLoad());
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
    }

    IEnumerator SpawnAllPlayersAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }

    void SpawnPlayer(ulong clientId)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is missing on SpawnManager.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn points are missing.");
            return;
        }

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
            return;

        if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
        {
            client.PlayerObject.Despawn(true);
        }

        int spawnIndex = (int)(clientId % (ulong)spawnPoints.Length);

        Vector3 spawnPosition = spawnPoints[spawnIndex].position;
        Quaternion spawnRotation = spawnPoints[spawnIndex].rotation;

        GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);

        NetworkObject netObj = player.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError("Player prefab needs NetworkObject.");
            Destroy(player);
            return;
        }

        netObj.SpawnAsPlayerObject(clientId, true);
    }
}
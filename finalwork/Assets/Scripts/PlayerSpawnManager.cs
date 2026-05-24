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
            SpawnPlayer(clientId);
    }

    void SpawnPlayer(ulong clientId)
    {
        if (playerPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
            return;

        if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
            client.PlayerObject.Despawn(true);

        int spawnIndex = clientId == 0 ? 0 : 1;

        if (spawnIndex >= spawnPoints.Length)
            spawnIndex = 0;

        Vector3 spawnPosition = spawnPoints[spawnIndex].position;
        Quaternion spawnRotation = spawnPoints[spawnIndex].rotation;

        GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);
        NetworkObject netObj = player.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Destroy(player);
            return;
        }

        netObj.SpawnAsPlayerObject(clientId, true);

        ForceSpawnPositionClientRpc(spawnPosition, spawnRotation, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        });
    }

    [ClientRpc]
    void ForceSpawnPositionClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams clientRpcParams = default)
    {
        StartCoroutine(ForceSpawnRoutine(position, rotation));
    }

    IEnumerator ForceSpawnRoutine(Vector3 position, Quaternion rotation)
    {
        yield return new WaitForSeconds(0.2f);

        if (NetworkManager.Singleton.LocalClient.PlayerObject == null)
            yield break;

        GameObject player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        player.transform.position = position;
        player.transform.rotation = rotation;

        if (controller != null)
            controller.enabled = true;

        ResetFadeUI();
    }

    void ResetFadeUI()
    {
        CanvasGroup[] groups = FindObjectsByType<CanvasGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (CanvasGroup group in groups)
        {
            if (group.name.Contains("Fade") || group.name.Contains("Black"))
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
                group.gameObject.SetActive(false);
            }
        }

        GameObject[] objects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (GameObject obj in objects)
        {
            if (obj.name.Contains("InventoryCanvas") || obj.name.Contains("InventoryUI"))
                obj.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
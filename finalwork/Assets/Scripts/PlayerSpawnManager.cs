using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : NetworkBehaviour
{
    public Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += MovePlayerToSpawn;
        StartCoroutine(MoveAllPlayersAfterSceneLoad());
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= MovePlayerToSpawn;
    }

    IEnumerator MoveAllPlayersAfterSceneLoad()
    {
        yield return new WaitForSeconds(1f);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            MovePlayerToSpawn(clientId);
        }
    }

    void MovePlayerToSpawn(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client)) return;
        if (client.PlayerObject == null) return;

        int spawnIndex = (int)(clientId % (ulong)spawnPoints.Length);

        Vector3 spawnPosition = spawnPoints[spawnIndex].position;
        Quaternion spawnRotation = spawnPoints[spawnIndex].rotation;

        GameObject player = client.PlayerObject.gameObject;

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        player.transform.position = spawnPosition;
        player.transform.rotation = spawnRotation;

        if (controller != null)
            controller.enabled = true;

        MovePlayerClientRpc(spawnPosition, spawnRotation, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        });
    }

    [ClientRpc]
    void MovePlayerClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject == null) return;

        GameObject player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        player.transform.position = position;
        player.transform.rotation = rotation;

        if (controller != null)
            controller.enabled = true;

        EnableGameplayUI();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void EnableGameplayUI()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("InventoryCanvas"))
                obj.SetActive(true);

            if (obj.name.Contains("InventoryUI"))
                obj.SetActive(true);
        }
    }
}
using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : NetworkBehaviour
{
    public Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += MovePlayerToSpawn;
    }

    public override void OnDestroy()
{
    base.OnDestroy();

    if (NetworkManager.Singleton != null)
        NetworkManager.Singleton.OnClientConnectedCallback -= MovePlayerToSpawn;
}

    void MovePlayerToSpawn(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        int spawnIndex = (int)(clientId % (ulong)spawnPoints.Length);

        Vector3 spawnPosition = spawnPoints[spawnIndex].position;
        Quaternion spawnRotation = spawnPoints[spawnIndex].rotation;

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
    }
}
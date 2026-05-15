using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExitPortal : NetworkBehaviour
{
    [Header("Scene")]
    public string nextSceneName = "PrehistoricPuzzle2";

    [Header("Portal")]
    public Transform portalCenter;
    public float pullDuration = 1.5f;
    public int requiredPlayers = 1;

    private HashSet<ulong> playersInside = new HashSet<ulong>();
    private bool transitionStarted = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!IsServer) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj == null) return;

        playersInside.Add(playerNetObj.OwnerClientId);

        if (!transitionStarted && playersInside.Count >= requiredPlayers)
        {
            transitionStarted = true;

            StartTransitionClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { playerNetObj.OwnerClientId }
                }
            });

            StartCoroutine(LoadNextSceneAfterDelay());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!IsServer) return;

        NetworkObject playerNetObj = other.GetComponent<NetworkObject>();
        if (playerNetObj == null) return;

        playersInside.Remove(playerNetObj.OwnerClientId);
    }

    [ClientRpc]
    void StartTransitionClientRpc(ClientRpcParams clientRpcParams = default)
    {
        StartCoroutine(PullLocalPlayer());
    }

    IEnumerator PullLocalPlayer()
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject == null)
            yield break;

        Transform localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.transform;

        if (portalCenter == null)
            yield break;

        CharacterController controller = localPlayer.GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        Vector3 startPos = localPlayer.position;
        Vector3 endPos = portalCenter.position;

        float t = 0f;

        while (t < pullDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / pullDuration);

            localPlayer.position = Vector3.Lerp(startPos, endPos, progress);
            localPlayer.Rotate(Vector3.up, 180f * Time.deltaTime);

            yield return null;
        }

        if (controller != null)
            controller.enabled = true;
    }

    IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(pullDuration + 0.3f);

        NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}
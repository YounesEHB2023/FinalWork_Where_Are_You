using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using TMPro;

public class NetworkStartUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject menuUI;
    public TMP_InputField ipInputField;

    [Header("Spawn Points")]
    public Transform hostSpawnPoint;
    public Transform clientSpawnPoint;

    public void StartHost()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        NetworkManager.Singleton.StartHost();

        HideMenu();
    }

    public void StartClient()
    {
        string ip = ipInputField.text;

        if (string.IsNullOrWhiteSpace(ip))
            ip = "127.0.0.1";

        UnityTransport transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetConnectionData(ip, 7777);

        NetworkManager.Singleton.StartClient();

        HideMenu();
    }

    void OnClientConnected(ulong clientId)
    {
        GameObject player =
            NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId).gameObject;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            player.transform.position = hostSpawnPoint.position;
            player.transform.rotation = hostSpawnPoint.rotation;
        }
        else
        {
            player.transform.position = clientSpawnPoint.position;
            player.transform.rotation = clientSpawnPoint.rotation;
        }
    }

    void HideMenu()
    {
        if (menuUI != null)
            menuUI.SetActive(false);
    }
}
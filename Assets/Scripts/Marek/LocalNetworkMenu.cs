using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Solo play, plus Host / Join-by-IP without Unity Gaming Services. Also reads "-host", "-solo" and "-join <address>" from the command line.
/// </summary>
public class LocalNetworkMenu : MonoBehaviour
{
    [SerializeField] private Button soloButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject[] hideWhenConnected;
    [Tooltip("Networked-only menus; a browser can't host or join, so these are hidden in WebGL builds.")]
    [SerializeField] private GameObject[] hideInWebGL;
    [SerializeField] private ushort port = 7777;

    private void Start()
    {
        soloButton.onClick.AddListener(PlaySolo);
        hostButton.onClick.AddListener(Host);
        joinButton.onClick.AddListener(Join);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            foreach (var menu in hideInWebGL)
            {
                if (menu != null) menu.SetActive(false);
            }
        }

        var networkManager = NetworkManager.Singleton;
        if (networkManager != null)
        {
            networkManager.OnClientStarted += HideMenus;
            networkManager.OnServerStarted += HideMenus;
            networkManager.OnClientDisconnectCallback += HandleDisconnect;
        }

        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-host")
            {
                Host();
            }
            else if (args[i] == "-solo")
            {
                PlaySolo();
            }
            else if (args[i] == "-join")
            {
                if (i + 1 < args.Length) addressInput.text = args[i + 1];
                Join();
            }
        }
    }

    private void OnDestroy()
    {
        var networkManager = NetworkManager.Singleton;
        if (networkManager != null)
        {
            networkManager.OnClientStarted -= HideMenus;
            networkManager.OnServerStarted -= HideMenus;
            networkManager.OnClientDisconnectCallback -= HandleDisconnect;
        }
    }

    public void PlaySolo()
    {
        if (!CanStart()) return;

        var networkManager = NetworkManager.Singleton;
        if (!networkManager.TryGetComponent<OfflineTransport>(out var offline))
        {
            offline = networkManager.gameObject.AddComponent<OfflineTransport>();
        }
        networkManager.NetworkConfig.NetworkTransport = offline;

        if (!networkManager.StartHost())
        {
            SetStatus("Could not start solo play.");
        }
    }

    public void Host()
    {
        if (!CanStart()) return;

        NetworkManager.Singleton.NetworkConfig.NetworkTransport = GetTransport();
        GetTransport().SetConnectionData("127.0.0.1", port, "0.0.0.0");
        if (!NetworkManager.Singleton.StartHost())
        {
            SetStatus("Could not host. Is port " + port + " already in use?");
        }
    }

    public void Join()
    {
        if (!CanStart()) return;

        string address = string.IsNullOrWhiteSpace(addressInput.text) ? "127.0.0.1" : addressInput.text.Trim();
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = GetTransport();
        GetTransport().SetConnectionData(address, port);
        SetStatus("Connecting to " + address + "...");
        if (!NetworkManager.Singleton.StartClient())
        {
            SetStatus("Could not connect to " + address + ".");
        }
    }

    private bool CanStart()
    {
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            SetStatus("No NetworkManager in the scene.");
            return false;
        }
        return !networkManager.IsListening;
    }

    private UnityTransport GetTransport()
    {
        return NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    private void HideMenus()
    {
        foreach (var menu in hideWhenConnected)
        {
            if (menu != null) menu.SetActive(false);
        }
    }

    private void HandleDisconnect(ulong clientId)
    {
        var networkManager = NetworkManager.Singleton;
        if (networkManager.IsServer || clientId != networkManager.LocalClientId) return;

        // Failed to connect or lost the host: bring the menu back.
        foreach (var menu in hideWhenConnected)
        {
            if (menu != null) menu.SetActive(true);
        }
        SetStatus("Disconnected. " + networkManager.DisconnectReason);
    }

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
        Debug.Log("[LocalNetworkMenu] " + message);
    }
}

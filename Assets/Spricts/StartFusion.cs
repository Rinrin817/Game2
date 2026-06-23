using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class StartFusion : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner runner;
    [SerializeField] PlayerName playerName;
    [SerializeField] Text roomText;
    [SerializeField] TextMeshProUGUI startText;
    [Networked, Capacity(16)] public NetworkString<_16> PlayerName { get; set; }
    
    bool gameStarted;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        runner.AddCallbacks(this);
    }

    public async void StartGame()
    {
        PlayerData.PlayerName = playerName.GetPlayerName();

        if (!runner.IsRunning)
        {
            await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = "TestRoom",
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
            });
        }

        if (!runner.IsSharedModeMasterClient) return;

        startText.text = "待機中...";
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (roomText != null) roomText.text = runner.ActivePlayers.Count() + "/5";
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (roomText != null) roomText.text = runner.ActivePlayers.Count() + "/5";
    }

    void Update()
    {
        if (gameStarted) return;

        if (runner.IsSharedModeMasterClient && runner.ActivePlayers.Count() >= 2)
        {
            gameStarted = true;
            runner.LoadScene(SceneRef.FromIndex(1));
        }
    }
    
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
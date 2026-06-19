using Fusion;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class StartFusion : MonoBehaviour
{
    [SerializeField] private NetworkRunner runner;
    bool gameStarted;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async void StartGame()
    {
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "TestRoom",
            PlayerCount = 4,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
        });
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
}

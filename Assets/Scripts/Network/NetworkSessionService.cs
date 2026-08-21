using Fusion;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum NetworkSessionState
{
    Offline,
    Connecting,
    Connected,
    Disconnecting
}

public sealed class NetworkSessionService : MonoBehaviour
{
    private const string GameSceneAddress = "Assets/Scenes/CV.unity";

    [Header("Configuration")]
    [SerializeField]
    private NetworkSessionConfig config;

    [Header("Runner")]
    [Tooltip("Prefab containing NetworkRunner, NetworkSceneManagerDefault and SharedPlayerSpawner.")]
    [SerializeField]
    private NetworkRunner runnerPrefab;

    public NetworkRunner Runner { get; private set; }

    public NetworkSessionState State { get; private set; } = NetworkSessionState.Offline;

    public event Action<NetworkSessionState> StateChanged;
    public event Action<StartGameResult> ConnectionFailed;

    private void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.targetFrameRate = -1;
QualitySettings.vSyncCount = 0;
#endif
        DontDestroyOnLoad(gameObject);
    }

    public Task<StartGameResult> JoinDefaultRoomAsync()
    {
        return JoinRoomAsync(config.DefaultRoomName);
    }

    public async Task<StartGameResult> JoinRoomAsync(string roomName)
    {
        if (State != NetworkSessionState.Offline)
        {
            throw new InvalidOperationException(
                $"Cannot join while session state is {State}.");
        }

        AnalyticsService.JoinRoomClick(PlayerInfoSave.GetName());

        roomName = roomName?.Trim();

        if (string.IsNullOrWhiteSpace(roomName))
        {
            throw new ArgumentException(
                "Room name cannot be empty.",
                nameof(roomName));
        }

        SetState(NetworkSessionState.Connecting);

        NetworkRunner newRunner = null;

        try
        {
            newRunner = Instantiate(runnerPrefab);
            newRunner.name = $"NetworkRunner [{roomName}]";

            DontDestroyOnLoad(newRunner.gameObject);

            NetworkSceneManagerDefault sceneManager = newRunner.GetComponent<NetworkSceneManagerDefault>();

            if (sceneManager == null)
            {
                throw new MissingComponentException(
                    "The NetworkRunner prefab must contain " +
                    "NetworkSceneManagerDefault.");
            }

            Scene activeScene = SceneManager.GetActiveScene();

            if (activeScene.buildIndex < 0)
            {
                throw new InvalidOperationException(
                    $"Scene '{activeScene.name}' is not in Build Settings.");
            }

            SceneRef sceneRef = SceneRef.FromPath(GameSceneAddress);
            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Single);

            newRunner.ProvideInput = false;

            StartGameResult result = await newRunner.StartGame(
                new StartGameArgs
                {
                    GameMode = GameMode.Shared,
                    SessionName = roomName,
                    PlayerCount = config.MaxPlayers,
                    Scene = sceneInfo,
                    SceneManager = sceneManager
                });

            if (!result.Ok)
            {
                Debug.LogError($"Fusion connection failed: {result.ShutdownReason}");
                AnalyticsService.JoinRoomFail(result.ShutdownReason);
                ConnectionFailed?.Invoke(result);

                Destroy(newRunner.gameObject);
                SetState(NetworkSessionState.Offline);

                return result;
            }

            Runner = newRunner;
            AnalyticsService.JoinRoomSuccess(PlayerInfoSave.GetName());


            SetState(NetworkSessionState.Connected);

            return result;
        }
        catch
        {
            if (newRunner != null)
            {
                Destroy(newRunner.gameObject);
            }

            Runner = null;
            AnalyticsService.JoinRoomFail(ShutdownReason.Error);
            SetState(NetworkSessionState.Offline);

            throw;
        }
    }

    public async Task LeaveAsync()
    {
        if (Runner == null ||
            State == NetworkSessionState.Offline ||
            State == NetworkSessionState.Disconnecting)
        {
            return;
        }

        SetState(NetworkSessionState.Disconnecting);

        NetworkRunner runnerToShutdown = Runner;
        Runner = null;

        try
        {
            await runnerToShutdown.Shutdown(destroyGameObject: true);
        }
        finally
        {
            SetState(NetworkSessionState.Offline);
        }
    }

    private void SetState(NetworkSessionState newState)
    {
        if (State == newState)
            return;

        State = newState;
        StateChanged?.Invoke(State);
    }
}
using System;
using System.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity;
using Metaplay.Unity.DefaultIntegration;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#region client_listener
public partial class MetaplayService : IPlayerModelClientListener
{
    public static Action NotifyThemeEventChanged;

    public void ThemeEventChanged()
    {
        NotifyThemeEventChanged?.Invoke();
    }
}
#endregion client_listener

/// <summary>
/// Singleton service that wraps the Metaplay client that can be attached to a Unity GameObject.
/// Also acts as the listener for changes to the server connection state and the listener for any
/// game-specific player model events (none are used yet).
/// </summary>
#region class_definition
public partial class MetaplayService : MonoBehaviour, IMetaplayLifecycleDelegate, IPlayerModelClientListener
#endregion class_definition
{
    public UnityEvent ConnectionEstablished;

    async Task InitUnityServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            var options = new InitializationOptions()
                .SetEnvironmentName("production");

            await UnityServices.InitializeAsync(options);
        }
    }

    void Update()
    {
        // Update MetaplayClient on each frame.
        MetaplayClient.Update();
    }

    #region init_connection
    private async void OnEnable()
    {
        await InitUnityServices();

        // Don't destroy this GameObject when loading new scenes.
        DontDestroyOnLoad(gameObject);

        // Start connecting to the Metaplay server.
        InitConnection();
    }
    
    public void InitConnection()
    {
        if (MetaplayClient.IsInitialized)
        {
            MetaplayClient.Deinitialize();
        }
        
        MetaplayClient.Initialize(new MetaplayClientOptions
        {
            // Hook all the lifecycle and connectivity callbacks back to this class.
            LifecycleDelegate = this,

            IAPOptions = new MetaplayIAPOptions
            {
                EnableIAPManager = true,
            }

            // Check out the other members from MetaplayClientOptions,
            // these are optional but have useful settings 
            // or provide useful information.
        });

        MetaplayClient.Connect();
    }
#endregion init_connection

#region session_started
    /// <summary>
    /// Callback invoked when the connection to the Metaplay server is established.
    /// </summary>
    public Task OnSessionStartedAsync()
    {
        // Hook this class as the listener for game-specific player model events.
        MetaplayClient.PlayerModel.ClientListener = this;
        // Initialize and load the player state
        PlayerData.Create();
        // Game specific hook to trigger loading state transitions
        ConnectionEstablished?.Invoke();
        return Task.CompletedTask;
    }
#endregion session_started

    /// <summary>
    /// Callback invoked when the connection to the Metaplay server is lost.
    /// </summary>
    public void OnSessionLost(ConnectionLostEvent connectionLost)
    {
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }

    /// <summary>
    /// Callback invoked when the connection to the Metaplay server could not be established.
    /// </summary>
    public void OnFailedToStartSession(ConnectionLostEvent connectionLost)
    {
        MetaplayClient.Connect();
    }
}

using System.Collections.Generic;
using System.Linq;
using Game.Logic;
using Metaplay.Unity;
using Metaplay.Unity.DefaultIntegration;

#region metaplay_client
/// <summary>
/// Helper class to access the Metaplay client. Provides APIs that are statically typed
/// with the game-specific <see cref="PlayerModel"/>.
/// </summary>
public class MetaplayClient : MetaplayClientBase<PlayerModel> { }
#endregion metaplay_client
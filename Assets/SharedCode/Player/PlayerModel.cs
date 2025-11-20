// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using System.Collections.Generic;
using System.Linq;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using System.Runtime.Serialization;
using Game.Logic.Events;
using Game.Logic.GameConfigs;
using Metaplay.Core.InAppPurchase;

namespace Game.Logic
{
    [MetaSerializable]
    public class Run
    {
        [MetaMember(1)] public ConsumableType ConsumableUsed { get; set; }
        [MetaMember(2)] public string Theme { get; set; }
        [MetaMember(3)] public string Character { get; set; }
        [MetaMember(4)] public string Accessory { get; set; }
    }

#region player_data
    /// <summary>
    /// Class for storing the state and updating the logic for a single player.
    /// </summary>
    [MetaSerializableDerived(1)]
    public partial class PlayerModel :
        PlayerModelBase<
            PlayerModel,
            PlayerStatisticsCore
            >
    {
        // Game-specific state
        // MetaMember is Metaplay's serialization attribute, 
        // Each id should be unique within this class.
        [MetaMember(103)] public ClientPlayerData PlayerData { get; set; } // [!code ++]
    }
#endregion player_data

    /// <summary>
    /// Class for storing the state and updating the logic for a single player.
    /// </summary>
    [SupportedSchemaVersions(1, 1)]
    public partial class PlayerModel :
        PlayerModelBase<
            PlayerModel,
            PlayerStatisticsCore
            >
    {
        public const int TicksPerSecond = 1;
        protected override int GetTicksPerSecond() => TicksPerSecond;
        
        [IgnoreDataMember] public IPlayerModelServerListener ServerListener { get; set; } = EmptyPlayerModelServerListener.Instance;
        [IgnoreDataMember] public IPlayerModelClientListener ClientListener { get; set; } = EmptyPlayerModelClientListener.Instance;

        [IgnoreDataMember] public new SharedGameConfig GameConfig => GetGameConfig<SharedGameConfig>();

        // Player profile
        [MetaMember(100)] public sealed override EntityId           PlayerId    { get; set; }
        [MetaMember(101), NoChecksum] public sealed override string PlayerName  { get; set; }
        [MetaMember(102)] public sealed override int                PlayerLevel { get; set; }

        // Game-specific state
        [MetaMember(104)] public MetaTime LastPlayerDataSyncTime { get; set; }
        [MetaMember(105)] public List<CompletedRun> RunHistory { get; set; } = new List<CompletedRun>();      
        [MetaMember(106)] public bool OfflineStateMigrated { get; set; } = false;
        
        [MetaMember(107)] public Run CurrentRun { get; set; } = new Run();
        
        protected override void GameInitializeNewPlayerModel(MetaTime now, ISharedGameConfig gameConfig, EntityId playerId, string name)
        {
            // Setup initial state for new player
            PlayerId    = playerId;
            PlayerName  = name;

            PlayerData = new ClientPlayerData();
            PlayerData.UsedAccessory = -1;
            PlayerData.UsedTheme = "Day";
            PlayerData.Themes.Add("Day");
            PlayerData.Characters.Add("Trash Cat");
            PlayerData.TutorialDone = true;
        }

        // Currently active ThemeEvent states
        public IEnumerable<ThemeEventModel> ActiveThemeEvents => LiveOpsEvents.EventModels.Values
            .OfType<ThemeEventModel>().Where(x => x.Phase.IsActivePhase());        
        
        [MetaSerializableDerived(1)]
        public class ResolvedPurchasePremiumCurrency : ResolvedPurchaseContentBase
        {
            [MetaMember(1)]
            public int Amount { get; }
            
            [MetaDeserializationConstructor]
            public ResolvedPurchasePremiumCurrency(int amount)
            {
                Amount = amount;
            }
        }
        
        public override void OnClaimedInAppProduct(InAppPurchaseEvent ev, InAppProductInfoBase productInfoBase, out ResolvedPurchaseContentBase resolvedContent)
        {
            InAppProductInfo productInfo = (InAppProductInfo)productInfoBase;

            if (productInfo.NumPremium > 0)
            {
                PlayerData.NumPremium += productInfo.NumPremium;
                resolvedContent = new ResolvedPurchasePremiumCurrency(productInfo.NumPremium);
            }
            else
            {
                resolvedContent = null;
            }
        }
    }
}

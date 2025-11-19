// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using System;
using Game.Logic.Analytics;
using System.Collections.Generic;
using System.Linq;
using Game.Logic.Events;
using Game.Logic.GameConfigs;
using Game.Logic.Missions;
using Metaplay.Core.Forms;
using Metaplay.Core.Math;
using Metaplay.Core.Model;
using Metaplay.Core.Player;

namespace Game.Logic
{
    /// <summary>
    /// Game-specific player action class, which attaches all game-specific actions to <see cref="PlayerModel"/>.
    /// </summary>
    public abstract class PlayerAction : PlayerActionCore<PlayerModel>
    {
    }
    
    /// <summary>
    /// Game-specific <see cref="PlayerUnsynchronizedServerActionCore{TModel}"/>
    /// </summary>
    public abstract class PlayerUnsynchronizedServerAction : PlayerUnsynchronizedServerActionCore<PlayerModel>
    {
    }

    /// <summary>
    /// Game-specific <see cref="PlayerSynchronizedServerActionCore{TModel}"/>
    /// </summary>
    public abstract class PlayerSynchronizedServerAction : PlayerSynchronizedServerActionCore<PlayerModel>
    {
    }

    /// <summary>
    /// Registry for game-specific ActionCodes, used by the individual PlayerAction classes.
    /// </summary>
    public static class ActionCodes
    {
        public const int UpdateClientAuthoritativeState = 1;
        public const int MigrateStateFromOffline = 2;
        public const int PayOnly = 3;
        public const int BuyItem = 6;
        public const int BuyTheme = 7;
        public const int StartRun = 8;
        public const int EndRun = 9;
        public const int AdminGrantCurrency = 10;
        public const int DebugAddCurrency = 11;
        public const int DebugAddConsumable = 12;
        
    }

    /// <summary>
    /// Game-specific results returned from <see cref="PlayerActionCore.Execute(PlayerModel, bool)"/>.
    /// </summary>
    public static class ActionResult
    {
        // Shadow success result
        public static readonly MetaActionResult Success             = MetaActionResult.Success;

        // Game-specific results
        public static readonly MetaActionResult UnknownError      = new MetaActionResult(nameof(UnknownError));
        public static readonly MetaActionResult InsufficientFunds = new MetaActionResult(nameof(InsufficientFunds));
        public static readonly MetaActionResult NoCurrentRun      = new MetaActionResult(nameof(NoCurrentRun));
        public static readonly MetaActionResult AlreadyOwned      = new MetaActionResult(nameof(AlreadyOwned));
        public static readonly MetaActionResult InvalidPrice      = new MetaActionResult(nameof(InvalidPrice));
        public static readonly MetaActionResult InvalidItem       = new MetaActionResult(nameof(InvalidItem));
    }

    [ModelAction(ActionCodes.PayOnly)]
    public class PayOnlyAction : PlayerAction
    {
        public int PriceCoins { get; }
        public int PricePremium { get; }

        [MetaDeserializationConstructor]
        public PayOnlyAction(int priceCoins, int pricePremium)
        {
            PriceCoins = priceCoins;
            PricePremium = pricePremium;
        }

        public override MetaActionResult Execute(PlayerModel player, bool commit)
        {
            if (PriceCoins < 0 || PricePremium < 0)
                return ActionResult.InvalidPrice;
            
            int newCoins = player.PlayerData.NumCoins - PriceCoins;
            int newPremium = player.PlayerData.NumPremium - PricePremium;

            if (newCoins < 0 || newPremium < 0)
                return ActionResult.InsufficientFunds;
            
            if (commit)
            {
                player.PlayerData.NumCoins = newCoins;
                player.PlayerData.NumPremium = newPremium;
            }
            
            return MetaActionResult.Success;
        }
    }

    #region buy_item
    [ModelAction(ActionCodes.BuyItem)]
    public class BuyItemAction : PlayerAction
    {
        public ShopId ID { get; }

        [MetaDeserializationConstructor]
        public BuyItemAction(ShopId id)
        {
            ID = id;
        }

        public override MetaActionResult Execute(PlayerModel player, bool commit)
        {
            // Verify that the item the player is trying to purchase does actually exist
            if (!player.GameConfig.Shop.TryGetValue(ID, out var shopItem))
                return ActionResult.InvalidItem;
            
            int newCoins = player.PlayerData.NumCoins - shopItem.CoinCost;
            int newPremium = player.PlayerData.NumPremium - shopItem.PremiumCost;

            // Verify that the player can afford the item
            if (newCoins < 0 || newPremium < 0)
                return ActionResult.InsufficientFunds;

            if (commit)
            {
                // Purchase success! Subtract the funds and give the reward to the user
                player.PlayerData.NumCoins = newCoins;
                player.PlayerData.NumPremium = newPremium;
                
                shopItem.Reward.InvokeConsume(player, source: null);
                
                player.EventStream.Event(new ItemReceivedEvent(shopItem.Reward.Type, shopItem.Category.ToString(), 1, shopItem.CoinCost, shopItem.PremiumCost, Context.Store));
            }

            return MetaActionResult.Success;
        }
    }
    #endregion buy_item

    [ModelAction(ActionCodes.BuyTheme)]
    public class BuyThemeAction : PlayerAction
    {
        public string Theme { get; }
        public int PriceCoins { get; }
        public int PricePremium { get; }

        [MetaDeserializationConstructor]
        public BuyThemeAction(string theme, int priceCoins, int pricePremium)
        {
            Theme = theme;
            PriceCoins = priceCoins;
            PricePremium = pricePremium;
        }

        public override MetaActionResult Execute(PlayerModel player, bool commit)
        {
            int newCoins = player.PlayerData.NumCoins - PriceCoins;
            int newPremium = player.PlayerData.NumPremium - PricePremium;

            if (newCoins < 0 || newPremium < 0)
                return ActionResult.InsufficientFunds;

            if (player.PlayerData.Themes.Contains(Theme))
                return ActionResult.AlreadyOwned;

            if (commit)
            {
                player.PlayerData.NumCoins = newCoins;
                player.PlayerData.NumPremium = newPremium;
                player.PlayerData.Themes.Add(Theme);

                player.EventStream.Event(new ItemReceivedEvent(Theme, "Theme", 1, PriceCoins, PricePremium, Context.Store));
            }

            return MetaActionResult.Success;
        }
    }
    
#region start_run
    [ModelAction(ActionCodes.StartRun)]
    public class StartRunAction : PlayerAction
    {
        public ConsumableType ConsumableUsed { get; }
        public string Theme { get; }
        public string Character { get; }
        public string Accessory { get; }

        [MetaDeserializationConstructor]
        public StartRunAction(ConsumableType consumableUsed, string theme, string character, string accessory)
        {
            ConsumableUsed = consumableUsed;
            Theme = theme;
            Character = character;
            Accessory = accessory;
        }

        public override MetaActionResult Execute(PlayerModel player, bool commit)
        {
            if (commit)
            {
                if (ConsumableUsed != ConsumableType.NONE)
                    player.PlayerData.Consumables[ConsumableUsed] -= 1;
                player.CurrentRun = new Run() { ConsumableUsed = ConsumableUsed, Character = Character, Theme = Theme, Accessory = Accessory };
                
                player.EventStream.Event(new RunStartedEvent(Theme, Character, Accessory));
            }
            return MetaActionResult.Success;
        }
    }

#endregion start_run
#region end_run
    [ModelAction(ActionCodes.EndRun)]
    public class EndRunAction : PlayerAction
    {
        public bool DidUseConsumable { get; }
        public bool DidCompleteRun { get; }
        public string Character { get; }
        public string ObstacleTypeHit { get; }
        public string ThemeUsed { get; }
        public int CoinsGained { get; }
        public int PremiumGained { get; }
        public int Score { get; }
        public F64 WorldDistance { get; }
        
        [MetaDeserializationConstructor]
        public EndRunAction(
            bool didUseConsumable,
            bool didCompleteRun,
            string character,
            string obstacleTypeHit,
            string themeUsed,
            int coinsGained,
            int premiumGained,
            int score,
            F64 worldDistance)
        {
            DidUseConsumable = didUseConsumable;
            DidCompleteRun = didCompleteRun;
            Character = character;
            ObstacleTypeHit = obstacleTypeHit;
            ThemeUsed = themeUsed;
            CoinsGained = coinsGained;
            PremiumGained = premiumGained;
            Score = score;
            WorldDistance = worldDistance;
        }

        public EndRunAction(bool didUseConsumable)
        {
            DidUseConsumable = didUseConsumable;
            DidCompleteRun = false;
        }
        
        public override MetaActionResult Execute(PlayerModel player, bool commit)
        {
            if (player.CurrentRun == null)
                return ActionResult.NoCurrentRun;

            if (commit)
            {
                // Return the consumable to player inventory if it wasn't used
                if (!DidUseConsumable && player.CurrentRun.ConsumableUsed != ConsumableType.NONE)
                {
                    if (!player.PlayerData.Consumables.TryAdd(player.CurrentRun.ConsumableUsed, 1))
                        player.PlayerData.Consumables[player.CurrentRun.ConsumableUsed] += 1;
                }

                if (DidCompleteRun)
                {
                    // Add high score entry, only keep 10
                    HighscoreEntry newEntry = new HighscoreEntry()
                        { Name = Character, Score = Score };
                    int index = player.PlayerData.Highscores.BinarySearch(newEntry);
                    player.PlayerData.Highscores.Insert(index < 0 ? ~index : index, newEntry);
                    while (player.PlayerData.Highscores.Count > 10)
                        player.PlayerData.Highscores.RemoveAt(player.PlayerData.Highscores.Count - 1);

                    // Rewards from run
                    player.PlayerData.NumCoins += CoinsGained;
                    player.PlayerData.NumPremium += PremiumGained;

                    // Update player rank based on distance traveled
                    int gainedRank = F64.FloorToInt(WorldDistance) / 300;
                    if (gainedRank > player.PlayerData.Rank)
                    {
                        player.PlayerData.Rank = gainedRank;
                        player.EventStream.Event(new RankUpEvent(gainedRank));
                    }

                    CompletedRun historyEntry = new CompletedRun()
                    {
                        Timestamp = player.CurrentTime,
                        Character = Character,
                        ObstacleTypeHit = ObstacleTypeHit,
                        ThemeUsed = ThemeUsed,
                        CoinsGained = CoinsGained,
                        PremiumGained = PremiumGained,
                        Score = Score,
                        WorldDistance = WorldDistance,
                    };
                    player.RunHistory.Add(historyEntry);
                    player.EventStream.Event(new RunEndedEvent(historyEntry));

                    #region increment_run_and_reward
                    foreach (var model in player.LiveOpsEvents.EventModels.Values.OfType<ThemeEventModel>().Where(x=>x.Phase.IsActivePhase()))
                    {
                        model.IncrementRunAndReward(player, ThemeUsed);
                    }
                    #endregion increment_run_and_reward
                }

                player.CurrentRun = null;
                
            }
            return MetaActionResult.Success;
        }
    }
    #endregion end_run

    [ModelAction(ActionCodes.DebugAddConsumable), DevelopmentOnlyAction]
    public class DebugAddConsumableAction : PlayerAction
    {
        public ConsumableType Type { get; }
        public int Amount { get; }

        [MetaDeserializationConstructor]
        public DebugAddConsumableAction(ConsumableType type, int amount)
        {
            Type = type;
            Amount = amount;
        }

        public override MetaActionResult Execute(PlayerModel player, bool commit)
        {
            int currentAmount = player.PlayerData.Consumables.TryGetValue(Type, out var existingAmount) ? existingAmount : 0;
            int newAmount = currentAmount + Amount;

            if (commit)
            {
                player.EventStream.Event(new ItemReceivedEvent(Type.ToString(), "Consumable", Amount, 0, 0, Context.Cheat));
                
                if (newAmount > 0)
                    player.PlayerData.Consumables.AddOrReplace(Type, newAmount);
                else
                    player.PlayerData.Consumables.Remove(Type);
            }

            return MetaActionResult.Success;
        }
    }

    [ModelAction(ActionCodes.DebugAddCurrency), DevelopmentOnlyAction]
    public class DebugAddCurrencyAction : PlayerAction
    {
        public int Coins { get; }
        public int Premium { get; }

        [MetaDeserializationConstructor]
        public DebugAddCurrencyAction(int coins, int premium)
        {
            Coins = coins;
            Premium = premium;
        }

        public override MetaActionResult Execute(PlayerModel player, bool commit)
        {
            if (commit)
            {
                player.PlayerData.NumCoins += Coins;
                player.PlayerData.NumPremium += Premium;
                
                if (Coins > 0)
                    player.EventStream.Event(new CurrencyGainedEvent(CurrencyType.Soft, Coins, Context.Cheat));
                if (Premium > 0)
                    player.EventStream.Event(new CurrencyGainedEvent(CurrencyType.Premium, Premium, Context.Cheat));
            }

            return MetaActionResult.Success;
        }
    }

    [ModelAction(ActionCodes.AdminGrantCurrency)]
    [PlayerDashboardAction("Grant Currency", "Immediately adds additional currency to the player", AdminActionPlacement.Gentle, confirmLabel: "Add")]
    public class AdminGrantCurrency : PlayerSynchronizedServerAction
    {
        [MetaFormRange(0, Double.MaxValue, 1)]
        [MetaFormDisplayProps(displayName: "Fishbones")]
        public int NumSoft { get; private set; }
        [MetaFormRange(0, Double.MaxValue, 1)]
        [MetaFormDisplayProps(displayName: "Premium")]
        public int NumPremium { get; private set; }

        [MetaDeserializationConstructor]
        public AdminGrantCurrency(int numSoft, int numPremium)
        {
            NumSoft = numSoft;
            NumPremium = numPremium;
        }

        public override MetaActionResult Execute(PlayerModel player, bool commit)
        {
            if (commit)
            {
                player.PlayerData.NumCoins += NumSoft;
                player.PlayerData.NumPremium += NumPremium;
            }
            
            return MetaActionResult.Success;
        }
    }
    
#region migrate_state
    [ModelAction(ActionCodes.MigrateStateFromOffline)]
    public class MigrateStateFromOfflineAction : PlayerAction
    {
        public ClientPlayerData OfflinePlayerData { get; set; }

        public override MetaActionResult Execute(PlayerModel player, bool commit)
        {
            // Only allow migration if not already migrated
            if (player.OfflineStateMigrated)
                return ActionResult.UnknownError;

            if (commit)
            {
                if (OfflinePlayerData != null)
                    player.PlayerData = OfflinePlayerData;
                player.OfflineStateMigrated = true;
            }

            return MetaActionResult.Success;
        }
    }
#endregion migrate_state

#region update_client_state
    [ModelAction(ActionCodes.UpdateClientAuthoritativeState)]
    public class UpdateClientAuthoritativeStateAction : PlayerAction
    {
        public bool LicenseAccepted { get; set; }
        public bool TutorialDone { get; set; }
        public string UsedTheme { get; set; }
        public int UsedCharacter { get; set; }
        public int UsedAccessory { get; set; }
        public int FTUELevel { get; set; }
        public string PreviousName { get; set; }
        public List<PersistedMission> Missions { get; set; }
        
        public override MetaActionResult Execute(PlayerModel player, bool commit)
        {
            if (commit)
            {
                player.PlayerData.LicenseAccepted = LicenseAccepted;
                player.PlayerData.TutorialDone = TutorialDone;
                player.PlayerData.UsedTheme = UsedTheme;
                player.PlayerData.UsedCharacter = UsedCharacter;
                player.PlayerData.UsedAccessory = UsedAccessory;
                player.PlayerData.FTUELevel = FTUELevel;
                player.PlayerData.PreviousName = PreviousName;
                player.PlayerData.Missions = Missions;
                player.LastPlayerDataSyncTime = player.CurrentTime;
            }
            
            return MetaActionResult.Success;
        }
    }
#endregion update_client_state
}

using Game.Logic.Events;
using Metaplay.Core.Analytics;
using Metaplay.Core.Math;
using Metaplay.Core.Model;
using Metaplay.Core.Player;

namespace Game.Logic.Analytics
{
    public class PlayerEventCodes
    {
        public const int RunStarted = 1;
        public const int RankUp = 2;
        public const int ItemReceived = 3;
        public const int CurrencyGained = 4;
        public const int RunEnded = 5;
    }

    [MetaSerializable]
    public enum CurrencyType
    {
        Soft,
        Premium
    }
    
    [MetaSerializable]
    public enum Context
    {
        Store,
        Game,
        GameOver,
        Cheat,
        Mission
    }
    
    [AnalyticsEvent(PlayerEventCodes.RunStarted, displayName: "Run Started", docString: "Emitted when a run has started")]
    public class RunStartedEvent : PlayerEventBase
    {
        [MetaMember(1)] public string   Theme  { get; private set; }
        [MetaMember(2)] public string   CharacterName  { get; private set; }
        [MetaMember(3)] public string   Accessory  { get; private set; }

        [MetaDeserializationConstructor]
        public RunStartedEvent(string theme, string characterName, string accessory)
        {
            Theme = theme;
            CharacterName = characterName;
            Accessory = accessory;
        }

        public override string EventDescription => $"Run Started with {CharacterName} ({Accessory}), in {Theme}";
    }   
    
    [AnalyticsEvent(PlayerEventCodes.RunEnded, displayName: "Run Ended", docString: "Emitted when a run has ended")]
    public class RunEndedEvent : PlayerEventBase
    {
        [MetaMember(1)] public CompletedRun Data { get; private set; }

        [MetaDeserializationConstructor]
        public RunEndedEvent(CompletedRun data)
        {
            Data = data;
        }

        public override string EventDescription => $"Run Ended with running into a {Data.ObstacleTypeHit} for {Data.Score} points, " +
                                                   $"{Data.PremiumGained} premium, {Data.CoinsGained} soft currency, and {F64.Round(Data.WorldDistance)}m distance.";
    }   
    
    [AnalyticsEvent(PlayerEventCodes.RankUp, displayName: "Rank Up", docString: "Emitted when the player reaches a new rank")]
    public class RankUpEvent : PlayerEventBase
    {
        [MetaMember(1)] public int NewRank  { get; private set; }

        [MetaDeserializationConstructor]
        public RankUpEvent(int newRank)
        {
            NewRank = newRank;           
        }

        public override string EventDescription => $"Player reached rank {NewRank}";
    }
    
    [AnalyticsEvent(PlayerEventCodes.CurrencyGained, displayName: "Currency Gained", docString: "Emitted when the purchases an item in the store")]
    public class CurrencyGainedEvent : PlayerEventBase
    {
        [MetaMember(1)]
        public CurrencyType Type { get; private set; }
        [MetaMember(2)]
        public int Quantity { get; private set; }
        
        [MetaMember(3)]
        public Context Context { get; }

        [MetaDeserializationConstructor]
        public CurrencyGainedEvent(CurrencyType type, int quantity, Context context)
        {
            Type = type;
            Context = context;
            Quantity = quantity;
        }

        public override string EventDescription => $"Player gained {Quantity}x {Type} currency from {Context}.";
    }
    
    [AnalyticsEvent(PlayerEventCodes.ItemReceived, displayName: "Item Received", docString: "Emitted when the player receives an item")]
    public class ItemReceivedEvent : PlayerEventBase
    {
        [MetaMember(1)]
        public string ItemId { get; private set; }
        [MetaMember(2)]
        public string ItemType { get; private set; }
        [MetaMember(3)]
        public int ItemQuantity { get; private set; }
        [MetaMember(4)]
        public int PremiumCost { get; private set; }
        [MetaMember(5)]
        public int SoftCost { get; private set; }
        
        [MetaMember(6)]
        public Context Context { get; }

        [MetaDeserializationConstructor]
        public ItemReceivedEvent(string itemId, string itemType, int itemQuantity, int premiumCost, int softCost, Context context)
        {
            ItemId = itemId;
            ItemType = itemType;
            PremiumCost = premiumCost;
            SoftCost = softCost;
            Context = context;
            ItemQuantity = itemQuantity;
        }

        public override string EventDescription => $"Player {(SoftCost > 0 || PremiumCost > 0 ? "purchased" : "received")} {ItemQuantity}x {ItemId} in the {Context} for {PremiumCost} premium and {SoftCost} soft currency.";
    }
}
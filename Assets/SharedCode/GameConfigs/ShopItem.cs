using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;
using Metaplay.Core.Rewards;

namespace Game.Logic.GameConfigs
{
    [MetaSerializable]
    public enum Category
    {
        Consumable,
        Character,
        Accessory
    }
    
    #region shop_item
    [MetaSerializable]
    public class ShopId : StringId<ShopId> {}
    
    [MetaSerializable]
    public class ShopItem : IGameConfigData<ShopId>
    {
        [MetaMember(1)]
        public ShopId ConfigKey { get; private set; }
        
        [MetaMember(2)]
        public Category Category { get; private set; }
        
        [MetaMember(3)]
        public PlayerReward Reward { get; private set; }
        
        [MetaMember(4)]
        public int PremiumCost { get; private set; }
        
        [MetaMember(5)]
        public int CoinCost { get; private set; }
    }
    #endregion shop_item    

    public abstract class PlayerReward : MetaPlayerReward<PlayerModel>
    {
        public abstract string Type { get; }
    }
    
    [MetaSerializableDerived(1)]
    public class ConsumableReward : PlayerReward
    {
        [MetaMember(1)]
        public ConsumableType ConsumableType { get; }

        public sealed override string Type => ConsumableType.ToString();

        [MetaMember(2)]
        public int Amount { get; }

        [MetaDeserializationConstructor]
        public ConsumableReward(ConsumableType consumableType, int amount)
        {
            ConsumableType = consumableType;
            Amount = amount;
        }

        public override void Consume(PlayerModel playerModel, IRewardSource source)
        {
            playerModel.PlayerData.Consumables.TryAdd(ConsumableType, 0);

            playerModel.PlayerData.Consumables[ConsumableType] += Amount;
        }
    }
    
    [MetaSerializableDerived(2)]
    public class CharacterReward : PlayerReward
    {
        [MetaMember(1)]
        public sealed override string Type { get; }

        [MetaDeserializationConstructor]
        public CharacterReward(string type)
        {
            Type = type;
        }

        public override void Consume(PlayerModel playerModel, IRewardSource source)
        {
            if (!playerModel.PlayerData.Characters.Contains(Type))
                playerModel.PlayerData.Characters.Add(Type);
        }
    }
    [MetaSerializableDerived(3)]
    public class AccessoryReward : PlayerReward
    {
        public sealed override string Type { get; }
        [MetaMember(1)]
        public string CharacterType { get; }
        
        [MetaMember(2)]
        public string AccessoryType { get; }
        
        [MetaDeserializationConstructor]
        public AccessoryReward(string characterType, string accessoryType)
        {
            CharacterType = characterType;
            AccessoryType = accessoryType;
        }
        
        public override void Consume(PlayerModel playerModel, IRewardSource source)
        {
            if (!playerModel.PlayerData.CharacterAccessories.Contains(Type))
                playerModel.PlayerData.CharacterAccessories.Add($"{CharacterType}:{AccessoryType}");
        }
    }
    
    public class ShopRewardConfigParsers : ConfigParserProvider
    {
        // Callback that is invoked during initialization
        public override void RegisterParsers(ConfigParser parser)
        {
            // Register a parsing method for the custom PlayerReward type
            parser.RegisterCustomParseFunc<PlayerReward>(ParsePlayerReward);
        }

        // Method for parsing a PlayerReward type
        static MetaReward ParsePlayerReward(ConfigParser parser, ConfigLexer lexer)
        {
            int amount = 1;
            if (lexer.CurrentToken.Type == ConfigLexer.TokenType.IntegerLiteral)
            {
                amount = lexer.ParseIntegerLiteral();    
            }
            
            string rewardType = lexer.ParseIdentifierOrString();

            switch (rewardType)
            {
                case "Magnet":
                    return new ConsumableReward(ConsumableType.COIN_MAG, amount);
                case "Multiplier":
                    return new ConsumableReward(ConsumableType.SCORE_MULTIPLAYER, amount);
                case "Invincible":
                    return new ConsumableReward(ConsumableType.INVINCIBILITY, amount);
                case "Life":
                    return new ConsumableReward(ConsumableType.EXTRALIFE, amount);

                case "Trash Cat":
                case "Rubbish Raccoon":
                    if (lexer.CurrentToken.Type == ConfigLexer.TokenType.ForwardSlash)
                    {
                        lexer.ParseToken(ConfigLexer.TokenType.ForwardSlash);
                        var accessory = lexer.ParseIdentifierOrString();
                        return new AccessoryReward(rewardType, accessory);
                    }
                    return new CharacterReward(rewardType);

                default:
                    throw new ParseError($"Unhandled PlayerReward type in config: {rewardType}");
            }
        }
    }
}
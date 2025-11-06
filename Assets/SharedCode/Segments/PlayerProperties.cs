using System;
using System.Linq;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;
using Metaplay.Core.Player;

namespace Game.Logic.Segments
{
    [MetaSerializableDerived(1)]
    public class RankProperty : TypedPlayerPropertyId<int>
    {
        public override string DisplayName => "Player Rank";
        public override int GetTypedValueForPlayer(IPlayerModelBase player) => ((PlayerModel)player).PlayerData.Rank;
    }

    [MetaSerializableDerived(2)]
    public class SessionsPerDayProperty : TypedPlayerPropertyId<F32>
    {
        public override string DisplayName => "Avg Sessions Per Day";
        public override F32 GetTypedValueForPlayer(IPlayerModelBase player)
        {
            if (player.LoginHistory.Count == 0)
                return F32.Zero;
            
            return F32.FromDouble(player.LoginHistory.GroupBy(x => x.Timestamp.ToDateTime().Date).Average(x => x.Count()));
        }
    }

    [MetaSerializableDerived(3)]
    public class SessionDurationProperty : TypedPlayerPropertyId<MetaDuration>
    {
        public override string DisplayName => "Avg Session Duration";
        public override MetaDuration GetTypedValueForPlayer(IPlayerModelBase player)
        {
            if (player.LoginHistory.Count == 0)
                return MetaDuration.Zero;
            
            return MetaDuration.FromMilliseconds((long)player.LoginHistory.Where(x => x.SessionLengthApprox.HasValue).Average(x => x.SessionLengthApprox.Value.Milliseconds));
        }
    }

    #region soft_currency
    [MetaSerializableDerived(4)]
    public class SoftCurrencyProperty : TypedPlayerPropertyId<int>
    {
        public override string DisplayName => "Soft Currency";
        public override int GetTypedValueForPlayer(IPlayerModelBase player) => ((PlayerModel)player).PlayerData.NumCoins;
    }
    #endregion soft_currency
    
    [MetaSerializableDerived(5)]
    public class PremiumCurrencyProperty : TypedPlayerPropertyId<int>
    {
        public override string DisplayName => "Premium Currency";
        public override int GetTypedValueForPlayer(IPlayerModelBase player) => ((PlayerModel)player).PlayerData.NumPremium;
    }

    [MetaSerializableDerived(6)]
    public class TimeSinceLastLoginProperty : TypedPlayerPropertyId<MetaDuration>
    {
        public override string DisplayName => "Time Since Last Login";

        public override MetaDuration GetTypedValueForPlayer(IPlayerModelBase player)
        {
            return MetaTime.Now - player.Stats.LastLoginAt;
        }
    }

    #region run_history
    [MetaSerializableDerived(7)]
    public class RunsWithThemeProperty : TypedPlayerPropertyId<int>
    {
        public override string DisplayName => $"Number of completed runs with theme {Theme}";

        [MetaMember(1)] public string Theme { get; set; }

        public override int GetTypedValueForPlayer(IPlayerModelBase player)
        {
            PlayerModel playerModel = (PlayerModel)player;
            return playerModel.RunHistory.Count(x => x.ThemeUsed == Theme);
        }
    }
    #endregion run_history

    public class ConfigParsers : ConfigParserProvider
    {
        public override void RegisterParsers(ConfigParser parser)
        {
            parser.RegisterCustomParseFunc<PlayerPropertyId>(ParsePlayerPropertyId);
        }

        static PlayerPropertyId ParsePlayerPropertyId(ConfigLexer lexer)
        {
            // We need this to parse SDK-defined PlayerPropertyId types.
            if (ConfigParser.TryParseCorePlayerPropertyId(lexer, out PlayerPropertyId propertyId))
                return propertyId;

            // Game-specific PlayerPropertyIds.

            string type = lexer.ParseIdentifier();
            var type1 = Type.GetType("Game.Logic.Segments." + type + "Property");
            if (type1 != null)
            {
                if (type1 == typeof(RunsWithThemeProperty))
                {
                    lexer.ParseToken(ConfigLexer.TokenType.ForwardSlash);
                    string theme = lexer.ParseIdentifier();
                    return new RunsWithThemeProperty() { Theme = theme };
                }

                return Activator.CreateInstance(type1) as PlayerPropertyId;
            }

            throw new ParseError($"Invalid PlayerPropertyId in config: {type}");
        }
    }
}
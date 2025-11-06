using Metaplay.Core.Config;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic.GameConfigs
{
    #region snippet
    [MetaSerializable]
    public class TrackConfig : GameConfigKeyValue<TrackConfig>
    {
        [MetaMember(1)]
        public F32 EvaluationInterval { get; private set; }
        [MetaMember(2)]
        public int MinPowerUpInterval { get; private set; }
        [MetaMember(3)]
        public int MaxPowerUpInterval { get; private set; }
        [MetaMember(4)]
        public int MinPremiumInterval { get; private set; }
        [MetaMember(5)]
        public int MaxPremiumInterval { get; private set; }
    }
    #endregion snippet
}
using Metaplay.Core.Config;
using Metaplay.Core.Player;

namespace Game.Logic.GameConfigs
{
    #region serverconfig
    public class ServerGameConfig : ServerGameConfigBase
    {
        [GameConfigEntry(PlayerExperimentsEntryName)]
        public GameConfigLibrary<PlayerExperimentId, PlayerExperimentInfo> PlayerExperiments { get; private set; }
    }
    #endregion serverconfig
}
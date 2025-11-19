using Metaplay.Core.Config;
using Metaplay.Core.Player;

namespace Game.Logic.GameConfigs
{
    #region serverconfig
    public class ServerGameConfig : ServerGameConfigBase
    {
        [GameConfigEntry(PlayerExperimentsEntryName)] // [!code ++]
        public GameConfigLibrary<PlayerExperimentId, PlayerExperimentInfo> PlayerExperiments { get; private set; } // [!code ++]
    }
    #endregion serverconfig
}
using System.Collections.Generic;
using Metaplay.Core.Config;

namespace Game.Logic.GameConfigs
{
    /// <summary>
    /// This is the integration hook for Game config and localization builds.
    /// We recommend implementing <see cref="GetAvailableLocalizationsBuildSources"/> and
    /// <see cref="GetAvailableGameConfigBuildSources"/> to enable dashboard builds and share sources in the project.
    /// </summary>
    public class GameGameConfigBuildIntegration : GameConfigBuildIntegration
    {
        public override IEnumerable<GameConfigBuildSource> GetAvailableLocalizationsBuildSources(string sourcePropertyInBuildParams)
        {
            // Return the predefined build sources here, for example:
            // return new GameConfigBuildSource[] { new GoogleSheetBuildSource("Development", "SPREADSHEET_ID") };
            // Note that Google Sheets require going through the setup https://docs.metaplay.io/feature-cookbooks/game-configs/implementing-google-sheets-integration.html
            return base.GetAvailableLocalizationsBuildSources(sourcePropertyInBuildParams);
        }

        public override IEnumerable<GameConfigBuildSource> GetAvailableGameConfigBuildSources(string sourcePropertyInBuildParams)
        {
            return new GameConfigBuildSource[] { new GoogleSheetBuildSource("Development", "1EcIUSPESWHPetCbzs8lug1F1miqf7uZ_mA0PZ6ObRmk") };
        }
    }
}
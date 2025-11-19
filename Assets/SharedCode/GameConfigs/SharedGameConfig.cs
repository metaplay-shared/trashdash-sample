using Metaplay.Core.Config;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Player;

namespace Game.Logic.GameConfigs
{
#if false
#region sharedconfig_trackconfig
    public class SharedGameConfig : SharedGameConfigBase
    {
        [GameConfigEntry("TrackConfig")] // [!code ++]
        public TrackConfig TrackConfig { get; private set; } // [!code ++]
    }
#endregion sharedconfig_trackconfig
#region sharedconfig_shop
    public class SharedGameConfig : SharedGameConfigBase
    {
        [GameConfigEntry("TrackConfig")]
        public TrackConfig TrackConfig { get; private set; }

        [GameConfigEntry("Shop")] // [!code ++]
        public GameConfigLibrary<ShopId, ShopItem> Shop { get; private set; } // [!code ++]
    }
#endregion sharedconfig_shop
#region sharedconfig_segments
    public class SharedGameConfig : SharedGameConfigBase
    {
        [GameConfigEntry("TrackConfig")]
        public TrackConfig TrackConfig { get; private set; }

        [GameConfigEntry("Shop")]
        public GameConfigLibrary<ShopId, ShopItem> Shop { get; private set; }
        
        [GameConfigEntry("PlayerSegments")] // [!code ++]
        [GameConfigEntryTransform(typeof(DefaultPlayerSegmentBasicInfoSourceItem))] // [!code ++] 
        public GameConfigLibrary<PlayerSegmentId, DefaultPlayerSegmentInfo> PlayerSegments { get; private set; }      // [!code ++]
    }
#endregion sharedconfig_segments
#endif

#region sharedconfig
    public partial class SharedGameConfig : SharedGameConfigBase
    {        
        [GameConfigEntry("TrackConfig")]
        public TrackConfig TrackConfig { get; private set; }

        [GameConfigEntry("Shop")]
        public GameConfigLibrary<ShopId, ShopItem> Shop { get; private set; }
        
        [GameConfigEntry("PlayerSegments")]
        [GameConfigEntryTransform(typeof(DefaultPlayerSegmentBasicInfoSourceItem))]
        public GameConfigLibrary<PlayerSegmentId, DefaultPlayerSegmentInfo> PlayerSegments { get; private set; }        

        [GameConfigEntry("InAppProducts")] 
        public GameConfigLibrary<InAppProductId, InAppProductInfo> InAppProducts { get; private set; }
    }
    #endregion sharedconfig
}

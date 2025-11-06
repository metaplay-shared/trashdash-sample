using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Model;

namespace Game.Logic.GameConfigs
{
    [MetaSerializableDerived(1)]
    public class InAppProductInfo : InAppProductInfoBase
    {
        [MetaMember(1)] public int NumPremium;
    }
}
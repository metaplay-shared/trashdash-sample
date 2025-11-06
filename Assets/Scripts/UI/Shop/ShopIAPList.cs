using Game.Logic.GameConfigs;
using Metaplay.Core.InAppPurchase;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UI.Shop
{
    public class ShopIAPList : ShopList
    {
        public override void Populate()
        {
            m_RefreshCallback = null;
            foreach (Transform t in listRoot)
            {
                Destroy(t.gameObject);
            }

            if (!MetaplayClient.IAPManager.StoreIsAvailable)
                return;

            foreach (var (key, value) in MetaplayClient.PlayerModel.GameConfig.InAppProducts)
            {
                prefabItem.InstantiateAsync().Completed += (op) =>
                {
                    if (op.Result == null || !(op.Result is GameObject))
                    {
                        Debug.LogWarning(string.Format("Unable to load theme shop list {0}.", prefabItem.Asset.name));
                        return;
                    }
                    GameObject newEntry = op.Result;
                    newEntry.transform.SetParent(listRoot, false);

                    ShopItemListItem itm = newEntry.GetComponent<ShopItemListItem>();

                    itm.nameText.text = value.Name;
                    itm.pricetext.text = "$" + value.Price;

                    itm.countText.text = "x" + value.NumPremium;
                    
                    itm.buyButton.onClick.AddListener(delegate() { Buy(value.ProductId); });

                    itm.buyButton.image.sprite = itm.buyButtonSprite;
                };
            }
        }

        private void Buy(InAppProductId product)
        {
            if (MetaplayClient.IAPFlowTracker.PurchaseFlowIsOngoing(product) ||
                !MetaplayClient.IAPManager.StoreProductIsAvailable(product))
                return;
            
            MetaplayClient.IAPManager.TryBeginPurchaseProduct(product);
            
            Refresh();
        }
    }
}
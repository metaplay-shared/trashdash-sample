using System.Collections.Generic;
using System.Linq;
using Game.Logic;
using Game.Logic.GameConfigs;
using Metaplay.Core.Model;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif

public class ShopItemList : ShopList
{
    static public ConsumableType[] s_ConsumablesTypes = System.Enum.GetValues(typeof(ConsumableType)) as ConsumableType[];

    #region populate_shop
    public override void Populate()
    {
        m_RefreshCallback = null;
        foreach (Transform t in listRoot)
        {
            Destroy(t.gameObject);
        }

        // foreach (var (key, value) in Shop.Instance.Consumables) 
        foreach (var (key, value) in MetaplayClient.PlayerModel.GameConfig.Shop.Where(x => x.Value.Category == Category.Consumable)) // [!code ++]
        {
            SpawnShopItem(value, key);
        }
    }
    #endregion populate_shop

    private void SpawnShopItem(ShopItem value, ShopId key)
    {
        if (value.Reward is ConsumableReward reward)
        {
            Consumable c = ConsumableDatabase.GetConsumbale(reward.ConsumableType);
            if (c != null)
            {
                prefabItem.InstantiateAsync().Completed += (op) =>
                {
                    if (op.Result == null || !(op.Result is GameObject))
                    {
                        Debug.LogWarning(string.Format("Unable to load item shop list {0}.", prefabItem.RuntimeKey));
                        return;
                    }

                    GameObject newEntry = op.Result;
                    newEntry.transform.SetParent(listRoot, false);

                    ShopItemListItem itm = newEntry.GetComponent<ShopItemListItem>();

                    itm.buyButton.image.sprite = itm.buyButtonSprite;

                    itm.nameText.text = c.GetConsumableName();
                    itm.pricetext.text = value.CoinCost.ToString();

                    if (value.PremiumCost > 0)
                    {
                        itm.premiumText.transform.parent.gameObject.SetActive(true);
                        itm.premiumText.text = value.PremiumCost.ToString();
                    }
                    else
                    {
                        itm.premiumText.transform.parent.gameObject.SetActive(false);
                    }

                    itm.icon.sprite = c.icon;

                    itm.countText.gameObject.SetActive(true);

                    itm.buyButton.onClick.AddListener(delegate() { Buy(c, key); });
                    m_RefreshCallback += delegate() { RefreshButton(itm, c, value); };
                    RefreshButton(itm, c, value);
                };
            }
        }
    }

    protected void RefreshButton(ShopItemListItem itemList, Consumable c, ShopItem value)
    {
        int count = 0;
        PlayerData.instance.consumables.TryGetValue(c.GetConsumableType(), out count);
        itemList.countText.text = count.ToString();

        if (value.CoinCost > PlayerData.instance.coins)
        {
            itemList.buyButton.interactable = false;
            itemList.pricetext.color = Color.red;
        }
        else
        {
            itemList.pricetext.color = Color.black;
        }

        if (value.PremiumCost > PlayerData.instance.premium)
        {
            itemList.buyButton.interactable = false;
            itemList.premiumText.color = Color.red;
        }
        else
        {
            itemList.premiumText.color = Color.black;
        }
    }

    public void Buy(Consumable c, ShopId id)
    {
        if (!ExecuteBuyItem(id)) return;

#if UNITY_ANALYTICS // Using Analytics Standard Events v0.3.0
        var transactionId = System.Guid.NewGuid().ToString();
        var transactionContext = "store";
        var level = PlayerData.instance.rank.ToString();
        var itemId = c.GetConsumableName();
        var itemType = "consumable";
        var itemQty = 1;

        AnalyticsEvent.ItemAcquired(
            AcquisitionType.Soft,
            transactionContext,
            itemQty,
            itemId,
            itemType,
            level,
            transactionId
        );

        var shopItem = MetaplayClient.PlayerModel.GameConfig.Shop[id];

        if (shopItem.CoinCost > 0)
        {
            AnalyticsEvent.ItemSpent(
                AcquisitionType.Soft, // Currency type
                transactionContext,
                shopItem.CoinCost,
                itemId,
                PlayerData.instance.coins, // Balance
                itemType,
                level,
                transactionId
            );
        }

        if (shopItem.PremiumCost > 0)
        {
            AnalyticsEvent.ItemSpent(
                AcquisitionType.Premium, // Currency type
                transactionContext,
                shopItem.PremiumCost,
                itemId,
                PlayerData.instance.premium, // Balance
                itemType,
                level,
                transactionId
            );
        }
#endif

        Refresh();
    }

    #region buy_item
    private static bool ExecuteBuyItem(ShopId id)
    {
        BuyItemAction action = new BuyItemAction(id);
        return MetaplayClient.PlayerContext.ExecuteAction(action).IsSuccess;
    }
    #endregion buy_item
}
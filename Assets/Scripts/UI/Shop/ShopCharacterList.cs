using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.Logic;
using Game.Logic.Analytics;
using Game.Logic.GameConfigs;
using UnityEngine.AddressableAssets;

#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif

public class ShopCharacterList : ShopList
{
    public override void Populate()
    {
		m_RefreshCallback = null;
        foreach (Transform t in listRoot)
        {
            Destroy(t.gameObject);
        }


        foreach (var (key, value) in MetaplayClient.PlayerModel.GameConfig.Shop.Where(x => x.Value.Category == Category.Character))
        {
	        if (CharacterDatabase.dictionary.TryGetValue(value.Reward.Type, out Character c))
	        {
                prefabItem.InstantiateAsync().Completed += (op) =>
                {
                    if (op.Result == null || !(op.Result is GameObject))
                    {
                        Debug.LogWarning(string.Format("Unable to load character shop list {0}.", prefabItem.Asset.name));
                        return;
                    }
                    GameObject newEntry = op.Result;
                    newEntry.transform.SetParent(listRoot, false);

                    ShopItemListItem itm = newEntry.GetComponent<ShopItemListItem>();

                    itm.icon.sprite = c.icon;
                    itm.nameText.text = c.characterName;
                    itm.pricetext.text = value.CoinCost.ToString();

                    itm.buyButton.image.sprite = itm.buyButtonSprite;

                    if (value.PremiumCost > 0)
                    {
                        itm.premiumText.transform.parent.gameObject.SetActive(true);
                        itm.premiumText.text = value.PremiumCost.ToString();
                    }
                    else
                    {
                        itm.premiumText.transform.parent.gameObject.SetActive(false);
                    }

                    itm.buyButton.onClick.AddListener(delegate() { Buy(c, key); });

                    m_RefreshCallback += delegate() { RefreshButton(itm, c, key); };
                    RefreshButton(itm, c, key);
                };
            }
        }
    }

	protected void RefreshButton(ShopItemListItem itm, Character c, ShopId key)
	{
		var shopItem = MetaplayClient.PlayerModel.GameConfig.Shop[key];
		if (shopItem.CoinCost > PlayerData.instance.coins)
		{
			itm.buyButton.interactable = false;
			itm.pricetext.color = Color.red;
		}
		else
		{
			itm.pricetext.color = Color.black;
		}

		if (shopItem.PremiumCost > PlayerData.instance.premium)
		{
			itm.buyButton.interactable = false;
			itm.premiumText.color = Color.red;
		}
		else
		{
			itm.premiumText.color = Color.black;
		}

		if (PlayerData.instance.characters.Contains(c.characterName))
		{
			itm.buyButton.interactable = false;
			itm.buyButton.image.sprite = itm.disabledButtonSprite;
			itm.buyButton.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Owned";
		}
	}

	public void Buy(Character c, ShopId key)
	{
		if (!MetaplayClient.PlayerContext.ExecuteAction(new BuyItemAction(key)).IsSuccess)
			return;
        PlayerData.instance.Save();

        var shopItem = MetaplayClient.PlayerModel.GameConfig.Shop[key];
        
#if UNITY_ANALYTICS // Using Analytics Standard Events v0.3.0
        var transactionId = System.Guid.NewGuid().ToString();
        var transactionContext = "store";
        var level = PlayerData.instance.rank.ToString();
        var itemId = c.characterName;
        var itemType = "non_consumable";
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

        // Repopulate to change button accordingly.
        Populate();
    }
}

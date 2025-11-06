using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.Logic;
using Game.Logic.Analytics;
using Game.Logic.GameConfigs;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif

public class ShopAccessoriesList : ShopList
{
    public AssetReference headerPrefab;

    List<Character> m_CharacterList = new List<Character>();
    public override void Populate()
    {
		m_RefreshCallback = null;

        foreach (Transform t in listRoot)
        {
            Destroy(t.gameObject);
        }

        m_CharacterList.Clear();

        foreach (KeyValuePair<string, Character> pair in CharacterDatabase.dictionary)
        {
            Character c = pair.Value;

            if (c.accessories !=null && c.accessories.Length > 0)
                m_CharacterList.Add(c);
        }

        for (var i = 0; i < m_CharacterList.Count; i++)
        {
	        int index = i;
	        var op = headerPrefab.InstantiateAsync().WaitForCompletion();
			LoadedCharacter(op, index);
        }
    }

    void LoadedCharacter(GameObject op, int currentIndex)
    {
        Character c = m_CharacterList[currentIndex];

        GameObject header = op;
        header.transform.SetParent(listRoot, false);
        ShopItemListItem itmHeader = header.GetComponent<ShopItemListItem>();
        itmHeader.nameText.text = c.characterName;

        foreach (var (key, value) in MetaplayClient.PlayerModel.GameConfig.Shop
                     .Where(x => x.Value.Category == Category.Accessory)
                     .Where(x => x.Value.Reward is AccessoryReward a && a.CharacterType == c.characterName))
        {
            var innerOp = prefabItem.InstantiateAsync().WaitForCompletion();
	        LoadedAccessory(innerOp, c, value);
        }
    }

    void LoadedAccessory(GameObject op, Character c, ShopItem shopItem)
    {
	    var accessoryReward = shopItem.Reward as AccessoryReward;
	    CharacterAccessories accessory = c.accessories.FirstOrDefault(x=> x.accessoryName == accessoryReward.AccessoryType);

	    GameObject newEntry = op;
	    newEntry.transform.SetParent(listRoot, false);

	    ShopItemListItem itm = newEntry.GetComponent<ShopItemListItem>();

	    string compoundName = c.characterName + ":" + accessory.accessoryName;

	    itm.nameText.text = accessory.accessoryName;
	    itm.pricetext.text = shopItem.CoinCost.ToString();
	    itm.icon.sprite = accessory.accessoryIcon;
	    itm.buyButton.image.sprite = itm.buyButtonSprite;

	    if (shopItem.PremiumCost > 0)
	    {
		    itm.premiumText.transform.parent.gameObject.SetActive(true);
		    itm.premiumText.text = shopItem.PremiumCost.ToString();
	    }
	    else
	    {
		    itm.premiumText.transform.parent.gameObject.SetActive(false);
	    }

	    itm.buyButton.onClick.AddListener(delegate()
	    {
		    Buy(compoundName, shopItem.ConfigKey);
	    });

	    m_RefreshCallback += delegate() { RefreshButton(itm, compoundName, shopItem.ConfigKey); };
	    RefreshButton(itm, compoundName, shopItem.ConfigKey);
    }

	protected void RefreshButton(ShopItemListItem itm, string compoundName, ShopId key)
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

		if (PlayerData.instance.characterAccessories.Contains(compoundName))
		{
			itm.buyButton.interactable = false;
			itm.buyButton.image.sprite = itm.disabledButtonSprite;
			itm.buyButton.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Owned";
		}
	}
	
	public void Buy(string name, ShopId key)
    {
	    if (!MetaplayClient.PlayerContext.ExecuteAction(new BuyItemAction(key)).IsSuccess)
		    return;
	    var shopItem = MetaplayClient.PlayerModel.GameConfig.Shop[key];
        PlayerData.instance.Save();

#if UNITY_ANALYTICS // Using Analytics Standard Events v0.3.0
        var transactionId = System.Guid.NewGuid().ToString();
        var transactionContext = "store";
        var level = PlayerData.instance.rank.ToString();
        var itemId = name;
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

        Refresh();
    }
}

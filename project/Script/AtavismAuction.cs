using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
namespace Atavism
{
    public class Auction
    {
        public int id = 0;
        public string groupId = "";
        public AtavismInventoryItem item;
        public string expirateDate = "";
        public Sprite icon = null;
        public long buyout = 0;
        public int currency = 0;
        public int countSell = 0;
        public int countOrder = 0;
        public int count = 0;
        public int mode = 0;
    }
    public class AuctionCountPrice
    {
        public long price = 0;
        public int count = 0;
        public int currency = 0;
    }

    public class AtavismAuction : MonoBehaviour
    {

        static AtavismAuction instance;
        Dictionary<string, Auction> auctions = new Dictionary<string, Auction>();
        Dictionary<int, Auction> ownAuctions = new Dictionary<int, Auction>();
        Dictionary<long, AuctionCountPrice> auctionsForGroupSell = new Dictionary<long, AuctionCountPrice>();
        Dictionary<long, AuctionCountPrice> auctionsForGroupOrder = new Dictionary<long, AuctionCountPrice>();
        GameObject tempItemSetStorage = null;
        Vector3 storageOpenLoc = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        int currecnyType = 3;
        long costStartValue = 0;
        float costStartPer = 0;
        long costEndValue = 0;
        float costEndPer = 0;
        int createLmit = 10;
        int auctionsLimit = 100;

        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            // Listen for messages from the server
            NetworkAPI.RegisterExtensionMessageHandler("AuctionOwnerListUpdate", HandleAuctionOwnerListUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("AuctionListUpdate", HandleAuctionListUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("AuctionListForGorupUpdate", HandleAuctionListForGroupUpdate);
        }


        public void GetAuctionList()
        {
            AtavismLogger.LogDebugMessage("GetAuctionList Start");
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(0, false, "auction.list", props);
            AtavismLogger.LogDebugMessage("GetAuctionList End");
        }
        public void CancelAuction(Auction auction, bool selling, bool buying)
        {
            AtavismLogger.LogDebugMessage("CancelAuction Start");
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("auctionId", auction.id);
            props.Add("selling", selling);
            props.Add("buying", buying);
            NetworkAPI.SendExtensionMessage(0, false, "auction.cancel", props);
            AtavismLogger.LogDebugMessage("CancelAuction End");
        }


        public void CreateAuction(AtavismInventoryItem item, Dictionary<string, object> currencies, int count, string itemGroupId)
        {
            AtavismLogger.LogDebugMessage("CreateAuction Start");
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("startbid", 0);
            props.Add("buyout", currencies);

            props.Add("item_oid", item.ItemId);
            props.Add("item_count", count);
            props.Add("itemgroup", itemGroupId);
            props.Add("auctioneer_oid", OID.fromLong(0));
            NetworkAPI.SendExtensionMessage(0, false, "auction.createSell", props);
            AtavismLogger.LogDebugMessage("CreateAuction End");
        }

        public void BuyAuction(string itemGroupId, Dictionary<string, object> currencies, int count)
        {
            AtavismLogger.LogDebugMessage("BuyAuction Start");
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("startbid", 0);
            props.Add("buyout", currencies);
            props.Add("groupId", itemGroupId);
            props.Add("item_count", count);
            props.Add("auctioneer_oid", OID.fromLong(0));

            NetworkAPI.SendExtensionMessage(0, false, "auction.buy", props);
            AtavismLogger.LogDebugMessage("BuyAuction End");
        }
        public void OrderAuction(string itemGroupId, Dictionary<string, object> currencies, int count)
        {
            AtavismLogger.LogDebugMessage("OrderAuction Start");
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("startbid", 0);
            props.Add("buyout", currencies);
            props.Add("groupId", itemGroupId);
            props.Add("item_count", count);
            props.Add("auctioneer_oid", OID.fromLong(0));

            NetworkAPI.SendExtensionMessage(0, false, "auction.order", props);
            AtavismLogger.LogDebugMessage("OrderAuction End");
        }


        public void SearchAuction(bool sortCount, bool sortName, bool sortPrice, string searchQuality, string searchRace, string searchClass, int searchLevelMin, int searchLevelMax, string searchCatType, string searchCat, string searchText, List<object> searchQualityList, bool asc, Dictionary<string, object> searchCatDic)
        {
            List<object> termNames = new List<object>();
#if AT_I2LOC_PRESET
        if (searchText.Length > 0)
        {
            List<string> _termNames = I2.Loc.LocalizationManager.GetTermsList("Items").FindAll(x => { return x.ToLower().Contains(searchText) || I2.Loc.LocalizationManager.GetTermTranslation(x).ToLower().Contains(searchText.ToLower()); });
           AtavismLogger.LogDebugMessage(_termNames.Count);
            foreach (string elm in _termNames)
            {
             AtavismLogger.LogDebugMessage(elm.Substring(6));
                termNames.Add(elm.Substring(6));
            }
        }
        else
        {
            termNames.Add(searchText);
        }
#else
            termNames.Add(searchText);
#endif

            AtavismLogger.LogDebugMessage("SearchAuction ");
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("sortCount", sortCount);
            props.Add("sortName", sortName);
            props.Add("sortPrice", sortPrice);
            props.Add("searchQuality", searchQuality);
            props.Add("searchQuality2", searchQualityList);
            props.Add("searchCatDic", searchCatDic);
            props.Add("searchRace", searchRace);
            props.Add("searchClass", searchClass);
            props.Add("searchLevelMin", searchLevelMin);
            props.Add("searchLevelMax", searchLevelMax);
            props.Add("searchCatType", searchCatType);
            props.Add("searchCat", searchCat);
            props.Add("searchText", termNames);
            props.Add("sortAsc", asc);
            NetworkAPI.SendExtensionMessage(0, false, "auction.search", props);
            AtavismLogger.LogDebugMessage("SearchAuction End");
        }

        public void GetAuctionsForGroup(string groupId = "", long itemOid = 0L)
        {
            AtavismLogger.LogDebugMessage("GetAuctionsForGroup Start");
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("groupId", groupId);
            props.Add("itemOid", itemOid);

            NetworkAPI.SendExtensionMessage(0, false, "auction.getAuctionForGroup", props);
            AtavismLogger.LogDebugMessage("GetAuctionsForGroup End");
        }

        public void GetOwnAuctionList(bool buying, bool selling, bool bought, bool sold, bool expired)
        {
            AtavismLogger.LogDebugMessage("GetOwnAuctionList Start");
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("buying", buying);
            props.Add("selling", selling);
            props.Add("bought", bought);
            props.Add("sold", sold);
            props.Add("expired", expired);
            NetworkAPI.SendExtensionMessage(0, false, "auction.ownerList", props);
            AtavismLogger.LogDebugMessage("GetOwnAuctionList End");
        }

        public void TakeReward(bool buying, bool selling, bool bought, bool sold, bool expired)
        {
            AtavismLogger.LogDebugMessage("TakeReward Start");
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("buying", buying);
            props.Add("selling", selling);
            props.Add("bought", bought);
            props.Add("sold", sold);
            props.Add("expired", expired);
            NetworkAPI.SendExtensionMessage(0, false, "auction.takeAll", props);
            AtavismLogger.LogDebugMessage("TakeReward End");
        }

        #region Message Handlers
        public long CalcCost(long price)
        {
            return (costStartValue + (long)Math.Ceiling(price * costStartPer / 100d));
        }

        public void HandleAuctionOwnerListUpdate(Dictionary<string, object> props)
        {
            AtavismLogger.LogDebugMessage("HandleAuctionOwnerListUpdate Start");

            /*    string keys = " [ ";
                foreach (var it in props.Keys)
                {
                    keys += " ; " + it + " => " + props[it];
                }*/
            //     Debug.LogWarning("HandleAuctionOwnerListUpdate: keys:" + keys);
            try
            {
                ownAuctions.Clear();
                costStartValue = (long)props["sPriceVal"];
                costStartPer = (float)props["SPricePerc"];
                costEndValue = (long)props["cPriceVal"];
                costEndPer = (float)props["cPricePerc"];
                currecnyType = (int)props["currency"];
                int numItems = (int)props["numItems"];
              //  Debug.LogWarning("HandleAuctionOwnerListUpdate: I");
                for (int i = 0; i < numItems; i++)
                {
                 //   Debug.LogWarning("HandleAuctionOwnerListUpdate: II");
                    Auction auctionInfo = new Auction();

                    auctionInfo.id = (int)props["auction_" + i + "Id"];
                    auctionInfo.buyout = (long)props["auction_" + i + "Buyout"];
                    auctionInfo.currency = (int)props["auction_" + i + "Currency"];
                    auctionInfo.mode = (int)props["auction_" + i + "Mode"];
                 //   Debug.LogWarning("HandleAuctionOwnerListUpdate: II-I");

                    string baseName = (string)props["auction_" + i + "ExpirateDate"];
                    int templateID = (int)props["item_" + i + "TemplateID"];
                    AtavismInventoryItem invInfo = Inventory.Instance.LoadItemPrefabData(templateID);
                    AtavismLogger.LogDebugMessage("Got item: " + invInfo.BaseName);
                   // Debug.LogWarning("HandleAuctionOwnerListUpdate: II-II");

                    //invInfo.copyData(GetGenericItemData(invInfo.baseName));
                    invInfo.Count = (int)props["item_" + i + "Count"];
                    //ClientAPI.Log("ITEM: item count for item %s is %s" % (invInfo.name, invInfo.count))
                    invInfo.ItemId = (OID)props["item_" + i + "Id"];
                    invInfo.name = (string)props["item_" + i + "Name"];
                    invInfo.IsBound = (bool)props["item_" + i + "Bound"];
                 //   Debug.LogWarning("HandleAuctionOwnerListUpdate: III");

                    //	invInfo.EnergyCost = (int)props["item_" + i + "EnergyCost"];
                    if (props["item_" + i + "MaxDurability"] == null)
                        invInfo.MaxDurability = 0;
                    else
                        invInfo.MaxDurability = (int)props["item_" + i + "MaxDurability"];
                    if (invInfo.MaxDurability > 0)
                    {
                        invInfo.Durability = (int)props["item_" + i + "Durability"];
                    }
                    int numResists = (int)props["item_" + i + "NumResistances"];
                    for (int j = 0; j < numResists; j++)
                    {
                        string resistName = (string)props["item_" + i + "Resist_" + j + "Name"];
                        int resistValue = (int)props["item_" + i + "Resist_" + j + "Value"];
                        invInfo.Resistances[resistName] = resistValue;
                    }
                    int numStats = (int)props["item_" + i + "NumStats"];
                    for (int j = 0; j < numStats; j++)
                    {
                        string statName = (string)props["item_" + i + "Stat_" + j + "Name"];
                        int statValue = (int)props["item_" + i + "Stat_" + j + "Value"];
                        invInfo.Stats[statName] = statValue;
                    }

                    int NumEStats = (int)props["item_" + i + "NumEStats"];
                    for (int j = 0; j < NumEStats; j++)
                    {
                        string statName = (string)props["item_" + i + "EStat_" + j + "Name"];
                        int statValue = (int)props["item_" + i + "EStat_" + j + "Value"];
                        invInfo.EnchantStats[statName] = statValue;
                        //  Debug.LogError("Enchant Stat : " + statName + " " + statValue);
                    }
                    int NumSocket = (int)props["item_" + i + "NumSocket"];
                    for (int j = 0; j < NumSocket; j++)
                    {
                        string socType = (string)props["item_" + i + "socket_" + j + "Type"];
                        int socItem = (int)props["item_" + i + "socket_" + j + "Item"];
                        long socItemOid = (long)props["item_" + i + "socket_" + j + "ItemOid"];
                        int socId = (int)props["item_" + i + "socket_" + j + "Id"];
                        if (invInfo.SocketSlots.ContainsKey(socType))
                        {
                            invInfo.SocketSlots[socType].Add(socId, socItem);
                            invInfo.SocketSlotsOid[socType].Add(socId, socItemOid);
                        }
                        else
                        {
                            Dictionary<int, int> dic = new Dictionary<int, int>();
                            dic.Add(socId, socItem);
                            Dictionary<int, long> dicLong = new Dictionary<int, long>();
                            dicLong.Add(socId, socItemOid);
                            invInfo.SocketSlotsOid.Add(socType, dicLong);
                            invInfo.SocketSlots.Add(socType, dic);
                        }
                    }
                  //  Debug.LogWarning("HandleAuctionOwnerListUpdate: IIII");


                    int NumOfSet = (int)props["item_" + i + "NumOfSet"];
                    invInfo.setCount = NumOfSet;

                    int ELevel = (int)props["item_" + i + "ELevel"];
                    invInfo.enchantLeval = ELevel;
                    //ClientAPI.Log("InventoryUpdateEntry fields: %s, %d, %d, %s" % (invInfo.itemId, bagNum, slotNum, invInfo.name))
                    if (invInfo.itemType == "Weapon")
                    {
                        invInfo.DamageValue = (int)props["item_" + i + "DamageValue"];
                        invInfo.DamageMaxValue = (int)props["item_" + i + "DamageValueMax"];
                        invInfo.DamageType = (string)props["item_" + i + "DamageType"];
                        invInfo.WeaponSpeed = (int)props["item_" + i + "Delay"];
                    }
                    auctionInfo.item = invInfo;
                    ownAuctions.Add(auctionInfo.id, auctionInfo);
                   // Debug.LogWarning("HandleAuctionOwnerListUpdate: IIIII");

                }
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("AUCTION_OWN_LIST_UPDATE", args);

            }
            catch (Exception e)
            {
                AtavismLogger.LogError("AtavismAuction.HandleAuctionOwnerListUpdate Exeption " + e.Message);
            }
            AtavismLogger.LogDebugMessage("HandleAuctionOwnerListUpdate End");

        }

        public void HandleAuctionListUpdate(Dictionary<string, object> props)
        {
            AtavismLogger.LogDebugMessage("HandleAuctionListUpdate");

            /*    string keys = " [ ";
                foreach (var it in props.Keys)
                {
                    keys += " ; " + it + " => " + props[it];
                }
                Debug.LogWarning("HandleAuctionListUpdate: keys:" + keys+" ]");
             */
            int numItems = 0;
            try
            {
                auctions.Clear();
                costStartValue = (long)props["sPriceVal"];
                costStartPer = (float)props["SPricePerc"];
                costEndValue = (long)props["cPriceVal"];
                costEndPer = (float)props["cPricePerc"];
                currecnyType = (int)props["currency"];
                auctionsLimit = (int)props["auctionLimit"];
                createLmit = (int)props["auctionOwnLimit"];
                numItems = (int)props["numItems"];
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("AtavismAuction.HandleAuctionListUpdate | Exception " + e.Message);
            }
            try
            {
                for (int i = 0; i < numItems; i++)
                {
                    //     Debug.LogWarning("HandleAuctionListUpdate i:"+i);

                    Auction auctionInfo = new Auction();
                    if (props.ContainsKey("auction_" + i + "Id"))
                        auctionInfo.id = (int)props["auction_" + i + "Id"];
                    if (props.ContainsKey("auction_" + i + "GroupId"))
                        auctionInfo.groupId = (string)props["auction_" + i + "GroupId"];
                    //     Debug.LogWarning("HandleAuctionListUpdate i:" + i + " 0-1");
                    try
                    {
                        auctionInfo.buyout = (long)props["auction_" + i + "Buyout"];
                    }
                    catch (Exception e)
                    {
                        AtavismLogger.LogError("AtavismAuction.HandleAuctionListUpdate || Exception " + e.Message);
                    }
                    auctionInfo.currency = (int)props["auction_" + i + "Currency"];
                    auctionInfo.countSell = (int)props["auction_" + i + "CountsSell"];
                    auctionInfo.countOrder = (int)props["auction_" + i + "CountsOrder"];
                    //      Debug.LogWarning("HandleAuctionListUpdate i:" + i + " 0-2");

                    string baseName = (string)props["auction_" + i + "ExpirateDate"];
                    int templateID = (int)props["item_" + i + "TemplateID"];
                    //    Debug.LogWarning("HandleAuctionListUpdate i:" + i + " 0-3");
                    AtavismInventoryItem invInfo = Inventory.Instance.LoadItemPrefabData(templateID);
                    AtavismLogger.LogDebugMessage("Got item: " + invInfo.BaseName);
                    //invInfo.copyData(GetGenericItemData(invInfo.baseName));
                    invInfo.Count = (int)props["item_" + i + "Count"];
                    //ClientAPI.Log("ITEM: item count for item %s is %s" % (invInfo.name, invInfo.count))
                    invInfo.ItemId = (OID)props["item_" + i + "Id"];
                    invInfo.name = (string)props["item_" + i + "Name"];
                    invInfo.IsBound = (bool)props["item_" + i + "Bound"];

                    //     Debug.LogWarning("HandleAuctionListUpdate i:" + i + " 0-4");

                    //  invInfo.EnergyCost = (int)props["item_" + i + "EnergyCost"];
                    if (props["item_" + i + "MaxDurability"] == null)
                        invInfo.MaxDurability = 0;
                    else
                        invInfo.MaxDurability = (int)props["item_" + i + "MaxDurability"];

                    //     Debug.LogWarning("HandleAuctionListUpdate i:" + i + " 1");

                    if (invInfo.MaxDurability > 0)
                    {
                        invInfo.Durability = (int)props["item_" + i + "Durability"];
                    }
                    int numResists = (int)props["item_" + i + "NumResistances"];

                    //    Debug.LogWarning("HandleAuctionListUpdate i:" + i + " 2");

                    for (int j = 0; j < numResists; j++)
                    {
                        string resistName = (string)props["item_" + i + "Resist_" + j + "Name"];
                        int resistValue = (int)props["item_" + i + "Resist_" + j + "Value"];
                        invInfo.Resistances[resistName] = resistValue;
                    }
                    int numStats = (int)props["item_" + i + "NumStats"];

                    //    Debug.LogWarning("HandleAuctionListUpdate i:" + i + " 3");

                    for (int j = 0; j < numStats; j++)
                    {
                        string statName = (string)props["item_" + i + "Stat_" + j + "Name"];
                        int statValue = (int)props["item_" + i + "Stat_" + j + "Value"];
                        invInfo.Stats[statName] = statValue;
                    }

                    int NumEStats = (int)props["item_" + i + "NumEStats"];
                    for (int j = 0; j < NumEStats; j++)
                    {
                        string statName = (string)props["item_" + i + "EStat_" + j + "Name"];
                        int statValue = (int)props["item_" + i + "EStat_" + j + "Value"];
                        invInfo.EnchantStats[statName] = statValue;
                        //  Debug.LogError("Enchant Stat : " + statName + " " + statValue);
                    }
                    //    Debug.LogWarning("HandleAuctionListUpdate i:" + i + " 4");

                    int NumSocket = (int)props["item_" + i + "NumSocket"];
                    for (int j = 0; j < NumSocket; j++)
                    {
                        string socType = (string)props["item_" + i + "socket_" + j + "Type"];
                        int socItem = (int)props["item_" + i + "socket_" + j + "Item"];
                        long socItemOid = (long)props["item_" + i + "socket_" + j + "ItemOid"];
                        int socId = (int)props["item_" + i + "socket_" + j + "Id"];
                        if (invInfo.SocketSlots.ContainsKey(socType))
                        {
                            invInfo.SocketSlots[socType].Add(socId, socItem);
                            invInfo.SocketSlotsOid[socType].Add(socId, socItemOid);
                        }
                        else
                        {
                            Dictionary<int, int> dic = new Dictionary<int, int>();
                            dic.Add(socId, socItem);
                            Dictionary<int, long> dicLong = new Dictionary<int, long>();
                            dicLong.Add(socId, socItemOid);
                            invInfo.SocketSlots.Add(socType, dic);
                            invInfo.SocketSlotsOid.Add(socType, dicLong);
                        }
                    }

                    //    Debug.LogWarning("HandleAuctionListUpdate i:" + i + " 5");

                    int NumOfSet = (int)props["item_" + i + "NumOfSet"];
                    invInfo.setCount = NumOfSet;

                    int ELevel = (int)props["item_" + i + "ELevel"];
                    invInfo.enchantLeval = ELevel;
                    //ClientAPI.Log("InventoryUpdateEntry fields: %s, %d, %d, %s" % (invInfo.itemId, bagNum, slotNum, invInfo.name))
                    if (invInfo.itemType == "Weapon")
                    {
                        invInfo.DamageValue = (int)props["item_" + i + "DamageValue"];
                        invInfo.DamageMaxValue = (int)props["item_" + i + "DamageValueMax"];
                        invInfo.DamageType = (string)props["item_" + i + "DamageType"];
                        invInfo.WeaponSpeed = (int)props["item_" + i + "Delay"];
                    }
                    auctionInfo.item = invInfo;
                    auctions.Add(auctionInfo.groupId, auctionInfo);
                    //    Debug.LogWarning("HandleAuctionListUpdate i:" + i+" end");

                }
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("AUCTION_LIST_UPDATE", args);
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("AtavismAuction.HandleAuctionListUpdate Exception " + e.Message);
            }
            AtavismLogger.LogDebugMessage("HandleAuctionListUpdate End");

        }



        public void HandleInventoryEvent(Dictionary<string, object> props)
        {
            AtavismLogger.LogDebugMessage("HandleInventoryEvent");
            try
            {
                string eventType = (string)props["event"];
                int itemID = (int)props["itemID"];
                int count = (int)props["count"];
                string data = (string)props["data"];

                // dispatch a ui event to tell the rest of the system
                string[] args = new string[4];
                args[0] = eventType;
                args[1] = itemID.ToString();
                args[2] = count.ToString();
                args[3] = data;
                AtavismEventSystem.DispatchEvent("AUCTION_LIST_UPDATE", args);

            }
            catch (Exception e)
            {
                AtavismLogger.LogError("AtavismAuction.HandleInventoryEvent Exeption " + e.Message);
            }
            AtavismLogger.LogDebugMessage("HandleInventoryEvent End");
        }

        public void HandleAuctionListForGroupUpdate(Dictionary<string, object> props)
        {
            AtavismLogger.LogDebugMessage("HandleAuctionListForGroupUpdate");

             /*  string keys = " [ ";
               foreach (var it in props.Keys)
               {
                   keys += " ; " + it + " => " + props[it];
               }
               Debug.LogWarning("HandleAuctionOwnerListUpdate: keys:" + keys);
            */
            try
            {
                auctionsForGroupSell.Clear();
                auctionsForGroupOrder.Clear();
                costStartValue = (long)props["sPriceVal"];
                costStartPer = (float)props["SPricePerc"];
                costEndValue = (long)props["cPriceVal"];
                costEndPer = (float)props["cPricePerc"];
                currecnyType = (int)props["currency"];
                int numItemsSell = (int)props["numItemsSell"];
                int numItemsOrder = (int)props["numItemsOrder"];
                for (int i = 0; i < numItemsSell; i++)
                {
                    AuctionCountPrice auctionInfo = new AuctionCountPrice();

                    auctionInfo.count = (int)props["auctionSell_" + i + "Count"];
                    auctionInfo.price = (long)props["auctionSell_" + i + "Price"];
                    auctionInfo.currency = (int)props["auctionSell_" + i + "Currency"];
                    auctionsForGroupSell.Add(auctionInfo.price, auctionInfo);
                }
                for (int i = 0; i < numItemsOrder; i++)
                {
                    AuctionCountPrice auctionInfo = new AuctionCountPrice();

                    auctionInfo.count = (int)props["auctionOrder_" + i + "Count"];
                    auctionInfo.price = (long)props["auctionOrder_" + i + "Price"];
                    auctionInfo.currency = (int)props["auctionOrder_" + i + "Currency"];
                    auctionsForGroupOrder.Add(auctionInfo.price, auctionInfo);
                }
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("AUCTION_LIST_FOR_GROUP_UPDATE", args);
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("AtavismAuction.HandleAuctionOwnerListUpdate Exeption " + e.Message);
            }
            AtavismLogger.LogDebugMessage("HandleAuctionOwnerListUpdate End");

        }
        #endregion Message Handlers

        #region Properties
        public static AtavismAuction Instance
        {
            get
            {
                return instance;
            }
        }
        public int AuctionDisplayLimit
        {
            get
            {
                return createLmit;
            }
        }
        public int AuctionsLimit
        {
            get
            {
                return auctionsLimit;
            }
        }


        public Dictionary<string, Auction> Auctions
        {
            get
            {
                return auctions;
            }
        }

        public Dictionary<int, Auction> OwnAuctions
        {
            get
            {
                return ownAuctions;
            }
        }
        public Dictionary<long, AuctionCountPrice> AuctionsForGroupOrder
        {
            get
            {
                return auctionsForGroupOrder;
            }
        }

        public Dictionary<long, AuctionCountPrice> AuctionsForGroupSell
        {
            get
            {
                return auctionsForGroupSell;
            }
        }
        public int GetCurrencyType
        {
            get
            {
                return currecnyType;
            }
        }

        #endregion Properties
    }
}
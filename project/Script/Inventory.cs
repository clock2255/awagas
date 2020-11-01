using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class Bag
    {
        public int slotNum;
        public int id = 0;
        public bool isActive = false;
        public AtavismInventoryItem itemTemplate;
        public string name = "";
        public Sprite icon = null;
        public int numSlots = 0;
        public Dictionary<int, AtavismInventoryItem> items = new Dictionary<int, AtavismInventoryItem>();
    }

    public class CurrencyDisplay
    {
        public Sprite icon;
        public string name;
        public long amount;
    }

    public class Inventory : MonoBehaviour
    {

        static Inventory instance;
        Dictionary<string, EquipmentDisplay> equipmentDisplays;
        Dictionary<int, AtavismInventoryItem> items;
        Dictionary<int, AtavismInventoryItemSet> itemSets;
        Dictionary<int, Currency> currencies;
        Dictionary<int, AtavismCraftingRecipe> craftingRecipes;
        public UGUIAtavismActivatable uguiAtavismItemPrefab;
        public int mainCurrencyGroup = 1;
        public float sellFactor = 0.25f;
        GameObject tempItemStorage = null;
        GameObject tempItemSetStorage = null;
        Vector3 storageOpenLoc = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        // These are filled by messages from the server
        Dictionary<int, Bag> bags = new Dictionary<int, Bag>();
        Dictionary<int, AtavismInventoryItem> equippedItems = new Dictionary<int, AtavismInventoryItem>();
        AtavismInventoryItem equippedAmmo;
        Dictionary<int, Bag> storageItems = new Dictionary<int, Bag>();
        Dictionary<int, AtavismInventoryItem> loot = new Dictionary<int, AtavismInventoryItem>();
        OID lootTarget;

        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            tempItemStorage = new GameObject("TemporaryItemData");
            DontDestroyOnLoad(tempItemStorage);
            tempItemSetStorage = new GameObject("TemporaryItemSetData");
            DontDestroyOnLoad(tempItemSetStorage);

            // Load in equipment displays - Not currently used
            /*equipmentDisplays = new Dictionary<string, EquipmentDisplay>();
            Object[] displayPrefabs = Resources.LoadAll("Content/EquipmentDisplay");
            foreach (Object displayPrefab in displayPrefabs) {
                GameObject go = (GameObject) displayPrefab;
                EquipmentDisplay displayData = go.GetComponent<EquipmentDisplay>();
                equipmentDisplays.Add(go.name, displayData);
            }*/

            // Load in items
            items = new Dictionary<int, AtavismInventoryItem>();
            Object[] itemPrefabs = Resources.LoadAll("Content/Items");
            foreach (Object displayPrefab in itemPrefabs)
            {
                GameObject go = (GameObject)displayPrefab;
                AtavismInventoryItem displayData = go.GetComponent<AtavismInventoryItem>();
                if (displayData != null)
                    if (!items.ContainsKey(displayData.TemplateId) && displayData.templateId > 0)
                    {
                        items.Add(displayData.TemplateId, displayData);
                    }
            }

            // Load in item Sets
            itemSets = new Dictionary<int, AtavismInventoryItemSet>();
            Object[] itemsetsPrefabs = Resources.LoadAll("Content/ItemSets");
            foreach (Object displayPrefab in itemsetsPrefabs)
            {
                GameObject go = (GameObject)displayPrefab;
                AtavismInventoryItemSet displayData = go.GetComponent<AtavismInventoryItemSet>();
                if (displayData != null)
                    if (!itemSets.ContainsKey(displayData.Setid) && displayData.Setid > 0)
                    {
                        itemSets.Add(displayData.Setid, displayData);
                    }
            }



            // Load in currencies
            currencies = new Dictionary<int, Currency>();
            Object[] currencyPrefabs = Resources.LoadAll("Content/Currencies");
            foreach (Object displayPrefab in currencyPrefabs)
            {
                GameObject go = (GameObject)displayPrefab;
                Currency displayData = go.GetComponent<Currency>();
                if (displayData != null)
                    if (!currencies.ContainsKey(displayData.id) && displayData.id > 0)
                    {
                        currencies.Add(displayData.id, displayData);
                    }
            }

            // Load in crafting recipes
            craftingRecipes = new Dictionary<int, AtavismCraftingRecipe>();
            Object[] recipePrefabs = Resources.LoadAll("Content/CraftingRecipes");
            foreach (Object displayPrefab in recipePrefabs)
            {
                GameObject go = (GameObject)displayPrefab;
                AtavismCraftingRecipe displayData = go.GetComponent<AtavismCraftingRecipe>();
                if (displayData != null)
                    if (!craftingRecipes.ContainsKey(displayData.recipeID) && displayData.recipeID > 0)
                    {
                        craftingRecipes.Add(displayData.recipeID, displayData);
                    }
            }

            // Listen for messages from the server
            NetworkAPI.RegisterExtensionMessageHandler("BagInventoryUpdate", HandleBagInventoryUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("EquippedInventoryUpdate", HandleEquippedInventoryUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("BankInventoryUpdate", HandleBankInventoryUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("StorageInventoryUpdate", HandleBankInventoryUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("currencies", HandleCurrencies);
            NetworkAPI.RegisterExtensionMessageHandler("LootList", HandleLootList);
            NetworkAPI.RegisterExtensionMessageHandler("inventory_event", HandleInventoryEvent);
        }

        void Update()
        {
            if (lootTarget != null && ClientAPI.GetObjectNode(lootTarget.ToLong()) != null)
            {
                if (Vector3.Distance(ClientAPI.GetPlayerObject().Position, ClientAPI.GetObjectNode(lootTarget.ToLong()).GameObject.transform.position) > 5)
                {
                    lootTarget = null;
                    string[] args = new string[1];
                    AtavismEventSystem.DispatchEvent("CLOSE_LOOT_WINDOW", args);
                }
            }
            if (lootTarget != null && ClientAPI.GetObjectNode(lootTarget.ToLong()) == null)
            {
                lootTarget = null;
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("CLOSE_LOOT_WINDOW", args);
            }
            if (storageOpenLoc != new Vector3(float.MinValue, float.MinValue, float.MinValue))
            {
                if(ClientAPI.GetPlayerObject()!=null)
                if (Vector3.Distance(ClientAPI.GetPlayerObject().Position, storageOpenLoc) > 5)
                {
                    storageOpenLoc = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                    string[] args = new string[1];
                    AtavismEventSystem.DispatchEvent("CLOSE_STORAGE_WINDOW", args);
                }
            }
        }

        #region Inventory API
        //Sets
        AtavismInventoryItemSet LoadItemSetPrefabData(int id)
        {
            if (!itemSets.ContainsKey(id))
            {
                return null;
            }
            else
            {
                return itemSets[id].Clone(tempItemSetStorage);
            }
        }
        public AtavismInventoryItemSet GetItemBySetID(int itemID)
        {
            if (itemSets.ContainsKey(itemID))
                return itemSets[itemID];
            return null;
        }

        //Items
        AtavismInventoryItem LoadItemPrefabData(string itemBaseName)
        {
            GameObject itemPrefab = (GameObject)Resources.Load("Content/Items/Item" + itemBaseName);
            if (itemPrefab == null)
            {
                return null;
            }
            else
            {
                AtavismInventoryItem itemData = itemPrefab.GetComponent<AtavismInventoryItem>();
                return itemData.Clone(tempItemStorage);
            }
        }

        public AtavismInventoryItem LoadItemPrefabData(int templateID)
        {
            if (!items.ContainsKey(templateID))
            {
                return null;
            }
            else
            {
                return items[templateID].Clone(tempItemStorage);
            }
        }

        public AtavismInventoryItem GetItemByTemplateID(int itemID)
        {
            if (items.ContainsKey(itemID))
                return items[itemID];
            return null;
        }

        public bool DoesPlayerHaveSufficientItems(int templateID, int count)
        {
            int totalCount = 0;
            foreach (Bag bag in bags.Values)
            {
                foreach (AtavismInventoryItem item in bag.items.Values)
                {
                    if (item.templateId == templateID)
                    {
                        totalCount += item.Count;
                        if (totalCount >= count)
                            return true;
                    }
                }
            }
            return false;
        }

        public AtavismInventoryItem GetInventoryItem(int templateID)
        {
            foreach (Bag bag in bags.Values)
            {
                foreach (AtavismInventoryItem item in bag.items.Values)
                {
                    if (item.templateId == templateID)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public AtavismInventoryItem GetInventoryItem(OID itemOID)
        {
            foreach (Bag bag in bags.Values)
            {
                foreach (AtavismInventoryItem item in bag.items.Values)
                {
                    if (item.ItemId == itemOID)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public int GetCountOfItem(int templateID)
        {
            int totalCount = 0;
            foreach (Bag bag in bags.Values)
            {
                foreach (AtavismInventoryItem item in bag.items.Values)
                {
                    if (item.templateId == templateID)
                    {
                        totalCount += item.Count;
                    }
                }
            }
            return totalCount;
        }

        public void PlaceItemInBag(int bagNum, int slotNum, AtavismInventoryItem item, int count)
        {
            PlaceItemInBag(bagNum, slotNum, item, count, false);
        }

        public void PlaceItemInBag(int bagNum, int slotNum, AtavismInventoryItem item, int count, bool swap)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("bagNum", bagNum);
            props.Add("slotNum", slotNum);
            props.Add("itemOid", item.ItemId);
            props.Add("count", count);
            props.Add("swap", swap);
            NetworkAPI.SendExtensionMessage(0, false, "inventory.MOVE_ITEM", props);
            //ClientAPI.Write("Sending move item");
        }

        public void PlaceItemAsBag(AtavismInventoryItem item, int slot)
        {
            long targetOid = ClientAPI.GetPlayerOid();
            NetworkAPI.SendTargetedCommand(targetOid, "/placeBag " + item.ItemId.ToString() + " " + slot);
        }

        public void MoveBag(int bagSlot, int targetSlot)
        {
            //TODO: Change this to an ExtensionMessage
            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/moveBag " + bagSlot + " " + targetSlot);
        }

        public void PlaceBagAsItem(int bagSlot, int bagNum, int slotNum)
        {
            //TODO: Change this to an ExtensionMessage
            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/removeBag " + bagSlot + " " + bagNum + " " + slotNum);
        }

        public void DeleteItemWithName(string name)
        {
            AtavismInventoryItem itemToDelete = null;
            foreach (Bag bag in bags.Values)
            {
                foreach (AtavismInventoryItem item in bag.items.Values)
                {
                    if (item.name == name)
                    {
                        itemToDelete = item;
                        break;
                    }
                }
            }

            if (itemToDelete != null)
            {
                long targetOid = ClientAPI.GetPlayerObject().Oid;
                NetworkAPI.SendTargetedCommand(targetOid, "/deleteItem " + itemToDelete.ItemId.ToString());
            }
        }

        public void DeleteItemStack(object item, bool accepted)
        {
            if (accepted)
                DeleteItemStack((AtavismInventoryItem)item);
        }

        public void DeleteItemStack(AtavismInventoryItem item)
        {
            long targetOid = ClientAPI.GetPlayerObject().Oid;
            string itemOid = item.ItemId.ToString();
            NetworkAPI.SendTargetedCommand(targetOid, "/deleteItemStack " + itemOid);
        }

        public List<EquipmentDisplay> LoadEquipmentDisplay(string equipmentDisplayName)
        {
            List<EquipmentDisplay> equipmentDisplays = new List<EquipmentDisplay>();
            int resourcePathPos = equipmentDisplayName.IndexOf("Resources/");
            equipmentDisplayName = equipmentDisplayName.Substring(resourcePathPos + 10);
            equipmentDisplayName = equipmentDisplayName.Remove(equipmentDisplayName.Length - 7);
            GameObject eqPrefab = (GameObject)Resources.Load(equipmentDisplayName);
            if (eqPrefab == null)
            {
                Debug.LogWarning("Could not load equipment display: " + equipmentDisplayName);
                return null;
            }
            foreach (EquipmentDisplay eqDisplay in eqPrefab.GetComponents<EquipmentDisplay>())
            {
                equipmentDisplays.Add(eqDisplay);
            }
            return equipmentDisplays;
        }

        public Bag GetBagData(int slot)
        {
            if (bags.ContainsKey(slot))
            {
                return bags[slot];
            }
            return null;
        }

        public bool CreateSplitStack(AtavismInventoryItem item, int amount)
        {
            // Check if the item has a count greater than one
            if (item.Count == 1)
            {
                return false;
            }

            // Check where the item is stored - it may be in storage
            if (IsItemInInventory(item))
            {
                // Find a free spot
                int bagNum = -1;
                int slotNum = -1;
                FindFreeInventorySlot(out bagNum, out slotNum);

                if (bagNum != -1)
                {
                    PlaceItemInBag(bagNum, slotNum, item, amount);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (IsItemInStorage(item))
            {
            }
            return false;
        }

        bool IsItemInInventory(AtavismInventoryItem item)
        {
            foreach (Bag bag in bags.Values)
            {
                foreach (AtavismInventoryItem itemInBag in bag.items.Values)
                {
                    if (itemInBag.ItemId == item.ItemId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool FindFreeInventorySlot(out int bagNum, out int slotNum)
        {
            for (int i = 0; i < bags.Count; i++)
            {
                for (int j = 0; j < bags[i].numSlots; j++)
                {
                    if (!bags[i].items.ContainsKey(j))
                    {
                        bagNum = i;
                        slotNum = j;
                        return true;
                    }
                }
            }
            bagNum = -1;
            slotNum = -1;
            return false;
        }

        public void UnequipAmmo(AtavismInventoryItem item)
        {
            if (item != null)
                item.Activate();
            //TODO
        }

        public void EquipItemInSlot(AtavismInventoryItem item, string slotName)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("itemOid", item.ItemId);
            props.Add("slotName", slotName);
            NetworkAPI.SendExtensionMessage(0, false, "ao.EQUIP_ITEM_IN_SLOT", props);
        }

        public void RequestBankInfo()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(0, false, "ao.GET_BANK_ITEMS", props);
        }

        public void PlaceItemInBank(AtavismInventoryItem item, int count)
        {
            for (int i = 0; i < storageItems.Count; i++)
            {
                for (int j = 0; j < storageItems[i].numSlots; j++)
                {
                    if (!storageItems[i].items.ContainsKey(j))
                    {
                        PlaceItemInBank(i, j, item, count, false);
                        return;
                    }
                }
            }
        }

        public void PlaceItemInBank(int bagNum, int slotNum, AtavismInventoryItem item, int count, bool swap)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("bagNum", bagNum);
            props.Add("bankSlot", slotNum);
            props.Add("itemOid", item.ItemId);
            props.Add("count", count);
            props.Add("swap", swap);
            NetworkAPI.SendExtensionMessage(0, false, "ao.STORE_ITEM_IN_BANK", props);
            //ClientAPI.Write("Sending move item");
        }

        public void RetrieveItemFromBank(AtavismInventoryItem item)
        {
            // Find first open space in the players bag
            int containerNum = -1;
            int slotNum = -1;
            if (!FindFreeInventorySlot(out containerNum, out slotNum))
            {
                //TODO: send an error
                return;
            }

            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("containerNum", containerNum);
            props.Add("slotNum", slotNum);
            props.Add("itemOid", item.ItemId);
            props.Add("count", item.Count);
            NetworkAPI.SendExtensionMessage(0, false, "ao.RETRIEVE_ITEM_FROM_BANK", props);
        }

        bool IsItemInStorage(AtavismInventoryItem item)
        {
            foreach (Bag bag in storageItems.Values)
            {
                foreach (AtavismInventoryItem itemInBag in bag.items.Values)
                {
                    if (itemInBag.ItemId == item.ItemId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void StorageClosed()
        {
            storageOpenLoc = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        }

        #endregion Inventory API

        #region Currency API
        public Currency GetCurrency(int currencyID)
        {
            if (currencies.ContainsKey(currencyID))
            {
                return currencies[currencyID];
            }
            return null;
        }

        public string GetCurrencyName(int currencyID)
        {
            if (currencies.ContainsKey(currencyID))
            {
#if AT_I2LOC_PRESET
   			return I2.Loc.LocalizationManager.GetTranslation(currencies[currencyID].name);
#else
                return currencies[currencyID].name;
#endif
            }
#if AT_I2LOC_PRESET
   			return I2.Loc.LocalizationManager.GetTranslation("Unknown Currency");
#else
            return "Unknown Currency";
#endif
        }

        public int GetCurrencyGroup(int currencyID)
        {
            if (currencies.ContainsKey(currencyID))
            {
                return currencies[currencyID].group;
            }
            return -1;
        }

        public List<Currency> GetMainCurrencies()
        {
            return GetCurrenciesInGroup(mainCurrencyGroup);
        }

        public Currency GetMainCurrency(int pos)
        {
            foreach (Currency c in GetMainCurrencies())
            {
                if (c != null && c.position == pos)
                    return c;
            }
            return null;
        }

        public List<Currency> GetCurrenciesInGroup(int currencyGroup)
        {
            List<Currency> currenciesInGroup = new List<Currency>();
            foreach (Currency c in currencies.Values)
            {
                if (c.group == currencyGroup)
                {
                    while (currenciesInGroup.Count <= c.position)
                    {
                        currenciesInGroup.Add(null);
                    }
                    currenciesInGroup[c.position] = c;
                }
            }

            return currenciesInGroup;
        }

        /// <summary>
        /// Generates a list of currencies from the specified currencyID and amount. This can be
        /// used for drawing currency amounts and icons in the UI.
        /// </summary>
        /// <returns>The currency list from amount.</returns>
        /// <param name="currencyID">Currency ID.</param>
        /// <param name="currencyAmount">Currency amount.</param>
        /// 
        public List<CurrencyDisplay> GenerateCurrencyListFromAmount(int currencyID, long currencyAmount)
        {
            return GenerateCurrencyListFromAmount( currencyID,  currencyAmount, false);
        }



        public List<CurrencyDisplay> GenerateCurrencyListFromAmount(int currencyID, long currencyAmount,bool allFromGroup)
        {
            List<CurrencyDisplay> generatedCurrencyList = new List<CurrencyDisplay>();
            Currency c = GetCurrency(currencyID);
            if (c != null)
            {
                List<Vector2> currencyValues = GetConvertedCurrencyValues(currencyID, currencyAmount, allFromGroup);
                currencyValues.Reverse();
               // Debug.LogError("Inventory GetCost =" + currencyValues.Count);

                //	string costString = "";
                for (int i = 0; i < currencyValues.Count; i++)
                {
                  //  Debug.LogError("Inventory GetCost =" + currencyValues[i]);

                    CurrencyDisplay currencyDisplay = new CurrencyDisplay();
                    currencyDisplay.icon = GetCurrency((int)currencyValues[i].x).icon;
                    currencyDisplay.name = GetCurrencyName((int)currencyValues[i].x);
                    currencyDisplay.amount = (long)currencyValues[i].y;
                    generatedCurrencyList.Add(currencyDisplay);
                }
            }
            return generatedCurrencyList;
        }

        public CurrencyDisplay GenerateCurrencyDisplay(int currencyID, long currencyAmount)
        {
            Currency c = GetCurrency(currencyID);
            if (c != null)
            {
                CurrencyDisplay currencyDisplay = new CurrencyDisplay();
                currencyDisplay.icon = GetCurrency(currencyID).icon;
                currencyDisplay.name = GetCurrencyName(currencyID);
                currencyDisplay.amount = currencyAmount;
                return currencyDisplay;
            }
            return null;
        }

        /// <summary>
        /// Creates a readable string of the cost of an item based on the currencyID and amount passed in.
        /// </summary>
        /// <returns>The cost string.</returns>
        /// <param name="currencyID">Currency I.</param>
        /// <param name="currencyAmount">Currency amount.</param>
        public string GetCostString(int currencyID, long currencyAmount)
        {
            Currency c = GetCurrency(currencyID);
            if (c != null)
            {
                List<Vector2> currencyValues = GetConvertedCurrencyValues(currencyID, currencyAmount);
                currencyValues.Reverse();
                string costString = "";
                for (int i = 0; i < currencyValues.Count; i++)
                {
                    if ((int)currencyValues[i].y > 0)
                        costString += (int)currencyValues[i].y + " " + GetCurrencyName((int)currencyValues[i].x) + " ";
                }
                if(costString.Length > 0)
                costString = costString.Remove(costString.Length - 1);
                return costString;
            }
            else
            {
                return currencyAmount.ToString();
            }
        }

        /// <summary>
        /// Splits out a single currency and amount (generally a base currency) and returns the
        /// currencies and amounts it would convert to.
        /// </summary>
        /// <returns>The converted currency values.</returns>
        /// <param name="currencyID">Currency I.</param>
        /// <param name="currencyAmount">Currency amount.</param>
        /// 
        public List<Vector2> GetConvertedCurrencyValues(int currencyID, long currencyAmount)
        {
            return GetConvertedCurrencyValues(currencyID, currencyAmount, false);
        }


        public List<Vector2> GetConvertedCurrencyValues(int currencyID, long currencyAmount,bool allFromGroup)
        {
            List<Vector2> currencyValues = new List<Vector2>();
            Currency c = GetCurrency(currencyID);
            if (c.convertsTo > 0 && c.conversionAmountReq > 0 && currencyAmount >= c.conversionAmountReq && !allFromGroup)
            {
             //   Debug.LogError("GetConvertedCurrencyValues | "+c.id+" " + (currencyAmount % c.conversionAmountReq));
                currencyValues.Add(new Vector2(c.id, currencyAmount % c.conversionAmountReq));
                while (true)
                {
                    currencyAmount = currencyAmount / c.conversionAmountReq;
              //      Debug.LogError("GetConvertedCurrencyValues || "+c.id+" " + currencyAmount);
                    c = GetCurrency(c.convertsTo);
                    if (c == null)
                    {
                    //    Debug.LogError("GetConvertedCurrencyValues currency is null " );

                        break;
                    }
                 //   Debug.LogError("GetConvertedCurrencyValues currencyAmount=" + currencyAmount + " c.conversionAmountReq=" + c.conversionAmountReq);
                    if (c.conversionAmountReq > 1 && currencyAmount >= (long)c.conversionAmountReq)
                    {
                        currencyValues.Add(new Vector2(c.id, currencyAmount % c.conversionAmountReq));
                   //     Debug.LogError("GetConvertedCurrencyValues ||| " + c.id+" "+(currencyAmount % c.conversionAmountReq));
                    }
                    else
                    {
                        currencyValues.Add(new Vector2(c.id, currencyAmount));
                  //      Debug.LogError("GetConvertedCurrencyValues |||| "+ c.id+" " + currencyAmount);
                        break;
                    }
                }
            } else if (c.convertsTo > 0 && c.conversionAmountReq > 0 && allFromGroup)
            {
              //  Debug.LogError("GetConvertedCurrencyValues| | " + (currencyAmount % c.conversionAmountReq));
                currencyValues.Add(new Vector2(c.id, currencyAmount % c.conversionAmountReq));
                while (true)
                {
                    if(c.conversionAmountReq > 0)
                        currencyAmount = currencyAmount / c.conversionAmountReq;
                //    Debug.LogError("GetConvertedCurrencyValues| || " + currencyAmount);
                    c = GetCurrency(c.convertsTo);
                    if (c == null)
                        break;
                    if (c.conversionAmountReq > 1 && currencyAmount >= c.conversionAmountReq)
                    {
                        currencyValues.Add(new Vector2(c.id, currencyAmount % c.conversionAmountReq));
                  //      Debug.LogError("GetConvertedCurrencyValues| ||| " + (currencyAmount % c.conversionAmountReq));
                    }
                    else
                    {
                        currencyValues.Add(new Vector2(c.id, currencyAmount));
                   //     Debug.LogError("GetConvertedCurrencyValues| |||| " + currencyAmount);
                        //  break;
                    }
                }
            }
            else
            {
                currencyValues.Add(new Vector2(c.id, currencyAmount));
            }



            return currencyValues;
        }

        /// <summary>
        /// Converts the currencies to base currency. Assumes all currencies convert back to the same base currency
        /// </summary>
        /// <param name="currencies">Currencies.</param>
        /// <param name="currencyID">Currency I.</param>
        /// <param name="currencyAmount">Currency amount.</param>
        public void ConvertCurrenciesToBaseCurrency(List<Vector2> currencies, out int currencyID, out long currencyAmount)
        {
            currencyID = -1;
            currencyAmount = 0;
            // Loop through each currency
            foreach (Vector2 currencyInfo in currencies)
            {
                int currencyOutID;
                long currencyOutAmount;
                ConvertCurrencyToBaseCurrency((int)currencyInfo.x, (int)currencyInfo.y, out currencyOutID, out currencyOutAmount);
                if (currencyOutID == currencyID || currencyID == -1)
                {
                    currencyID = currencyOutID;
                    currencyAmount += currencyOutAmount;
                }
            }
        }

        /// <summary>
        /// Converts the currency down to the base currency.
        /// </summary>
        /// <param name="currencyInID">Currency in ID.</param>
        /// <param name="currencyInAmount">Currency in amount.</param>
        /// <param name="currencyOutID">Currency out ID.</param>
        /// <param name="currencyOutAmount">Currency out amount.</param>
        public void ConvertCurrencyToBaseCurrency(int currencyInID, long currencyInAmount, out int currencyOutID, out long currencyOutAmount)
        {
            // Find a currency that converts to this currency
            foreach (Currency c in currencies.Values)
            {
                if (c.convertsTo == currencyInID)
                {
                    currencyOutID = c.id;
                    currencyOutAmount = c.conversionAmountReq * currencyInAmount;
                    // Check if there is a currency that converts into this new currency
                    foreach (Currency cu in currencies.Values)
                    {
                        if (c.convertsTo == currencyInID)
                        {
                            ConvertCurrencyToBaseCurrency(currencyOutID, currencyOutAmount, out currencyOutID, out currencyOutAmount);
                        }
                    }
                    return;
                }
            }
            currencyOutID = currencyInID;
            currencyOutAmount = currencyInAmount;
        }

        /// <summary>
        /// Checks if the player has enough currency to match the given list of currencies.
        /// </summary>
        /// <returns><c>true</c>, if player has enough currency  <c>false</c> otherwise.</returns>
        /// <param name="currencies">Currencies.</param>
        public bool DoesPlayerHaveEnoughCurrency(List<Vector2> currencies)
        {
            // Convert both the given amounts and the actual curreny amount the player has down to the base currency
            int givenCurrencyID;
            long givenCurrencyAmount;
            ConvertCurrenciesToBaseCurrency(currencies, out givenCurrencyID, out givenCurrencyAmount);

            int currencyGroup = GetCurrencyGroup(givenCurrencyID);
            if (currencyGroup == -1)
                return false;

            int playerBaseCurrencyID;
            long playerCurrencyAmount = GetPlayerBaseCurrencyAmount(currencyGroup, out playerBaseCurrencyID);

            if (givenCurrencyAmount <= playerCurrencyAmount)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the total amount of base currency for the given currency group.
        /// </summary>
        /// <returns>The player base currency amount.</returns>
        /// <param name="currencyID">Currency I.</param>
        public long GetPlayerBaseCurrencyAmount(int currencyGroup, out int baseCurrencyID)
        {
            List<Currency> playerCurrencies = GetCurrenciesInGroup(currencyGroup);
            List<Vector2> playerCurrenciesList = new List<Vector2>();
            foreach (Currency c in playerCurrencies)
            {
                playerCurrenciesList.Add(new Vector2(c.id, c.Current));
            }

            long playerCurrencyAmount;
            ConvertCurrenciesToBaseCurrency(playerCurrenciesList, out baseCurrencyID, out playerCurrencyAmount);
            return playerCurrencyAmount;
        }

        #endregion Currency API
        #region Gear Modification

        public void EmbedInTheSlot(AtavismInventoryItem item, AtavismInventoryItem socketItem)
        {
            if (item != null && socketItem != null)
            {
                //     Debug.LogWarning("EmbedInTheSlot");
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("socketItemOid", socketItem.ItemId);
                props.Add("itemOid", item.ItemId);
                NetworkAPI.SendExtensionMessage(0, false, "inventory.INSERT_TO_SOCKET", props);
                //     Debug.LogWarning("EmbedInTheSlot send " + props);
            }
        }
        public void SocketingCost(AtavismInventoryItem item, AtavismInventoryItem socketItem)
        {
            if (item != null && socketItem != null)
            {
                //     Debug.LogWarning("SocketingCost");
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("socketItemOid", socketItem.ItemId);
                props.Add("itemOid", item.ItemId);
                NetworkAPI.SendExtensionMessage(0, false, "inventory.SOCKETING_DETAIL", props);
                //     Debug.LogWarning("SocketingCost send " + props);
            }
        }
        //Reset
        public void ResetSloctsSlot(AtavismInventoryItem item)
        {
            if (item != null)
            {
                //    Debug.LogWarning("ResetSloctsSlot");
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("itemOid", item.ItemId);
                NetworkAPI.SendExtensionMessage(0, false, "inventory.SOCKETING_RESET", props);
                //    Debug.LogWarning("ResetSloctsSlot send " + props);
            }
        }
        public void ResetSocketsCost(AtavismInventoryItem item)
        {
            if (item != null)
            {
                //    Debug.LogWarning("ResetSocketsCost");
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("itemOid", item.ItemId);
                NetworkAPI.SendExtensionMessage(0, false, "inventory.SOCKETING_RESET_DETAIL", props);
                //    Debug.LogWarning("ResetSocketsCost send " + props);
            }
        }
        //Enchant
        public void EnchantItem(AtavismInventoryItem item)
        {
            if (item != null)
            {
                //    Debug.LogWarning("EnchantItem");
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("itemOid", item.ItemId);
                NetworkAPI.SendExtensionMessage(0, false, "inventory.ENCHANT", props);
                //     Debug.LogWarning("EnchantItem send " + props);
            }
        }
        public void EnchantCost(AtavismInventoryItem item)
        {
            if (item != null)
            {
                //    Debug.LogWarning("EnchantCost");
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("itemOid", item.ItemId);
                NetworkAPI.SendExtensionMessage(0, false, "inventory.ENCHANTING_DETAIL", props);
                //    Debug.LogWarning("EnchantCost send " + props);
            }
        }




        #endregion Gear Modification
        public AtavismCraftingRecipe GetCraftingRecipe(int recipeID)
        {
            if (craftingRecipes.ContainsKey(recipeID))
            {
                return craftingRecipes[recipeID];
            }
            return null;
        }

        public List<AtavismCraftingRecipe> GetCraftingRecipeMatch(List<int> list, int skillId)
        {
            List<AtavismCraftingRecipe> _list = new List<AtavismCraftingRecipe>();
            foreach (int recId in list)
            {
                //  int _recId = int.Parse(recId);
                if (craftingRecipes.ContainsKey(recId))
                {
                    if (craftingRecipes[recId].skillID.Equals(skillId))
                    {
                        _list.Add(craftingRecipes[recId]);
                    }
                }
            }
            return _list;
        }
        public List<int> GetCraftingRecipeMatch(List<int> list, string station)
        {
            List<int> _list = new List<int>();
            foreach (int recId in list)
            {
                //  int _recId = int.Parse(recId);
                if (craftingRecipes.ContainsKey(recId))
                {
                    //    Debug.LogError("GetCraftingRecipeMatch " + craftingRecipes[recId].stationReq + " station:" + station);
                    if (craftingRecipes[recId].stationReq.Equals(station) || craftingRecipes[recId].stationReq.Equals("~ none ~"))
                    {
                        //     Debug.LogError("GetCraftingRecipeMatch add skill :"+craftingRecipes[recId].skillID+ " _list:"+ _list);

                        if (!_list.Contains(craftingRecipes[recId].skillID))
                            _list.Add(craftingRecipes[recId].skillID);
                    }
                    if (station.Equals(""))
                    {
                        if (!_list.Contains(craftingRecipes[recId].skillID))
                            _list.Add(craftingRecipes[recId].skillID);
                    }
                }
            }
            return _list;
        }



        public Dictionary<int, int> GetItemGruped()
        {
            Dictionary<int, int> groupitem = new Dictionary<int, int>();
            //     int it = 0;
            for (int i = 0; i < bags.Count; i++)
            {
                for (int k = 0; k < bags[i].numSlots; k++)
                {
                    if (bags[i].items.ContainsKey(k))
                    {
                        if (!bags[i].items[k].isBound && bags[i].items[k].sellable)
                        {

                            if (groupitem.ContainsKey(bags[i].items[k].templateId))
                            {
                                groupitem[bags[i].items[k].templateId] += bags[i].items[k].Count;
                            }
                            else
                            {
                                groupitem.Add(bags[i].items[k].templateId, bags[i].items[k].Count);
                            }
                            //                        it++;
                        }

                    }

                }

            }
            return groupitem;
        }

        #region Message Handlers

        /*public EquipmentDisplay GetEquipmentDisplay(string displayID) {
            // Player does not have this ability - lets use the template
            if (equipmentDisplays.ContainsKey(displayID))
                return equipmentDisplays[displayID];
            return null;
        }*/

        public void HandleBagInventoryUpdate(Dictionary<string, object> props)
        {
            //    Debug.LogWarning("HandleBagInventoryUpdate");
            /*
                    string keys = " [ ";
                    foreach (var it in props.Keys)
                    {
                        keys += " ; " + it + " => " + props[it];
                    }
                    Debug.LogWarning("HandleBagInventoryUpdate: keys:" + keys);
                   */
            bags.Clear();
            try
            {

                sellFactor = (float)props["SellFactor"];
                int numBags = (int)props["numBags"];
                for (int i = 0; i < numBags; i++)
                {
                    Bag bag = new Bag();
                    bag.id = (int)props["bag_" + i + "ID"];
                    bag.name = (string)props["bag_" + i + "Name"];
                    AtavismInventoryItem invInfo = LoadItemPrefabData(bag.name);
                    bag.itemTemplate = invInfo;
                    if (invInfo != null)
                        bag.icon = invInfo.icon;
                    AtavismLogger.LogDebugMessage("Got bag with name: " + bag.name);
                    bag.numSlots = (int)props["bag_" + i + "NumSlots"];
                    if (bag.numSlots == 0)
                    {
                        bag.isActive = false;
                    }
                    else
                    {
                        bag.isActive = true;
                    }
                    bag.slotNum = i;
                    //CSVReader.loadBagData(bag);
                    bags[i] = bag;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("HandleBagInventoryUpdate Bag Exception:" + e);
            }
            try
            {
                int numItems = (int)props["numItems"];
                for (int i = 0; i < numItems; i++)
                {
                    int bagNum = (int)props["item_" + i + "BagNum"];
                    int slotNum = (int)props["item_" + i + "SlotNum"];
                    //string baseName = (string)props["item_" + i + "BaseName"];
                    int templateID = (int)props["item_" + i + "TemplateID"];
                    AtavismInventoryItem invInfo = LoadItemPrefabData(templateID);
                    AtavismLogger.LogDebugMessage("Got item: " + invInfo.BaseName);
                    //invInfo.copyData(GetGenericItemData(invInfo.baseName));
                    invInfo.Count = (int)props["item_" + i + "Count"];
                    //ClientAPI.Log("ITEM: item count for item %s is %s" % (invInfo.name, invInfo.count))
                    invInfo.ItemId = (OID)props["item_" + i + "Id"];
                    invInfo.name = (string)props["item_" + i + "Name"];
                    invInfo.IsBound = (bool)props["item_" + i + "Bound"];
                    invInfo.EnergyCost = (int)props["item_" + i + "EnergyCost"];
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
                            Dictionary<int, long> dicLong = new Dictionary<int, long>();
                            dicLong.Add(socId, socItemOid);
                            invInfo.SocketSlotsOid.Add(socType, dicLong);
                            Dictionary<int, int> dic = new Dictionary<int, int>();
                            dic.Add(socId, socItem);
                            invInfo.SocketSlots.Add(socType, dic);
                        }
                    }


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
                    bags[bagNum].items[slotNum] = invInfo;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("HandleBagInventoryUpdate items Exception:" + e +e.Message+e.StackTrace);
            }
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("INVENTORY_UPDATE", args);
            //   Debug.LogWarning("HandleBagInventoryUpdate End");

        }

        public void HandleEquippedInventoryUpdate(Dictionary<string, object> props)
        {
            //     Debug.LogWarning("HandleEquippedInventoryUpdate");
            /*
                    string keys = " [ ";
                    foreach (var it in props.Keys)
                    {
                        keys += " ; " + it + " => " + props[it];
                    }
                    Debug.LogWarning("HandleEquippedInventoryUpdate: keys:" + keys);
            */
            equippedItems.Clear();
            try
            {
                int numSlots = (int)props["numSlots"];
                for (int i = 0; i < numSlots; i++)
                {
                    string name = (string)props["item_" + i + "Name"];
                    if (name == null || name == "")
                        continue;
                    string baseName = (string)props["item_" + i + "BaseName"];
                    AtavismInventoryItem invInfo = LoadItemPrefabData(baseName);
                    invInfo.name = name;
                    invInfo.Count = (int)props["item_" + i + "Count"];
                    invInfo.slot = (string)props["item_" + i + "Slot"];
                    invInfo.ItemId = (OID)props["item_" + i + "Id"];
                    invInfo.IsBound = (bool)props["item_" + i + "Bound"];
                    invInfo.EnergyCost = (int)props["item_" + i + "EnergyCost"];
                    invInfo.MaxDurability = (int)props["item_" + i + "MaxDurability"];
                    if (invInfo.MaxDurability > 0)
                    {
                        invInfo.Durability = (int)props["item_" + i + "Durability"];
                        //AtavismLogger.LogDebugMessage("Durability: " + invInfo.Durability + "/" + invInfo.MaxDurability);
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
                            invInfo.SocketSlots.Add(socType, dic);
                            Dictionary<int, long> dicLong = new Dictionary<int, long>();
                            dicLong.Add(socId, socItemOid);
                            invInfo.SocketSlotsOid.Add(socType, dicLong);
                        }
                    }
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
                    equippedItems.Add(i, invInfo);
                    AtavismLogger.LogDebugMessage("Added equipped item: " + invInfo.name + " to slot: " + i);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("HandleEquippedInventoryUpdate Bag Exception:" + e);
            }
            // Get Equipped Ammo
            int equippedAmmoID = (int)props["equippedAmmo"];
            if (equippedAmmoID > 0)
            {
                equippedAmmo = GetItemByTemplateID(equippedAmmoID);
                equippedAmmo.Count = GetCountOfItem(equippedAmmoID);

            }
            else
            {
                equippedAmmo = null;
            }

            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("EQUIPPED_UPDATE", args);
            //   Debug.LogWarning("HandleEquippedInventoryUpdate END");

        }

        public void HandleBankInventoryUpdate(Dictionary<string, object> props)
        {
            //  Debug.LogWarning("HandleBankInventoryUpdate");

            /*    string keys = " [ ";
                 foreach (var it in props.Keys)
                 {
                     keys += " ; " + it+" => "+props[it];
                 }
                 Debug.LogWarning("HandleBankInventoryUpdate: keys:" + keys);
                 */
            storageItems.Clear();
            try
            {
                int numBags = (int)props["numBags"];
                for (int i = 0; i < numBags; i++)
                {
                    Bag bag = new Bag();
                    bag.id = (int)props["bag_" + i + "ID"];
                    bag.name = (string)props["bag_" + i + "Name"];
                    AtavismInventoryItem invInfo = LoadItemPrefabData(bag.name);
                    bag.itemTemplate = invInfo;
                    if (invInfo != null)
                        bag.icon = invInfo.icon;
                    AtavismLogger.LogDebugMessage("Got bag with name: " + bag.name);
                    bag.numSlots = (int)props["bag_" + i + "NumSlots"];
                    if (bag.numSlots == 0)
                    {
                        bag.isActive = false;
                    }
                    else
                    {
                        bag.isActive = true;
                    }
                    bag.slotNum = i;
                    //CSVReader.loadBagData(bag);
                    storageItems[i] = bag;
                }
                int numItems = (int)props["numItems"];
                for (int i = 0; i < numItems; i++)
                {
                    int bagNum = (int)props["item_" + i + "BagNum"];
                    int slotNum = (int)props["item_" + i + "SlotNum"];
                    //string baseName = (string)props["item_" + i + "BaseName"];
                    int templateID = (int)props["item_" + i + "TemplateID"];
                    AtavismInventoryItem invInfo = LoadItemPrefabData(templateID);
                    AtavismLogger.LogDebugMessage("Got item: " + invInfo.BaseName);
                    //invInfo.copyData(GetGenericItemData(invInfo.baseName));
                    invInfo.Count = (int)props["item_" + i + "Count"];
                    //ClientAPI.Log("ITEM: item count for item %s is %s" % (invInfo.name, invInfo.count))
                    invInfo.ItemId = (OID)props["item_" + i + "Id"];
                    invInfo.name = (string)props["item_" + i + "Name"];
                    invInfo.IsBound = (bool)props["item_" + i + "Bound"];
                    invInfo.EnergyCost = (int)props["item_" + i + "EnergyCost"];
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
                    /*  int NumEStats = (int)props["item_" + i + "NumEStats"];
                      for (int j = 0; j < NumEStats; j++)
                      {
                          string statName = (string)props["item_" + i + "EStat_" + j + "Name"];
                          int statValue = (int)props["item_" + i + "EStat_" + j + "Value"];
                          invInfo.EnchantStats[statName] = statValue;
                      }
                      int ELevel = (int)props["item_" + i + "ELevel"];
                      invInfo.enchantLeval = ELevel;
                      //ClientAPI.Log("InventoryUpdateEntry fields: %s, %d, %d, %s" % (invInfo.itemId, bagNum, slotNum, invInfo.name))
                     */
                    if (invInfo.itemType == "Weapon")
                    {
                        invInfo.DamageValue = (int)props["item_" + i + "DamageValue"];
                        invInfo.DamageType = (string)props["item_" + i + "DamageType"];
                        invInfo.WeaponSpeed = (int)props["item_" + i + "Delay"];
                    }
                    storageItems[bagNum].items[slotNum] = invInfo;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("HandleBankInventoryUpdate  Exception:" + e);
            }
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("BANK_UPDATE", args);

            storageOpenLoc = ClientAPI.GetPlayerObject().Position;
        }

        public void HandleCurrencies(Dictionary<string, object> props)
        {
            try
            {
                sellFactor = (float)props["SellFactor"];
                int numCurrencies = (int)props["numCurrencies"];
                for (int i = 0; i < numCurrencies; i++)
                {
                    int currencyID = (int)props["currency" + i + "ID"];
                   // Debug.LogError(">"+props["currency" + i + "Current"]+"<");
                    if (currencies.ContainsKey(currencyID))
                        currencies[currencyID].Current = (long)props["currency" + i + "Current"];
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("HandleCurrencies  Exception:" + e);
            }
            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("CURRENCY_UPDATE", args);
        }

        public void HandleLootList(Dictionary<string, object> props)
        {
            loot.Clear();
            try
            {
                int numItems = (int)props["numItems"];
                AtavismLogger.LogDebugMessage("Got Loot list with num items: " + numItems);
                lootTarget = (OID)props["lootTarget"];
                for (int i = 0; i < numItems; i++)
                {
                    string name = (string)props["item_" + i + "Name"];
                    if (name == null || name == "")
                        continue;
                    string baseName = (string)props["item_" + i + "BaseName"];
                    AtavismInventoryItem invInfo = LoadItemPrefabData(baseName);
                    invInfo.name = name;
                    invInfo.Count = (int)props["item_" + i + "Count"];
                    invInfo.ItemId = (OID)props["item_" + i + "Id"];
                    invInfo.EnergyCost = (int)props["item_" + i + "EnergyCost"];
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
                    loot.Add(i, invInfo);
                    AtavismLogger.LogDebugMessage("Added loot item: " + invInfo.name + " to slot: " + i);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("HandleLootList  Exception:" + e);
            }
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("LOOT_UPDATE", args);
        }

        public void HandleInventoryEvent(Dictionary<string, object> props)
        {
            //  Debug.LogWarning("HandleInventoryEvent");
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
                AtavismEventSystem.DispatchEvent("INVENTORY_EVENT", args);
            }
            catch (System.Exception e)
            {
                Debug.LogError("HandleInventoryEvent  Exception:" + e);
            }
        }

        #endregion Message Handlers

        #region Properties
        public static Inventory Instance
        {
            get
            {
                return instance;
            }
        }

        public Dictionary<int, AtavismInventoryItem> Items
        {
            get
            {
                return items;
            }
        }

        public Dictionary<int, Bag> Bags
        {
            get
            {
                return bags;
            }
        }

        public Dictionary<int, AtavismInventoryItem> EquippedItems
        {
            get
            {
                return equippedItems;
            }
        }

        public AtavismInventoryItem EquippedAmmo
        {
            get
            {
                return equippedAmmo;
            }
        }

        public Dictionary<int, Bag> StorageItems
        {
            get
            {
                return storageItems;
            }
        }

        public Dictionary<int, Currency> Currencies
        {
            get
            {
                return currencies;
            }
        }

        public Dictionary<int, AtavismInventoryItem> Loot
        {
            get
            {
                return loot;
            }
        }

        public OID LootTarget
        {
            get
            {
                return lootTarget;
            }
        }
        #endregion Properties
    }
}
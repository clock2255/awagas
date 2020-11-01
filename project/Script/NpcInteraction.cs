using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class NpcInteractionEntry
    {
        public string interactionType;
        public string interactionTitle;
        public int interactionID;
        public OID npcId;

        public virtual void StartInteraction()
        {
            if (interactionType == "Bank")
            {
                Inventory.Instance.RequestBankInfo();
            }
            else
            {
                NetworkAPI.SendTargetedCommand(npcId.ToLong(), "/startInteraction " + interactionID + " "
                    + interactionType);
            }
        }
    }

    public class Dialogue
    {
        public int dialogueID = 0;
        public string title = "";
        public string text = "";
        public List<DialogueAction> actions = new List<DialogueAction>();
        public OID npcId;

        public void PerformDialogueAction(DialogueAction action)
        {
            action.StartInteraction();
        }
    }


    public class DialogueAction : NpcInteractionEntry
    {
        public int dialogueID;

        public override void StartInteraction()
        {
            NetworkAPI.SendTargetedCommand(npcId.ToLong(), "/dialogueOption " + dialogueID + " " + interactionType + " " + interactionID);
            AtavismLogger.LogDebugMessage("Dialogue Clicked");
        }
    }

    public class MerchantItem
    {
        public int itemID;
        public int count;
        public int cost;
        public int purchaseCurrency;
    }

    public class NpcInteraction : MonoBehaviour
    {
        static NpcInteraction instance;
        public OID NpcId;
        public float repairRate = 0.5f;

        // info about the last quests we were offered
        //int interactionOptionSelected = 0;
        List<NpcInteractionEntry> interactionOptions = new List<NpcInteractionEntry>();
        List<MerchantItem> merchantItems = new List<MerchantItem>();

        Dialogue dialogue;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            NetworkAPI.RegisterExtensionMessageHandler("npc_interactions", _HandleNpcInteractionOptionsResponse);
            NetworkAPI.RegisterExtensionMessageHandler("npc_dialogue", _HandleNpcDialogue);
            NetworkAPI.RegisterExtensionMessageHandler("close_dialogue", _HandleCloseDialogue);
            NetworkAPI.RegisterExtensionMessageHandler("MerchantList", _HandleMerchantList);
            NetworkAPI.RegisterExtensionMessageHandler("repair_start", _HandleRepair);
            NetworkAPI.RegisterExtensionMessageHandler("repair_successful", _HandleRepairComplete);
            NetworkAPI.RegisterExtensionMessageHandler("item_purchase_result", HandlePurchaseResponse);
        }

        // Update is called once per frame
        void Update()
        {
            if (NpcId != null && ClientAPI.GetObjectNode(NpcId.ToLong()) != null)
            {
                if (Vector3.Distance(ClientAPI.GetPlayerObject().Position, ClientAPI.GetObjectNode(NpcId.ToLong()).GameObject.transform.position) > 5)
                {
                    NpcId = null;
                    string[] args = new string[1];
                    AtavismEventSystem.DispatchEvent("CLOSE_NPC_DIALOGUE", args);
                }
            }
        }

        public void GetInteractionOptionsForNpc(long npcOid)
        {
            NetworkAPI.SendTargetedCommand(npcOid, "/getNpcInteractions");
        }

        public void PurchaseItemConfirmed(object item, bool response)
        {
            if (!response)
                return;

            MerchantItem mItem = (MerchantItem)item;
            PurchaseItemFromMerchant(mItem.itemID, mItem.count);
        }

        public void PurchaseItemFromMerchant(int itemID, int count)
        {
            NetworkAPI.SendTargetedCommand(NpcId.ToLong(), "/purchaseItem " + itemID + " " + count);
        }

        public MerchantItem GetMerchantItem(int itemPos)
        {
            if (merchantItems.Count > itemPos)
            {
                return merchantItems[itemPos];
            }
            return null;
        }

        public void SellItemToMerchant(AtavismInventoryItem item)
        {
            if (!item.sellable || item.cost < 1)
            {
                AtavismEventMessageHandler.Instance.ProcessErrorEvent("CannotSellItem", item.templateId, "");
                return;
            }
            long amount = item.cost;
            int currency = item.currencyType;
            Inventory.Instance.ConvertCurrencyToBaseCurrency(item.currencyType, item.cost, out currency, out amount);
            int price = (int)Mathf.Round(amount * Inventory.Instance.sellFactor);
            if (price < 1)
                price = 1;
            string costString = Inventory.Instance.GetCostString(currency, item.Count * price);
#if AT_I2LOC_PRESET
        UGUIConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Do you want to sell") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + item.name) + " " + I2.Loc.LocalizationManager.GetTranslation("for") + " " + costString + "?", item, SellItemConfirmed);
#else
            UGUIConfirmationPanel.Instance.ShowConfirmationBox("Do you want to sell " + item.name + " for " + costString + "?", item, SellItemConfirmed);
#endif
        }

        public void SellItemConfirmed(object item, bool response)
        {
            if (!response)
                return;

            AtavismInventoryItem mItem = (AtavismInventoryItem)item;
            SellItemToMerchant(mItem.ItemId, mItem.Count);
        }

        public void SellItemToMerchant(OID itemOid, int count)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("sellType", "");
            props.Add("itemOid", itemOid);
            props.Add("count", count);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "inventory.SELL_ITEM", props);
        }

        public void RepairItem(AtavismInventoryItem item)
        {
            float cost = item.cost * repairRate * (item.MaxDurability - item.Durability) / item.MaxDurability;
            string costString = Inventory.Instance.GetCostString(item.currencyType, (int)(cost * item.Count * Inventory.Instance.sellFactor));
#if AT_I2LOC_PRESET
        UGUIConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Do you want to repair") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + item.name) + " for " + costString + "?", item, RepairItemConfirmed);
#else
            UGUIConfirmationPanel.Instance.ShowConfirmationBox("Do you want to repair " + item.name + " for " + costString + "?", item, RepairItemConfirmed);
#endif
        }

        public void RepairItemConfirmed(object item, bool response)
        {
            if (!response)
                return;

            AtavismInventoryItem mItem = (AtavismInventoryItem)item;
            SendRepairItem(mItem.ItemId);
        }

        public void SendRepairItem(OID itemOid)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("repairAll", false);
            props.Add("itemOid", itemOid);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.REPAIR_ITEMS", props);
        }

        public void RepairItems(List<AtavismInventoryItem> items)
        {
            int currencyType = 3;
            float cost = 0;
            foreach (AtavismInventoryItem item in items)
            {
                cost += item.cost * repairRate * (item.MaxDurability - item.Durability) / item.MaxDurability;
            }
            string costString = Inventory.Instance.GetCostString(currencyType, (int)cost);
#if AT_I2LOC_PRESET
        UGUIConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Do you want to repair these items for") + " " + costString + "?", items, RepairItemsConfirmed);
#else
            UGUIConfirmationPanel.Instance.ShowConfirmationBox("Do you want to repair these items for " + costString + "?", items, RepairItemsConfirmed);
#endif
        }

        public void RepairItemsConfirmed(object item, bool response)
        {
            if (!response)
                return;

            List<AtavismInventoryItem> mItem = (List<AtavismInventoryItem>)item;
            SendRepairItems(mItem);
        }

        public void SendRepairItems(List<AtavismInventoryItem> items)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("repairAll", false);
            props.Add("numItems", items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                props.Add("itemOid" + i, items[i].ItemId);
            }
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.REPAIR_ITEMS", props);
        }

        public void RepairAllItems()
        {
            int currencyType = 3;
            float cost = 0;
            foreach (AtavismInventoryItem item in Inventory.Instance.EquippedItems.Values)
            {
                if (item.MaxDurability > 0)
                    cost += item.cost * repairRate * (item.MaxDurability - item.Durability) / item.MaxDurability;
            }
            foreach (Bag bag in Inventory.Instance.Bags.Values)
            {
                foreach (AtavismInventoryItem item in bag.items.Values)
                {
                    if (item.MaxDurability > 0)
                        cost += item.cost * repairRate * (item.MaxDurability - item.Durability) / item.MaxDurability;
                }
            }

            string costString = Inventory.Instance.GetCostString(currencyType, (int)cost);
            //string costString = "some currency";
#if AT_I2LOC_PRESET
        UGUIConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Do you want to repair all items for") + " " + costString + "?", null, RepairAllItemsConfirmed);
#else
            UGUIConfirmationPanel.Instance.ShowConfirmationBox("Do you want to repair all items for " + costString + "?", null, RepairAllItemsConfirmed);
#endif
        }

        public void RepairAllItemsConfirmed(object item, bool response)
        {
            if (!response)
                return;

            SendRepairAllItems();
        }

        public void SendRepairAllItems()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("repairAll", true);
            props.Add("itemOid", null);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.REPAIR_ITEMS", props);
        }

        #region Message Handlers
        void _HandleNpcInteractionOptionsResponse(Dictionary<string, object> props)
        {
            // update the information about the interaction options on offer from this npc
            interactionOptions.Clear();
            int numInteractions = (int)props["numInteractions"];
            NpcId = (OID)props["npcOid"];
            for (int i = 0; i < numInteractions; i++)
            {
                NpcInteractionEntry entry = new NpcInteractionEntry();
                entry.interactionType = (string)props["interactionType_" + i];
                //entry.interactionTitle = (string)props ["interactionTitle_" + i];
                string sComplete = "";

                if (props["interactionValue_" + i] != null && (bool)props["interactionValue_" + i])
#if AT_I2LOC_PRESET
                sComplete = string.Concat(" (", I2.Loc.LocalizationManager.GetTranslation("Complete"), ")");
#else
                    sComplete = " (Complete)";
#endif

                entry.interactionTitle = (string)props["interactionTitle_" + i] + sComplete;

                entry.interactionID = (int)props["interactionID_" + i];
                entry.npcId = NpcId;

                interactionOptions.Add(entry);

                //ClientAPI.Write("Quest grades: %s" % logEntry.grades)
            }

            string dialogueText = (string)props["dialogue_text"];
            if (dialogueText != "")
            {
                dialogue = new Dialogue();
                dialogue.text = dialogueText;
            }
            else
            {
                dialogue = null;
            }

            //
            // dispatch a ui event to tell the rest of the system
            //
            string[] args = new string[1];
            // args [0] = npcID.ToString ();
            AtavismEventSystem.DispatchEvent("NPC_INTERACTIONS_UPDATE", args);
        }

        void _HandleNpcDialogue(Dictionary<string, object> props)
        {
            // update our idea of the state
            //QuestLogEntry logEntry = null;
            NpcId = (OID)props["npcOid"];
            dialogue = new Dialogue();
            dialogue.dialogueID = (int)props["dialogueID"];
            dialogue.title = (string)props["title"];
            dialogue.text = (string)props["text"];
            dialogue.npcId = NpcId;

            int numOptions = (int)props["numOptions"];
            for (int i = 0; i < numOptions; i++)
            {
                DialogueAction action = new DialogueAction();
                action.interactionType = (string)props["option" + i + "action"];
                action.interactionID = (int)props["option" + i + "actionID"];
                action.interactionTitle = (string)props["option" + i + "text"];
                action.dialogueID = dialogue.dialogueID;
                action.npcId = NpcId;
                dialogue.actions.Add(action);
            }

            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("DIALOGUE_UPDATE", args);
        }

        void _HandleCloseDialogue(Dictionary<string, object> props)
        {
            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("CLOSE_NPC_DIALOGUE", args);
        }

        void _HandleRepair(Dictionary<string, object> props)
        {
            //   Debug.LogError("_HandleRepair");
            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("REPAIR_START", args);
            //   Debug.LogError("_HandleRepair End");
        }
        void _HandleRepairComplete(Dictionary<string, object> props)
        {
            //   Debug.LogError("_HandleRepair");
            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("REPAIR_COMPLETE", args);
            //   Debug.LogError("_HandleRepair End");
        }

        void _HandleMerchantList(Dictionary<string, object> props)
        {
            merchantItems.Clear();
            NpcId = (OID)props["npcOid"];

            int numItems = (int)props["numItems"];
            for (int i = 0; i < numItems; i++)
            {
                MerchantItem merchantItem = new MerchantItem();
                merchantItem.itemID = (int)props["item_" + i + "ID"];
                merchantItem.count = (int)props["item_" + i + "Count"];
                merchantItem.cost = (int)props["item_" + i + "Cost"];
                merchantItem.purchaseCurrency = (int)props["item_" + i + "Currency"];
                merchantItems.Add(merchantItem);
            }

            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            args[0] = NpcId.ToString();
            AtavismEventSystem.DispatchEvent("MERCHANT_UPDATE", args);
        }

        public void HandlePurchaseResponse(Dictionary<string, object> props)
        {
            string result = (string)props["result"];
            if (result == "insufficient_funds")
            {
                // dispatch a ui event to tell the rest of the system
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("SOUND_FAILED", args);

                string currencyName = (string)props["currency"];
#if AT_I2LOC_PRESET
            args[0] = I2.Loc.LocalizationManager.GetTranslation("You do not have enough") + " " + currencyName + " " + I2.Loc.LocalizationManager.GetTranslation("to purchase that item");
#else
                args[0] = "You do not have enough " + currencyName + " to purchase that item";
#endif
                AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
            }
            else if (result == "insufficient_space")
            {
                // dispatch a ui event to tell the rest of the system
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("SOUND_FAILED", args);
#if AT_I2LOC_PRESET
            args[0] = I2.Loc.LocalizationManager.GetTranslation("You do not have any space in your bags to purchase that item");
#else
                args[0] = "You do not have any space in your bags to purchase that item";
#endif
                AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
            }
            else if (result == "no_item")
            {
                // dispatch a ui event to tell the rest of the system
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("SOUND_FAILED", args);
#if AT_I2LOC_PRESET
            args[0] = I2.Loc.LocalizationManager.GetTranslation("The merchant does not have that item available for purchase");
#else
                args[0] = "The merchant does not have that item available for purchase";
#endif
                AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
            }
            else if (result == "success")
            {
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("SOUND_SUCCESS", args);
            }
        }

        #endregion Message Handlers

        #region Properties
        public static NpcInteraction Instance
        {
            get
            {
                return instance;
            }
        }

        public List<NpcInteractionEntry> InteractionOptions
        {
            get
            {
                return interactionOptions;
            }
        }

        public Dialogue Dialogue
        {
            get
            {
                return dialogue;
            }
        }

        public List<MerchantItem> MerchantItems
        {
            get
            {
                return merchantItems;
            }
        }

        #endregion Properties
    }
}
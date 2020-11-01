using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class AtavismTrade : MonoBehaviour
    {

        static AtavismTrade instance;

        public int tradeSlotCount = 6;
        List<AtavismInventoryItem> myOffers;
        List<AtavismInventoryItem> theirOffers;
        Dictionary<string, int> myCurrencyOffers;
        List<CurrencyDisplay> theirCurrencyOffers;
        OID tradePartnerOid = null;
        bool acceptedByMe = false;
        bool acceptedByPartner = false;
        bool cancelled = false;
        [SerializeField] int interactionDistance = 7;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            myOffers = new List<AtavismInventoryItem>();
            for (int i = 0; i < tradeSlotCount; i++)
            {
                myOffers.Add(null);
            }
            theirOffers = new List<AtavismInventoryItem>();
            for (int i = 0; i < tradeSlotCount; i++)
            {
                theirOffers.Add(null);
            }
            myCurrencyOffers = new Dictionary<string, int>();
            theirCurrencyOffers = new List<CurrencyDisplay>();

            NetworkAPI.RegisterExtensionMessageHandler("ao.TRADE_START", HandleTradeStart);
            NetworkAPI.RegisterExtensionMessageHandler("ao.TRADE_START_REQ_PARTNER", HandleTradeRequest);
            NetworkAPI.RegisterExtensionMessageHandler("ao.TRADE_OFFER_UPDATE", HandleTradeOfferUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("ao.TRADE_COMPLETE", HandleTradeComplete);
        }

        private void OnDestroy()
        {
            NetworkAPI.RemoveExtensionMessageHandler("ao.TRADE_START", HandleTradeStart);
            NetworkAPI.RemoveExtensionMessageHandler("ao.TRADE_START_REQ_PARTNER", HandleTradeRequest);
            NetworkAPI.RemoveExtensionMessageHandler("ao.TRADE_OFFER_UPDATE", HandleTradeOfferUpdate);
            NetworkAPI.RemoveExtensionMessageHandler("ao.TRADE_COMPLETE", HandleTradeComplete);
        }
        /// <summary>
        /// Called when a new trade is started. Clears out currencies
        /// </summary>
        public void NewTradeStarted()
        {
            myCurrencyOffers = new Dictionary<string, int>();
        }

        public AtavismInventoryItem GetTradeItemInfo(bool myOffer, int slotId)
        {
            if (myOffer)
            {
                if (slotId < myOffers.Count)
                {
                    return myOffers[slotId];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (slotId < theirOffers.Count)
                {
                    return theirOffers[slotId];
                }
                else
                {
                    return null;
                }
            }
        }

        public void SendTradeOfferMessage(List<object> itemOids, Dictionary<string, object> currencies)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("requesterOid", OID.fromLong(ClientAPI.GetPlayerOid()));
            props.Add("partnerOid", tradePartnerOid);
            props.Add("offerItems", itemOids);
            props.Add("offerCurrencies", currencies);
            props.Add("accepted", acceptedByMe);
            props.Add("cancelled", cancelled);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.TRADE_OFFER_REQ", props);
        }

        public void SendTradeOffer()
        {
            List<object> itemOids = new List<object>();
            int maxSlotId = myOffers.Count;
            for (int i = 0; i < maxSlotId; i++)
            {
                if (myOffers[i] != null)
                {
                    itemOids.Add(myOffers[i].ItemId);
                }
                else
                {
                    itemOids.Add(null);
                }
            }

            Dictionary<string, object> currencies = new Dictionary<string, object>();
            foreach (string currencyID in myCurrencyOffers.Keys)
            {
                currencies.Add(currencyID, myCurrencyOffers[currencyID]);
            }
            SendTradeOfferMessage(itemOids, currencies);
        }

        public void ItemPlacedInTradeSlot(AtavismInventoryItem item, int slotId, bool send)
        {
            if (myOffers.Count < slotId)
            {
                return;
            }
            myOffers[slotId] = item;
            if (send)
                SendTradeOffer();
        }

        public void SetCurrencyAmount(int currencyID, int amount)
        {
            myCurrencyOffers[currencyID.ToString()] = amount;
            SendTradeOffer();
        }

        public void AcceptTrade()
        {
            acceptedByMe = true;
            cancelled = false;
            SendTradeOffer();
        }

        public void CancelTrade()
        {
            acceptedByMe = false;
            cancelled = true;
            SendTradeOffer();
            tradePartnerOid = null;
        }

        void ConvertList(bool myOfferList, List<object> itemList)
        {
            int slotId = 0;

            foreach (object obj in itemList)
            {
                List<object> itemData = (List<object>)obj;
                int itemID = (int)itemData[1];
                if (myOfferList)
                {
                    AtavismInventoryItem item = Inventory.Instance.GetInventoryItem((OID)itemData[0]);
                    if (item != null)
                        item.Count = (int)itemData[2];
                    myOffers[slotId] = item;
                }
                else
                {
                    AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(itemID);
                    if (item != null)
                        item.Count = (int)itemData[2];
                    theirOffers[slotId] = item;
                }
                slotId++;
            }
        }

        public void HandleTradeStart(Dictionary<string, object> props)
        {
            myOffers.Clear();
            for (int i = 0; i < tradeSlotCount; i++)
            {
                myOffers.Add(null);
            }
            theirOffers.Clear();
            for (int i = 0; i < tradeSlotCount; i++)
            {
                theirOffers.Add(null);
            }
            acceptedByMe = false;
            acceptedByPartner = false;
            cancelled = false;
            long partnerOid = (long)props["ext_msg_subject_oid"];
            tradePartnerOid = OID.fromLong(partnerOid);
            myCurrencyOffers.Clear();
            theirCurrencyOffers.Clear();
            // dispatch a ui event to tell the rest of the system
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("TRADE_START", event_args);
        }


        public void HandleTradeRequest(Dictionary<string, object> props)
        {
            myOffers.Clear();
            for (int i = 0; i < tradeSlotCount; i++)
            {
                myOffers.Add(null);
            }
            theirOffers.Clear();
            for (int i = 0; i < tradeSlotCount; i++)
            {
                theirOffers.Add(null);
            }
            acceptedByMe = false;
            acceptedByPartner = false;
            cancelled = false;

            tradePartnerOid = (OID)props["requesterOid"];
            long partnerOid = tradePartnerOid.ToLong();
            int count = (int)props["tradeInviteTimeout"];
            myCurrencyOffers.Clear();

            theirCurrencyOffers.Clear();
            // dispatch a ui event to tell the rest of the system
#if AT_I2LOC_PRESET
        UGUIConfirmationPanel.Instance.ShowConfirmationBox(ClientAPI.GetObjectNode(partnerOid).Name + " " + I2.Loc.LocalizationManager.GetTranslation("has invited you to trade"), tradePartnerOid, TradeInviteResponse, count);
#else
            UGUIConfirmationPanel.Instance.ShowConfirmationBox(ClientAPI.GetObjectNode(partnerOid).Name + " has invited you to trade", tradePartnerOid, TradeInviteResponse, count);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inviter"></param>
        /// <param name="accepted"></param>
        public void TradeInviteResponse(object inviter, bool accepted)
        {
            if (accepted)
            {
                //   string[] event_args = new string[1];
                //  AtavismEventSystem.DispatchEvent("TRADE_START", event_args);
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add("requesterOid", (OID)inviter);
                props.Add("partnerOid", OID.fromLong(ClientAPI.GetPlayerOid()));
                props.Add("response", "accept");
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.TRADE_START_REQ_RESPONSE", props);
            }
            else
            {
                CancelTrade();
            }
        }


        void Update()
        {
            if (tradePartnerOid != null)
            {
                if (ClientAPI.GetPlayerObject() != null && ClientAPI.GetObjectNode(tradePartnerOid.ToLong()) != null)
                    if (Vector3.Distance(ClientAPI.GetPlayerObject().Position, ClientAPI.GetObjectNode(tradePartnerOid.ToLong()).Position) > interactionDistance)
                    {
                        CancelTrade();

                    }
            }

        }


        public void HandleTradeOfferUpdate(Dictionary<string, object> props)
        {
            ConvertList(true, (List<object>)props["offer1"]);
            ConvertList(false, (List<object>)props["offer2"]);
            myCurrencyOffers.Clear();
            Dictionary<string, object> myCurrencies = (Dictionary<string, object>)props["currencyOffer1"];
            foreach (string currencyID in myCurrencies.Keys)
            {
                myCurrencyOffers.Add(currencyID, (int)myCurrencies[currencyID]);
            }
            theirCurrencyOffers.Clear();
            Dictionary<string, object> theirCurrencies = (Dictionary<string, object>)props["currencyOffer2"];
            foreach (string currencyID in theirCurrencies.Keys)
            {
                CurrencyDisplay currencyDisplay = Inventory.Instance.GenerateCurrencyDisplay(int.Parse(currencyID), (int)theirCurrencies[currencyID]);
                theirCurrencyOffers.Add(currencyDisplay);
            }
            acceptedByMe = (bool)props["accepted1"];
            acceptedByPartner = (bool)props["accepted2"];
            cancelled = false;

            // dispatch a ui event to tell the rest of the system
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("TRADE_OFFER_UPDATE", event_args);
        }

        public void HandleTradeComplete(Dictionary<string, object> props)
        {
            // dispatch a ui event to tell the rest of the system
            string[] event_args = new string[1];
            byte status = (byte)props["status"];
            if (status == 2 || status == 3)
            {
#if AT_I2LOC_PRESET
   			event_args[0] = I2.Loc.LocalizationManager.GetTranslation("Trade Cancelled");
#else
                event_args[0] = "Trade Cancelled";
#endif
                UGUIConfirmationPanel.Instance.Hide();
                AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", event_args);
                event_args = new string[1];
            }
            else if (status == 1)
            {
#if AT_I2LOC_PRESET
   			event_args[0] = I2.Loc.LocalizationManager.GetTranslation("Trade Completed");
#else
                event_args[0] = "Trade Completed";
#endif
                AtavismEventSystem.DispatchEvent("ANNOUNCEMENT", event_args);
            }

            AtavismEventSystem.DispatchEvent("TRADE_COMPLETE", event_args);
        }

        public static AtavismTrade Instance
        {
            get
            {
                return instance;
            }
        }

        public List<AtavismInventoryItem> MyOffers
        {
            get
            {
                return myOffers;
            }
        }

        public List<AtavismInventoryItem> TheirOffers
        {
            get
            {
                return theirOffers;
            }
        }

        public Dictionary<string, int> MyCurrencyOffers
        {
            get
            {
                return myCurrencyOffers;
            }
        }

        public List<CurrencyDisplay> TheirCurrencyOffers
        {
            get
            {
                return theirCurrencyOffers;
            }
        }

        public OID TradePartnerOid
        {
            get
            {
                return tradePartnerOid;
            }
        }

        public bool AcceptedByMe
        {
            get
            {
                return acceptedByMe;
            }
        }

        public bool AcceptedByPartner
        {
            get
            {
                return acceptedByPartner;
            }
        }
        public int InteractionDistance
        {
            get
            {
                return interactionDistance;
            }
        }
    }
}
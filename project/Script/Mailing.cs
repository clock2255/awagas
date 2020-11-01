using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class MailItem
    {
        public AtavismInventoryItem item = null;
        public int count = 1;
    }

    public class MailEntry
    {
        public int mailId = -1;
        public OID senderOid;
        public string senderName = "";
        public string subject = "";
        public string message = "";
        public List<MailItem> items = new List<MailItem>();
        public Dictionary<Currency, int> currencies = new Dictionary<Currency, int>();
        public bool cashOnDelivery = false;
        public bool read = false;

        public Currency GetMainCurrency()
        {
            foreach (Currency c in currencies.Keys)
            {
                return c;
            }
            return null;
        }
    }

    public class Mailing : MonoBehaviour
    {

        static Mailing instance;
        public int itemLimit = 10;
        List<MailEntry> mailList = new List<MailEntry>();
        MailEntry selectedMail;
        MailEntry mailBeingComposed;
        Vector3 mailboxLocation = Vector3.zero;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            NetworkAPI.RegisterExtensionMessageHandler("MailList", HandleMailList);
            NetworkAPI.RegisterExtensionMessageHandler("MailSendResult", HandleMailSendResult);
        }

        // Update is called once per frame
        void Update()
        {
            if (mailboxLocation != Vector3.zero)
            {
                if (Vector3.Distance(ClientAPI.GetPlayerObject().Position, mailboxLocation) > 5)
                {
                    mailboxLocation = Vector3.zero;
                    string[] args = new string[1];
                    AtavismEventSystem.DispatchEvent("CLOSE_MAIL_WINDOW", args);
                }
            }
        }

        #region Mail Reading Functions

        public void RequestMailList(Vector3 location)
        {
            RequestMailList();
            this.mailboxLocation = location;
            //dispatch an event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("SHOW_MAIL", args);
        }

        public void RequestMailList()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "inventory.GET_MAIL", props);
        }

        public MailEntry GetMailEntry(int pos)
        {
            if (mailList.Count > pos)
            {
                return mailList[pos];
            }
            else
            {
                return null;
            }
        }

        public MailEntry GetMailEntryById(int id)
        {
            foreach (MailEntry entry in mailList)
            {
                if (entry.mailId == id)
                {
                    return entry;
                }
            }
            return null;
        }

        public Currency GetMailCurrencyType()
        {
            foreach (Currency c in selectedMail.currencies.Keys)
            {
                return c;
            }
            return null;
        }

        public void SetMailRead(MailEntry mail)
        {
            mail.read = true;
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("mailID", mail.mailId);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "inventory.MAIL_READ", props);
        }

        public void ReturnMail(MailEntry mail)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("mailID", mail.mailId);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "inventory.RETURN_MAIL", props);
        }

        public void DeleteMail(MailEntry mail)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("mailID", mail.mailId);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "inventory.DELETE_MAIL", props);
        }

        public void TakeMailItem(int itemNum)
        {
            TakeMailItem(selectedMail, itemNum);
        }

        public void TakeMailItem(MailEntry mail, int itemNum)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("mailID", mail.mailId);
            props.Add("itemPos", itemNum);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "inventory.MAIL_TAKE_ITEM", props);
            //ClientAPI.Write("Send take mail command with mail ID: " + mail.mailId);
        }

        public void TakeMailCurrency(MailEntry mail)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("mailID", mail.mailId);
            props.Add("itemPos", -1);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "inventory.MAIL_TAKE_ITEM", props);
            ;
        }

        #endregion Mail Reading Functions

        #region Mail Writing Functions

        public void StartComposingMail()
        {
            StartComposingMail("");
        }

        public void StartComposingMail(string recipient)
        {
            mailBeingComposed = new MailEntry();
            mailBeingComposed.senderName = recipient;
            // Items
            List<MailItem> items = new List<MailItem>();
            for (int i = 0; i < itemLimit; i++)
            {
                items.Add(new MailItem());
            }
            mailBeingComposed.items = items;

            // Currency
            foreach (Currency c in Inventory.Instance.GetMainCurrencies())
            {
                mailBeingComposed.currencies.Add(c, 0);
            }
        }

        public void SetMailItem(int gridPos, AtavismInventoryItem item)
        {

            if (item == null)
            {
                if (mailBeingComposed.items[gridPos].item != null)
                    mailBeingComposed.items[gridPos].item.AlterUseCount(-mailBeingComposed.items[gridPos].count);
                mailBeingComposed.items[gridPos].item = null;
                mailBeingComposed.items[gridPos].count = 1;
            }
            else if (mailBeingComposed.items[gridPos].item == item)
            {
                mailBeingComposed.items[gridPos].count++;
            }
            else
            {
                mailBeingComposed.items[gridPos].item = item;
                mailBeingComposed.items[gridPos].count = 1;
            }

            if (item != null)
                item.AlterUseCount(1);
        }

        public void SetMailCurrencyAmount(int position, int amount)
        {
            Currency currencyAltered = null;
            foreach (Currency c in mailBeingComposed.currencies.Keys)
            {
                if (c.position == position)
                {
                    currencyAltered = c;
                    break;
                }
            }

            if (currencyAltered != null)
            {
                mailBeingComposed.currencies[currencyAltered] = amount;
            }
        }

        public void SendMail(MailEntry mailToSend)
        {
            mailBeingComposed = mailToSend;
            SendMail();
        }

        public void SendMail()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("recipient", mailBeingComposed.senderName);
            props.Add("subject", mailBeingComposed.subject);
            props.Add("message", mailBeingComposed.message);
            // Items
            int itemPos = 0;
            foreach (MailItem mailItem in mailBeingComposed.items)
            {
                if (mailItem.item != null)
                {
                    props.Add("item" + itemPos, mailItem.item.ItemId);
                }
                else
                {
                    props.Add("item" + itemPos, null);
                }
                itemPos++;
            }
            props.Add("numItems", mailBeingComposed.items.Count);

            // Currencies
            int currencyPos = 0;
            foreach (Currency currencyType in mailBeingComposed.currencies.Keys)
            {
                if (currencyType != null)
                {
                    props.Add("currencyType" + currencyPos, currencyType.id);
                    props.Add("currencyAmount" + currencyPos, mailBeingComposed.currencies[currencyType]);
                }
                currencyPos++;
            }

            props.Add("numCurrencies", mailBeingComposed.currencies.Count);
            props.Add("CoD", mailBeingComposed.cashOnDelivery);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "inventory.SEND_MAIL", props);
        }

        #endregion Mail Writing Functions

        public void HandleMailList(Dictionary<string, object> props)
        {
            mailList = new List<MailEntry>();
            int numMail = (int)props["numMail"];
            for (int i = 0; i < numMail; i++)
            {
                MailEntry entry = new MailEntry();
                entry.mailId = (int)props["mail_" + i + "ID"];
                entry.senderOid = (OID)props["mail_" + i + "SenderOid"];
                entry.senderName = (string)props["mail_" + i + "SenderName"];
                entry.subject = (string)props["mail_" + i + "Subject"];
                entry.message = (string)props["mail_" + i + "Message"];
                List<MailItem> items = new List<MailItem>();
                //TODO: put item reading code here
                int numItems = (int)props["mail_" + i + "NumItems"];
                for (int j = 0; j < numItems; j++)
                {
                    MailItem mailItem = new MailItem();
                    int itemTemplate = (int)props["mail_" + i + "ItemTemplate" + j];
                    if (itemTemplate > 0)
                    {
                        mailItem.item = Inventory.Instance.GetItemByTemplateID(itemTemplate);
                        mailItem.count = (int)props["mail_" + i + "ItemCount" + j];
                    }
                    items.Add(mailItem);
                }
                entry.items = items;

                // Currency
                int currencyType = (int)props["mail_" + i + "CurrencyType"];
                int currencyAmount = (int)props["mail_" + i + "CurrencyAmount"];
                if (currencyType > 0)
                    entry.currencies.Add(Inventory.Instance.GetCurrency(currencyType), currencyAmount);
                entry.cashOnDelivery = (bool)props["mail_" + i + "CoD"];
                entry.read = (bool)props["mail_" + i + "Read"];
                mailList.Add(entry);

                // Update the mail being read
                if (selectedMail != null && selectedMail.mailId == entry.mailId)
                {
                    selectedMail = entry;
                }
            }


            //dispatch an event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("MAIL_UPDATE", args);
        }

        public void HandleMailSendResult(Dictionary<string, object> props)
        {
            string result = (string)props["result"];
            ClientAPI.Write("Mail result: " + result);

            if (result == "Success")
            {
                //dispatch an event to tell the rest of the system
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("MAIL_SENT", args);

                // Send announcement message
                args = new string[1];
                args[0] = "Mail Sent";
                AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
            }
        }

        #region Properties
        public static Mailing Instance
        {
            get
            {
                return instance;
            }
        }

        public MailEntry MailBeingComposed
        {
            get
            {
                return mailBeingComposed;
            }
        }

        public List<MailEntry> MailList
        {
            get
            {
                return mailList;
            }
        }

        public Vector3 MailboxLocation
        {
            get
            {
                return mailboxLocation;
            }
        }

        public MailEntry SelectedMail
        {
            get
            {
                return selectedMail;
            }
            set
            {
                selectedMail = value;
                // Send out event to show this mail
                //dispatch an event to tell the rest of the system
                string[] args = new string[1];
                // args[1] = value.mailId.ToString();
                AtavismEventSystem.DispatchEvent("MAIL_SELECTED", args);
            }
        }
        #endregion Properties
    }
}
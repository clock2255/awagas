using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Atavism
{

    public enum ActionType
    {
        Ability,
        Item,
        None
    }

    public class AtavismAction
    {
        public ActionType actionType;
        public Activatable actionObject;
        public int bar;
        public int slot;

        public void Activate()
        {
            if (actionObject != null)
                actionObject.Activate();
        }

        public void DrawTooltip(float x, float y)
        {
            actionObject.DrawTooltip(x, y);
        }
    }

    public class Actions : MonoBehaviour
    {

        static Actions instance;

        public bool removeEmptyItemsFromActionBar = true;
        List<List<AtavismAction>> actions = new List<List<AtavismAction>>();
        int mainActionBar = 0; // Which bar is currently sitting in the main Action Bar UI

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            NetworkAPI.RegisterExtensionMessageHandler("actions", HandleActionsUpdate);

            // Listen for the Abilities and Inventory updates as the action bar may need to be updated to match
            AtavismEventSystem.RegisterEvent("ABILITY_UPDATE", this);
            AtavismEventSystem.RegisterEvent("INVENTORY_UPDATE", this);
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("ABILITY_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("INVENTORY_UPDATE", this);
        }

        void ClientReady()
        {
            //ClientAPI.WorldManager.RegisterObjectPropertyChangeHandler("actions", ActionsPropertyHandler);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "INVENTORY_UPDATE")
            {
                if (!removeEmptyItemsFromActionBar)
                {
                    return;
                }

                for (int i = 0; i < actions.Count; i++)
                {
                    for (int j = 0; j < actions[i].Count; j++)
                    {
                        AtavismAction action = actions[i][j];
                        if (action != null && action.actionObject != null && action.actionType == ActionType.Item)
                        {
                            // verify the item count is still > 0
                            AtavismInventoryItem actionItem = (AtavismInventoryItem)action.actionObject;
                            if (Inventory.Instance.GetCountOfItem(actionItem.templateId) < 1)
                            {
                                SetAction(i, j, null, false, 0, 0);
                            }
                        }
                    }
                }
            }
        }

        public void SetAction(int bar, int slot, Activatable action, bool movingSlot, int sourceBar, int sourceSlot)
        {
            string actionString = "";
            if (action is AtavismAbility)
            {
                AtavismAbility ability = (AtavismAbility)action;
                actionString = "a" + ability.id;
            }
            else if (action is AtavismInventoryItem)
            {
                AtavismInventoryItem item = (AtavismInventoryItem)action;
                actionString = "i" + item.templateId;
            }
            //NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/updateActionBar " + slot + " " + actionString);
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("bar", bar);
            props.Add("slot", slot);
            props.Add("action", actionString);
            props.Add("movingSlot", movingSlot);
            props.Add("sourceBar", sourceBar);
            props.Add("sourceSlot", sourceSlot);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "combat.UPDATE_ACTIONBAR", props);
        }

        void UpdateActions()
        {
            if (ClientAPI.GetPlayerObject() == null || !ClientAPI.GetPlayerObject().PropertyExists("actions"))
                return;
            actions.Clear();

            List<object> actions_prop = (List<object>)ClientAPI.GetPlayerObject().GetProperty("actions");
            AtavismLogger.LogDebugMessage("Got player actions property change: " + actions_prop);
            int pos = 0;
            //	int bar = 0;

            //foreach (List<object> actionList in actions_prop) {
            List<AtavismAction> actionBar = new List<AtavismAction>();
            foreach (string actionString in actions_prop)
            {
                AtavismAction action = new AtavismAction();
                if (actionString.StartsWith("a"))
                {
                    action.actionType = ActionType.Ability;
                    int abilityID = int.Parse(actionString.Substring(1));
                    action.actionObject = GetComponent<Abilities>().GetAbility(abilityID);
                }
                else if (actionString.StartsWith("i"))
                {
                    action.actionType = ActionType.Item;
                    int itemID = int.Parse(actionString.Substring(1));
                    action.actionObject = Inventory.Instance.GetItemByTemplateID(itemID);
                }
                else
                {
                    action.actionType = ActionType.None;
                }
                action.slot = pos;
                //if (actionBars[bar] != null)
                //	actionBars[bar].SendMessage("ActionUpdate", action);
                pos++;
                actionBar.Add(action);
            }
            actions.Add(actionBar);
            //}
            // dispatch a ui event to tell the rest of the system
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("ACTION_UPDATE", event_args);
        }

        public void HandleActionsUpdate(Dictionary<string, object> props)
        {
            AtavismLogger.LogInfoMessage("Got Actions Update");
            try
            {
                actions.Clear();
                mainActionBar = (int)props["currentBar"];
                int numBars = (int)props["numBars"];
                for (int i = 0; i < numBars; i++)
                {
                    List<AtavismAction> actionBar = new List<AtavismAction>();
                    int barActionCount = (int)props["barActionCount" + i];
                    for (int j = 0; j < barActionCount; j++)
                    {
                        AtavismAction action = new AtavismAction();
                        string actionString = (string)props["bar" + i + "action" + j];
                        if (actionString.StartsWith("a"))
                        {
                            action.actionType = ActionType.Ability;
                            int abilityID = int.Parse(actionString.Substring(1));
                            action.actionObject = GetComponent<Abilities>().GetAbility(abilityID);
                        }
                        else if (actionString.StartsWith("i"))
                        {
                            action.actionType = ActionType.Item;
                            int itemID = int.Parse(actionString.Substring(1));
                            action.actionObject = Inventory.Instance.GetItemByTemplateID(itemID);
                        }
                        else
                        {
                            action.actionType = ActionType.None;
                        }
                        action.slot = j;
                        actionBar.Add(action);
                    }
                    actions.Add(actionBar);
                }

                // dispatch a ui event to tell the rest of the system
                string[] event_args = new string[1];
                AtavismEventSystem.DispatchEvent("ACTION_UPDATE", event_args);
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("Auction.HandleActionsUpdate Exeption " + e.Message);
            }
            AtavismLogger.LogDebugMessage("HandleActionsUpdate End");
        }

        /*public void ActionsPropertyHandler(object sender, ObjectPropertyChangeEventArgs args) {
            if (args.Oid != ClientAPI.GetPlayerOid())
                return;
            UpdateActions();
        }*/

        public void AddActionBar(GameObject actionBar, int slot)
        {
        }

        public static Actions Instance
        {
            get
            {
                return instance;
            }
        }

        public List<List<AtavismAction>> PlayerActions
        {
            get
            {
                return actions;
            }
        }

        public int MainActionBar
        {
            get
            {
                return mainActionBar;
            }
        }
    }
}
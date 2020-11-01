using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class AtavismEventMessageHandler : MonoBehaviour
    {

        static AtavismEventMessageHandler instance;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            NetworkAPI.RegisterExtensionMessageHandler("general_event", HandleGeneralEvent);
            NetworkAPI.RegisterExtensionMessageHandler("error_event", HandleErrorEvent);
            NetworkAPI.RegisterExtensionMessageHandler("requirement_failed_event", HandleRequirementFailedEvent);
        }

        private void OnDestroy()
        {
            NetworkAPI.RemoveExtensionMessageHandler("general_event", HandleGeneralEvent);
            NetworkAPI.RemoveExtensionMessageHandler("error_event", HandleErrorEvent);
            NetworkAPI.RemoveExtensionMessageHandler("requirement_failed_event", HandleRequirementFailedEvent);
        }


        public virtual void HandleGeneralEvent(Dictionary<string, object> props)
        {
            string eventType = (string)props["event"];
            int val = (int)props["val"];
            string data = (string)props["data"];

            string[] args = new string[1];
            string errorMessage = "";
            //string errorMessage = eventType;
            if (eventType == "ReputationChanged")
            {
#if AT_I2LOC_PRESET
	    	errorMessage = I2.Loc.LocalizationManager.GetTranslation("Reputation for faction")+" " + (string)props["name"] + " "+I2.Loc.LocalizationManager.GetTranslation("increased by")+": " + data;
#else
                errorMessage = "Reputation for faction " + (string)props["name"] + " increased by: " + data;
#endif
            }
            else if (eventType == "DuelCountdown")
            {
                errorMessage = "" + val;
            }
            else if (eventType == "DuelStart")
            {
#if AT_I2LOC_PRESET
            			errorMessage = I2.Loc.LocalizationManager.GetTranslation("Fight!");
#else
                errorMessage = "Fight!";
#endif
                args[0] = "120";//Dual Time
                AtavismEventSystem.DispatchEvent("TimerStart", args);
            }
            else if (eventType == "DuelVictory")
            {
                string[] results = data.Split(',');
                string winner = results[0];
                string loser = results[1];
                string[] cArgs = new string[3];
#if AT_I2LOC_PRESET
            cArgs[0] = winner + " "+I2.Loc.LocalizationManager.GetTranslation("has defeated")+" " + loser + " "+I2.Loc.LocalizationManager.GetTranslation("in a duel");
#else
                cArgs[0] = winner + " has defeated " + loser + " in a duel";
#endif
                cArgs[1] = "System";
                cArgs[2] = "";
                AtavismEventSystem.DispatchEvent("CHAT_MSG_SYSTEM", cArgs);
                if (winner == ClientAPI.GetPlayerObject().Name)
                {
                    AtavismEventSystem.DispatchEvent("TimerStop", args);
#if AT_I2LOC_PRESET
                errorMessage = I2.Loc.LocalizationManager.GetTranslation("Victory!");
#else
                    errorMessage = "Victory!";
#endif
                }
                else
                {
                    return;
                }
            }
            else if (eventType == "DuelDefeat")
            {
                AtavismEventSystem.DispatchEvent("TimerStop", args);
                if (data == ClientAPI.GetPlayerObject().Name)
                {
#if AT_I2LOC_PRESET
                errorMessage = I2.Loc.LocalizationManager.GetTranslation("Defeat!");
#else
                    errorMessage = "Defeat!";
#endif
                }
                else
                {
                    return;
                }
            }
            else if (eventType == "DuelOutOfBounds")
            {
#if AT_I2LOC_PRESET
   			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You have left the Duel Area, if you do not return you will forfeit the duel");
#else
                errorMessage = "You have left the Duel Area, if you do not return you will forfeit the duel";
#endif
            }
            else if (eventType == "DuelNotOutOfBounds")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You have returned to the Duel Area");
#else
                errorMessage = "You have returned to the Duel Area";
#endif
            }
            else if (eventType == "GuildMemberJoined")
            {
                string[] cArgs = new string[3];
#if AT_I2LOC_PRESET
            cArgs[0] = data + " "+I2.Loc.LocalizationManager.GetTranslation("has joined the Guild.");
#else
                cArgs[0] = data + " has joined the Guild.";
#endif
                cArgs[1] = "System";
                cArgs[2] = "";
                AtavismEventSystem.DispatchEvent("CHAT_MSG_SYSTEM", cArgs);
            }
            else if (eventType == "GuildMemberLeft")
            {
                string[] cArgs = new string[3];
#if AT_I2LOC_PRESET
            cArgs[0] = data + " "+I2.Loc.LocalizationManager.GetTranslation("has left the Guild.");
#else
                cArgs[0] = data + " has left the Guild.";
#endif
                cArgs[1] = "System";
                cArgs[2] = "";
                AtavismEventSystem.DispatchEvent("CHAT_MSG_SYSTEM", cArgs);
            }
            else if (eventType == "GuildRankNoDeleteIsMember")
            {
                string[] cArgs = new string[3];
#if AT_I2LOC_PRESET
            cArgs[0] = I2.Loc.LocalizationManager.GetTranslation("Can't delete rank because rank is assign to Guild member.");
#else
                cArgs[0] = "Can't delete rank because rank is assign to Guild member.";
#endif
                cArgs[1] = "System";
                cArgs[2] = "";
                AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", cArgs);
            }
            else if (eventType == "GuildMasterNoLeave")
            {
                string[] cArgs = new string[3];
#if AT_I2LOC_PRESET
            cArgs[0] = I2.Loc.LocalizationManager.GetTranslation("Guild Master can't leave Guild.");
#else
                cArgs[0] = "Guild Master can't leave Guild.";
#endif
                cArgs[1] = "System";
                cArgs[2] = "";
                AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", cArgs);
            }
            else if (eventType == "SocketingFail")
            {
                string[] cArgs = new string[3];
#if AT_I2LOC_PRESET
            cArgs[0] = I2.Loc.LocalizationManager.GetTranslation("Socketing Failed");
#else
                cArgs[0] = "Socketing Failed.";
#endif
                cArgs[1] = "System";
                cArgs[2] = "";
                AtavismEventSystem.DispatchEvent("CHAT_MSG_SYSTEM", cArgs);
            }
            else if (eventType == "SocketingSuccess")
            {
                string[] cArgs = new string[3];
#if AT_I2LOC_PRESET
            cArgs[0] =  I2.Loc.LocalizationManager.GetTranslation("Socketing Succeeded");
#else
                cArgs[0] = "Socketing Succeeded";
#endif
                cArgs[1] = "System";
                cArgs[2] = "";
                AtavismEventSystem.DispatchEvent("CHAT_MSG_SYSTEM", cArgs);
            }
            else if (eventType == "EnchantingFail")
            {
                string[] cArgs = new string[3];
#if AT_I2LOC_PRESET
            cArgs[0] = I2.Loc.LocalizationManager.GetTranslation("Enchanting Failed");
#else
                cArgs[0] = "Enchanting Failed.";
#endif
                cArgs[1] = "System";
                cArgs[2] = "";
                AtavismEventSystem.DispatchEvent("CHAT_MSG_SYSTEM", cArgs);
            }
            else if (eventType == "EnchantingSuccess")
            {
                string[] cArgs = new string[3];
#if AT_I2LOC_PRESET
            cArgs[0] =  I2.Loc.LocalizationManager.GetTranslation("Enchanting Succeeded");
#else
                cArgs[0] = "Enchanting Succeeded.";
#endif
                cArgs[1] = "System";
                cArgs[2] = "";
                AtavismEventSystem.DispatchEvent("CHAT_MSG_SYSTEM", cArgs);
            }
            else if (eventType == "RepairSuccessful")
            {
                string[] cArgs = new string[3];
#if AT_I2LOC_PRESET
            cArgs[0] =  I2.Loc.LocalizationManager.GetTranslation("Repair Succeeded");
#else
                cArgs[0] = "Repair Succeeded.";
#endif
                cArgs[1] = "System";
                cArgs[2] = "";
                AtavismEventSystem.DispatchEvent("CHAT_MSG_SYSTEM", cArgs);
            }
            else
            {
                Debug.LogWarning("general_event "+ eventType+" is not found");
            }

            // dispatch a ui event to tell the rest of the system
            args[0] = errorMessage;
            AtavismEventSystem.DispatchEvent("ANNOUNCEMENT", args);
        }

        public void HandleErrorEvent(Dictionary<string, object> props)
        {
            string eventType = (string)props["event"];
            int val = (int)props["val"];
            string data = (string)props["data"];
            ProcessErrorEvent(eventType, val, data);
        }

        public virtual void ProcessErrorEvent(string eventType, int val, string data)
        {
            string errorMessage = "";
            //string errorMessage = eventType;
            if (eventType == "NotEnoughCurrency")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not have enough currency to perform that action");
#else
                errorMessage = "You do not have enough currency to perform that action";
#endif
            }
            else if (eventType == "InvalidTradeCurrency")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You cannot trade more currency than you have");
#else
                errorMessage = "You cannot trade more currency than you have";
#endif
            }
            else if (eventType == "TooFarAway")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You are too far away from the object to use it");
#else
                errorMessage = "You are too far away from the object to use it";
#endif
            }
            else if (eventType == "SkillLevelTooLow")
            {
                if (int.Parse(data) == 0)
                {
#if AT_I2LOC_PRESET
                errorMessage = I2.Loc.LocalizationManager.GetTranslation("Requires Skill")+": " + Skills.Instance.GetSkillByID(val).skillname;
#else
                    errorMessage = "Requires Skill: " + Skills.Instance.GetSkillByID(val).skillname;
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                errorMessage = I2.Loc.LocalizationManager.GetTranslation("Requires level")+" " + data + " " + Skills.Instance.GetSkillByID(val).skillname;
#else
                    errorMessage = "Requires level " + data + " " + Skills.Instance.GetSkillByID(val).skillname;
#endif
                }
            }
            else if (eventType == "EquipMissing")
            {
                //	string weaponReq = data.ToLower();
                if (data.StartsWith("a") || data.StartsWith("e") || data.StartsWith("i") ||
                    data.StartsWith("o") || data.StartsWith("u"))
                {
#if AT_I2LOC_PRESET
                errorMessage = I2.Loc.LocalizationManager.GetTranslation("Requires an")+" " + data + " "+I2.Loc.LocalizationManager.GetTranslation("equipped in your Main Hand");
#else
                    errorMessage = "Requires an " + data + " equipped in your Main Hand";
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                errorMessage = I2.Loc.LocalizationManager.GetTranslation("Requires a")+" " + data + " "+I2.Loc.LocalizationManager.GetTranslation("equipped in your Main Hand");
#else
                    errorMessage = "Requires a " + data + " equipped in your Main Hand";
#endif
                }
            }
            else if (eventType == "ResourceNodeBusy")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("That Resource Node is currently being used by another player");
#else
                errorMessage = "That Resource Node is currently being used by another player";
#endif
            }
            else if (eventType == "ResourceHarvestFailed")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You failed to harvest the Resource Node");
#else
                errorMessage = "You failed to harvest the Resource Node";
#endif
            }
            else if (eventType == "InstanceRequiresGroup")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You must be in a group to enter this Instance");
#else
                errorMessage = "You must be in a group to enter this Instance";
#endif
            }
            else if (eventType == "SkillAlreadyKnown")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You already know the")+" " + I2.Loc.LocalizationManager.GetTranslation("Ability/"+Skills.Instance.GetSkillByID(val).skillname) + " "+I2.Loc.LocalizationManager.GetTranslation("Skill");
#else
                errorMessage = "You already know the " + Skills.Instance.GetSkillByID(val).skillname + " Skill";
#endif
            }
            else if (eventType == "CannotSellItem")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You cannot sell that Item");
#else
                errorMessage = "You cannot sell that Item";
#endif
            }
            else if (eventType == "InventoryFull")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("Your Inventory is full");
#else
                errorMessage = "Your Inventory is full";
#endif
            }
            else if (eventType == "StorageNotEmpty")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("That Storage is not empty");
#else
                errorMessage = "That Storage is not empty";
#endif
            }
            else if (eventType == "ErrorDead")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You cannot perform that action while you are dead");
#else
                errorMessage = "You cannot perform that action while you are dead";
#endif
            }
            else if (eventType == "ErrorMounted")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You cannot perform that action while mounted");
#else
                errorMessage = "You cannot perform that action while mounted";
#endif
            }
            else if (eventType == "InvalidItem")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You have not used the correct item");
#else
                errorMessage = "You have not used the correct item";
#endif
            }
            else if (eventType == "EquipFailItemUnique")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("EquipFailItemUnique");
#else
                errorMessage = "Only one and the same unique item can be equiped";
#endif
            }
            else if (eventType == "InsufficientClaimObjectItems")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You must place all required items into the Upgade Interface to upgrade this object");
#else
                errorMessage = "You must place all required items into the Upgade Interface to upgrade this object";
#endif
            }
            else if (eventType == "ErrorInCombat")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You cannot do that while in combat");
#else
                errorMessage = "You cannot do that while in combat";
#endif
            }
            else if (eventType == "ErrorInsufficientPermission")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not have permission to do that");
#else
                errorMessage = "You do not have permission to do that";
#endif
            }
            else if (eventType == "ErrorAlreadyInGuild")
            {
#if AT_I2LOC_PRESET
            errorMessage = data + " "+I2.Loc.LocalizationManager.GetTranslation("is already in a Guild");
#else
                errorMessage = data + " is already in a Guild";
#endif
            }
            else if (eventType == "ErrorNoEquipSlot")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("Slot does not exist") +": " + data;
#else
                errorMessage = "Slot does not exist: " + data;
#endif
            }
            else if (eventType == "ErrorWrongEquipSlot")
            {
#if AT_I2LOC_PRESET
             errorMessage = I2.Loc.LocalizationManager.GetTranslation(Inventory.Instance.GetItemByTemplateID(val).name) + " "+ I2.Loc.LocalizationManager.GetTranslation("cannot be placed in slot")+": " + data;
#else
                errorMessage = Inventory.Instance.GetItemByTemplateID(val).name + " cannot be placed in slot: " + data;
#endif
            }
            else if (eventType == "GuildNoDemote")
            {
#if AT_I2LOC_PRESET
             errorMessage =  I2.Loc.LocalizationManager.GetTranslation("You can not demote")+" " + data;
#else
                errorMessage = "You can not demote " + data;
#endif
            }
            else if (eventType == "GuildNoPromote")
            {
#if AT_I2LOC_PRESET
             errorMessage = I2.Loc.LocalizationManager.GetTranslation("You can not promote")+" " + data;
#else
                errorMessage = "You can not promote " + data;
#endif
            }
            else if (eventType == "GuildMasterNoLeave")
            {
#if AT_I2LOC_PRESET
             errorMessage =  I2.Loc.LocalizationManager.GetTranslation("Guild Master can't leave Guild")+" " + data;
#else
                errorMessage = "Guild Master can't leave Guild" + data;
#endif
            }
            else if (eventType == "cooldownNoEnd")
            {
#if AT_I2LOC_PRESET
             errorMessage =  I2.Loc.LocalizationManager.GetTranslation("cooldownNoEnd")+" " + data;
#else
                errorMessage = "Cooldown has not finished yet" + data;
#endif
            }
            else if (eventType == "SocialPlayerOffline")
            {
#if AT_I2LOC_PRESET
             errorMessage =  I2.Loc.LocalizationManager.GetTranslation("Can not add Friend because is offline")+" " + data;
#else
                errorMessage = "Can not add Friend because is offline" + data;
#endif
            }
            else if (eventType == "InstanceRequiresGuild")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You must be in a Guild to enter this Instance");
#else
                errorMessage = "You must be in a Guild to enter this Instance";
#endif
            }
            else if (eventType == "SocketingFail")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("Socketing Failed");
#else
                errorMessage = "Socketing Failed";
#endif
            }
            else if (eventType == "SocketingInterrupt")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("Socketing Interrupted");
#else
                errorMessage = "Socketing Interrupted";
#endif
            }
            else if (eventType == "EnchantingFail")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("Enchanting Failed");
#else
                errorMessage = "Enchanting Failed";
#endif
            }
            else if (eventType == "EnchantingInterrupt")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("Enchanting Interrupted");
#else
                errorMessage = "Enchanting Interrupted";
#endif
            }
            else if (eventType == "SocketResetInterrupt")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("Socket Reseting Interrupted");
#else
                errorMessage = "Socket Reseting Interrupted";
#endif
            }
            else if (eventType == "EnchantIsMaxLevel")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("EnchantIsMaxLevel");
#else
                errorMessage = "Item can not be enchanted because it has reached the maximal level";
#endif
            }
            else if (eventType == "ErrorPlayerYourOnBlockList")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("ErrorPlayerYourOnBlockList");
#else
                errorMessage = "Player is on my block list";
#endif
            }
            else if (eventType == "ErrorPlayerOnBlockList")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("ErrorPlayerOnBlockList");
#else
                errorMessage = "You are on block list";
#endif
            }
            else if (eventType == "AuctionOwnLimit")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("AuctionOwnLimit");
#else
                errorMessage = "The auction limit has been reached you can no longer list items";
#endif
            }
            else if (eventType == "NoItemDurability")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("No item that could be repaired");
#else
                errorMessage = "No item that could be repaired";
#endif
            }
            else
            {
                Debug.LogError("No translate to :" + eventType);
            }

            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            args[0] = errorMessage;
            AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
        }

        public virtual void HandleRequirementFailedEvent(Dictionary<string, object> props)
        {
            string eventType = (string)props["event"];
            int val = (int)props["val"];
            string data = (string)props["data"];

            string errorMessage = eventType;
            if (eventType == "RequirementResultSkillTooLow")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not meet the requirements");
#else
                errorMessage = "You do not meet the requirements";
#endif
            }
            else if (eventType == "RequirementResultWrongRace")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not meet the requirements");
#else
                errorMessage = "You do not meet the requirements";
#endif
            }
            else if (eventType == "RequirementResultWrongClass")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not meet the requirements");
#else
                errorMessage = "You do not meet the requirements";
#endif
            }
            else if (eventType == "RequirementResultLevelTooLow")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not meet the requirements");
#else
                errorMessage = "You do not meet the requirements";
#endif
            }
            else if (eventType == "RequirementResultStatTooLow")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not meet the requirements");
#else
                errorMessage = "You do not meet the requirements";
#endif
            }

            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            args[0] = errorMessage;
            AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
        }

        public static AtavismEventMessageHandler Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
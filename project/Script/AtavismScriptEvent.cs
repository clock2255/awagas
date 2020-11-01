using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{
    public class AtavismScriptEvent : MonoBehaviour
    {

        public static AtavismScriptEvent instance;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            if (instance == null)
            {
                instance = this;
                MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Comm, _HandleComm);
                MessageDispatcher.Instance.RegisterHandler(WorldMessageType.ObjectProperty, _HandleObjectProperty);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void _HandleComm(BaseWorldMessage message)
        {
            CommMessage commMessage = (CommMessage)message;
            AtavismLogger.LogDebugMessage("Got comm message with channel: " + commMessage.ChannelId);
            if (commMessage.ChannelId == 0)
            {
                // Server channel (0)
                //ClientAPI.Interface.DispatchEvent("CHAT_MSG_SAY", [message.Message, nodeName, ""]);
                string[] args = new string[1];
                args[0] = commMessage.Message;
                AtavismEventSystem.DispatchEvent("CHAT_MSG_SERVER", args);
            }
            else if (commMessage.ChannelId == 2)
            {
                // ServerInfo channel (2)
                //ClientAPI.Interface.DispatchEvent("CHAT_MSG_SYSTEM", [message.Message, ""]);
                string[] args = new string[3];
                args[0] = commMessage.Message;
                args[1] = "System";
                args[2] = "";
                AtavismEventSystem.DispatchEvent("CHAT_MSG_SYSTEM", args);
            }
            else if (commMessage.ChannelId == -2)
            {
                string[] args = new string[3];
                args[0] = commMessage.Message;
                args[1] = "Admin";
                args[2] = "";
                AtavismEventSystem.DispatchEvent("ADMIN_MESSAGE", args);
            }
            else
            {
                // Say channel (1)
                //ClientAPI.Interface.DispatchEvent("CHAT_MSG_SAY", [message.Message, nodeName, ""]);
                string[] args = new string[4];
                string msg = commMessage.Message;
#if AT_I2LOC_PRESET
            if (msg.IndexOf("has joined the group") != -1)
            {
                msg = msg.Remove(msg.IndexOf("has joined the group")) + I2.Loc.LocalizationManager.GetTranslation("has joined the group") + ".";
            }
            if (msg.IndexOf("has left the group") != -1)
            {
                msg = msg.Remove(msg.IndexOf("has left the group")) + I2.Loc.LocalizationManager.GetTranslation("has left the group") + ".";
            }
            if (msg.IndexOf("has disbanded the group") != -1)
            {
                msg = msg.Remove(msg.IndexOf("has disbanded the group")) + I2.Loc.LocalizationManager.GetTranslation("has disbanded the group") + ".";
            }
            if (msg.IndexOf("is now the group leader") != -1)
            {
                msg = msg.Remove(msg.IndexOf("is now the group leader")) + I2.Loc.LocalizationManager.GetTranslation("is now the group leader") + ".";
            }
            if (msg.IndexOf("has muted the group") != -1)
            {
                msg = msg.Remove(msg.IndexOf("has muted the group")) + I2.Loc.LocalizationManager.GetTranslation("has muted the group") + ".";
            }
            if (msg.IndexOf("has un-muted the group") != -1)
            {
                msg = msg.Remove(msg.IndexOf("has un-muted the group")) + I2.Loc.LocalizationManager.GetTranslation("has un-muted the group") + ".";
            }
            if (msg.IndexOf("Only the group leader can invite new members") != -1)
            {
                msg = I2.Loc.LocalizationManager.GetTranslation("Only the group leader can invite new members") + ".";
            }
            if (msg.IndexOf("Your group is full") != -1)
            {
                msg = I2.Loc.LocalizationManager.GetTranslation("Your group is full") + ".";
            }
            if (msg.IndexOf("is already grouped") != -1)
            {
                msg = msg.Remove(msg.IndexOf("is already grouped")) + I2.Loc.LocalizationManager.GetTranslation("is already grouped") + ".";
            }
            if (msg.IndexOf("is already considering a group invite") != -1)
            {
                msg = msg.Remove(msg.IndexOf("is already considering a group invite")) + I2.Loc.LocalizationManager.GetTranslation("is already considering a group invite") + ".";
            }
            if (msg.IndexOf("You have invited") != -1)
            {
                msg = msg.Remove(msg.IndexOf("to your group"));
                msg = I2.Loc.LocalizationManager.GetTranslation("You have invited") + msg.Remove(0, 16) + I2.Loc.LocalizationManager.GetTranslation("to your group") + ".";
            }
            if (msg.IndexOf("Invitation for") != -1)
            {
                msg = msg.Remove(msg.IndexOf("was cancelled"));
                msg = I2.Loc.LocalizationManager.GetTranslation("Invitation for") + msg.Remove(0, 14) + I2.Loc.LocalizationManager.GetTranslation("was cancelled") + ".";
            }
            if (msg.IndexOf("Invitation from") != -1) 
            {
                msg = msg.Remove(msg.IndexOf("was cancelled"));
                msg = I2.Loc.LocalizationManager.GetTranslation("Invitation from") + msg.Remove(0, 15) + I2.Loc.LocalizationManager.GetTranslation("was cancelled") + ".";
            }

#endif
                args[0] = msg;
                args[1] = commMessage.SenderName;
                args[2] = commMessage.ChannelId.ToString();
                if (commMessage.Oid > 0)
                    args[3] = OID.fromLong(commMessage.Oid).ToString();
                else
                    args[3] = "";
                AtavismEventSystem.DispatchEvent("CHAT_MSG_SAY", args);
            }
        }

        public void _HandleObjectProperty(BaseWorldMessage message)
        {
            ObjectPropertyMessage propMessage = (ObjectPropertyMessage)message;
            if (propMessage.Properties.Count <= 0)
                return;
            AtavismObjectNode target = ClientAPI.GetTargetObject();
            AtavismPlayer player = ClientAPI.GetPlayerObject();
            //pet = None;
            //activePet = MarsUnit._GetUnitProperty2("player", "activePet", None);
            //if (activePet != null)
            //	pet = ClientAPI.World.GetObjectByOID(activePet);
            string[] args;
            foreach (string prop in propMessage.Properties.Keys)
            {
                string eventName = "PROPERTY_" + prop;
                if (target != null && message.Oid == target.Oid)
                {
                    args = new string[1];
                    args[0] = "target";
                    AtavismEventSystem.DispatchEvent(eventName, args);
                }
                if (player != null && propMessage.Oid == player.Oid)
                {
                    args = new string[1];
                    args[0] = "player";
                    AtavismEventSystem.DispatchEvent(eventName, args);
                }
                //if (pet != null && message.Oid == pet.OID)
                //	ClientAPI.Interface.DispatchEvent(eventName, ["pet"]);
                // Always post an "any" unit event.
                args = new string[2];
                args[0] = "any";
                args[1] = propMessage.Oid.ToString();
                AtavismEventSystem.DispatchEvent(eventName, args);
            }
        }
    }
}
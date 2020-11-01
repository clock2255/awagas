using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

namespace Atavism
{
    public class GroupMember
    {
        public OID oid;
        public int status;
        public string name;
        public Dictionary<string, object> properties = new Dictionary<string, object>();
        public int raceID;
        public int classID;
    }

    public class AtavismGroup : MonoBehaviour
    {

        static AtavismGroup instance;

        List<GroupMember> members = new List<GroupMember>();
        OID leaderOid;
        int roll = 0;
        int dice = 0;
        int grade = 0;
        OID mob = null;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            NetworkAPI.RegisterExtensionMessageHandler("ao.GROUP_INVITE_REQUEST", HandleGroupInviteRequest);
            NetworkAPI.RegisterExtensionMessageHandler("ao.GROUP_CANCEL_INVITE_REQUEST", HandleGroupCancelInviteRequest);
            NetworkAPI.RegisterExtensionMessageHandler("ao.GROUP_UPDATE", HandleGroupUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("ao.GROUP_PROPERTY_UPDATE", HandleGroupPropertyUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("ao.GROUP_DICE", HandleGroupDice);
            //NetworkAPI.RegisterExtensionMessageHandler("ao.GROUP", HandleGroupDice);

            SceneManager.sceneLoaded += sceneLoaded;
        }

        private void HandleGroupDice(Dictionary<string, object> props)
        {
            int itemId = (int)props["itemId"];
            int count = (int)props["time"];
             mob = (OID)props["mob"];
            string[] event_args = new string[2];
            event_args[0] = count.ToString();
            event_args[1] = itemId.ToString();
            AtavismEventSystem.DispatchEvent("GROUP_DICE", event_args);
        }

        private void sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name.Equals("Login") || arg0.name.Equals(ClientAPI.Instance.characterSceneName))
            {
                members.Clear();
                leaderOid = null;
             }
        }

        /// <summary>
        /// Function get response from confirm panel
        /// </summary>
        /// <param name="inviter"></param>
        /// <param name="accepted"></param>
        public void GroupInviteResponse(object inviter, bool accepted)
        {
            if (accepted)
                SendInviteResponseMessage((OID)inviter, "accept");
            else
                SendInviteResponseMessage((OID)inviter, "decline");
        }

        #region Message Senders
        /// <summary>
        /// Function send message request group invitation to server
        /// </summary>
        /// <param name="targetOid"></param>
        public void SendInviteRequestMessage(OID targetOid)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("target", targetOid);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GROUP_INVITE", props);
        }
        /// <summary>
        /// Function send respons group invitation to server
        /// </summary>
        /// <param name="targetOid"></param>
        /// <param name="response"></param>

        public void SendInviteResponseMessage(OID targetOid, string response)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("groupLeaderOid", targetOid);
            props.Add("response", response);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GROUP_INVITE_RESPONSE", props);
        }

        public void SendGroupChatMessage(string message)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("message", message);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GROUP_CHAT", props);
        }

        public void RemoveGroupMember(int slotId)
        {
            RemoveGroupMember(members[slotId].oid);
        }

        /// <summary>
        /// Function send message remove plyer from group to server
        /// </summary>
        /// <param name="oid"></param>
        public void RemoveGroupMember(OID oid)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("target", oid);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GROUP_REMOVE_MEMBER", props);
        }

        /// <summary>
        /// Function send message promote leader group to server
        /// </summary>
        /// <param name="oid"></param>
        public void PromoteToLeader(OID oid)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("target", oid);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GROUP_PROMOTE_LEADER", props);
        }

        /// <summary>
        /// Function send message leave group to server
        /// </summary>
        public void LeaveGroup()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GROUP_LEAVE", props);
        }


      /*  public void lootGroup()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GROUP_SETTINGS", props);
        }
        */
        public void SetLootGroup(bool ffa, bool rr, bool leader, bool norm, bool dice,int grade)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("roll", ffa?0:rr?1:2);
            props.Add("dice", norm?0:1);
            props.Add("grade", grade);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GROUP_SETTINGS", props);
        }
        public void Roll()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("roll", 1);
            props.Add("loottarget", mob);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "inventory.LOOT_ROLL", props);
        }
        public void Pass()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("roll", 0);
            props.Add("loottarget", mob);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "inventory.LOOT_ROLL", props);
        }

        #endregion Message Senders

        #region Message Handlers




        public void HandleGroupInviteRequest(Dictionary<string, object> props)
        {
            OID groupLeaderOid = (OID)props["groupLeaderOid"];
            string groupLeaderName = (string)props["groupLeaderName"];
            int count = (int)props["groupInviteTimeout"];
#if AT_I2LOC_PRESET
        UGUIConfirmationPanel.Instance.ShowConfirmationBox(groupLeaderName + " " + I2.Loc.LocalizationManager.GetTranslation("has invited you to join their group"), groupLeaderOid, GroupInviteResponse,(float)count);
#else
            UGUIConfirmationPanel.Instance.ShowConfirmationBox(groupLeaderName + " " + "has invited you to join their group", groupLeaderOid, GroupInviteResponse, (float)count);
#endif
        }

        public void HandleGroupCancelInviteRequest(Dictionary<string, object> props)
        {
            OID groupLeaderOid = (OID)props["groupLeaderOid"];
            string groupLeaderName = (string)props["groupLeaderName"];
            UGUIConfirmationPanel.Instance.Hide();
        }

        public void HandleGroupUpdate(Dictionary<string, object> props)
        {
            members.Clear();
            leaderOid = null;

            foreach (string propName in props.Keys)
            {
                if (props[propName] is Dictionary<string, object>)
                {
                    Dictionary<string, object> memberInfo = (Dictionary<string, object>)props[propName];
                    GroupMember groupMember = new GroupMember();
                    groupMember.oid = (OID)memberInfo["memberOid"];
                    if (groupMember.oid.ToLong() == ClientAPI.GetPlayerOid())
                        continue;
                    groupMember.status = (int)memberInfo["status"];
                    groupMember.name = (string)memberInfo["name"];
                    int propCount = (int)memberInfo["statCount"];
                    for (int i = 0; i < propCount; i++)
                    {
                        string statName = (string)memberInfo["stat" + i];
                        if (!statName.Equals("effects"))
                        {
                            groupMember.properties.Add(statName, memberInfo["stat" + i + "Value"]);
                        }
                        else
                        {
                            //     Debug.LogError("HandleGroupUpdate stat name effect");
                        }
                    }
                    if (memberInfo.ContainsKey("level"))
                        if (groupMember.properties.ContainsKey("level"))
                            groupMember.properties["level"] = memberInfo["level"];
                        else
                            groupMember.properties.Add("level", memberInfo["level"]);

                    if (memberInfo.ContainsKey("effects"))
                        if (groupMember.properties.ContainsKey("effects"))
                        {
                            //     Debug.LogError("HandleGroupUpdate update effect "+ memberInfo["effects"]);
                            groupMember.properties["effects"] = memberInfo["effects"];
                            groupMember.properties["effects"] = Time.time;
                        }
                        else
                        {
                            //      Debug.LogError("HandleGroupUpdate add effect");
                            groupMember.properties.Add("effects", memberInfo["effects"]);
                            groupMember.properties.Add("effects_t", Time.time);
                        }

                    if (memberInfo.ContainsKey("portrait"))
                        if (groupMember.properties.ContainsKey("portrait"))
                            groupMember.properties["portrait"] = memberInfo["portrait"];
                        else
                            groupMember.properties.Add("portrait", memberInfo["portrait"]);
                    members.Add(groupMember);
                }
            }
            if(props.ContainsKey("roll"))
                roll = (int)props["roll"];
            if (props.ContainsKey("dice"))
                dice = (int)props["dice"];
            if (props.ContainsKey("grade"))
                grade = (int)props["grade"];

            if (members.Count > 0)
            {
                leaderOid = (OID)props["groupLeaderOid"];
            }

            // dispatch a ui event to tell the rest of the system
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("GROUP_UPDATE", event_args);
        }

        public void HandleGroupPropertyUpdate(Dictionary<string, object> props)
        {
            //   Debug.LogError("HandleGroupPropertyUpdate start");
            /*  string ss = "";
              foreach (string s in props.Keys)
              {
                  ss += s + " | ";

              }*/
            //   Debug.LogError("HandleGroupPropertyUpdate: keys=" + ss);
            OID memberOid = (OID)props["memberOid"];
            foreach (GroupMember member in members)
            {
                if (member.oid == memberOid)
                {
                    int propCount = (int)props["statCount"];
                    for (int i = 0; i < propCount; i++)
                    {
                        string statName = (string)props["stat" + i];
                        //     Debug.LogError("HandleGroupPropertyUpdate: statName=" + statName);
                        if (member.properties.ContainsKey(statName))
                        {
                            member.properties[statName] = props["stat" + i + "Value"];
                            member.properties[statName + "_t"] = Time.time;
                        }
                        else
                        {
                            member.properties.Add(statName, props["stat" + i + "Value"]);
                            member.properties.Add(statName + "_t", Time.time);
                        }
                    }
                    if (props.ContainsKey("status"))
                        member.status = (int)props["status"];

                    if (props.ContainsKey("level"))
                        if (member.properties.ContainsKey("level"))
                            member.properties["level"] = props["level"];
                        else
                            member.properties.Add("level", props["level"]);

                    if (props.ContainsKey("effects"))
                        if (member.properties.ContainsKey("effects"))
                        {
                            //        Debug.LogError("HandleGroupPropertyUpdate update effect "+ props["effects"]);
                            member.properties["effects"] = props["effects"];
                            member.properties["effects_t"] = Time.time;
                        }
                        else
                        {
                            //         Debug.LogError("HandleGroupPropertyUpdate add effect");
                            member.properties.Add("effects", props["effects"]);
                            member.properties.Add("effects_t", Time.time);
                        }

                    if (props.ContainsKey("portrait"))
                        if (member.properties.ContainsKey("portrait"))
                            member.properties["portrait"] = props["portrait"];
                        else
                            member.properties.Add("portrait", props["portrait"]);
                }
            }

            // dispatch a ui event to tell the rest of the system
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("GROUP_UPDATE", event_args);
        }
        #endregion Message Handlers

        public static AtavismGroup Instance
        {
            get
            {
                return instance;
            }
        }

        public List<GroupMember> Members
        {
            get
            {
                return members;
            }
        }

        public OID LeaderOid
        {
            get
            {
                return leaderOid;
            }
        }
        public int GetRoll
        {
            get
            {
                return roll;
            }
        }

        public int GetDice
        {
            get
            {
                return dice;
            }
        }

        public int GetGrade
        {
            get
            {
                return grade;
            }
        }



    }
}
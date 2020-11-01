using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Atavism
{

    public class AtavismSocialMember
    {
        public OID oid;
        public string name;
        public int level;
        //	public string zone;
        public bool status;
    }

    public class AtavismSocial : MonoBehaviour
    {

        static AtavismSocial instance;

        string guildName;
        int factionID;
        List<AtavismSocialMember> banneds;
        List<AtavismSocialMember> friends;
        string motd;
        string omotd;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            friends = new List<AtavismSocialMember>();
            banneds = new List<AtavismSocialMember>();
            // Add message handlers
            //		NetworkAPI.RegisterExtensionMessageHandler("friedList", HandleGuildInvite);
            NetworkAPI.RegisterExtensionMessageHandler("ao.friedList", HandleSocialData);
            NetworkAPI.RegisterExtensionMessageHandler("social.INVITE_FRIENDS", HandleInviteRequest);
            NetworkAPI.RegisterExtensionMessageHandler("social.CANCEL_FRIENDS", HandleCancelInviteRequest);
        }

        public AtavismSocialMember GetSocialMemberByOid(OID memberOid)
        {
            foreach (AtavismSocialMember member in friends)
            {
                if (member.oid == memberOid)
                {
                    return member;
                }
            }
            return null;
        }

        public void InviteResponse(object inviter, bool accepted)
        {
            if (accepted)
                SendInviteResponseMessage((OID)inviter, "accept");
            else
                SendInviteResponseMessage((OID)inviter, "decline");
        }

        #region Message Senders
        public void SendInviteResponseMessage(OID targetOid, string response)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("inviterOid", targetOid);
            props.Add("response", response);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "social.INVITE_RESPONSE", props);
        }

        public void SendInvitation(OID friendOid, String friendName)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("friendOid", friendOid);
            props.Add("friendName", friendName);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "social.ADD_FRIEND", props);
        }

        public void SendDelFriend(OID targetOid)
        {
            //     Debug.LogError("SendDelFriend");
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("friendOid", targetOid);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "social.DEL_FRIEND", props);
        }
        public void SendGetFriends()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "social.GET_FRIENDS", props);
        }
        public void SendAddBlock(OID blockOid, String blockName)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("blockOid", blockOid);
            props.Add("blockName", blockName);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "social.ADD_BLOCK", props);
        }
        public void SendDelBlock(OID targetOid)
        {
            //    Debug.LogError("SendDelBlock");
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("blockOid", targetOid);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "social.DEL_BLOCK", props);
        }

        #endregion Message Senders

        #region Message Handlers

        public void HandleCancelInviteRequest(Dictionary<string, object> props)
        {
            OID inviterOid = (OID)props["inviterOid"];
            string inviterName = (string)props["inviterName"];
            UGUIConfirmationPanel.Instance.Hide();
        }


        public void HandleInviteRequest(Dictionary<string, object> props)
        {
            //    Debug.LogError("Social HandleInviteRequest");
            OID inviterOid = (OID)props["inviterOid"];
            string inviterName = (string)props["inviterName"];
            int count = (int)props["inviteTimeout"];
#if AT_I2LOC_PRESET
        UGUIConfirmationPanel.Instance.ShowConfirmationBox(inviterName + " " + I2.Loc.LocalizationManager.GetTranslation("has invited you to be a friend"), inviterOid, InviteResponse,(float)count);
#else
            UGUIConfirmationPanel.Instance.ShowConfirmationBox(inviterName + " " + "has invited you to be a friend", inviterOid, InviteResponse, (float)count);
#endif
        }

        public void HandleSocialData(Dictionary<string, object> props)
        {
            //   Debug.LogError("HandleSocialData");
            friends.Clear();
            int numMembers = (int)props["friendsCount"];
            for (int i = 0; i < numMembers; i++)
            {
                AtavismSocialMember member = new AtavismSocialMember();
                member.oid = (OID)props["friendOid" + i];
                member.name = (string)props["friendName" + i];
                member.status = (bool)props["friendOnline" + i];
                /*
                        member.level = (int)props["memberLevel" + i];
                        member.zone = (string)props["memberZone" + i];
    */
                //  member.status = (int)props["memberStatus" + i];
                friends.Add(member);
            }
            //   Debug.LogError("HandleSocialData block list");

            banneds.Clear();
            numMembers = (int)props["blockCount"];
            for (int i = 0; i < numMembers; i++)
            {
                AtavismSocialMember member = new AtavismSocialMember();
                member.oid = (OID)props["blockOID" + i];
                member.name = (string)props["blockName" + i];
                /*
                        member.level = (int)props["memberLevel" + i];
                        member.zone = (string)props["memberZone" + i];
    */
                //  member.status = (int)props["memberStatus" + i];
                banneds.Add(member);
            }


            // dispatch a ui event to tell the rest of the system
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("SOCIAL_UPDATE", event_args);
            //   Debug.LogError("HandleSocialData End");
        }

        #endregion Message Handlers

        #region Properties
        public static AtavismSocial Instance
        {
            get
            {
                return instance;
            }
        }

        public List<AtavismSocialMember> Friends
        {
            get
            {
                return friends;
            }
        }
        public List<AtavismSocialMember> Banneds
        {
            get
            {
                return banneds;
            }
        }

        public AtavismSocialMember SelectedMember
        {
            get;
            set;
        }
        #endregion Properties
    }
}
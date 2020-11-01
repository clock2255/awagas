using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

namespace Atavism
{
    public class AtavismGuildMember
    {
        public OID oid;
        public string name;
        public int rank;
        public int level;
        public string zone;
        public string note;
        public int status;
    }

    public class AtavismGuildRank
    {
        public int rankLevel;
        public string rankName;
        public List<AtavismGuildRankPermission> permissions;
    }

    /// <summary>
    /// These must match the permissions defined in agis.plugins.GuildPlugin exactly
    /// </summary>
    public enum AtavismGuildRankPermission
    {
        chat,
        invite,
        kick,
        promote,
        demote,
        setmotd,
        disband,
        addRank,
        editRank,
        editPublic,
        delRank,
        claimAdd,
        claimEdit,
        claimAction
    }

    public class AtavismGuild : MonoBehaviour
    {

        static AtavismGuild instance;

        string guildName = "";
        int factionID = -1;
        List<AtavismGuildRank> ranks;
        List<AtavismGuildMember> members;
        string motd;
        string omotd;

        OID inviterOid = null; // Stores the OID of the player who tried to invite you to a Guild

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            // Add message handlers
            NetworkAPI.RegisterExtensionMessageHandler("guildInvite", HandleGuildInvite);
            NetworkAPI.RegisterExtensionMessageHandler("sendGuildData", HandleGuildData);
            NetworkAPI.RegisterExtensionMessageHandler("guildMemberUpdate", HandleMemberUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("guildRankUpdate", HandleRankUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("guildMotd", HandleGuildMotd);
            NetworkAPI.RegisterExtensionMessageHandler("guildChat", HandleGuildChat);
            SceneManager.sceneLoaded += sceneLoaded;
        }

        private void sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name.Equals("Login") || arg0.name.Equals(ClientAPI.Instance.characterSceneName))
            {
                guildName = "";
                if (ranks != null)
                    ranks.Clear();
                if (members != null)
                    members.Clear();
            }
        }

        public AtavismGuildMember GetGuildMemberByOid(OID memberOid)
        {
            if (members != null)
                foreach (AtavismGuildMember member in members)
            {
                if (member.oid == memberOid)
                {
                    return member;
                }
            }
            return null;
        }

        public void GuildInviteResponse(object obj, bool accepted)
        {
            int guildID = (int)obj;
            RespondToGuildInvitation(inviterOid, guildID, accepted);
        }

        #region Message Senders
        public void CreateGuild(string guildName)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("guildName", guildName);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "guild.createGuild", props);
        }

        public void RespondToGuildInvitation(OID inviterOid, int guildID, bool response)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("inviter", inviterOid);
            props.Add("guildID", guildID);
            props.Add("response", response);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "guild.inviteResponse", props);
        }

        public void SendGuildCommand(string commandType, OID targetOid, string data)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("commandType", commandType);
            props.Add("targetOid", targetOid);
            props.Add("data", data);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "guild.guildCommand", props);
        }
        #endregion Message Senders

        #region Message Handlers
        public void HandleGuildInvite(Dictionary<string, object> props)
        {
            inviterOid = (OID)props["inviterOid"];
            string guildName = (string)props["guildName"];
            int guildID = (int)props["guildID"];
            string inviterName = (string)props["inviterName"];
#if AT_I2LOC_PRESET
		string inviteMessage = inviterName + " "+I2.Loc.LocalizationManager.GetTranslation("has invited you to join their Guild")+": " + guildName;
#else

            string inviteMessage = inviterName + " has invited you to join their Guild: " + guildName;
#endif
            UGUIConfirmationPanel.Instance.ShowConfirmationBox(inviteMessage, guildID, GuildInviteResponse);
        }

        public void HandleGuildData(Dictionary<string, object> props)
        {
#if AT_I2LOC_PRESET
		//ClientAPI.Write(I2.Loc.LocalizationManager.GetTranslation("Got guild update"));
#else
            //   ClientAPI.Write("Got guild update");
#endif
            members = new List<AtavismGuildMember>();
            ranks = new List<AtavismGuildRank>();

            guildName = (string)props["guildName"];
            if (guildName == null)
            {
                // dispatch a ui event to tell the rest of the system
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("GUILD_UPDATE", args);
                return;
            }
            motd = (string)props["motd"];
            omotd = (string)props["omotd"];

            int numMembers = (int)props["numMembers"];
            for (int i = 0; i < numMembers; i++)
            {
                AtavismGuildMember member = new AtavismGuildMember();
                member.oid = (OID)props["memberOid" + i];
                member.name = (string)props["memberName" + i];
                member.rank = (int)props["memberRank" + i];
                member.level = (int)props["memberLevel" + i];
                member.zone = (string)props["memberZone" + i];
                member.note = (string)props["memberNote" + i];
                member.status = (int)props["memberStatus" + i];
                members.Add(member);
            }

            int numRanks = (int)props["numRanks"];
            for (int i = 0; i < numRanks; i++)
            {
                AtavismGuildRank rank = new AtavismGuildRank();
                rank.rankLevel = (int)props["rankLevel" + i];
                rank.rankName = (string)props["rankName" + i];
                rank.permissions = new List<AtavismGuildRankPermission>();
                int rankNumPermissions = (int)props["rankNumPermissions" + i];
                for (int j = 0; j < rankNumPermissions; j++)
                {
                    string permission = (string)props["rankNum" + i + "Permission" + j];
                    rank.permissions.Add((AtavismGuildRankPermission)Enum.Parse(typeof(AtavismGuildRankPermission), permission));
                }
                ranks.Add(rank);
            }
            // dispatch a ui event to tell the rest of the system
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("GUILD_UPDATE", event_args);
        }

        public void HandleMemberUpdate(Dictionary<string, object> props)
        {
            OID memberOid = (OID)props["memberOid"];
            string action = (string)props["action"];

            //ClientAPI.Write("Got Member update with action: " + action);

            AtavismGuildMember member;
            if (action == "Remove")
            {
                member = GetGuildMemberByOid(memberOid);
                if (member != null)
                {
                    members.Remove(member);
                }
                // dispatch a ui event to tell the rest of the system
                string[] event_args = new string[1];
                AtavismEventSystem.DispatchEvent("GUILD_UPDATE", event_args);
                return;
            }

            if (action == "Update")
            {
                member = GetGuildMemberByOid(memberOid);
            }
            else
            {
                member = new AtavismGuildMember();
                members.Add(member);
            }
            member.oid = (OID)props["memberOid"];
            member.name = (string)props["memberName"];
            member.rank = (int)props["memberRank"];
            member.level = (int)props["memberLevel"];
            member.zone = (string)props["memberZone"];
            member.note = (string)props["memberNote"];
            member.status = (int)props["memberStatus"];

            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("GUILD_UPDATE", args);
        }

        public void HandleRankUpdate(Dictionary<string, object> props)
        {
            ranks.Clear();
            int numRanks = (int)props["numRanks"];
            for (int i = 0; i < numRanks; i++)
            {
                AtavismGuildRank rank = new AtavismGuildRank();
                rank.rankLevel = (int)props["rankLevel" + i];
                rank.rankName = (string)props["rankName" + i];
                rank.permissions = new List<AtavismGuildRankPermission>();
                int rankNumPermissions = (int)props["rankNumPermissions" + i];
                for (int j = 0; j < rankNumPermissions; j++)
                {
                    string permission = (string)props["rankNum" + i + "Permission" + j];
                    rank.permissions.Add((AtavismGuildRankPermission)Enum.Parse(typeof(AtavismGuildRankPermission), permission));
                }
                ranks.Add(rank);
            }
            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("GUILD_UPDATE", args);
        }

        public void HandleGuildMotd(Dictionary<string, object> props)
        {
            motd = (string)props["motd"];
            omotd = (string)props["omotd"];
            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("GUILD_UPDATE", args);
        }

        public void HandleGuildChat(Dictionary<string, object> props)
        {
            OID senderOid = (OID)props["senderOid"];
            string senderName = (string)props["senderName"];
            string message = (string)props["message"];

            string[] args = new string[4];
            args[0] = message;
            args[1] = senderName;
            args[2] = "5";
            args[3] = senderOid.ToString();
            AtavismEventSystem.DispatchEvent("CHAT_MSG_SAY", args);
        }

        #endregion Message Handlers

        #region Properties
        public static AtavismGuild Instance
        {
            get
            {
                return instance;
            }
        }

        public string GuildName
        {
            get
            {
                return guildName;
            }
        }

        public int FactionID
        {
            get
            {
                return factionID;
            }
        }

        public List<AtavismGuildRank> Ranks
        {
            get
            {
                return ranks;
            }
        }

        public List<AtavismGuildMember> Members
        {
            get
            {
                return members;
            }
        }

        public string Motd
        {
            get
            {
                return motd;
            }
        }

        public string Omotd
        {
            get
            {
                return omotd;
            }
        }

        public AtavismGuildMember SelectedMember
        {
            get;
            set;
        }
        #endregion Properties
    }
}
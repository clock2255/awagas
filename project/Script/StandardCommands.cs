using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class StandardCommands : MonoBehaviour
    {

        static StandardCommands instance;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            AtavismCommand.RegisterCommandHandler("loc", HandleLoc);
            AtavismCommand.RegisterCommandHandler("orient", HandleOrient);
            AtavismCommand.RegisterCommandHandler("props", HandleProperties);
            AtavismCommand.RegisterCommandHandler("prop", HandleProperty);
            AtavismCommand.RegisterCommandHandler("say", HandleSay);
            AtavismCommand.RegisterCommandHandler("s", HandleSay);
            AtavismCommand.RegisterCommandHandler("group", HandleGroupChat);
            AtavismCommand.RegisterCommandHandler("p", HandleGroupChat);
            AtavismCommand.RegisterCommandHandler("leave", HandleLeaveGroup);
            AtavismCommand.RegisterCommandHandler("pet", HandleSpawnPet);
            AtavismCommand.RegisterCommandHandler("purchaseSkillPoint", HandlePurchaseSkillPoint);
            AtavismCommand.RegisterCommandHandler("probe", HandleBuildingGridProbe);
            AtavismCommand.RegisterCommandHandler("spawner", HandleToggleBuilder);
            AtavismCommand.RegisterCommandHandler("claim", HandleClaim);
            AtavismCommand.RegisterCommandHandler("wave", HandleWave);
            AtavismCommand.RegisterCommandHandler("admin", HandleAdmin);
            AtavismCommand.RegisterCommandHandler("gcreate", HandleCreateGuild);
            AtavismCommand.RegisterCommandHandler("ginvite", HandleInviteToGuild);
            AtavismCommand.RegisterCommandHandler("gquit", HandleGuildQuit);
            AtavismCommand.RegisterCommandHandler("gdisband", HandleGuildDisband);
            AtavismCommand.RegisterCommandHandler("guild", HandleGuildChat);
            AtavismCommand.RegisterCommandHandler("g", HandleGuildChat);
            AtavismCommand.RegisterCommandHandler("arena", HandleArena);
        }

        public void HandleLoc(string args_str)
        {
            AtavismLogger.LogDebugMessage("Got loc command");
            AtavismObjectNode target = ClientAPI.GetTargetObject();
            if (target == null)
                target = ClientAPI.GetPlayerObject();
#if AT_I2LOC_PRESET
        ClientAPI.Write(I2.Loc.LocalizationManager.GetTranslation("Target Position") + ": " + target.Position);
#else
            ClientAPI.Write("Target Position: " + target.Position);
#endif
        }

        public void HandleOrient(string args_str)
        {
            AtavismLogger.LogDebugMessage("Got orient command");
            AtavismPlayer player = ClientAPI.GetPlayerObject();
#if AT_I2LOC_PRESET
        ClientAPI.Write(I2.Loc.LocalizationManager.GetTranslation("Player Position") + ": " + player.Orientation);
#else
            ClientAPI.Write("Player Position: " + player.Orientation);
#endif
        }

        public void HandleProperties(string args_str)
        {
            AtavismObjectNode target = ClientAPI.GetTargetObject();
            if (target == null)
                target = ClientAPI.GetPlayerObject();
            int adminLevel = (int)ClientAPI.GetObjectProperty(ClientAPI.GetPlayerOid(), "adminLevel");
            if (adminLevel >= 3)
            {
#if AT_I2LOC_PRESET
            ClientAPI.Write(I2.Loc.LocalizationManager.GetTranslation("Properties for") + ": " + target.Name + " " + I2.Loc.LocalizationManager.GetTranslation("with num Properties") + ": " + target.PropertyNames.Count);
#else
                ClientAPI.Write("Properties for: " + target.Name + " with num Properties: " + target.PropertyNames.Count);
#endif
                foreach (string prop in target.PropertyNames)
                    ClientAPI.Write(prop + ": " + target.GetProperty(prop));
            }
        }

        public void HandleProperty(string args_str)
        {
            AtavismObjectNode target = ClientAPI.GetTargetObject();
            if (target == null)
                target = ClientAPI.GetPlayerObject();
            int adminLevel = (int)ClientAPI.GetObjectProperty(ClientAPI.GetPlayerOid(), "adminLevel");
            if (adminLevel >= 3)
            {
#if AT_I2LOC_PRESET
            ClientAPI.Write(I2.Loc.LocalizationManager.GetTranslation("Properties for") + ": " + target.Name + " " + I2.Loc.LocalizationManager.GetTranslation("matching") + ": " + args_str);
#else
                ClientAPI.Write("Properties for: " + target.Name + " matching: " + args_str);
#endif
                foreach (string prop in target.PropertyNames)
                {
                    if (prop.Contains(args_str))
                        ClientAPI.Write(prop + ": " + target.GetProperty(prop));
                }
            }
        }

        public void HandleSay(string args_str)
        {
            AtavismLogger.LogDebugMessage("Got say command with message: " + args_str);
            //ClientAPI.Network.SendCommMessage(args_str);
            CommMessage commMessage = new CommMessage();
            commMessage.ChannelId = 1;  // CommChannel.Say
            commMessage.Message = args_str;
            commMessage.SenderName = ClientAPI.GetPlayerObject().Name;
            AtavismNetworkHelper.Instance.SendMessage(commMessage);
            AtavismLogger.LogDebugMessage("Sent chat message: " + commMessage);
        }

        public void HandleGroupChat(string args_str)
        {
            AtavismGroup.Instance.SendGroupChatMessage(args_str);
        }

        public void HandleLeaveGroup(string args_str)
        {
            AtavismGroup.Instance.LeaveGroup();
        }

        public void HandleSpawnPet(string args_str)
        {
            Instantiate(Resources.Load(""));
        }

        public void HandlePurchaseSkillPoint(string args_str)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "combat.PURCHASE_SKILL_POINT", props);
        }

        public void HandleBuildingGridProbe(string args_str)
        {
            GameObject probe = (GameObject)Resources.Load("Content/BuildingGridProbe");
            Instantiate(probe, ClientAPI.GetPlayerObject().Position, ClientAPI.GetPlayerObject().Orientation);
            AtavismLogger.LogDebugMessage("Created probe");
        }

        public void HandleToggleBuilder(string args_str)
        {
            //Camera.main.GetComponentInChildren<MobCreator>().ToggleBuildingModeEnabled();
            UGUIMobCreator.Instance.ToggleBuildingModeEnabled();

        }

        public void HandleClaim(string args_str)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("loc", ClientAPI.GetPlayerObject().Position);
            int size = int.Parse(args_str);
            props.Add("size", size);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.CREATE_CLAIM", props);
#if AT_I2LOC_PRESET
        ClientAPI.Write(I2.Loc.LocalizationManager.GetTranslation("Sent claim message"));
#else
            ClientAPI.Write("Sent claim message");
#endif
        }

        public void HandleWave(string args_str)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("coordEffect", "PlayWaveAnimation");
            props.Add("hasTarget", false);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.PLAY_COORD_EFFECT", props);
        }

        public void HandleAdmin(string args_str)
        {
            int adminLevel = (int)ClientAPI.GetObjectProperty(ClientAPI.GetPlayerOid(), "adminLevel");
            if (adminLevel >= 3 && UGUIAdminPanel.Instance != null)
            {
                UGUIAdminPanel.Instance.Show();
            }
        }

        public void HandleCreateGuild(string args_str)
        {
            AtavismGuild.Instance.CreateGuild(args_str);
        }

        public void HandleInviteToGuild(string args_str)
        {
            if (args_str == "")
            {
                if (ClientAPI.GetTargetOid() > 0 && ClientAPI.GetPlayerOid() != ClientAPI.GetTargetOid())
                {
                    AtavismGuild.Instance.SendGuildCommand("invite", OID.fromLong(ClientAPI.GetTargetOid()), null);
                }
            }
            else
            {
                AtavismGuild.Instance.SendGuildCommand("invite", null, args_str);
            }
        }

        public void HandleGuildQuit(string args_str)
        {
            AtavismGuild.Instance.SendGuildCommand("quit", null, null);
        }

        public void HandleGuildDisband(string args_str)
        {
            AtavismGuild.Instance.SendGuildCommand("disband", null, args_str);
        }

        public void HandleGuildChat(string args_str)
        {
            AtavismGuild.Instance.SendGuildCommand("chat", null, args_str);
        }

        public void HandleArena(string args_str)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("arenaType", 1);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "arena.joinQueue", props);
#if AT_I2LOC_PRESET
        ClientAPI.Write(I2.Loc.LocalizationManager.GetTranslation("Sent arena message"));
#else
            ClientAPI.Write("Sent arena message");
#endif
        }

        public void Quit()
        {
            Application.Quit();
        }

    }
}
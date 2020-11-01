using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Atavism
{
    public class ArenaTeamPlayerEntry
    {
        public OID playerOid;
        public string playerName = "";
        public int score = 0;
        public int kills = 0;
        public int deaths = 0;
        public int damageTaken = 0;
        public int damageDealt = 0;
    }
    public class ArenaTeamEntry
    {
        public int goal = -1;
        public int score = 0;
        public string teamName = "";
        public List<ArenaTeamPlayerEntry> players = new List<ArenaTeamPlayerEntry>();
    }


    public class ArenaEntry
    {
        public int ArenaId = -1;
        // public OID NpcId;
        public string ArenaName = "";
        public string Description = "";
        //public string Objective = "";
        public int ArenaType = 0;
        public int StartMin = 0;
        public int EndMin = 0;
        public int StartHour = 0;
        public int EndHour = 0;
        //public string ProgressText = "";
        //public List<QuestGradeEntry> gradeInfo;
        //public bool Complete = false;
        public int ArenaLenght = 0;
        public List<int> teamSize = new List<int>();
        public int numTeams = 2;
        public int ReqLeval = 1;
        public int MaxLeval = 1;
        public bool ArenaQueued = false;
        //public string CompleteText = "";
    }

    public class Arena : MonoBehaviour
    {
        static Arena instance;
        int arenaSelectedIndex = 0;
        List<ArenaEntry> arenaEntries = new List<ArenaEntry>();
        List<ArenaTeamEntry> activeArenaTeams = new List<ArenaTeamEntry>();
        //   int activeArenaType = -1;
        //   int activeArenaCategory = -1;
        bool in_arena = false;
        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            NetworkAPI.RegisterExtensionMessageHandler("Arena_List", _HandleArenaList);
            NetworkAPI.RegisterExtensionMessageHandler("Arena_stat_update", _HandleArenaStatUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("Arena_teamstat_update", _HandleArenaTeamStatUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("arena_setup", _HandleArenaSetup);
            NetworkAPI.RegisterExtensionMessageHandler("arena_end", HandleArenaEndMessage);

        }


        private void _HandleArenaTeamStatUpdate(Dictionary<string, object> props)
        {
            AtavismLogger.LogDebugMessage("_HandleArenaStatUpdate START");
            try
            {
                int team = (int)props["team"];
                int score = (int)props["score"];
                if (activeArenaTeams[team] != null)
                    activeArenaTeams[team].score = score;
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("ARENA_SCORE_UPDATE", args);
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("Arena._HandleArenaStatUpdate Exeption " + e.Message);
            }
            AtavismLogger.LogDebugMessage("_HandleArenaStatUpdate END");
        }


        private void _HandleArenaStatUpdate(Dictionary<string, object> props)
        {
            AtavismLogger.LogDebugMessage("_HandleArenaStatUpdate START");
            try
            {
                string stat = (string)props["stat"];
                OID plyOid = (OID)props["player"];
                int team = (int)props["team"];
                int score = (int)props["score"];
                if (activeArenaTeams[team] != null)
                    foreach (ArenaTeamPlayerEntry ply in activeArenaTeams[team].players)
                    {
                        if (ply.playerOid.Equals(plyOid))
                        {
                            switch (stat)
                            {
                                case "score":
                                    ply.score = score;
                                    break;
                                case "kill":
                                    ply.kills = score;
                                    break;
                                case "death":
                                    ply.deaths = score;
                                    break;
                                case "damageTaken":
                                    ply.damageTaken = score;
                                    break;
                                case "damageDealt":
                                    ply.damageDealt = score;
                                    break;
                            }
                        }
                    }
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("ARENA_SCORE_UPDATE", args);
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("Arena._HandleArenaStatUpdate Exeption " + e.Message);
            }
            AtavismLogger.LogDebugMessage("_HandleArenaStatUpdate END");
        }
        private void _HandleArenaSetup(Dictionary<string, object> props)
        {
            in_arena = true;
            AtavismLogger.LogDebugMessage("_HandleArenaSetup START");

            try
            {
                int numTeams = (int)props["numTeams"];
                activeArenaTeams.Clear();
                for (int i = 0; i < numTeams; i++)
                {
                    ArenaTeamEntry activeArenaTeam = new ArenaTeamEntry();

                    activeArenaTeam.goal = (int)props["teamGoal" + i];
                    activeArenaTeam.score = (int)props["teamScore" + i];
                    activeArenaTeam.teamName = (string)props["teamName" + i];
                    int plys = (int)props["teamSize" + i];
                    for (int j = 0; j < plys; j++)
                    {
                        ArenaTeamPlayerEntry ply = new ArenaTeamPlayerEntry();
                        ply.playerOid = (OID)props["team" + i + "OID" + j];
                        ply.playerName = (string)props["team" + i + "Name" + j];
                        ply.score = (int)props["team" + i + "Score" + j];
                        ply.kills = (int)props["team" + i + "Kills" + j];
                        ply.deaths = (int)props["team" + i + "Deaths" + j];
                        ply.damageTaken = (int)props["team" + i + "DamageTaken" + j];
                        ply.damageDealt = (int)props["team" + i + "DamageDealt" + j];
                        activeArenaTeam.players.Add(ply);
                    }
                    /* 
                     for (int i = 0; i < usableSkins.size(); i++)
                     {
                         props.put("skin" + i, usableSkins.get(i));
                     }
                     props.put("numSkins", usableSkins.size());
                     */
                    activeArenaTeams.Add(activeArenaTeam);
                }
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("ARENA_SCORE_SETUP", args);
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("Arena._HandleArenaSetup Exeption " + e.Message);
            }
            AtavismLogger.LogDebugMessage("_HandleArenaSetup END");
        }

        private void OnDestroy()
        {
            NetworkAPI.RemoveExtensionMessageHandler("Arena_List", _HandleArenaList);
            NetworkAPI.RemoveExtensionMessageHandler("Arena_stat_update", _HandleArenaStatUpdate);
            NetworkAPI.RemoveExtensionMessageHandler("arena_setup", _HandleArenaSetup);
            NetworkAPI.RemoveExtensionMessageHandler("arena_end", HandleArenaEndMessage);
        }

        private void HandleArenaEndMessage(Dictionary<string, object> props)
        {
            in_arena = false;
        }

        public void ArenaEntrySelected(int pos)
        {
            arenaSelectedIndex = pos;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("ARENA_LIST_UPDATE", args);

        }
        public ArenaEntry GetSelectedArenaEntry()
        {
            if (arenaEntries.Count - 1 < arenaSelectedIndex)
                return null;
            if (arenaSelectedIndex == -1)
                return null;
            return arenaEntries[arenaSelectedIndex];
        }
        private void _HandleArenaList(Dictionary<string, object> props)
        {
            AtavismLogger.LogDebugMessage("_HandleArenaList start");
            try
            {
                arenaEntries.Clear();
                int numArena = (int)props["numArena"];
                //    npcID = (OID)props["npcID"];
                for (int i = 0; i < numArena; i++)
                {
                    ArenaEntry arenaEntry = new ArenaEntry();
                    arenaEntries.Add(arenaEntry);
                    arenaEntry.ArenaName = (string)props["arenaName" + i];
                    arenaEntry.ArenaId = (int)props["arenaTemp" + i];
                    arenaEntry.ArenaType = (int)props["arenaType" + i];
                    arenaEntry.ReqLeval = (int)props["arenaLevel" + i];
                    arenaEntry.MaxLeval = (int)props["arenaMaxLevel" + i];
                    arenaEntry.ArenaLenght = (int)props["arenaLenght" + i];
                    //   arenaEntry.NpcId = npcID;
                    arenaEntry.Description = (string)props["arenaDesc" + i];
                    arenaEntry.ArenaQueued = (bool)props["arenaQueued" + i];

                    arenaEntry.StartHour = (int)props["arenaStartHour" + i];
                    arenaEntry.EndHour = (int)props["arenaEndHour" + i];
                    arenaEntry.numTeams = (int)props["arenaNumTeams" + i];
                    for (int j = 0; j < arenaEntry.numTeams; j++)
                    {
                        arenaEntry.teamSize.Add((int)props["arenaTeamSize" + i + "_" + j]);
                    }
                    arenaEntry.StartMin = (int)props["arenaStartMin" + i];
                    arenaEntry.EndMin = (int)props["arenaEndMin" + i];

                    //    arenaEntry.Objective = (string)props["objective" + i];
                }
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("ARENA_LIST_UPDATE", args);
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("Arena._HandleArenaList Exeption " + e.Message);
            }
            AtavismLogger.LogDebugMessage("_HandleArenaList End");
        }

        public List<ArenaEntry> ArenaEntries
        {
            get
            {
                return arenaEntries;
            }
        }
        public List<ArenaTeamEntry> ArenaTeamEntries
        {
            get
            {
                return activeArenaTeams;
            }
        }
        public bool InArena
        {
            get
            {
                return in_arena;
            }
        }
        public static Arena Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
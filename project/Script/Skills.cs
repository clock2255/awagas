using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{
    public class Skills : MonoBehaviour
    {
        static Skills instance;

        int currentSkillPoints;
        int totalSkillPoints;
        int currentTalentPoints;
        int totalTalentPoints;
        int skillPointCost;
        Dictionary<int, Skill> playerSkills = new Dictionary<int, Skill>();
        Dictionary<int, Skill> skills;

        GameObject tempSkillDataStorage = null;

        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            tempSkillDataStorage = new GameObject("TemporarySkillData");
            DontDestroyOnLoad(tempSkillDataStorage);

            skills = new Dictionary<int, Skill>();
            Object[] skillPrefabs = Resources.LoadAll("Content/Skills");
            foreach (Object skillPrefab in skillPrefabs)
            {
                GameObject go = (GameObject)skillPrefab;
                Skill skillData = go.GetComponent<Skill>();
                if (skillData != null && skillData.id > 0 && !skills.ContainsKey(skillData.id))
                    skills.Add(skillData.id, skillData);
            }

            // Register for skills message
            NetworkAPI.RegisterExtensionMessageHandler("skills", HandleSkillUpdate);
        }

        public void ResetSkills(bool talents)
        {
            //NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/skillIncrease " + skillID);
        }

        public void IncreaseSkill(int skillID)
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/skillIncrease " + skillID);
        }

        public void DecreaseSkill(int skillID)
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/skillDecrease " + skillID);
        }

        public void PurchaseSkillPoint()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "combat.PURCHASE_SKILL_POINT", props);
        }

        public void HandleSkillUpdate(Dictionary<string, object> props)
        {
          //  Debug.LogError("HandleSkillUpdate");
            foreach (Skill skill in playerSkills.Values)
            {
                Destroy(skill);
            }
            playerSkills.Clear();
            currentSkillPoints = (int)props["skillPoints"];
            totalSkillPoints = (int)props["totalSkillPoints"];
            currentTalentPoints = (int)props["talentPoints"];
            totalTalentPoints = (int)props["totalTalentPoints"];
            skillPointCost = (int)props["skillPointCost"];
            int numSkills = (int)props["numSkills"];
            AtavismLogger.LogDebugMessage("Got skill update with numSkills: " + numSkills);
            for (int i = 0; i < numSkills; i++)
            {
                //Skill skill = gameObject.AddComponent<Skill> ();
                int skillID = (int)props["skill" + i + "ID"];

                if (!skills.ContainsKey(skillID))
                {
                    UnityEngine.Debug.LogWarning("Skill " + skillID + " does not exist");
                    continue;
                }
                Skill skill = GetSkillByID(skillID).Clone(tempSkillDataStorage);
                /*skill.id = skillID;
                skill.skillname = skills[skillID].skillname;
                skill.icon = skills[skillID].icon;
                skill.mainAspect = skills[skillID].mainAspect;
                skill.mainAspectOnly = skills[skillID].mainAspectOnly;*/
                skill.CurrentPoints = (int)props["skill" + i + "Current"];
                skill.CurrentLevel = (int)props["skill" + i + "Level"];
                skill.MaximumLevel = (int)props["skill" + i + "Max"];
                skill.exp = (int)props["skill" + i + "Xp"];
                skill.expMax = (int)props["skill" + i + "XpMax"];
                //Debug.LogError("skillID:" + skillID + " Xp:" + props["skill" + i + "Xp"] + " XpMax:" + props["skill" + i + "XpMax"]+ " CurrentLevel:"+ skill.CurrentLevel+ "  MaximumLevel:" + skill.MaximumLevel+ " CurrentPoints:"+ skill.CurrentPoints);
                playerSkills.Add(skillID, skill);
            }
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("SKILL_UPDATE", args);
        }
        public List<Skill> GetAllKnownCraftSkills()
        {
            List<Skill> _list = new List<Skill>();

            foreach (Skill s in playerSkills.Values)
            {
                if(s.type==0)
                    _list.Add(s);
            }
            return _list;
        }

        public List<int> GetAllKnownCraftSkillsID()
        {
            List<int> _list = new List<int>();
//            Debug.LogError("Ply skills " + playerSkills.Count+" skills="+skills.Count);
            foreach (Skill s in playerSkills.Values)
            {
//                Debug.LogError("Ply skill n=" + s.skillname+" t="+s.type);
                if (s.type == 0)
                    _list.Add(s.id);
            }

            return _list;
        }

        public Skill GetSkillByID(int id)
        {
            if (skills.ContainsKey(id))
            {
                return skills[id];
            }
            return null;
        }

        public int GetPlayerSkillLevel(int skillID)
        {
            if (playerSkills.ContainsKey(skillID))
            {
                return playerSkills[skillID].CurrentLevel;
            }
            return 0;
        }
        public int GetPlayerSkillLevel(string name)
        {
            foreach (Skill s in playerSkills.Values)
            {
                if (s.skillname.Equals(name))
                    return s.CurrentLevel;
            }
            return 0;
        }

        public Skill GetSkillOfAbility(int abilityID)
        {
            foreach (Skill skill in skills.Values)
            {
                if (skill.abilities.Contains(abilityID))
                    return skill;
            }
            return null;
        }

        #region Properties
        public static Skills Instance
        {
            get
            {
                return instance;
            }
        }

        public Dictionary<int, Skill> SkillsList
        {
            get
            {
                return skills;
            }
        }

        public Dictionary<int, Skill> PlayerSkills
        {
            get
            {
                return playerSkills;
            }
        }

        public int CurrentSkillPoints
        {
            get
            {
                return currentSkillPoints;
            }
        }

        public int TotalSkillPoints
        {
            get
            {
                return totalSkillPoints;
            }
        }
        public int CurrentTalentPoints
        {
            get
            {
                return currentTalentPoints;
            }
        }

        public int TotalTalentPoints
        {
            get
            {
                return totalTalentPoints;
            }
        }

        public int SkillPointCost
        {
            get
            {
                return skillPointCost;
            }
        }

        #endregion Properties
    }
}
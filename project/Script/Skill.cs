using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{
    public class Skill : MonoBehaviour
    {

        public int id = 0;
        public string skillname = "";
        public Sprite icon;
        public int mainAspect = -1;
        public int type = -1;
        public int oppositeAspect = -1;
        public bool mainAspectOnly = false;
        public int parentSkill = -1;
        public int parentSkillLevelReq = -1;
        public int playerLevelReq = 1;
        public List<int> abilities;
        public List<int> abilityLevelReqs;
        public int exp = 0;
        public int pcost = 0;
        public int expMax = 0;
        [SerializeField]
        int currentPoints;
        [SerializeField]
        int currentLevel;
        [SerializeField]
        int maximumLevel;
        public bool talent = false;

        // Use this for initialization
        void Start()
        {

        }

        public Skill Clone(GameObject go)
        {
            Skill clone = go.AddComponent<Skill>();
            clone.id = id;
            clone.skillname = skillname;
            clone.icon = icon;
            clone.mainAspect = mainAspect;
            clone.oppositeAspect = oppositeAspect;
            clone.mainAspectOnly = mainAspectOnly;
            clone.type = type;
            clone.parentSkill = parentSkill;
            clone.parentSkillLevelReq = parentSkillLevelReq;
            clone.playerLevelReq = playerLevelReq;
            clone.abilities = abilities;
            clone.abilityLevelReqs = abilityLevelReqs;
            clone.pcost = pcost;
            clone.talent = talent;
            return clone;
        }

        public int CurrentPoints
        {
            get
            {
                return currentPoints;
            }
            set
            {
                currentPoints = value;
            }
        }

        public int CurrentLevel
        {
            get
            {
                return currentLevel;
            }
            set
            {
                currentLevel = value;
            }
        }

        public int MaximumLevel
        {
            get
            {
                return maximumLevel;
            }
            set
            {
                maximumLevel = value;
            }
        }
    }
}
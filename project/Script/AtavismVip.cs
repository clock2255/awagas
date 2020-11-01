using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Atavism
{
    public class Bonus
    {
        public string name = "";
        public int value = 0;
        public float percentage = 0f;
    }
    public class Vip
    {
        public string name = "";
        public int level = 0;
        public string desc = "";
       // public float percentage = 0f;
        public Dictionary<string, Bonus> bonuses = new Dictionary<string,Bonus>();
    }
    public class AtavismVip : MonoBehaviour
    {
        static AtavismVip instance;
        private int level = 0;
        private long points = 0;
        private long max_points = 0;
        private long time = 0L;
        private float calculatedTime = 0L;
        private string Name = "";
        private string desc = "";
        private List<Bonus> bonuses = new List<Bonus>();
        private Dictionary<int, Vip> vips = new Dictionary<int, Vip>();
        private List<string> bonusesNames = new List<string>();
        // Start is called before the first frame update
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            NetworkAPI.RegisterExtensionMessageHandler("ao.VIP_UPDATE", handleVipUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("ao.ALL_VIP_UPDATE", handleAllVipUpdate);
            //  SceneManager.sceneLoaded += sceneLoaded;
        }

        private void handleVipUpdate(Dictionary<string, object> props)
        {
        //    Debug.LogError("handleVipUpdate");
            bonuses.Clear();
            level = (int)props["lev"];
            points = (long)props["points"];
            max_points = (long)props["mpoints"];
            time = (long)props["time"]/1000;
            Name = (string)props["name"];
            desc = (string)props["desc"];
            int bNum = (int)props["bNum"];
            for (int i = 0; i < bNum; i++)
            {
                string b = (string)props["b" + i];
                string[] ba = b.Split('|');
                Bonus bonus = new Bonus();
                bonus.name = ba[0];
                bonus.value = int.Parse(ba[1]);
                bonus.percentage = float.Parse(ba[2]);
                bonuses.Add(bonus);
            }
            if (time > 0)
                calculatedTime = Time.time + time;
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("VIP_UPDATE", event_args);
        //    Debug.LogError("handleVipUpdate END");
        }

        private void handleAllVipUpdate(Dictionary<string, object> props)
        {
           //    Debug.LogError("handleAllVipUpdate");
            vips.Clear();
            int levNum = (int)props["levNum"];
            for (int j = 0; j < levNum; j++)
            {
                Vip vipL = new Vip();

                vipL.level = (int)props["lev"+j];
              //  vipL. = (long)props["mpoints"];
                vipL.name = (string)props["name"+j];
                vipL.desc = (string)props["desc"+j];
                int bNum = (int)props["bNum"+j];
           //     Debug.LogError("handleAllVipUpdate name="+ vipL.name);
                for (int i = 0; i < bNum; i++)
                {
                    string b = (string)props["b" +j+"_"+ i];
                    string[] ba = b.Split('|');
                    Bonus bonus = new Bonus();
                    bonus.name = ba[0];
                    if (!bonusesNames.Contains(ba[0]))
                        bonusesNames.Add(ba[0]);
                    bonus.value = int.Parse(ba[1]);
                    bonus.percentage = float.Parse(ba[2]);
                    vipL.bonuses.Add(bonus.name,bonus);
                }
                vips.Add(vipL.level,vipL);
            }
          
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("VIPS_UPDATE", event_args);
           //     Debug.LogError("handleAllVipUpdate END");
        }
        public void GetVipStatus()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GET_VIP", props);
        }
        public void GetAllVips()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GET_ALL_VIP", props);
        }

        // Update is called once per frame
        void Update()
        {

        }
        public static AtavismVip Instance
        {
            get
            {
                return instance;
            }
        }

        public List<Bonus> GetBonuses
        {
            get
            {
                return bonuses;
            }
        }
        public Dictionary<int, Vip> GetVips
        {
            get
            {
                return vips;
            }
        }
        public List<string> GetBonuseNames
        {
            get
            {
                return bonusesNames;
            }
        }

        public int GetLevel
        {
            get
            {
                return level;
            }
        }
        public string GetName
        {
            get
            {
                return Name;
            }
        }
        public string GetDescription
        {
            get
            {
                return desc;
            }
        }

        public long GetPoints
        {
            get
            {
                return points;
            }
        }
        public long GetMaxPoints
        {
            get
            {
                return max_points;
            }
        }
        public long GetTime
        {
            get
            {

                return time;
            }
        }

        public float GetTimeElapsed
        {
            get
            {

                return calculatedTime;
            }
        }

    }
}
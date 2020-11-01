using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Atavism
{
    public class AtavismAchievement
    {
        public int id = -1;
        public string name;
        public string desc;
        public int value;
        public int max;
        public bool active;
    }

    public class AtavismAchievements : MonoBehaviour
    {
        static AtavismAchievements instance;
        public List<AtavismAchievement> achivments = new List<AtavismAchievement>();
        // Start is called before the first frame update
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            NetworkAPI.RegisterExtensionMessageHandler("ao.ACHIEVEMENTS_UPDATE", handleAchievementUpdate);
        }

        void OnDestroy()
        {
        }
        private void handleAchievementUpdate(Dictionary<string, object> props)
        {
            //  Debug.LogError("handleAchievementUpdate");
            achivments.Clear();
            int num = (int)props["num"];
            for (int i = 0; i < num;i++)
            {
                AtavismAchievement a = new AtavismAchievement();
                a.id = (int)props["id" + i];
                a.name = (string)props["name" + i];
                a.desc = (string)props["desc" + i];
                a.active = (bool)props["active" + i];
                a.value = (int)props["value" + i];
                a.max = (int)props["max" + i];
             //   Debug.LogError("handleAchievementUpdate "+a.name);
                achivments.Add(a);
            }
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("ACHIEVEMENT_UPDATE", event_args);

         //   Debug.LogError("handleAchievementUpdate end");
        }
        // Update is called once per frame
        public void GetAchievementStatus()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GET_ACHIEVEMENTS", props);
        }

        void Update()
        {

        }
        public static AtavismAchievements Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{
    public class TimeEventController : MonoBehaviour
    {

        public List<GameObject> objectsToToggle;
        public int activateHour;
        public int activateMinute;
        public int deactivateHour;
        public int deactivateMinute;
        bool currentlyActive = false;
        bool firstRun = true;
        //int frameSkip = 10;
        //int frameCount = 0;

        // Use this for initialization
        void Start()
        {
            AtavismEventSystem.RegisterEvent("WORLD_TIME_UPDATE", this);
            UpdateObjectStatus();
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("WORLD_TIME_UPDATE", this);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "WORLD_TIME_UPDATE")
            {
                UpdateObjectStatus();
            }
        }

        void UpdateObjectStatus()
        {
            //frameCount++;
            //if (frameCount % frameSkip != 0)
            //	return;

            bool withinActiveTime = false;
            if (deactivateHour < activateHour || (deactivateHour == activateHour && deactivateMinute < activateMinute))
            {
                if ((TimeManager.Instance.Hour < deactivateHour || (TimeManager.Instance.Hour == deactivateHour && TimeManager.Instance.Minute < deactivateMinute))
                    || (TimeManager.Instance.Hour > activateHour || TimeManager.Instance.Hour == activateHour && TimeManager.Instance.Minute >= activateMinute))
                {
                    withinActiveTime = true;
                }
            }
            else
            {
                if ((TimeManager.Instance.Hour > activateHour || TimeManager.Instance.Hour == activateHour && TimeManager.Instance.Minute >= activateMinute)
                    && (TimeManager.Instance.Hour < deactivateHour || (TimeManager.Instance.Hour == deactivateHour && TimeManager.Instance.Minute < deactivateMinute)))
                {
                    withinActiveTime = true;
                }
            }

            if (firstRun)
            {
                foreach (GameObject obj in objectsToToggle)
                {
                    obj.SetActive(false);
                }
                firstRun = false;
            }
            else if (currentlyActive && !withinActiveTime)
            {
                foreach (GameObject obj in objectsToToggle)
                {
                    obj.SetActive(false);
                }
                currentlyActive = false;
            }
            else if (!currentlyActive && withinActiveTime)
            {
                foreach (GameObject obj in objectsToToggle)
                {
                    obj.SetActive(true);
                }
                currentlyActive = true;
            }
        }

    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{
    public class TimeManager : MonoBehaviour
    {

        static TimeManager instance;

        public string timeGameObjectName;
        int day;
        int hour;
        int minute;
        float second = -1;
        float worldTimeSpeed = 1;
        string currentTime = "";

        float delay = 3;
        float updateTime = -1;
        float lastFrameTime = -1;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            NetworkAPI.RegisterExtensionMessageHandler("server_time", ServerTimeMessage);
        }

        // Update is called once per frame
        void Update()
        {
            if (second == -1)
                return;

            // Recalculate the current time
            second += (Time.realtimeSinceStartup - lastFrameTime) * worldTimeSpeed;
            lastFrameTime = Time.realtimeSinceStartup;
            if (second >= 60)
            {
                int minutesPassed = (int)second / 60;
                second -= 60 * minutesPassed;
                minute += minutesPassed;

                if (minute >= 60)
                {
                    minute -= 60;
                    hour += 1;
                }
                if (hour >= 24)
                {
                    hour -= 24;
                    day += 1;
                }

                // Send out a time update message since the minute/hour has changed
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("WORLD_TIME_UPDATE", args);
                //Debug.Log("Time is now: " + hour + ":" + minute + " with deltaTime: " + Time.deltaTime + " and minutesPassed: " + minutesPassed);
            }

            if (timeGameObjectName != "" && updateTime != -1 && Time.time > updateTime)
            {
                GameObject timeReqGameObject = GameObject.Find(timeGameObjectName);
                if (timeReqGameObject != null)
                {
                    timeReqGameObject.SendMessage("SetSecond", second);
                    timeReqGameObject.SendMessage("SetMinute", minute);
                    timeReqGameObject.SendMessage("SetHour", hour);
                    timeReqGameObject.SendMessage("SetDay", day);
                }
                updateTime = -1;
            }
        }

        public void ServerTimeMessage(Dictionary<string, object> props)
        {
            day = (int)props["day"];
            hour = (int)props["hour"];
            minute = (int)props["minute"];
            second = (int)props["second"];
            worldTimeSpeed = (float)props["worldTimeSpeed"];
            Debug.Log("Got Server Time Message with Day: " + day + ", hour: " + hour + ". minute: " + minute);
            lastFrameTime = Time.realtimeSinceStartup;
            updateTime = Time.time + delay;
        }

        public static TimeManager Instance
        {
            get
            {
                return instance;
            }
        }

        public string CurrentTime
        {
            get
            {
                return currentTime;
            }
        }

        public int Hour
        {
            get
            {
                return hour;
            }
        }

        public int Minute
        {
            get
            {
                return minute;
            }
        }
    }
}
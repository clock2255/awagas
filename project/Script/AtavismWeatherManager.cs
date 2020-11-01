using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if WORLDAPI_PRESENT
using WAPI;
#endif

namespace Atavism
{
    public class AtavismWeatherManager : MonoBehaviour
    {
        [AtavismSeparator("Destination Value")]
        [SerializeField] protected float Temperature = 0f;
        [SerializeField] protected float Humidity = 0f;
        [SerializeField] protected float WindDirection = 0f;
        [SerializeField] protected float WindSpeed = 0f;
        [SerializeField] protected float WindTurbulence = 0f;
        [SerializeField] protected float FogHeightPower = 0f;
        [SerializeField] protected float FogHeightMax = 0f;
        [SerializeField] protected float FogDistancePower = 0f;
        [SerializeField] protected float FogDistanceMax = 0f;
        [SerializeField] protected float RainPower = 0f;
        [SerializeField] protected float RainPowerTerrain = 0f;
        [SerializeField] protected float RainMinHeight = 0f;
        [SerializeField] protected float RainMaxHeight = 0f;
        [SerializeField] protected float HailPower = 0f;
        [SerializeField] protected float HailPowerTerrain = 0f;
        [SerializeField] protected float HailMinHeight = 0f;
        [SerializeField] protected float HailMaxHeight = 0f;
        [SerializeField] protected float SnowPower = 0f;
        [SerializeField] protected float SnowPowerTerrain = 0f;
        [SerializeField] protected float SnowMinHeight = 0f;
        [SerializeField] protected float SnowAge = 0f;
        [SerializeField] protected float ThunderPower = 0f;
        [SerializeField] protected float CloudPower = 0f;
        [SerializeField] protected float CloudMinHeight = 0f;
        [SerializeField] protected float CloudMaxHeight = 0f;
        [SerializeField] protected float CloudSpeed = 0f;
        [SerializeField] protected float MoonPhase = 0f;
        [SerializeField] protected float Season = 0f;
        [SerializeField] protected int year = 1;
        [SerializeField] protected int month = 1;
        [SerializeField] protected int day = 1;
        [SerializeField] protected int hour = 0;
        [SerializeField] protected int min = 0;
        [SerializeField] protected float second = 0;
        [SerializeField] protected float worldTimeSpeed = 1;
        [AtavismSeparator("")]
        [SerializeField] protected float TemperatureDelta = 0f;
        [SerializeField] protected float HumidityDelta = 0f;
        [SerializeField] protected float WindDirectionDelta = 0f;
        [SerializeField] protected float WindSpeedDelta = 0f;
        [SerializeField] protected float WindTurbulenceDelta = 0f;
        [SerializeField] protected float FogHeightPowerDelta = 0f;
        [SerializeField] protected float FogHeightMaxDelta = 0f;
        [SerializeField] protected float FogDistancePowerDelta = 0f;
        [SerializeField] protected float FogDistanceMaxDelta = 0f;
        [SerializeField] protected float RainPowerDelta = 0f;
        [SerializeField] protected float RainPowerTerrainDelta = 0f;
        [SerializeField] protected float RainMinHeightDelta = 0f;
        [SerializeField] protected float RainMaxHeightDelta = 0f;
        [SerializeField] protected float HailPowerDelta = 0f;
        [SerializeField] protected float HailPowerTerrainDelta = 0f;
        [SerializeField] protected float HailMinHeightDelta = 0f;
        [SerializeField] protected float HailMaxHeightDelta = 0f;
        [SerializeField] protected float SnowPowerDelta = 0f;
        [SerializeField] protected float SnowPowerTerrainDelta = 0f;
        [SerializeField] protected float SnowMinHeightDelta = 0f;
        [SerializeField] protected float SnowAgeDelta = 0f;
        [SerializeField] protected float ThunderPowerDelta = 0f;
        [SerializeField] protected float CloudPowerDelta = 0f;
        [SerializeField] protected float CloudMinHeightDelta = 0f;
        [SerializeField] protected float CloudMaxHeightDelta = 0f;
        [SerializeField] protected float CloudSpeedDelta = 0f;
        [SerializeField] protected float MoonPhaseDelta = 0f;
        [SerializeField] protected float SeasonDelta = 0f;
        [SerializeField] protected string profile = "";
        [AtavismSeparator("Settings")]

        [SerializeField] protected float timeUpdateParam = 3f;
        [SerializeField] protected float timeUpdateTerrainParam = 4f;

        protected float timeDiff = 0f;
        Coroutine cor;
        bool firstSync = true;
        //   float syncTimeTik = 0f;
        // float syncTime = 0f;
        int zoneDiff = 0;
        DateTime wTime = new DateTime();
        bool apioff = true;
        bool resynctime = true;
        // Use this for initialization
        void Start()
        {
            NetworkAPI.RegisterExtensionMessageHandler("ao.weather_sync", HandleWeatherSync);
            SceneManager.sceneLoaded += sceneLoaded;
            //    StartCoroutine(UpdateTimer());
        }

        private void sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (!arg0.name.Equals("Login") && !arg0.name.Equals(ClientAPI.Instance.characterSceneName))
            {
#if WORLDAPI_PRESENT
                WorldManager.Instance.WorldAPIActive = true;
                Dictionary<string, object> props = new Dictionary<string, object>();
                NetworkAPI.SendExtensionMessage(0, false, "ao.GET_WEATHER", props);
#endif
                apioff = false;
            }
            else
            {
                apioff = true;
                firstSync = true;
#if WORLDAPI_PRESENT
                WorldManager.Instance.WorldAPIActive = false;
                /*  StopAllCoroutines();
                  cor = null;
                  */
#endif
            }
        }

        private void HandleWeatherSync(Dictionary<string, object> props)
        {
            if (props.ContainsKey("temperature"))
                Temperature = (float)props["temperature"];
            if (props.ContainsKey("humidity"))
                Humidity = (float)props["humidity"];
            if (props.ContainsKey("windDirection"))
                WindDirection = (float)props["windDirection"];
            if (props.ContainsKey("windSpeed"))
                WindSpeed = (float)props["windSpeed"];
            if (props.ContainsKey("windTurbulence"))
                WindTurbulence = (float)props["windTurbulence"];
            if (props.ContainsKey("fogHeightPower"))
                FogHeightPower = (float)props["fogHeightPower"];
            if (props.ContainsKey("fogHeightMax"))
                FogHeightMax = (float)props["fogHeightMax"];
            if (props.ContainsKey("fogDistancePower"))
                FogDistancePower = (float)props["fogDistancePower"];
            if (props.ContainsKey("fogDistanceMax"))
                FogDistanceMax = (float)props["fogDistanceMax"];
            if (props.ContainsKey("rainPower"))
                RainPower = (float)props["rainPower"];
            if (props.ContainsKey("rainPowerTerrain"))
                RainPowerTerrain = (float)props["rainPowerTerrain"];
            if (props.ContainsKey("rainMinHeight"))
                RainMinHeight = (float)props["rainMinHeight"];
            if (props.ContainsKey("rainMaxHeight"))
                RainMaxHeight = (float)props["rainMaxHeight"];
            if (props.ContainsKey("hailPower"))
                HailPower = (float)props["hailPower"];
            if (props.ContainsKey("hailPowerTerrain"))
                HailPowerTerrain = (float)props["hailPowerTerrain"];
            if (props.ContainsKey("hailMinHeight"))
                HailMinHeight = (float)props["hailMinHeight"];
            if (props.ContainsKey("hailMaxHeight"))
                HailMaxHeight = (float)props["hailMaxHeight"];
            if (props.ContainsKey("snowPower"))
                SnowPower = (float)props["snowPower"];
            if (props.ContainsKey("snowPowerTerrain"))
                SnowPowerTerrain = (float)props["snowPowerTerrain"];
            if (props.ContainsKey("snowMinHeight"))
                SnowMinHeight = (float)props["snowMinHeight"];
            if (props.ContainsKey("snowAge"))
                SnowAge = (float)props["snowAge"];
            if (props.ContainsKey("thunderPower"))
                ThunderPower = (float)props["thunderPower"];
            if (props.ContainsKey("cloudPower"))
                CloudPower = (float)props["cloudPower"];
            if (props.ContainsKey("cloudMinHeight"))
                CloudMinHeight = (float)props["cloudMinHeight"];
            if (props.ContainsKey("cloudMaxHeight"))
                CloudMaxHeight = (float)props["cloudMaxHeight"];
            if (props.ContainsKey("cloudSpeed"))
                CloudSpeed = (float)props["cloudSpeed"];
            if (props.ContainsKey("moonPhase"))
                MoonPhase = (float)props["moonPhase"];
            if (props.ContainsKey("season"))
                Season = (float)props["season"];
            int _year = 1;
            int _month = 1;
            int _day = 1;
            int _hour = 0;
            int _min = 0;
            int _second = 0;
            if (props.ContainsKey("year"))
                _year = (int)props["year"];
            if (props.ContainsKey("month"))
                _month = (int)props["month"];
            if (props.ContainsKey("day"))
                _day = (int)props["day"];
            if (props.ContainsKey("hour"))
                _hour = (int)props["hour"];
            if (props.ContainsKey("minute"))
                _min = (int)props["minute"];
            if (props.ContainsKey("second"))
                _second = (int)props["second"];
            if (props.ContainsKey("worldTimeSpeed"))
                worldTimeSpeed = (float)props["worldTimeSpeed"];
            
            if (props.ContainsKey("zone"))
                zoneDiff = (int)props["zone"];

            if (firstSync)
            {
                //  syncTimeTik = Time.time;
                year = _year;
                month = _month;
                day = _day;
                hour = _hour;
                min = _min;
                second = _second;
                firstSync = false;
            }
            if (resynctime)
            {
                year = _year;
                month = _month;
                day = _day;
                hour = _hour;
                min = _min;
                second = _second;
                resynctime = false;
            }
            if (!firstSync)
            {
                //  syncTime = Time.time - syncTimeTik;
                //  syncTimeTik = Time.time;
            }
            //      if (ClientAPI.Instance.logLevel.Equals(LogLevel.Debug))
            AtavismLogger.LogDebugMessage("Weather Got Time  " + _year + " - " + _month + " - " + _day + " " + _hour + " : " + _min);
            float ts = (_year * 12 * 30 * 24 * 60 * 60f + _month * 30 * 24 * 60 * 60f + _day * 24 * 60 * 60f + _hour * 60 * 60f + _min * 60f + _second);
            float tc = (year * 12 * 30 * 24 * 60 * 60f + month * 30 * 24 * 60 * 60f + day * 24 * 60 * 60f + hour * 60 * 60f + min * 60f + second);
            if (Math.Abs(ts - tc) > (300 * worldTimeSpeed))
            {
                year = _year;
                month = _month;
                day = _day;
                hour = _hour;
                min = _min;
                second = _second;
                ts = (_year * 12 * 30 * 24 * 60 * 60f + _month * 30 * 24 * 60 * 60f + _day * 24 * 60 * 60f + _hour * 60 * 60f + _min * 60f + _second);
                tc = (year * 12 * 30 * 24 * 60 * 60f + month * 30 * 24 * 60 * 60f + day * 24 * 60 * 60f + hour * 60 * 60f + min * 60f + second);
            }

            if ((ts - tc) != 0)
            {
                timeDiff = (ts - tc);/// (syncTime*0.5f);
                if (timeDiff > 20 * worldTimeSpeed)
                    timeDiff = 20 * worldTimeSpeed;
                if (timeDiff < -20 * worldTimeSpeed)
                    timeDiff = -20 * worldTimeSpeed;
            }
            else
            {
                timeDiff = 0f;
            }
            //      if (ClientAPI.Instance.logLevel.Equals(LogLevel.Debug))
            //           Debug.LogError("AtavismWeatherManager: Time Diff (s) " +timeDiff + " "+ worldTimeSpeed);

          
            if (props.ContainsKey("profile"))
                profile = (string)props["profile"];
#if WORLDAPI_PRESENT
            if (year < 1)
                year = 1;
            if (month < 1)
                month = 1;
            if (day < 1)
                year = 1;
            if (hour < 0)
                hour = 0;
            if (min < 0)
                min = 0;
            if (second < 0)
                second = 0;

         //   ClientAPI.Write("sT="+ _year + "-" + _month + "-" + _day + " " + _hour + ":" + _min + ":" + _second + " | cT="+year + "-" + month + "-" + day + " " + hour + ":" + min + ":" + second + " tdiff=" + timeDiff + " " + profile+" dT="+ (ts - tc));

            //Time
            //Temperature
            if (Temperature != WorldManager.Instance.Temperature)
                TemperatureDelta = (Temperature - WorldManager.Instance.Temperature) / timeUpdateParam;
            //Humidity
            if (Humidity != WorldManager.Instance.Humidity)
                HumidityDelta = (Humidity - WorldManager.Instance.Humidity) / timeUpdateParam;
            //Wind
            if (WindDirection != WorldManager.Instance.WindDirection)
                WindDirectionDelta = (WindDirection - WorldManager.Instance.WindDirection) / timeUpdateParam;
            if (WindSpeed != WorldManager.Instance.WindSpeed)
                WindSpeedDelta = (WindSpeed - WorldManager.Instance.WindSpeed) / timeUpdateParam;
            if (WindTurbulence != WorldManager.Instance.WindTurbulence)
                WindTurbulenceDelta = (WindTurbulence - WorldManager.Instance.WindTurbulence) / timeUpdateParam;
            //Fog
            if (FogHeightPower != WorldManager.Instance.FogHeightPower)
                FogHeightPowerDelta = (FogHeightPower - WorldManager.Instance.FogHeightPower) / timeUpdateParam;
            if (FogHeightMax != WorldManager.Instance.FogHeightMax)
                FogHeightMaxDelta = (FogHeightMax - WorldManager.Instance.FogHeightMax) / timeUpdateParam;
            if (FogDistancePower != WorldManager.Instance.FogDistancePower)
                FogDistancePowerDelta = (FogDistancePower - WorldManager.Instance.FogDistancePower) / timeUpdateParam;
            if (FogDistanceMax != WorldManager.Instance.FogDistanceMax)
                FogDistanceMaxDelta = (FogDistanceMax - WorldManager.Instance.FogDistanceMax) / timeUpdateParam;
            //Rain
            if (RainPower != WorldManager.Instance.RainPower)
                RainPowerDelta = (RainPower - WorldManager.Instance.RainPower) / timeUpdateParam;
            if (RainPowerTerrain != WorldManager.Instance.RainPowerTerrain)
                RainPowerTerrainDelta = (RainPowerTerrain - WorldManager.Instance.RainPowerTerrain) / (timeUpdateParam + timeUpdateTerrainParam);
            if (RainMinHeight != WorldManager.Instance.RainMinHeight)
                RainMinHeightDelta = (RainMinHeight - WorldManager.Instance.RainMinHeight) / timeUpdateParam;
            if (RainMaxHeight != WorldManager.Instance.RainMaxHeight)
                RainMaxHeightDelta = (RainMaxHeight - WorldManager.Instance.RainMaxHeight) / timeUpdateParam;
            //Hail
            if (HailPowerTerrain != WorldManager.Instance.HailPowerTerrain)
                HailPowerTerrainDelta = (HailPowerTerrain - WorldManager.Instance.HailPowerTerrain) / (timeUpdateParam + timeUpdateTerrainParam);
            if (HailPower != WorldManager.Instance.HailPower)
                HailPowerDelta = (HailPower - WorldManager.Instance.HailPower) / timeUpdateParam;
            if (HailMinHeight != WorldManager.Instance.HailMinHeight)
                HailMinHeightDelta = (HailMinHeight - WorldManager.Instance.HailMinHeight) / timeUpdateParam;
            if (HailMaxHeight != WorldManager.Instance.HailMaxHeight)
                HailMaxHeightDelta = (HailMaxHeight - WorldManager.Instance.HailMaxHeight) / timeUpdateParam;
            //Snow
            if (SnowPowerTerrain != WorldManager.Instance.SnowPowerTerrain)
                SnowPowerTerrainDelta = (SnowPowerTerrain - WorldManager.Instance.SnowPowerTerrain) / (timeUpdateParam + timeUpdateTerrainParam);
            if (SnowPower != WorldManager.Instance.SnowPower)
                SnowPowerDelta = (SnowPower - WorldManager.Instance.SnowPower) / timeUpdateParam;
            if (SnowMinHeight != WorldManager.Instance.SnowMinHeight)
                SnowMinHeightDelta = (SnowMinHeight - WorldManager.Instance.SnowMinHeight) / timeUpdateParam;
            if (SnowAge != WorldManager.Instance.SnowAge)
                SnowAgeDelta = (SnowAge - WorldManager.Instance.SnowAge) / timeUpdateParam;
            //Thunder
            if (ThunderPower != WorldManager.Instance.ThunderPower)
                ThunderPowerDelta = (ThunderPower - WorldManager.Instance.ThunderPower) / timeUpdateParam;
            //Cloud
            if (CloudPower != WorldManager.Instance.CloudPower)
                CloudPowerDelta = (CloudPower - WorldManager.Instance.CloudPower) / timeUpdateParam;
            if (CloudMinHeight != WorldManager.Instance.CloudMinHeight)
                CloudMinHeightDelta = (CloudMinHeight - WorldManager.Instance.CloudMinHeight) / timeUpdateParam;
            if (CloudMaxHeight != WorldManager.Instance.CloudMaxHeight)
                CloudMaxHeightDelta = (CloudMaxHeight - WorldManager.Instance.CloudMaxHeight) / timeUpdateParam;
            if (CloudSpeed != WorldManager.Instance.CloudSpeed)
                CloudSpeedDelta = (CloudSpeed - WorldManager.Instance.CloudSpeed) / timeUpdateParam;
            //Moon Phase
            if (MoonPhase != WorldManager.Instance.MoonPhase)
                MoonPhaseDelta = (MoonPhase - WorldManager.Instance.MoonPhase) / timeUpdateParam;
            //Season
            WorldManager.Instance.Season = Season;
            /*   if (cor == null) 
                   cor = StartCoroutine(UpdateTimer());*/

#endif
        }


        private void FixedUpdate()
        {

            second = second + worldTimeSpeed * Time.fixedDeltaTime * ((16 * worldTimeSpeed < timeDiff) ? 1.3f : (worldTimeSpeed * 8 < timeDiff) ? 1.15f : (worldTimeSpeed * 2 < timeDiff) ? 1.05f :
                (-worldTimeSpeed * 16 > timeDiff) ? 0.7f : (-worldTimeSpeed * 8 > timeDiff) ? 0.85f : (-worldTimeSpeed * 2 > timeDiff) ? 0.95f : 1f);
            if (second >= 60f)
            {
                int _minute = (int)(second / 60f);
                min = min + _minute;
                second -= _minute * 60f;
            }
            int sec = (int)Math.Round(second);
            if (sec >= 60)
            {
                int _minute = (int)(sec / 60f);
                min = min + _minute;
                second -= _minute * 60f;
                sec = sec - 60;
            }
            if (min >= 60)
            {
                int _hour = min / 60;
                hour = hour + _hour;
                min -= _hour * 60;
            }
            if (hour >= 24)
            {
                int _day = hour / 24;
                day = day + _day;
                hour -= _day * 24;
            }
            if (month == 2 && day > 28)
            {
                month++;
                day = day - 28;
            }
            if (day >= 31 && (month == 1 || month == 3 || month == 4 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12 ))
            {
                month++;
                day = day - 31;
            }
            if (day >= 30 && (month == 4 || month == 6 || month == 9 || month == 11 ))
            {
                month++;
                day = day - 30;
            }

            if (month > 12)
            {
                year++;
                month = month - 12;
            }
            if (year < 1)
                year = 1;
            if (month < 1)
                month = 1;
            if (day < 1)
                day = 1;
            if (hour < 0)
                hour = 0;
            if (min < 0)
                min = 0;
            if (second < 0f)
                second = 0f;

            if (!apioff)
            {
                string zone = "Z";
                if (zoneDiff != 0)
                {
                    if (zoneDiff > 0)
                        zone = "+" + zoneDiff.ToString("D2") + ":00";
                    else
                        zone = zoneDiff.ToString("D2") + ":00";
                }
                //Debug.LogError(">"+(year < 1000 ? "0" + (year < 100 ? "0" + (year < 10 ? "0" + year : "" + year) : "" + year) : "" + year) + "-" + (month < 10 ? "0" + month : "" + month) + "-" + (day < 10 ? "0" + day : "" + day) + " " + (hour < 10 ? "0" + hour : "" + hour) + ":" + (min < 10 ? "0" + min : "" + min) + ":" + (second < 10f ? "0" + (int)second : "" + (int)second) + "Z<"+timeDiff);
                //  Debug.LogError(">" + (year < 1000 ? "0" + (year < 100 ? "0" + (year < 10 ? "0" + year : "" + year) : "" + year) : "" + year) + "-" + (month < 10 ? "0" + month : "" + month) + "-" + (day < 10 ? "0" + day : "" + day) + " " + (hour < 10 ? "0" + hour : "" + hour) + ":" + (min < 10 ? "0" + min : "" + min) + ":" + (second < 10f ? "0" + (int)second : "" + (int)second) + zone + "<" + timeDiff);
                try
                {
                   // wTime = DateTime.Parse((year < 1000 ? "0" + (year < 100 ? "0" + (year < 10 ? "0" + year : "" + year) : "" + year) : "" + year) + "-" +(month < 10 ? "0" + month : "" + month) + "-" + (day < 10 ? "0" + day : "" + day) + "T" +(hour < 10 ? "0" + hour : "" + hour) + ":" + (min < 10 ? "0" + min : "" + min)+ ":" + (second < 10f ? "0" + (int)second : "" + (int)second) + zone);
                    wTime = new DateTime(year, month, day, hour, min, sec);
                    
                }
                catch (Exception e)
                {
                    Debug.LogError("DateTime Parse Error >" + (year < 1000 ? "0" + (year < 100 ? "0" + (year < 10 ? "0" + year : "" + year) : "" + year) : "" + year) + "-" + (month < 10 ? "0" + month : "" + month) + "-" + (day < 10 ? "0" + day : "" + day) + "T" + (hour < 10 ? "0" + hour : "" + hour) + ":" + (min < 10 ? "0" + min : "" + min) + ":" + (second < 10f ? "0" + (int)second : "" + (int)second) + zone + "<" + timeDiff + " " + e);
                }

#if WORLDAPI_PRESENT
                WorldManager.Instance.GameTime = wTime; // new DateTime(year, month, day, hour, min, (int)second, 1);

                //Temperature
                if (WorldManager.Instance.Temperature != Temperature)
                    if (Math.Abs(Temperature - WorldManager.Instance.Temperature) > Math.Abs(TemperatureDelta))
                        WorldManager.Instance.Temperature += TemperatureDelta;
                    else
                        WorldManager.Instance.Temperature = Temperature;
                //Humidity
                if (WorldManager.Instance.Humidity != Humidity)
                    if (Math.Abs(Humidity - WorldManager.Instance.Humidity) > Math.Abs(HumidityDelta))
                        WorldManager.Instance.Humidity += HumidityDelta;
                    else
                        WorldManager.Instance.Humidity = Humidity;
                //Wind
                if (WorldManager.Instance.WindDirection != WindDirection)
                    if (Math.Abs(WindDirection - WorldManager.Instance.WindDirection) > Math.Abs(WindDirectionDelta))
                        WorldManager.Instance.WindDirection += WindDirectionDelta;
                    else
                        WorldManager.Instance.WindDirection = WindDirection;
                if (WorldManager.Instance.WindSpeed != WindSpeed)
                    if (Math.Abs(WindSpeed - WorldManager.Instance.WindSpeed) > Math.Abs(WindSpeedDelta))
                        WorldManager.Instance.WindSpeed += WindSpeedDelta;
                    else
                        WorldManager.Instance.WindSpeed = WindSpeed;
                if (WorldManager.Instance.WindTurbulence != WindTurbulence)
                    if (Math.Abs(WindTurbulence - WorldManager.Instance.WindTurbulence) > Math.Abs(WindTurbulenceDelta))
                        WorldManager.Instance.WindTurbulence += WindTurbulenceDelta;
                    else
                        WorldManager.Instance.WindTurbulence = WindTurbulence;
                //Fog
                if (WorldManager.Instance.FogHeightPower != FogHeightPower)
                    if (Math.Abs(FogHeightPower - WorldManager.Instance.FogHeightPower) > Math.Abs(FogHeightPowerDelta))
                        WorldManager.Instance.FogHeightPower += FogHeightPowerDelta;
                    else
                        WorldManager.Instance.FogHeightPower = FogHeightPower;
                if (WorldManager.Instance.FogHeightMax != FogHeightMax)
                    if (Math.Abs(FogHeightMax - WorldManager.Instance.FogHeightMax) > Math.Abs(FogHeightMaxDelta))
                        WorldManager.Instance.FogHeightMax += FogHeightMaxDelta;
                    else
                        WorldManager.Instance.FogHeightMax = FogHeightMax;
                if (WorldManager.Instance.FogDistancePower != FogDistancePower)
                    if (Math.Abs(FogDistancePower - WorldManager.Instance.FogDistancePower) > Math.Abs(FogDistancePowerDelta))
                        WorldManager.Instance.FogDistancePower += FogDistancePowerDelta;
                    else
                        WorldManager.Instance.FogDistancePower = FogDistancePower;
                if (WorldManager.Instance.FogDistanceMax != FogDistanceMax)
                    if (Math.Abs(FogDistanceMax - WorldManager.Instance.FogDistanceMax) > Math.Abs(FogDistanceMaxDelta))
                        WorldManager.Instance.FogDistanceMax += FogDistanceMaxDelta;
                    else
                        WorldManager.Instance.FogDistanceMax = FogDistanceMax;
                //Rain
                if (WorldManager.Instance.RainPower != RainPower)
                    if (Math.Abs(RainPower - WorldManager.Instance.RainPower) > Math.Abs(RainPowerDelta))
                        WorldManager.Instance.RainPower += RainPowerDelta;
                    else
                        WorldManager.Instance.RainPower = RainPower;
                if (WorldManager.Instance.RainPowerTerrain != RainPowerTerrain)
                    if (Math.Abs(RainPowerTerrain - WorldManager.Instance.RainPowerTerrain) > Math.Abs(RainPowerTerrainDelta))
                        WorldManager.Instance.RainPowerTerrain += RainPowerTerrainDelta;
                    else
                        WorldManager.Instance.RainPowerTerrain = RainPowerTerrain;
                if (WorldManager.Instance.RainMinHeight != RainMinHeight)
                    if (Math.Abs(RainMinHeight - WorldManager.Instance.RainMinHeight) > Math.Abs(RainMinHeightDelta))
                        WorldManager.Instance.RainMinHeight += RainMinHeightDelta;
                    else
                        WorldManager.Instance.RainMinHeight = RainMinHeight;
                if (WorldManager.Instance.RainMaxHeight != RainMaxHeight)
                    if (Math.Abs(RainMaxHeight - WorldManager.Instance.RainMaxHeight) > Math.Abs(RainMaxHeightDelta))
                        WorldManager.Instance.RainMaxHeight += RainMaxHeightDelta;
                    else
                        WorldManager.Instance.RainMaxHeight = RainMaxHeight;
                //Hail
                if (WorldManager.Instance.HailPower != HailPower)
                    if (Math.Abs(HailPower - WorldManager.Instance.HailPower) > Math.Abs(HailPowerDelta))
                        WorldManager.Instance.HailPower += HailPowerDelta;
                    else
                        WorldManager.Instance.HailPower = HailPower;
                if (WorldManager.Instance.HailPowerTerrain != HailPowerTerrain)
                    if (Math.Abs(HailPowerTerrain - WorldManager.Instance.HailPowerTerrain) > Math.Abs(HailPowerTerrainDelta))
                        WorldManager.Instance.HailPowerTerrain += HailPowerTerrainDelta;
                    else
                        WorldManager.Instance.HailPowerTerrain = HailPowerTerrain;
                if (WorldManager.Instance.HailMinHeight != HailMinHeight)
                    if (Math.Abs(HailMinHeight - WorldManager.Instance.HailMinHeight) > Math.Abs(HailMinHeightDelta))
                        WorldManager.Instance.HailMinHeight += HailMinHeightDelta;
                    else
                        WorldManager.Instance.HailMinHeight = HailMinHeight;
                if (WorldManager.Instance.HailMaxHeight != HailMaxHeight)
                    if (Math.Abs(HailMaxHeight - WorldManager.Instance.HailMaxHeight) > Math.Abs(HailMaxHeightDelta))
                        WorldManager.Instance.HailMaxHeight += HailMaxHeightDelta;
                    else
                        WorldManager.Instance.HailMaxHeight = HailMaxHeight;
                //Snow
                if (WorldManager.Instance.SnowPower != SnowPower)
                    if (Math.Abs(SnowPower - WorldManager.Instance.SnowPower) > Math.Abs(SnowPowerDelta))
                        WorldManager.Instance.SnowPower += SnowPowerDelta;
                    else
                        WorldManager.Instance.SnowPower = SnowPower;
                if (WorldManager.Instance.SnowPowerTerrain != SnowPowerTerrain)
                    if (Math.Abs(SnowPowerTerrain - WorldManager.Instance.SnowPowerTerrain) > Math.Abs(SnowPowerTerrainDelta))
                        WorldManager.Instance.SnowPowerTerrain += SnowPowerTerrainDelta;
                    else
                        WorldManager.Instance.SnowPowerTerrain = SnowPowerTerrain;
                if (WorldManager.Instance.SnowMinHeight != SnowMinHeight)
                    if (Math.Abs(SnowMinHeight - WorldManager.Instance.SnowMinHeight) > Math.Abs(SnowMinHeightDelta))
                        WorldManager.Instance.SnowMinHeight += SnowMinHeightDelta;
                    else
                        WorldManager.Instance.SnowMinHeight = SnowMinHeight;
                if (WorldManager.Instance.SnowAge != SnowAge)
                    if (Math.Abs(SnowAge - WorldManager.Instance.SnowAge) > Math.Abs(SnowAgeDelta))
                        WorldManager.Instance.SnowAge += SnowAgeDelta;
                    else
                        WorldManager.Instance.SnowAge = SnowAge;
                //Thunder
                if (WorldManager.Instance.ThunderPower != ThunderPower)
                    if (Math.Abs(ThunderPower - WorldManager.Instance.ThunderPower) > Math.Abs(ThunderPowerDelta))
                        WorldManager.Instance.ThunderPower += ThunderPowerDelta;
                    else
                        WorldManager.Instance.ThunderPower = ThunderPower;
                //Cloud
                if (WorldManager.Instance.CloudPower != CloudPower)
                    if (Math.Abs(CloudPower - WorldManager.Instance.CloudPower) > Math.Abs(CloudPowerDelta))
                        WorldManager.Instance.CloudPower += CloudPowerDelta;
                    else
                        WorldManager.Instance.CloudPower = CloudPower;
                if (WorldManager.Instance.CloudMinHeight != CloudMinHeight)
                    if (Math.Abs(CloudMinHeight - WorldManager.Instance.CloudMinHeight) > Math.Abs(CloudMinHeightDelta))
                        WorldManager.Instance.CloudMinHeight += CloudMinHeightDelta;
                    else
                        WorldManager.Instance.CloudMinHeight = CloudMinHeight;
                if (WorldManager.Instance.CloudMaxHeight != CloudMaxHeight)
                    if (Math.Abs(CloudMaxHeight - WorldManager.Instance.CloudMaxHeight) > Math.Abs(CloudMaxHeightDelta))
                        WorldManager.Instance.CloudMaxHeight += CloudMaxHeightDelta;
                    else
                        WorldManager.Instance.CloudMaxHeight = CloudMaxHeight;

                if (WorldManager.Instance.CloudSpeed != CloudSpeed)
                    if (Math.Abs(CloudSpeed - WorldManager.Instance.CloudSpeed) > Math.Abs(CloudSpeedDelta))
                        WorldManager.Instance.CloudSpeed += CloudSpeedDelta;
                    else
                        WorldManager.Instance.CloudSpeed = CloudSpeed;
                //Moon Phase
                if (WorldManager.Instance.MoonPhase != MoonPhase)
                    if (Math.Abs(MoonPhase - WorldManager.Instance.MoonPhase) > Math.Abs(MoonPhaseDelta))
                        WorldManager.Instance.MoonPhase += MoonPhaseDelta;
                    else
                        WorldManager.Instance.MoonPhase = MoonPhase;
#endif
                string[] args = new string[6];

                args[0] = min.ToString();// wTime.Minute.ToString();
                args[1] = hour.ToString();// wTime.Hour.ToString();
                args[2] = day.ToString();// wTime.Day.ToString();
                args[3] = month.ToString(); //wTime.Month.ToString();
                args[4] = year.ToString();// wTime.Year.ToString();
                args[5] = profile;
                AtavismEventSystem.DispatchEvent("WORLD_TIME_UPDATE_WAPI", args);
            }
        }


        /*

            IEnumerator UpdateTimer()
            {

                WaitForSeconds delay = new WaitForSeconds(0.1f);
                while (true) {
                    //Time
                    second = second + worldTimeSpeed * 0.1f *((worldTimeSpeed - timeDiff < 0f)?0.7f:1f) + (timeDiff<0?0:timeDiff) * 0.1f;
                    if (second >= 60f) {
                        int _minute = (int)(second / 60f);
                        min = min + _minute;
                        second -= _minute * 60f;
                    }
                    if (min >= 60) {
                        int _hour = min / 60;
                        hour = hour + _hour;
                        min -= _hour * 60;
                    }
                    if (hour >= 24) {
                        int _day = hour / 24;
                        day = day + _day;
                        hour -= _day * 24;
                    }
                     if (day > 30) {
                        month++;
                        day = day - 30;
                    }
                    if (month > 12) {
                        year++;
                        month = month - 12;
                    }
                    if (year < 1) year = 1;
                    if (month < 1) month = 1;
                    if (day < 1) year = 1;
                    if (hour < 0) hour = 0;
                    if (min < 0) min = 0;
                    if (second < 0f) second = 0f;
                    //Debug.LogError((year < 1000 ? "0" + (year < 100 ? "0" + (year < 10 ? "0" + year : "" + year) : "" + year) : "" + year) + "-" + (month < 10 ? "0" + month : "" + month) + "-" + (day < 10 ? "0" + day : "" + day) + " " + (hour < 10 ? "0" + hour : "" + hour) + ":" + (min < 10 ? "0" + min : "" + min) + ":" + (second < 10f ? "0" + (int)second : "" + (int)second) + "Z"+timeDiff);
                  wTime = DateTime.Parse((year < 1000 ? "0" + (year < 100 ? "0" + (year < 10 ? "0" + year : "" + year) : "" + year) : "" + year) + "-" + (month < 10 ? "0" + month : "" + month) + "-" + (day < 10 ? "0" + day : "" + day) + " " + (hour < 10 ? "0" + hour : "" + hour) + ":" + (min < 10 ? "0" + min : "" + min) + ":" + (second < 10f ? "0" + (int)second : "" + (int)second)+"Z");
        #if WORLDAPI_PRESENT
                    WorldManager.Instance.GameTime = wTime; // new DateTime(year, month, day, hour, min, (int)second, 1);

                    //Temperature
                    if (WorldManager.Instance.Temperature != Temperature)
                        if (Math.Abs(Temperature - WorldManager.Instance.Temperature) > Math.Abs(TemperatureDelta))
                            WorldManager.Instance.Temperature += TemperatureDelta;
                        else
                            WorldManager.Instance.Temperature = Temperature;
                    //Humidity
                    if (WorldManager.Instance.Humidity != Humidity)
                        if (Math.Abs(Humidity - WorldManager.Instance.Humidity) > Math.Abs(HumidityDelta))
                            WorldManager.Instance.Humidity += HumidityDelta;
                        else
                            WorldManager.Instance.Humidity = Humidity;
                    //Wind
                    if (WorldManager.Instance.WindDirection != WindDirection)
                        if (Math.Abs(WindDirection - WorldManager.Instance.WindDirection) > Math.Abs(WindDirectionDelta))
                            WorldManager.Instance.WindDirection += WindDirectionDelta;
                        else
                            WorldManager.Instance.WindDirection = WindDirection;
                    if (WorldManager.Instance.WindSpeed != WindSpeed)
                        if (Math.Abs(WindSpeed - WorldManager.Instance.WindSpeed )> Math.Abs(WindSpeedDelta))
                            WorldManager.Instance.WindSpeed += WindSpeedDelta;
                        else
                            WorldManager.Instance.WindSpeed = WindSpeed;
                    if (WorldManager.Instance.WindTurbulence != WindTurbulence)
                        if (Math.Abs(WindTurbulence - WorldManager.Instance.WindTurbulence )> Math.Abs(WindTurbulenceDelta))
                            WorldManager.Instance.WindTurbulence += WindTurbulenceDelta;
                        else
                            WorldManager.Instance.WindTurbulence = WindTurbulence;
                    //Fog
                    if (WorldManager.Instance.FogHeightPower != FogHeightPower)
                        if (Math.Abs(FogHeightPower - WorldManager.Instance.FogHeightPower) > Math.Abs(FogHeightPowerDelta))
                            WorldManager.Instance.FogHeightPower += FogHeightPowerDelta;
                        else
                            WorldManager.Instance.FogHeightPower = FogHeightPower;
                    if (WorldManager.Instance.FogHeightMax != FogHeightMax)
                        if (Math.Abs(FogHeightMax - WorldManager.Instance.FogHeightMax) > Math.Abs(FogHeightMaxDelta))
                            WorldManager.Instance.FogHeightMax += FogHeightMaxDelta;
                        else
                            WorldManager.Instance.FogHeightMax = FogHeightMax;
                    if (WorldManager.Instance.FogDistancePower != FogDistancePower)
                        if (Math.Abs(FogDistancePower - WorldManager.Instance.FogDistancePower) > Math.Abs(FogDistancePowerDelta))
                            WorldManager.Instance.FogDistancePower += FogDistancePowerDelta;
                        else
                            WorldManager.Instance.FogDistancePower = FogDistancePower;
                    if (WorldManager.Instance.FogDistanceMax != FogDistanceMax)
                        if (Math.Abs(FogDistanceMax - WorldManager.Instance.FogDistanceMax) > Math.Abs(FogDistanceMaxDelta))
                            WorldManager.Instance.FogDistanceMax += FogDistanceMaxDelta;
                        else
                            WorldManager.Instance.FogDistanceMax = FogDistanceMax;
                    //Rain
                    if (WorldManager.Instance.RainPower != RainPower)
                        if (Math.Abs(RainPower - WorldManager.Instance.RainPower) > Math.Abs(RainPowerDelta))
                            WorldManager.Instance.RainPower += RainPowerDelta;
                        else
                            WorldManager.Instance.RainPower = RainPower;
                    if (WorldManager.Instance.RainPowerTerrain != RainPowerTerrain)
                        if (Math.Abs(RainPowerTerrain - WorldManager.Instance.RainPowerTerrain )> Math.Abs(RainPowerTerrainDelta))
                            WorldManager.Instance.RainPowerTerrain += RainPowerTerrainDelta;
                        else
                            WorldManager.Instance.RainPowerTerrain = RainPowerTerrain;
                    if (WorldManager.Instance.RainMinHeight != RainMinHeight)
                        if (Math.Abs(RainMinHeight - WorldManager.Instance.RainMinHeight) > Math.Abs(RainMinHeightDelta))
                            WorldManager.Instance.RainMinHeight += RainMinHeightDelta;
                        else
                            WorldManager.Instance.RainMinHeight = RainMinHeight;
                    if (WorldManager.Instance.RainMaxHeight != RainMaxHeight)
                        if (Math.Abs(RainMaxHeight - WorldManager.Instance.RainMaxHeight )> Math.Abs(RainMaxHeightDelta))
                            WorldManager.Instance.RainMaxHeight += RainMaxHeightDelta;
                        else
                            WorldManager.Instance.RainMaxHeight = RainMaxHeight;
                    //Hail
                    if (WorldManager.Instance.HailPower != HailPower)
                        if (Math.Abs(HailPower - WorldManager.Instance.HailPower )> Math.Abs(HailPowerDelta))
                            WorldManager.Instance.HailPower += HailPowerDelta;
                        else
                            WorldManager.Instance.HailPower = HailPower;
                    if (WorldManager.Instance.HailPowerTerrain != HailPowerTerrain)
                        if (Math.Abs(HailPowerTerrain - WorldManager.Instance.HailPowerTerrain )> Math.Abs(HailPowerTerrainDelta))
                            WorldManager.Instance.HailPowerTerrain += HailPowerTerrainDelta;
                        else
                            WorldManager.Instance.HailPowerTerrain = HailPowerTerrain;
                    if (WorldManager.Instance.HailMinHeight != HailMinHeight)
                        if (Math.Abs(HailMinHeight - WorldManager.Instance.HailMinHeight) > Math.Abs(HailMinHeightDelta))
                            WorldManager.Instance.HailMinHeight += HailMinHeightDelta;
                        else
                            WorldManager.Instance.HailMinHeight = HailMinHeight;
                    if (WorldManager.Instance.HailMaxHeight != HailMaxHeight)
                        if (Math.Abs(HailMaxHeight - WorldManager.Instance.HailMaxHeight )> Math.Abs(HailMaxHeightDelta))
                            WorldManager.Instance.HailMaxHeight += HailMaxHeightDelta;
                        else
                            WorldManager.Instance.HailMaxHeight = HailMaxHeight;
                    //Snow
                    if (WorldManager.Instance.SnowPower != SnowPower)
                        if (Math.Abs(SnowPower - WorldManager.Instance.SnowPower) > Math.Abs(SnowPowerDelta))
                            WorldManager.Instance.SnowPower += SnowPowerDelta;
                        else
                            WorldManager.Instance.SnowPower = SnowPower;
                    if (WorldManager.Instance.SnowPowerTerrain != SnowPowerTerrain)
                        if (Math.Abs(SnowPowerTerrain - WorldManager.Instance.SnowPowerTerrain )> Math.Abs(SnowPowerTerrainDelta))
                            WorldManager.Instance.SnowPowerTerrain += SnowPowerTerrainDelta;
                        else
                            WorldManager.Instance.SnowPowerTerrain = SnowPowerTerrain;
                    if (WorldManager.Instance.SnowMinHeight != SnowMinHeight)
                        if (Math.Abs(SnowMinHeight - WorldManager.Instance.SnowMinHeight) > Math.Abs(SnowMinHeightDelta))
                            WorldManager.Instance.SnowMinHeight += SnowMinHeightDelta;
                        else
                            WorldManager.Instance.SnowMinHeight = SnowMinHeight;
                    if (WorldManager.Instance.SnowAge != SnowAge)
                        if (Math.Abs(SnowAge - WorldManager.Instance.SnowAge )> Math.Abs(SnowAgeDelta))
                            WorldManager.Instance.SnowAge += SnowAgeDelta;
                        else
                            WorldManager.Instance.SnowAge = SnowAge;
                    //Thunder
                    if (WorldManager.Instance.ThunderPower != ThunderPower)
                        if (Math.Abs(ThunderPower - WorldManager.Instance.ThunderPower) > Math.Abs(ThunderPowerDelta))
                            WorldManager.Instance.ThunderPower += ThunderPowerDelta;
                        else
                            WorldManager.Instance.ThunderPower = ThunderPower;
                    //Cloud
                    if (WorldManager.Instance.CloudPower != CloudPower)
                        if (Math.Abs(CloudPower - WorldManager.Instance.CloudPower) > Math.Abs(CloudPowerDelta))
                            WorldManager.Instance.CloudPower += CloudPowerDelta;
                        else
                            WorldManager.Instance.CloudPower = CloudPower;
                    if (WorldManager.Instance.CloudMinHeight != CloudMinHeight)
                        if (Math.Abs(CloudMinHeight - WorldManager.Instance.CloudMinHeight )> Math.Abs(CloudMinHeightDelta))
                            WorldManager.Instance.CloudMinHeight += CloudMinHeightDelta;
                        else
                            WorldManager.Instance.CloudMinHeight = CloudMinHeight;
                    if (WorldManager.Instance.CloudMaxHeight != CloudMaxHeight)
                        if (Math.Abs(CloudMaxHeight - WorldManager.Instance.CloudMaxHeight) > Math.Abs(CloudMaxHeightDelta))
                            WorldManager.Instance.CloudMaxHeight += CloudMaxHeightDelta;
                        else
                            WorldManager.Instance.CloudMaxHeight = CloudMaxHeight;

                    if (WorldManager.Instance.CloudSpeed != CloudSpeed)
                        if (Math.Abs(CloudSpeed - WorldManager.Instance.CloudSpeed) > Math.Abs(CloudSpeedDelta))
                            WorldManager.Instance.CloudSpeed += CloudSpeedDelta;
                        else
                            WorldManager.Instance.CloudSpeed = CloudSpeed;
                    //Moon Phase
                    if (WorldManager.Instance.MoonPhase != MoonPhase)
                        if (Math.Abs(MoonPhase - WorldManager.Instance.MoonPhase )> Math.Abs(MoonPhaseDelta))
                            WorldManager.Instance.MoonPhase += MoonPhaseDelta;
                        else
                            WorldManager.Instance.MoonPhase = MoonPhase;
        #endif
                    string[] args = new string[6];
                    args[0] = wTime.Minute.ToString();
                    args[1] = wTime.Hour.ToString();
                    args[2] = wTime.Day.ToString();
                    args[3] = wTime.Month.ToString();
                    args[4] = wTime.Year.ToString();
                    args[5] = profile;
                    AtavismEventSystem.DispatchEvent("WORLD_TIME_UPDATE_WAPI", args);
                    yield return delay;
                    }
            }
        */
        void OnDestroy()
        {
            NetworkAPI.RemoveExtensionMessageHandler("ao.weather_sync", HandleWeatherSync);
        }
    }
}
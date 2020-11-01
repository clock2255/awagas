using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class Cooldown
    {
        public string name = "";
        public float length = 0;
        public float expiration;
    }

    public class Abilities : MonoBehaviour
    {

        static Abilities instance;

        public UGUIAtavismActivatable uguiAtavismAbilityPrefab;
        List<AtavismAbility> playerAbilities;
        Dictionary<int, AtavismAbility> abilities;
        Dictionary<int, AtavismEffect> effects;
        List<AtavismEffect> playerEffects;
        //   List<AtavismAbility> abilitiesList;

        List<Cooldown> cooldowns = new List<Cooldown>();
        GameObject tempCombatDataStorage = null;

        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            tempCombatDataStorage = new GameObject("TemporaryCombatData");
            DontDestroyOnLoad(tempCombatDataStorage);

            playerAbilities = new List<AtavismAbility>();
            //   abilitiesList = new List<AtavismAbility>();

            abilities = new Dictionary<int, AtavismAbility>();
            Object[] abilityPrefabs = Resources.LoadAll("Content/Abilities");
            foreach (Object abilityPrefab in abilityPrefabs)
            {
                GameObject go = (GameObject)abilityPrefab;
                AtavismAbility abilityData = go.GetComponent<AtavismAbility>();
                if (!abilities.ContainsKey(abilityData.id))
                {
                    abilities.Add(abilityData.id, abilityData);
                }
            }

            playerEffects = new List<AtavismEffect>();
            effects = new Dictionary<int, AtavismEffect>();
            Object[] effectPrefabs = Resources.LoadAll("Content/Effects");
            foreach (Object effectPrefab in effectPrefabs)
            {
                GameObject go = (GameObject)effectPrefab;
                AtavismEffect effectData = go.GetComponent<AtavismEffect>();
                if (!effects.ContainsKey(effectData.id))
                {
                    effects.Add(effectData.id, effectData);
                }
            }
        }

        void ClientReady()
        {
            // Register for abilities property
            ClientAPI.WorldManager.RegisterObjectPropertyChangeHandler("abilities", AbilitiesPropertyHandler);
            ClientAPI.WorldManager.RegisterObjectPropertyChangeHandler("effects", EffectsPropertyHandler);

            NetworkAPI.RegisterExtensionMessageHandler("cooldown", HandleCooldown);
        }

        public AtavismAbility GetAbility(int abilityID)
        {
            // First check if the player has a copy of this ability
            AtavismAbility ability = GetPlayerAbility(abilityID);
            if (ability == null)
            {
                // Player does not have this ability - lets use the template
                if (abilities.ContainsKey(abilityID))
                    return abilities[abilityID].Clone(tempCombatDataStorage);
            }
            return ability;
        }

        public AtavismAbility GetPlayerAbility(int abilityID)
        {
            AtavismAbility ability = null;
            foreach (AtavismAbility ab in playerAbilities)
            {
                if (ab.id == abilityID)
                {
                    return ab;
                }
            }
            return ability;
        }

        public AtavismEffect GetEffect(int effectID)
        {
            // Player does not have this ability - lets use the template
            if (effects.ContainsKey(effectID))
                return effects[effectID];
            return null;
        }

        public AtavismEffect GetPlayerEffect(int effectID)
        {
            foreach (AtavismEffect ab in playerEffects)
            {
                if (ab.id == effectID)
                {
                    return ab;
                }
            }
            return null;
        }

        public void RemoveBuff(AtavismEffect effect, int pos)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("effectID", effect.id);
            props.Add("pos", pos);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.REMOVE_BUFF", props);
        }

        public List<AtavismEffect> GetTargetEffects()
        {
            List<AtavismEffect> targetEffects = new List<AtavismEffect>();
            LinkedList<object> effects_prop = (LinkedList<object>)ClientAPI.GetTargetObject().GetProperty("effects");
            AtavismLogger.LogDebugMessage("Got target effects property change: " + effects_prop);
            //	int pos = 0;
            foreach (string effectsProp in effects_prop)
            {
                string[] effectData = effectsProp.Split(',');
                int effectID = int.Parse(effectData[0]);
                //long endTime = long.Parse(effectData[2]);
                //long serverTime = ClientAPI.ScriptObject.GetComponent<TimeManager>().ServerTime;
                //long timeTillEnd = endTime - serverTime;
                long timeUntilEnd = long.Parse(effectData[3]);
                bool active = bool.Parse(effectData[4]);
                long duration = long.Parse(effectData[5]);
                AtavismLogger.LogInfoMessage("Got effect " + effectID + " active? " + active);
                //if (timeTillEnd < duration)
                //	duration = timeTillEnd;

                float secondsLeft = (float)timeUntilEnd / 1000f;

                if (!effects.ContainsKey(effectID))
                {
                    AtavismLogger.LogWarning("Effect " + effectID + " does not exist");
                    continue;
                }
                AtavismEffect effect = effects[effectID].Clone(tempCombatDataStorage);
                effect.Active = active;
                effect.Expiration = Time.time + secondsLeft;
                effect.Length = duration / 1000f;
                targetEffects.Add(effect);
            }
            return targetEffects;
        }

        /// <summary>
        /// Currently not used by the default atavism system.
        /// </summary>
        /// <returns>The cooldown expiration.</returns>
        /// <param name="cooldownName">Cooldown name.</param>
        /// <param name="globalCooldown">If set to <c>true</c> global cooldown.</param>
        public float GetCooldownExpiration(string cooldownName, bool globalCooldown)
        {
            foreach (Cooldown cooldown in cooldowns)
            {
                if (cooldown.name == cooldownName)
                {
                    return cooldown.expiration;
                }
            }
            if (globalCooldown)
            {
                foreach (Cooldown cooldown in cooldowns)
                {
                    if (cooldown.name == "GLOBAL")
                    {
                        return cooldown.expiration;
                    }
                }
            }
            return -1;
        }

        public Cooldown GetCooldown(string cooldownName, bool globalCooldown)
        {
            Cooldown c = null;
            foreach (Cooldown cooldown in cooldowns)
            {
                if (cooldown.name == cooldownName)
                {
                    c = cooldown;
                    break;
                }
            }
            if (globalCooldown)
            {
                foreach (Cooldown cooldown in cooldowns)
                {
                    if (cooldown.name == "GLOBAL" && (c == null || c.expiration < cooldown.expiration))
                    {
                        c = cooldown;
                        break;
                    }
                }
            }
            return c;
        }

        public void AbilitiesPropertyHandler(object sender, ObjectPropertyChangeEventArgs args)
        {
            if (args.Oid != ClientAPI.GetPlayerOid())
                return;
            List<object> abilities_prop = (List<object>)ClientAPI.GetPlayerObject().GetProperty("abilities");
            AtavismLogger.LogDebugMessage("Got player abilities property change: " + abilities_prop);
            playerAbilities.Clear();
            //int pos = 0;
            foreach (int abilityNum in abilities_prop)
            {
                if (!abilities.ContainsKey(abilityNum))
                {
                    AtavismLogger.LogWarning("Ability " + abilityNum + " does not exist");
                    continue;
                }
                AtavismAbility ability = abilities[abilityNum].Clone(tempCombatDataStorage);
                playerAbilities.Add(ability);
            }
            // dispatch a ui event to tell the rest of the system
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("ABILITY_UPDATE", event_args);
        }

        public void EffectsPropertyHandler(object sender, ObjectPropertyChangeEventArgs args)
        {
            if (args.Oid != ClientAPI.GetPlayerOid())
                return;
            //ClientAPI.Write("Got effect update at time: " + Time.realtimeSinceStartup);
            LinkedList<object> effects_prop = (LinkedList<object>)ClientAPI.GetPlayerObject().GetProperty("effects");
            AtavismLogger.LogDebugMessage("Got player effects property change: " + effects_prop);
            playerEffects.Clear();
            //	int pos = 0;
            foreach (string effectsProp in effects_prop)
            {
                AtavismLogger.LogWarning("Effect: " + effectsProp);
                string[] effectData = effectsProp.Split(',');
                int effectID = int.Parse(effectData[0]);
                //long endTime = long.Parse(effectData[3]);
                // long serverTime = ClientAPI.ScriptObject.GetComponent<TimeManager>().ServerTime;
                //long timeTillEnd = endTime - serverTime;
                long timeUntilEnd = long.Parse(effectData[4]);
                bool active = bool.Parse(effectData[5]);
                long duration = long.Parse(effectData[6]);
                AtavismLogger.LogInfoMessage("Got effect " + effectID + " active? " + active);
                //if (timeTillEnd < duration)
                //	duration = timeTillEnd;

                float secondsLeft = (float)timeUntilEnd / 1000f;

                if (!effects.ContainsKey(effectID))
                {
                    AtavismLogger.LogWarning("Effect " + effectID + " does not exist");
                    continue;
                }
                AtavismEffect effect = effects[effectID].Clone(tempCombatDataStorage);
                effect.StackSize = int.Parse(effectData[1]);
                effect.isBuff = bool.Parse(effectData[2]);
                effect.Active = active;
                effect.Expiration = Time.time + secondsLeft;
                effect.Length = (float)duration / 1000f;
                playerEffects.Add(effect);
            }
            // dispatch a ui event to tell the rest of the system
            string[] event_args = new string[1];
            AtavismEventSystem.DispatchEvent("EFFECT_UPDATE", event_args);
        }

        public void HandleCooldown(Dictionary<string, object> props)
        {
            Cooldown old = GetCooldown((string)props["CdType"], false);
            if (old != null)
                cooldowns.Remove(old);
            Cooldown cooldown = new Cooldown();
            cooldown.name = (string)props["CdType"];
            cooldown.length = (long)props["CdLength"] / 1000f;
            cooldown.expiration = Time.time + cooldown.length;
            cooldowns.Add(cooldown);
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("COOLDOWN_UPDATE", args);
            //ClientAPI.Write("Got cooldown: " + cooldown.name + " with length: " + cooldown.length);
        }

        #region Properties
        public static Abilities Instance
        {
            get
            {
                return instance;
            }
        }

        public List<AtavismAbility> PlayerAbilities
        {
            get
            {
                return playerAbilities;
            }
        }

        public List<AtavismEffect> PlayerEffects
        {
            get
            {
                return playerEffects;
            }
        }

        #endregion Properties
    }
}
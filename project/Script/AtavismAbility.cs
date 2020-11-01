using UnityEngine;
using System.Collections;

namespace Atavism
{

    public class AtavismAbility : Activatable
    {

        public int id = 0;
        //public string command = "";
        //public string style = "";
        public string rank = "";
        public int cost = 0;
        public string costProperty = "mana";
        public int distance = 0;
        public float castTime = 0;
        public bool globalcd = false;
        public bool weaponcd = false;
        public string cooldownType = "";
        public float cooldownLength = 0;
        public string weaponReq = "";
        public int reagentReq = -1;
        public bool passive = false;
        public int reqLevel = 1;
        public bool castingInRun = false;
        //public string stancereq = "";
        public TargetType targetType;
        //Coroutine _activateWait = null;
        // Use this for initialization
        void Start()
        {

        }

        public AtavismAbility Clone(GameObject go)
        {
            AtavismAbility clone = go.AddComponent<AtavismAbility>();
            clone.id = id;
            clone.name = name;
            clone.icon = icon;
            //clone.style = style;
            clone.costProperty = costProperty;
            clone.rank = rank;
            clone.cost = cost;
            clone.distance = distance;
            clone.castTime = castTime;
            clone.globalcd = globalcd;
            clone.weaponcd = weaponcd;
            clone.cooldownType = cooldownType;
            clone.cooldownLength = cooldownLength;
            clone.weaponReq = weaponReq;
            clone.targetType = targetType;
            clone.tooltip = tooltip;
            clone.passive = passive;
            clone.reqLevel = reqLevel;
            clone.castingInRun = castingInRun;
            return clone;
        }

        public override bool Activate()
        {
            // TODO: Enhance the target/self setting based on whether the current
            // target is friendly or an enemy
            Cooldown _cooldown = GetLongestActiveCooldown();
            if (_cooldown != null)
            {
                if (_cooldown.expiration > Time.time)
                    return false;
            }
            if (ClientAPI.GetTargetObject() != null && targetType != TargetType.Self)
            {
                //  if (Vector3.Distance(ClientAPI.GetTargetObject().GameObject.transform.position, ClientAPI.GetPlayerObject().GameObject.transform.position) < distance)
                if (castTime > 0 && !castingInRun)
                {
                    StartCoroutine(ActivateWait());
                }
                else
                {
                    NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/ability " + id);
                }
            }
            else
            {
                if (castTime > 0 && !castingInRun)
                {
                    StartCoroutine(ActivateWait());
                }
                else
                {
                    NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ability " + id);
                }
            }
            return true;
        }


        /// <summary>
        /// Coroutine to stop player for start cast
        /// </summary>
        /// <returns></returns>
        IEnumerator ActivateWait()
        {
            ClientAPI.GetPlayerObject().GameObject.SendMessage("NoMove", 0.05f);

            yield return new WaitForSeconds(0.01f);

            if (ClientAPI.GetTargetObject() != null && targetType != TargetType.Self)
                NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/ability " + id);
            else
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ability " + id);

        }



        /// <summary>
        /// Draws the tooltip via the old GUI system
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public override void DrawTooltip(float x, float y)
        {
            int width = 150;
            int height = 50;
            Rect tooltipRect = new Rect(x, y - height, width, height);
            GUI.Box(tooltipRect, "");
            GUI.Label(new Rect(tooltipRect.x + 5, tooltipRect.y + 5, 140, 20), name);
            GUI.Label(new Rect(tooltipRect.x + 5, tooltipRect.y + 25, 140, 20), cost + " " + costProperty);
        }
        /// <summary>
        /// Function return longest effect cooldown 
        /// </summary>
        /// <returns></returns>
        public override Cooldown GetLongestActiveCooldown()
        {
            if (cooldownType == "" && Abilities.Instance != null)
            {
                return Abilities.Instance.GetCooldown(id.ToString(), true);
            }
            else
            {
                return Abilities.Instance.GetCooldown(cooldownType, true);
            }
        }

        /// <summary>
        /// Shows the tooltip using the new UGUI system.
        /// </summary>
        /// <param name="target">Target.</param>
        public void ShowTooltip(GameObject target)
        {

#if AT_I2LOC_PRESET
        UGUITooltip.Instance.SetTitle(I2.Loc.LocalizationManager.GetTranslation("Ability/" + name));
#else
            UGUITooltip.Instance.SetTitle(name);
#endif
            UGUITooltip.Instance.SetQuality(1);
            if (icon != null)
            {
                UGUITooltip.Instance.SetIcon(icon);
            }


            if (passive)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Passive"));
#else
                UGUITooltip.Instance.SetType("Passive");
#endif
                UGUITooltip.Instance.SetWeight("");

                if (weaponReq != "~ none ~")
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UGUITooltip.Instance.AddAttribute("Required Weapon: ", weaponReq, true);
#endif
                }


              /*  if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.itemStatLowerColour.r), ToByte(UGUITooltip.Instance.itemStatLowerColour.g), ToByte(UGUITooltip.Instance.itemStatLowerColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color="+ colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/

            }
            else
            {
                if ( targetType == TargetType.Enemy)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Enemy"));
#else
                    UGUITooltip.Instance.SetType("Target: Enemy");
#endif
                }
                else if (targetType == TargetType.AoE_Enemy )
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("AoE Enemy"));
#else
                    UGUITooltip.Instance.SetType("Target: AoE Enemy");
#endif
                }
                else if (targetType == TargetType.AoE_Friendly )
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " +I2.Loc.LocalizationManager.GetTranslation("AoE Friendly"));
#else
                    UGUITooltip.Instance.SetType("Target: AoE Friendly");
#endif
                }
                else if ( targetType == TargetType.Friendly)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " +I2.Loc.LocalizationManager.GetTranslation("Friendly"));
#else
                    UGUITooltip.Instance.SetType("Target: Friendly");
#endif
                }
                else if (targetType == TargetType.Self)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Self"));
#else
                    UGUITooltip.Instance.SetType("Target: Self");
#endif
                }
                else
                {
                    UGUITooltip.Instance.SetType("");
                }
                UGUITooltip.Instance.SetWeight("");
                if (castTime == 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetWeight(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Instant"));
#else
                    UGUITooltip.Instance.AddAttribute("CastTime ", "Instant", true);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetWeight(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": "+castTime + " seconds");
#else
                    UGUITooltip.Instance.AddAttribute("CastTime: ", castTime + " seconds", true);
#endif
                }


                if (cooldownLength > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Cooldown") + ": ", cooldownLength.ToString() + "s", true);
#else
                    UGUITooltip.Instance.AddAttribute("Cooldown: ", cooldownLength.ToString() + "s", true);
#endif
                }
                if (weaponReq != "~ none ~")
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UGUITooltip.Instance.AddAttribute("Required Weapon: ", weaponReq, true);
#endif
                }
              /*  if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.itemStatLowerColour.r), ToByte(UGUITooltip.Instance.itemStatLowerColour.g), ToByte(UGUITooltip.Instance.itemStatLowerColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/
                UGUITooltip.Instance.SetTypeColour(UGUITooltip.Instance.abilityCastTimeColour);
                if (cost > 0)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Cost") + ": ", cost.ToString() + " " + I2.Loc.LocalizationManager.GetTranslation(costProperty) , true, UGUITooltip.Instance.abilityCostColour);
#else
                    UGUITooltip.Instance.AddAttribute("Cost: ", cost.ToString() + " " + costProperty, true, UGUITooltip.Instance.abilityCostColour);
#endif
                }
                if (distance > 4)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ", distance.ToString() + "m" , true, UGUITooltip.Instance.abilityRangeColour);
#else
                    UGUITooltip.Instance.AddAttribute("Range: ", distance.ToString() + "m", true, UGUITooltip.Instance.abilityRangeColour);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ",  I2.Loc.LocalizationManager.GetTranslation("Melee") , true, UGUITooltip.Instance.abilityRangeColour);
#else
                    UGUITooltip.Instance.AddAttribute("Range: ", "Melee ", true, UGUITooltip.Instance.abilityRangeColour);
#endif
                }
            }
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.SetDescription(I2.Loc.LocalizationManager.GetTranslation("Ability/" +tooltip));
#else
            UGUITooltip.Instance.SetDescription(tooltip);
#endif
            UGUITooltip.Instance.Show(target);
        }

        /// <summary>
        /// Show aditional  tooltip
        /// </summary>
        public void ShowAdditionalTooltip()
        {
#if AT_I2LOC_PRESET
        UGUITooltip.Instance.SetAdditionalTitle(I2.Loc.LocalizationManager.GetTranslation("Ability/" + name));
#else
            UGUITooltip.Instance.SetAdditionalTitle(name);
#endif
            UGUITooltip.Instance.SetAdditionalQuality(1);
            if (icon != null)
            {
                UGUITooltip.Instance.SetAdditionalIcon(icon);
            }

            if (passive)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Passive"));
#else
                UGUITooltip.Instance.SetAdditionalType("Passive");
#endif
                UGUITooltip.Instance.SetAdditionalWeight("");
                if (weaponReq != "~ none ~")
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Required Weapon: ", weaponReq, true);
#endif
                }
               /* if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.itemStatLowerColour.r), ToByte(UGUITooltip.Instance.itemStatLowerColour.g), ToByte(UGUITooltip.Instance.itemStatLowerColour.b));
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/
            }
            else
            {
                if (targetType == TargetType.AoE_Enemy || targetType == TargetType.Enemy)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Enemy"));
#else
                    UGUITooltip.Instance.SetAdditionalType("Target: Enemy");
#endif
                }
                else if (targetType == TargetType.AoE_Friendly || targetType == TargetType.Friendly)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("Friendly"));
#else
                    UGUITooltip.Instance.SetAdditionalType("Target: Friendly");
#endif
                }
                else if (targetType == TargetType.Self)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("Self"));
#else
                    UGUITooltip.Instance.SetAdditionalType("Target: Self");
#endif
                }
                else
                {
                    UGUITooltip.Instance.SetAdditionalType("");
                }

                if (castTime == 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalWeight(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Instant"));
#else
                    UGUITooltip.Instance.SetAdditionalWeight("CastTime : Instant");
#endif
                }
                else if (castTime > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalWeight(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": " + castTime + " seconds");
#else
                    UGUITooltip.Instance.SetAdditionalWeight("CastTime: " + castTime + " seconds");
#endif
                }

                if (cooldownLength > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Cooldown") + ": ", cooldownLength.ToString() + "s", true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Cooldown: ", cooldownLength.ToString() + "s", true);
#endif
                }

                if (weaponReq != "~ none ~")
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Required Weapon: ", weaponReq, true);
#endif
                }

                if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.itemStatLowerColour.r), ToByte(UGUITooltip.Instance.itemStatLowerColour.g), ToByte(UGUITooltip.Instance.itemStatLowerColour.b));
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }

                if (cost > 0)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Cost") + ": ", cost.ToString() + " " + I2.Loc.LocalizationManager.GetTranslation(costProperty) , true, UGUITooltip.Instance.abilityCostColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Cost: ", cost.ToString() + " " + costProperty, true, UGUITooltip.Instance.abilityCostColour);
#endif
                }

                if (distance > 4)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ", distance.ToString() + "m" , true, UGUITooltip.Instance.abilityRangeColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Range: ", distance.ToString() + "m", true, UGUITooltip.Instance.abilityRangeColour);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ",  I2.Loc.LocalizationManager.GetTranslation("Melee") , true, UGUITooltip.Instance.abilityRangeColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Range: ", "Melee ", true, UGUITooltip.Instance.abilityRangeColour);
#endif
                }
            }
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.SetAdditionalDescription(I2.Loc.LocalizationManager.GetTranslation("Ability/" +tooltip));
#else
            UGUITooltip.Instance.SetAdditionalDescription(tooltip);
#endif
            UGUITooltip.Instance.ShowAdditionalTooltip();
        }

        /// <summary>
        /// Show aditional  tooltip
        /// </summary>
        public void ShowAdditionalTooltip2()
        {
#if AT_I2LOC_PRESET
        UGUITooltip.Instance.SetAdditionalTitle2(I2.Loc.LocalizationManager.GetTranslation("Ability/" + name));
#else
            UGUITooltip.Instance.SetAdditionalTitle2(name);
#endif
            UGUITooltip.Instance.SetAdditionalQuality2(1);
            if (icon != null)
            {
                UGUITooltip.Instance.SetAdditionalIcon2(icon);
            }

            if (passive)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Passive"));
#else
                UGUITooltip.Instance.SetAdditionalType2("Passive");
#endif
                UGUITooltip.Instance.SetAdditionalWeight2("");
                if (weaponReq != "~ none ~")
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Required Weapon: ", weaponReq, true);
#endif
                }
            /*    if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.itemStatLowerColour.r), ToByte(UGUITooltip.Instance.itemStatLowerColour.g), ToByte(UGUITooltip.Instance.itemStatLowerColour.b));
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute2("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute2("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/
            }
            else
            {
                if (targetType == TargetType.AoE_Enemy || targetType == TargetType.Enemy)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Enemy"));
#else
                    UGUITooltip.Instance.SetAdditionalType2("Target: Enemy");
#endif
                }
                else if (targetType == TargetType.AoE_Friendly || targetType == TargetType.Friendly)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("Friendly"));
#else
                    UGUITooltip.Instance.SetAdditionalType2("Target: Friendly");
#endif
                }
                else if (targetType == TargetType.Self)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("Self"));
#else
                    UGUITooltip.Instance.SetAdditionalType2("Target: Self");
#endif
                }
                else
                {
                    UGUITooltip.Instance.SetAdditionalType2("");
                }

                if (castTime == 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalWeight2(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Instant"));
#else
                    UGUITooltip.Instance.SetAdditionalWeight2("CastTime : Instant");
#endif
                }
                else if (castTime > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalWeight2(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": " + castTime + " seconds");
#else
                    UGUITooltip.Instance.SetAdditionalWeight2("CastTime: " + castTime + " seconds");
#endif
                }

                if (cooldownLength > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Cooldown") + ": ", cooldownLength.ToString() + "s", true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Cooldown: ", cooldownLength.ToString() + "s", true);
#endif
                }

                if (weaponReq != "~ none ~")
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Required Weapon: ", weaponReq, true);
#endif
                }

            /*    if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.itemStatLowerColour.r), ToByte(UGUITooltip.Instance.itemStatLowerColour.g), ToByte(UGUITooltip.Instance.itemStatLowerColour.b));
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute2("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute2("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/

                if (cost > 0)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Cost") + ": ", cost.ToString() + " " + I2.Loc.LocalizationManager.GetTranslation(costProperty) , true, UGUITooltip.Instance.abilityCostColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Cost: ", cost.ToString() + " " + costProperty, true, UGUITooltip.Instance.abilityCostColour);
#endif
                }

                if (distance > 4)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ", distance.ToString() + "m" , true, UGUITooltip.Instance.abilityRangeColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Range: ", distance.ToString() + "m", true, UGUITooltip.Instance.abilityRangeColour);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ",  I2.Loc.LocalizationManager.GetTranslation("Melee") , true, UGUITooltip.Instance.abilityRangeColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Range: ", "Melee ", true, UGUITooltip.Instance.abilityRangeColour);
#endif
                }
            }
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.SetAdditionalDescription2(I2.Loc.LocalizationManager.GetTranslation("Ability/" +tooltip));
#else
            UGUITooltip.Instance.SetAdditionalDescription2(tooltip);
#endif
            UGUITooltip.Instance.ShowAdditionalTooltip2();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
    }
}
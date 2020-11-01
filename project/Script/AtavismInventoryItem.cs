using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class AtavismInventoryItem : Activatable
    {

        string baseName = "";
        OID itemId = null;
        public int templateId = -1;
        string category = "";
        string subcategory = "";
        int count = 1;
        public string itemType = "";
        public string subType = "";
        public string slot = "";
        public int quality = 0;
        public int binding = 0;
        public bool isBound = false;
        bool unique = false;
        [SerializeField] int stackLimit = 1;
        public int currencyType = 0;
        public int cost = 0;
        public bool sellable = true;
        int displayID = 0;
        int energyCost = 0;
        int encumberance = 0;
        Dictionary<string, int> resistances = new Dictionary<string, int>();
        Dictionary<string, int> stats = new Dictionary<string, int>();
        Dictionary<string, int> enchantStats = new Dictionary<string, int>();
        Dictionary<string, Dictionary<int, int>> socketSlots = new Dictionary<string, Dictionary<int, int>>();
        Dictionary<string, Dictionary<int, long>> socketSlotsOid = new Dictionary<string, Dictionary<int, long>>();
        public int setCount = 0;
        public int enchantLeval = 0;
        [SerializeField] int damageValue = 0;
        [SerializeField] int damageMaxValue = 0;
        [SerializeField] string damageType = "";
        [SerializeField] int weaponSpeed = 2000;
        bool randomisedStats = false;
        int globalcd = 0;
        int weaponcd = 0;
        string cdtype2 = "";
        int durability = 0;
        int maxDurability = 0;
        public List<string> itemEffectTypes = new List<string>();
        public List<string> itemEffectNames = new List<string>();
        public List<string> itemEffectValues = new List<string>();
        public List<string> itemReqTypes = new List<string>();
        public List<string> itemReqNames = new List<string>();
        public List<string> itemReqValues = new List<string>();
        [SerializeField]
        int reqLevel = -1;
        public int setId = 0;
        [SerializeField] int enchantId = 0;
        // Dynamic settings for crafting and other systems that allow temporary placement of items
        int usedCount = 0;
        public bool auctionHouse = false;
        public int gear_score = 0;

        public AtavismInventoryItem Clone(GameObject go)
        {
            AtavismInventoryItem clone = go.AddComponent<AtavismInventoryItem>();
            clone.templateId = templateId;
            clone.name = name;
            clone.icon = icon;
            clone.baseName = baseName;
            clone.category = category;
            clone.subcategory = subcategory;
            clone.count = count;
            clone.itemType = itemType;
            clone.subType = subType;
            clone.slot = slot;
            clone.quality = quality;
            clone.binding = binding;
            clone.unique = unique;
            clone.stackLimit = stackLimit;
            clone.currencyType = currencyType;
            clone.cost = cost;
            clone.sellable = sellable;
            clone.displayID = displayID;
            clone.energyCost = energyCost;
            clone.encumberance = encumberance;
            clone.resistances = resistances;
            clone.stats = stats;
            clone.damageValue = damageValue;
            clone.damageMaxValue = damageMaxValue;
            clone.weaponSpeed = weaponSpeed;
            clone.randomisedStats = randomisedStats;
            clone.globalcd = globalcd;
            clone.weaponcd = weaponcd;
            clone.cdtype2 = cdtype2;
            clone.durability = durability;
            clone.maxDurability = maxDurability;
            clone.tooltip = tooltip;
            clone.itemEffectTypes = itemEffectTypes;
            clone.itemEffectNames = itemEffectNames;
            clone.itemEffectValues = itemEffectValues;
            clone.itemReqTypes = itemReqTypes;
            clone.itemReqNames = itemReqNames;
            clone.itemReqValues = itemReqValues;
            clone.ReqLeval = reqLevel;
            clone.SetId = setId;
            clone.enchantId = enchantId;
            clone.auctionHouse = auctionHouse;
            clone.gear_score = gear_score;
            return clone;
        }

        public override bool Activate()
        {
            // Not really the best way to do this, but it works - check if the item is a Claim Object
            if (GetEffectPositionsOfTypes("ClaimObject").Count > 0)
            {
                if (WorldBuilder.Instance.ActiveClaim != null)
                {
                    WorldBuilder.Instance.StartPlaceClaimObject(this);
                    return true;
                }
            }
            //TODO: provide proper target setup
            if (ClientAPI.GetTargetOid() > 0)
            {
                NetworkAPI.SendActivateItemMessage(itemId, ClientAPI.GetTargetOid());
            }
            else
            {
                NetworkAPI.SendActivateItemMessage(itemId, ClientAPI.GetPlayerOid());
            }

            return true;
        }

        /// <summary>
        /// Old Unity GUI Tooltip code. Not used by UGUI
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public override void DrawTooltip(float x, float y)
        {
            List<int> statPositions = GetEffectPositionsOfTypes("Stat");
            int width = 150;
            int height = 50 + statPositions.Count * 20;
            Rect tooltipRect = new Rect(x, y - height, width, height);
            GUI.Box(tooltipRect, "");
            GUI.Label(new Rect(tooltipRect.x + 5, tooltipRect.y + 5, 140, 20), name);
            GUI.Label(new Rect(tooltipRect.x + 5, tooltipRect.y + 25, 140, 20), itemType);
            for (int i = 0; i < statPositions.Count; i++)
            {
                string prefix = "";
                if (!itemEffectValues[statPositions[i]].Contains("-"))
                    prefix = "+";
                GUI.Label(new Rect(tooltipRect.x + 5, tooltipRect.y + 25 + ((i + 1) * 20), 140, 20),
                          prefix + itemEffectValues[statPositions[i]] + " " + itemEffectNames[statPositions[i]]);
            }
        }

        public void AlterUseCount(int delta)
        {
            usedCount += delta;
        }

        public void ResetUseCount()
        {
            usedCount = 0;
        }

        public void AddItemEffect(string itemEffectType, string itemEffectName, string itemEffectValue)
        {
            itemEffectTypes.Add(itemEffectType);
            itemEffectNames.Add(itemEffectName);
            itemEffectValues.Add(itemEffectValue);
        }

        public void ClearEffects()
        {
            itemEffectTypes.Clear();
            itemEffectNames.Clear();
            itemEffectValues.Clear();
        }

        public void AddItemRequirement(string itemReqType, string itemReqName, string itemReqValue)
        {
            itemReqTypes.Add(itemReqType);
            itemReqNames.Add(itemReqName);
            itemReqValues.Add(itemReqValue);
        }

        public void ClearRequirements()
        {
            itemReqTypes.Clear();
            itemReqNames.Clear();
            itemReqValues.Clear();
        }

        public List<int> GetEffectPositionsOfTypes(string effectType)
        {
            List<int> effectPositions = new List<int>();
            for (int i = 0; i < itemEffectTypes.Count; i++)
            {
                if (itemEffectTypes[i] == effectType)
                    effectPositions.Add(i);
            }
            return effectPositions;
        }

        //  List<int> equipEffectPositions = new List<int>();
        /// <summary>
        /// Shows the tooltip for UGUI implementation.
        /// </summary>
        /// <param name="target">Target.</param>
        public void ShowTooltip(GameObject target)
        {
            //  string defaultColor = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));
            List<AtavismInventoryItem> items = new List<AtavismInventoryItem>();
            foreach (AtavismInventoryItem item in Inventory.Instance.EquippedItems.Values)
            {
                if (item.itemId != itemId)
                    if (item.slot.ToLower() == slot.ToLower())
                    {
                        items.Add(item);
                    }
                    else if (item.slot == "Two Hand" && slot == "Main Hand")
                    {
                        items.Add(item);
                    }
                    else if (item.slot == "PrimaryWeapon" && slot == "Main Hand")
                    {
                        items.Add(item);
                    }
                    else if (item.slot == "PrimaryWeapon" && slot == "Two Hand")
                    {
                        items.Add(item);
                    }
                    else if (item.slot == "PrimaryRing" && slot == "Ring")
                    {
                        items.Add(item);
                    }
                    else if (item.slot == "SecondaryRing" && slot == "Ring")
                    {
                        items.Add(item);
                    }
                    else if (item.slot == "SecondaryWeapon" && slot == "Off Hand")
                    {
                        items.Add(item);
                    }
                    else if (item.slot == "SecondaryWeapon" && slot == "Two Hand")
                    {
                        items.Add(item);
                    }
                    else if (item.slot == "PrimaryEarring" && slot == "Earring")
                    {
                        items.Add(item);
                    }
                    else if (item.slot == "SecondaryEarring" && slot == "Earring")
                    {
                        items.Add(item);
                    }
            }

#if AT_I2LOC_PRESET
        UGUITooltip.Instance.SetTitle((enchantLeval > 0?" +"+ enchantLeval : "")+" "+I2.Loc.LocalizationManager.GetTranslation("Items/"+name));
#else
            UGUITooltip.Instance.SetTitle((enchantLeval > 0 ? " +" + enchantLeval : "") + " " + name);
#endif
            if (icon != null)
            {
                UGUITooltip.Instance.SetIcon(icon);
            }
            UGUITooltip.Instance.SetQuality(quality);
            UGUITooltip.Instance.SetTitleColour(AtavismSettings.Instance.ItemQualityColor(quality));
            string slotName = Inventory.Instance.GetItemByTemplateID(TemplateId).slot;
            /* switch (slot)
             {
                 case "PrimaryWeapon":
                     slotName = "Main Hand";
                     break;
                 case "SecondaryWeapon":
                     slotName = "Off Hand";
                     break;
                 case "PrimaryRing":
                     slotName = "Ring";
                     break;
                 case "SecondaryRing":
                     slotName = "Ring";
                     break;
                 case "PrimaryEarring":
                     slotName = "Earring";
                     break;
                 case "SecondaryEarring":
                     slotName = "Earring";
                     break;
                 default:
                     slotName = slot;
                     break;

             }*/

            if (itemType == "Armor")
            {
#if AT_I2LOC_PRESET
      		UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Slot") + ": "+I2.Loc.LocalizationManager.GetTranslation(slotName));
                  if(gear_score>0)
                    UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Gear Score"), gear_score.ToString(), true);
#else
                UGUITooltip.Instance.SetType(slotName);
                if (gear_score > 0)
                    UGUITooltip.Instance.AddAttribute("Gear Score", gear_score.ToString(), true);
#endif
            }
            else if (itemType == "Weapon")
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(" "+I2.Loc.LocalizationManager.GetTranslation(subType));
            UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation(slotName),"",true);
                 if(gear_score>0)
                    UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Gear Score"), gear_score.ToString(), true);
#else
                UGUITooltip.Instance.SetType(" " + subType);
                UGUITooltip.Instance.AddAttribute(slotName, "", true);
                if(gear_score>0)
                    UGUITooltip.Instance.AddAttribute("Gear Score", gear_score.ToString(), true);

#endif
            }
            else
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation(itemType));
#else
                UGUITooltip.Instance.SetType(itemType);
#endif
            }
            if (itemType == "Bag")
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Number of Slots"),stackLimit.ToString(),true);
#else
                UGUITooltip.Instance.AddAttribute("Number of Slots", stackLimit.ToString(), true);
#endif
            }
            UGUITooltip.Instance.SetTypeColour(UGUITooltip.Instance.itemTypeColour);
            if (Weight > 0)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetWeight(I2.Loc.LocalizationManager.GetTranslation("Weight")+": " + Weight + "(" + (Weight*count) + ")");
#else
                UGUITooltip.Instance.SetWeight("Weight: " + Weight + " (" + (Weight * count) + ")");
#endif
            }
            else
            {
                UGUITooltip.Instance.SetWeight("");
            }
            if (itemType == "Weapon" || itemEffectTypes.Contains("Stat"))
            {
                UGUITooltip.Instance.AddAttributeSeperator();
#if AT_I2LOC_PRESET
        UGUITooltip.Instance.AddAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Stats"), UGUITooltip.Instance.itemSectionTitleColour);
#else
                UGUITooltip.Instance.AddAttributeTitle("Stats", UGUITooltip.Instance.itemSectionTitleColour);
#endif
            }
            //Color colour = UGUITooltip.Instance.defaultTextColour;
            // string defaultColourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(colour.r), ToByte(colour.g), ToByte(colour.b));
            if (itemType == "Weapon")
            {
                // string colourText = "";
                //  colour = UGUITooltip.Instance.defaultTextColour;
                string textDamage = damageValue.ToString();
                string mark = "";
                if (damageMaxValue - damageValue > 0)
                    textDamage += " - " + damageMaxValue.ToString();


                if (enchantStats.ContainsKey("dmg-base"))
                    if ((damageMaxValue + damageValue) / 2 - (enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 != 0)
                        textDamage += " (" + ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 - (damageMaxValue + damageValue) / 2) + ")";
                if (itemType == "Weapon" || itemType == "Armor")
                {
                    foreach (AtavismInventoryItem item in items)
                    {
                        if (enchantStats.ContainsKey("dmg-base"))
                        {
                            if (item.enchantStats.ContainsKey("dmg-base"))
                            {
                                if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 < (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                    mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 > (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                    mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 == (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                    mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                            }
                            else
                            {
                                if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 < (item.damageMaxValue + item.damageValue) / 2)
                                    mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 > (item.damageMaxValue + item.damageValue) / 2)
                                    mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 == (item.damageMaxValue + item.damageValue) / 2)
                                    mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                            }
                        }
                        else
                        {
                            if (item.enchantStats.ContainsKey("dmg-base"))
                            {
                                if ((damageMaxValue + damageValue) / 2 < (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                    mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                if ((damageMaxValue + damageValue) / 2 > (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                    mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                if ((damageMaxValue + damageValue) / 2 == (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                    mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                            }
                            else
                            {
                                if ((damageMaxValue + damageValue) / 2 < (item.damageMaxValue + item.damageValue) / 2)
                                    mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                if ((damageMaxValue + damageValue) / 2 > (item.damageMaxValue + item.damageValue) / 2)
                                    mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                if ((damageMaxValue + damageValue) / 2 == (item.damageMaxValue + item.damageValue) / 2)
                                    mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                            }
                        }
                    }
                }
                //  colour = UGUITooltip.Instance.defaultTextColour;
                string textSpeed = ((float)weaponSpeed / 1000).ToString();
                string mark2 = "";

                if (itemType == "Weapon" || itemType == "Armor")
                    foreach (AtavismInventoryItem item in items)
                    {
                        if (item.weaponSpeed > weaponSpeed)
                            mark2 += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                        if (item.weaponSpeed < weaponSpeed)
                            mark2 += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                        if (item.weaponSpeed == weaponSpeed)
                            mark2 += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                    }
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAttribute(FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("Damage")) + " "+mark,  textDamage, true);
            UGUITooltip.Instance.AddAttribute(FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("attack_speed")) + " "+mark2,  textSpeed, true);
#else
                UGUITooltip.Instance.AddAttribute("Damage " + mark, textDamage, false, UGUITooltip.Instance.itemTypeColour);
                UGUITooltip.Instance.AddAttribute("Speed " + mark2, textSpeed, false, UGUITooltip.Instance.itemTypeColour);
#endif
            }

            List<string> additonalStats = new List<string>();

            foreach (string st in enchantStats.Keys)
            {
                if (!itemEffectNames.Contains(st) && !additonalStats.Contains(st))
                {
                    // Debug.LogError("Stat " + st);
                    additonalStats.Add(st);
                }
            }
            int ite = 0;
            if (itemType == "Weapon" || itemType == "Armor")
            {
                foreach (AtavismInventoryItem item in items)
                {
                    if (ite == 0)
                        showAdditionalTooltip(item);
                    else
                        showAdditionalTooltip2(item);
                    ite++;
                }
            }
            foreach (int statPos in GetEffectPositionsOfTypes("Stat"))
            {

                string statName = itemEffectNames[statPos];
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));
#else
                string[] statNames = statName.Split('_');
                if (statNames.Length > 1)
                {
                    statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                }
                else
                {
                    statName = FirstCharToUpper(statNames[0]);
                }
#endif
                //   colour = UGUITooltip.Instance.defaultTextColour;
                string textParam = "";
                if (int.Parse(itemEffectValues[statPos]) != 0)
                    textParam = itemEffectValues[statPos] + " ";
                if (enchantStats.ContainsKey(itemEffectNames[statPos]))
                    if (int.Parse(itemEffectValues[statPos]) - enchantStats[itemEffectNames[statPos]] != 0)
                        textParam += "(" + (enchantStats[itemEffectNames[statPos]]) + ")";
                string mark = "";

                bool printStat = false;
                if (textParam.Length > 0)
                    printStat = true;
                if (itemType == "Weapon" || itemType == "Armor")
                {

                    foreach (AtavismInventoryItem item in items)
                    {

                        if (item.itemEffectNames.Contains(itemEffectNames[statPos]))
                        {

                            int itemStatIndex = item.itemEffectNames.IndexOf(itemEffectNames[statPos]);
                            if (enchantStats.ContainsKey(itemEffectNames[statPos]))
                            {
                                if (item.enchantStats.ContainsKey(itemEffectNames[statPos]))
                                {
                                    if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[itemEffectNames[statPos]] > int.Parse(itemEffectValues[statPos]) + enchantStats[itemEffectNames[statPos]])
                                        mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[itemEffectNames[statPos]] < int.Parse(itemEffectValues[statPos]) + enchantStats[itemEffectNames[statPos]])
                                        mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[itemEffectNames[statPos]] == int.Parse(itemEffectValues[statPos]) + enchantStats[itemEffectNames[statPos]])
                                        mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                                }
                                else
                                {
                                    if (int.Parse(item.itemEffectValues[itemStatIndex]) > int.Parse(itemEffectValues[statPos]) + enchantStats[itemEffectNames[statPos]])
                                        mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[itemStatIndex]) < int.Parse(itemEffectValues[statPos]) + enchantStats[itemEffectNames[statPos]])
                                        mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[itemStatIndex]) == int.Parse(itemEffectValues[statPos]) + enchantStats[itemEffectNames[statPos]])
                                        mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                                }
                            }
                            else
                            {
                                if (item.enchantStats.ContainsKey(itemEffectNames[statPos]))
                                {
                                    if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[itemEffectNames[statPos]] > int.Parse(itemEffectValues[statPos]))
                                        mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[itemEffectNames[statPos]] < int.Parse(itemEffectValues[statPos]))
                                        mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[itemEffectNames[statPos]] == int.Parse(itemEffectValues[statPos]))
                                        mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                                }
                                else
                                {
                                    if (int.Parse(item.itemEffectValues[itemStatIndex]) > int.Parse(itemEffectValues[statPos]))
                                        mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[itemStatIndex]) < int.Parse(itemEffectValues[statPos]))
                                        mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[itemStatIndex]) == int.Parse(itemEffectValues[statPos]))
                                        mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                                }
                            }

                        }
                        else
                        {
                            mark += "<sprite=" + UGUITooltip.Instance.newSpriteId + ">";
                            ;
                        }

                    }
                }
                if (!string.IsNullOrEmpty(itemEffectValues[statPos]))
                    if (int.Parse(itemEffectValues[statPos]) > 0)
                    {
                        UGUITooltip.Instance.AddAttribute(statName + "  " + mark, textParam, false);
                    }
                    else if (int.Parse(itemEffectValues[statPos]) < 0)
                    {
                        UGUITooltip.Instance.AddAttribute(statName + "  " + mark, textParam, false);
                    }
                    else if (int.Parse(itemEffectValues[statPos]) == 0 && printStat)
                    {
                        UGUITooltip.Instance.AddAttribute(statName + "  " + mark, textParam, false);
                    }

            }

            //add stats from equiped items thats not in check item
            foreach (string addStatName in additonalStats)
            {
                bool printStat = false;
                string textParam = "";
                string mark = "";
                if (!addStatName.Equals("dmg-base") && !addStatName.Equals("dmg-max"))
                {
                    printStat = true;
                    textParam = "(" + enchantStats[addStatName] + ")";
                    foreach (AtavismInventoryItem item in items)
                    {
                        if (item.itemEffectNames.Contains(addStatName))
                        {
                            int itemStatIndex = item.itemEffectNames.IndexOf(addStatName);
                            if (item.enchantStats.ContainsKey(addStatName))
                            {
                                if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[addStatName] > enchantStats[addStatName])
                                    mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[addStatName] < enchantStats[addStatName])
                                    mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[addStatName] == enchantStats[addStatName])
                                    mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";

                            }
                            else
                            {
                                if (int.Parse(item.itemEffectValues[itemStatIndex]) > enchantStats[addStatName])
                                    mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                if (int.Parse(item.itemEffectValues[itemStatIndex]) < enchantStats[addStatName])
                                    mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                if (int.Parse(item.itemEffectValues[itemStatIndex]) == enchantStats[addStatName])
                                    mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                            }
                        }
                        else
                        {
                            mark += "<sprite=" + UGUITooltip.Instance.newSpriteId + ">";
                            ;
                        }
                    }
                }
                if (printStat)
                {
#if AT_I2LOC_PRESET
                string _addStatName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(addStatName));
#else
                    string[] statNames = addStatName.Split('_');
                    string _addStatName = FirstCharToUpper(statNames[0]);
                    if (statNames.Length > 1)
                    {
                        _addStatName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                    }
#endif
                    UGUITooltip.Instance.AddAttribute(_addStatName + mark, textParam, false);
                }
            }


            if (socketSlots.Count > 0)
            {
                UGUITooltip.Instance.AddAttributeSeperator();
                foreach (string key in socketSlots.Keys)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Sockets of")+" " + I2.Loc.LocalizationManager.GetTranslation(key), UGUITooltip.Instance.itemSectionTitleColour);
#else
                    UGUITooltip.Instance.AddAttributeTitle("Sockets of " + key, UGUITooltip.Instance.itemSectionTitleColour);
#endif
                    foreach (int it in socketSlots[key].Keys)
                    {
                        //  Debug.LogError("Socket " + key + " slot: " + it + " item:" + socketSlots[key][it]);
                        if (socketSlots[key][it] > 0)
                        {
                            AtavismInventoryItem aii = Inventory.Instance.GetItemByTemplateID(socketSlots[key][it]);
                            if (aii == null)
                            {
#if AT_I2LOC_PRESET
                        UGUITooltip.Instance.AddAttributeSocket(I2.Loc.LocalizationManager.GetTranslation("Empty"), null, false);
#else
                                UGUITooltip.Instance.AddAttributeSocket("Empty", null, false);
#endif
                                continue;
                            }
                            string socketStat = "";
                            foreach (int statPos in aii.GetEffectPositionsOfTypes("Stat"))
                            {
                                //Debug.LogError("Socket " + key + " slot: " + it + " item:" + socketSlots[key][it]+" Stat pos:"+ statPos+ " count:"+itemEffectNames.Count);

                                string statName = aii.itemEffectNames[statPos];
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));
#else
                                string[] statNames = statName.Split('_');
                                if (statNames.Length > 1)
                                {
                                    statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                                }
                                else
                                {
                                    statName = FirstCharToUpper(statNames[0]);
                                }
#endif
                                socketStat += statName + " " + (int.Parse(aii.itemEffectValues[statPos]) > 0 ? "+" + aii.itemEffectValues[statPos] : int.Parse(aii.itemEffectValues[statPos]) < 0 ? "-" + aii.itemEffectValues[statPos] : "0") + "\n";
                            }
                            UGUITooltip.Instance.AddAttributeSocket(socketStat, aii.icon, false);

                        }
                        else
                        {
#if AT_I2LOC_PRESET
                        UGUITooltip.Instance.AddAttributeSocket(I2.Loc.LocalizationManager.GetTranslation("Empty"), null, false);
#else
                            UGUITooltip.Instance.AddAttributeSocket("Empty", null, false);
#endif
                        }
                    }
                }
            }
            if (setId > 0)
            {
                AtavismInventoryItemSet aiis = Inventory.Instance.GetItemBySetID(setId);
                UGUITooltip.Instance.AddAttributeSeperator();
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Set Effects")+" "+I2.Loc.LocalizationManager.GetTranslation(aiis.Name),UGUITooltip.Instance.itemSectionTitleColour);
#else
                UGUITooltip.Instance.AddAttributeTitle("Set Effects " + aiis.Name, UGUITooltip.Instance.itemSectionTitleColour);
#endif

                foreach (AtavismInventoryItemSetLevel level in aiis.levelList)
                {
                    Color setColor = UGUITooltip.Instance.itemSetColour;
                    Color setTitleColor = UGUITooltip.Instance.itemSectionTitleColour;
                    if (setCount < level.NumerOfParts)
                    {
                        setColor = UGUITooltip.Instance.itemInactiveSetColour;
                        setTitleColor = UGUITooltip.Instance.itemInactiveSetColour;

                    }
#if AT_I2LOC_PRESET
                    UGUITooltip.Instance.AddAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Set")+" "+level.NumerOfParts+" "+I2.Loc.LocalizationManager.GetTranslation("parts"),setTitleColor);
#else
                    UGUITooltip.Instance.AddAttributeTitle("Set " + level.NumerOfParts + " parts", setTitleColor);
#endif
                    if (level.DamageValue != 0)
                    {
                        string statName = "Damage";
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("Damage"));
#else
                        string[] statNames = statName.Split('_');
                        if (statNames.Length > 1)
                        {
                            statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                        }
                        else
                        {
                            statName = FirstCharToUpper(statNames[0]);
                        }
#endif

                        if (level.DamageValue > 0)
                            UGUITooltip.Instance.AddAttribute(statName, "+" + level.DamageValue.ToString(), false, setColor);
                        if (level.DamageValue < 0)
                            UGUITooltip.Instance.AddAttribute(statName, "-" + level.DamageValue.ToString(), false, setColor);
                    }
                    if (level.DamageValuePercentage != 0)
                    {
                        string statName = "Damage";
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("Damage"));
#else
                        string[] statNames = statName.Split('_');
                        if (statNames.Length > 1)
                        {
                            statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                        }
                        else
                        {
                            statName = FirstCharToUpper(statNames[0]);
                        }
#endif

                        if (level.DamageValuePercentage > 0)
                            UGUITooltip.Instance.AddAttribute(statName, "+" + level.DamageValuePercentage.ToString() + "%", false, setColor);
                        if (level.DamageValuePercentage < 0)
                            UGUITooltip.Instance.AddAttribute(statName, "-" + level.DamageValuePercentage.ToString() + "%", false, setColor);
                    }

                    for (int k = 0; k < level.itemStatName.Count; k++)
                    {
                        string statName = level.itemStatName[k];
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));
#else
                        string[] statNames = statName.Split('_');
                        if (statNames.Length > 1)
                        {
                            statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                        }
                        else
                        {
                            statName = FirstCharToUpper(statNames[0]);
                        }
#endif
                        if (level.itemStatValues[k] > 0)
                            UGUITooltip.Instance.AddAttribute(statName, "+" + level.itemStatValues[k].ToString(), false, setColor);
                        if (level.itemStatValues[k] < 0)
                            UGUITooltip.Instance.AddAttribute(statName, "-" + level.itemStatValues[k].ToString(), false, setColor);
                        if (level.itemStatValuesPercentage[k] > 0)
                            UGUITooltip.Instance.AddAttribute(statName, "+" + level.itemStatValuesPercentage[k].ToString() + "%", false, setColor);
                        if (level.itemStatValuesPercentage[k] < 0)
                            UGUITooltip.Instance.AddAttribute(statName, "-" + level.itemStatValuesPercentage[k].ToString() + "%", false, setColor);
                    }

                }
            }
            foreach (int statPos in GetEffectPositionsOfTypes("TalentPoints"))
            {
                string statValue = itemEffectValues[statPos];
                string statName = "Talent Points";
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));

#else
                //statName += ((int.Parse(vals[0])!=0)?" "+vals[0]:"")+ ((float.Parse(vals[1]) != 0) ? " " + vals[1]+"%" : "");
#endif
                if (int.Parse(statValue) != 0)
                    UGUITooltip.Instance.AddAttribute(statName, statValue, false);
            }
            foreach (int statPos in GetEffectPositionsOfTypes("SkillPoints"))
            {
                string statValue = itemEffectValues[statPos];
                string statName = "Skill Points";
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));

#else
                //statName += ((int.Parse(vals[0])!=0)?" "+vals[0]:"")+ ((float.Parse(vals[1]) != 0) ? " " + vals[1]+"%" : "");
#endif
                if (int.Parse(statValue) != 0)
                    UGUITooltip.Instance.AddAttribute(statName, statValue, false);
            }

            UGUITooltip.Instance.AddAttributeSeperator();
            bool bonus = false;
            if (GetEffectPositionsOfTypes("Bonus").Count>0)
            {
#if AT_I2LOC_PRESET
        UGUITooltip.Instance.AddAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Bonuses"), UGUITooltip.Instance.itemSectionTitleColour);
#else
                UGUITooltip.Instance.AddAttributeTitle("Bonuses", UGUITooltip.Instance.itemSectionTitleColour);
#endif
            }

            foreach (int statPos in GetEffectPositionsOfTypes("Bonus"))
            {
                bonus = true;
                string statName = itemEffectNames[statPos];
                string statValue = itemEffectValues[statPos];
             //   Debug.LogError(statName+" "+ statValue);
                string[] vals = statValue.Split('|');
            //    Debug.LogError(statName + " " + vals[0]+" : "+vals[1]+" : "+vals[2]);
                if(vals.Length>2)
                statName = vals[2];

#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));

#else
                //statName += ((int.Parse(vals[0])!=0)?" "+vals[0]:"")+ ((float.Parse(vals[1]) != 0) ? " " + vals[1]+"%" : "");
#endif
                if (int.Parse(vals[0]) != 0)
                    UGUITooltip.Instance.AddAttribute(statName, vals[0], false);
                if (float.Parse(vals[1]) != 0)
                    UGUITooltip.Instance.AddAttribute(statName, vals[1]+"%", false);

            }
            if (bonus)
                UGUITooltip.Instance.AddAttributeSeperator();


            bool vip = false;
            if (GetEffectPositionsOfTypes("VipPoints").Count > 0 || GetEffectPositionsOfTypes("VipTime").Count > 0)
            {
#if AT_I2LOC_PRESET
        UGUITooltip.Instance.AddAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Vip"), UGUITooltip.Instance.itemSectionTitleColour);
#else
                UGUITooltip.Instance.AddAttributeTitle("Vip", UGUITooltip.Instance.itemSectionTitleColour);
#endif
            }

            foreach (int statPos in GetEffectPositionsOfTypes("VipPoints"))
            {
                vip = true;
                string statName = itemEffectNames[statPos];
                string statValue = itemEffectValues[statPos];
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("VipPoints"));
#else
                statName = "Vip Points";
#endif
                if (int.Parse(statValue) != 0)
                    UGUITooltip.Instance.AddAttribute(statName, statValue, false);
             

            }
         
            foreach (int statPos in GetEffectPositionsOfTypes("VipTime"))
            {
                vip = true;
                string statName = itemEffectNames[statPos];
                string statValue = itemEffectValues[statPos];

#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("VipTime"));
#else
                statName = "Vip Time";
#endif
                if (int.Parse(statValue) != 0)
                {
                    int time = int.Parse(statValue)*60;
                    int days = 0;
                    int hours = 0;
                    int minutes = 0;
                    if (time > 86400)
                    {
                        days = (int)time / 86400;
                        time -= days * 86400;
                    }
                    if (time > 3600)
                    {
                        hours = (int)time / 3600;
                        time -= hours * 3600;
                    }
                    if (time > 60)
                    {
                        minutes = (int)time / 60;
                        time = minutes * 60;
                    }
                    if (days > 0)
                    {
                        UGUITooltip.Instance.AddAttribute(statName , days + " days", false);
                    }
                    else if (hours > 0)
                    {
                        UGUITooltip.Instance.AddAttribute(statName,  hours + " hour", false);
                    }
                    else
                    {
                        UGUITooltip.Instance.AddAttribute(statName,  minutes + " minutes", false);
                    }
                }

            }
            if (vip)
                UGUITooltip.Instance.AddAttributeSeperator();


            if (itemReqTypes.Count > 0)


                for (int r = 0; r < itemReqTypes.Count; r++)
                {
                    if (itemReqTypes[r].Equals("Level"))
                    {
                        if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < int.Parse(itemReqValues[r]))
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel"),itemReqValues[r] , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAttribute("Required Level ", itemReqValues[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                        else
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel"),  itemReqValues[r] , true);
#else
                            UGUITooltip.Instance.AddAttribute("Required Level ", itemReqValues[r], true);
#endif
                    }

                    if (itemReqTypes[r].Equals("Class"))
                    {
                        if (((int)ClientAPI.GetPlayerObject().GetProperty("aspect")) == int.Parse(itemReqValues[r]))
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredClass") , I2.Loc.LocalizationManager.GetTranslation(itemReqNames[r] ), true);
#else
                            UGUITooltip.Instance.AddAttribute("Required Class ", itemReqNames[r], true);
#endif
                        else
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredClass") , I2.Loc.LocalizationManager.GetTranslation(itemReqNames[r]) , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAttribute("Required Class", itemReqNames[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif

                    }
                    if (itemReqTypes[r].Equals("Race"))
                    {
                        if (((int)ClientAPI.GetPlayerObject().GetProperty("race")) == int.Parse(itemReqValues[r]))
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredRace") , I2.Loc.LocalizationManager.GetTranslation(itemReqNames[r] ), true);
#else
                            UGUITooltip.Instance.AddAttribute("Required Race ", itemReqNames[r], true);
#endif
                        else
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredRace") , I2.Loc.LocalizationManager.GetTranslation(itemReqNames[r]) , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAttribute("Required Race ", itemReqNames[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif


                    }

                    if (itemReqTypes[r].Equals("Skill Level"))
                    {


                        if (Skills.Instance.GetPlayerSkillLevel(itemReqNames[r]) < int.Parse(itemReqValues[r]))
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+ I2.Loc.LocalizationManager.GetTranslation(itemReqNames[r]),  itemReqValues[r] , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAttribute("Required " + itemReqNames[r], itemReqValues[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                        else
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+ I2.Loc.LocalizationManager.GetTranslation(itemReqNames[r]),  itemReqValues[r] , true);
#else
                            UGUITooltip.Instance.AddAttribute("Required " + itemReqNames[r], itemReqValues[r], true);
#endif
                    }

                    if (itemReqTypes[r].Equals("Stat"))
                    {
                        int val = 0;
                        if (ClientAPI.GetPlayerObject().PropertyExists(itemReqNames[r]))
                        {
                            val = (int)ClientAPI.GetPlayerObject().GetProperty(itemReqNames[r]);
                        }

                        string statName = itemReqNames[r];
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));
#else
                        string[] statNames = statName.Split('_');
                        if (statNames.Length > 1)
                        {
                            statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                        }
                        else
                        {
                            statName = FirstCharToUpper(statNames[0]);
                        }
#endif
                        if (val < int.Parse(itemReqValues[r]))
#if AT_I2LOC_PRESET
                    UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+ statName  ,  itemReqValues[r], true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAttribute("Required " + statName, itemReqValues[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                        else
#if AT_I2LOC_PRESET
                    UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+statName ,  itemReqValues[r] , true);
#else
                            UGUITooltip.Instance.AddAttribute("Required " + statName, itemReqValues[r], true);
#endif
                    }
                }

            if (Unique)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Unique"),"",true);
#else
                UGUITooltip.Instance.AddAttribute("Unique", "", true);
#endif
            }

            if (isBound)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Soulbound"), "", true,UGUITooltip.Instance.itemStatLowerColour);
#else
                UGUITooltip.Instance.AddAttribute("Soulbound", "", true, UGUITooltip.Instance.itemStatLowerColour);
#endif
            }
            else if (binding > 0)
            {
                if (binding == 1)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("SoulboundOnEquip"), "", true);
#else
                    UGUITooltip.Instance.AddAttribute("Soulbound On Equip", "", true);
#endif
                }
                else if (binding == 2)
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("SoulboundOnPickup"), "", true);
#else
                    UGUITooltip.Instance.AddAttribute("Soulbound On Pickup", "", true);
#endif
            }

            if (sellable)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Sellable"), "", true);
#else
                UGUITooltip.Instance.AddAttribute("Sellable", "", true);
#endif
            }
            else
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Not Sellable"), "", true,UGUITooltip.Instance.itemStatLowerColour);
#else
                UGUITooltip.Instance.AddAttribute("Not Sellable", "", true, UGUITooltip.Instance.itemStatLowerColour);
#endif

            }
            if ((itemType == "Weapon" || itemType == "Armor"))
                if (enchantId > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Enchantable"), "", true);
#else
                    UGUITooltip.Instance.AddAttribute("Enchantable", "", true);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Not Enchantable"), "", true,UGUITooltip.Instance.itemStatLowerColour);
#else
                    UGUITooltip.Instance.AddAttribute("Not Enchantable", "", true, UGUITooltip.Instance.itemStatLowerColour);
#endif

                }
            if (maxDurability > 0)
            {
#if AT_I2LOC_PRESET
             UGUITooltip.Instance.AddAttribute( I2.Loc.LocalizationManager.GetTranslation("Durability")+" ", durability + "/" + maxDurability, true, UGUITooltip.Instance.itemTypeColour);
#else
                UGUITooltip.Instance.AddAttribute(" Durability", durability + "/" + maxDurability, true, UGUITooltip.Instance.itemTypeColour);
#endif
            }



            if (GetEffectPositionsOfTypes("ClaimObject").Count > 0)
            {
                UGUITooltip.Instance.AddAttributeSeperator();
#if AT_I2LOC_PRESET
            string tooltipDescription = I2.Loc.LocalizationManager.GetTranslation("Items/" + tooltip);
#else
                string tooltipDescription = tooltip;
#endif
                int templateId = int.Parse(itemEffectValues[GetEffectPositionsOfTypes("ClaimObject")[0]]);
                AtavismBuildObjectTemplate abot = WorldBuilder.Instance.GetBuildObjectTemplate(templateId);
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Claim Object")+" : " + I2.Loc.LocalizationManager.GetTranslation(abot.buildObjectName));
            UGUITooltip.Instance.AddAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Claim type") + " : " + I2.Loc.LocalizationManager.GetTranslation(abot.validClaimTypes.ToString()));
            UGUITooltip.Instance.AddAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Resources")+" : ");
            for (int i = 0; i < abot.itemsReq.Count; i++)
            {
                AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(abot.itemsReq[i]);
                UGUITooltip.Instance.AddAttributeResource(I2.Loc.LocalizationManager.GetTranslation(item.name), abot.itemsReqCount[i].ToString(),item.icon,false);
            }
            UGUITooltip.Instance.AddAttributeSeperator();
            if (abot.skill > 0)
            {
                Skill skill = Skills.Instance.GetSkillByID(abot.skill);
                if (skill != null)
                {
                    if (Skills.Instance.GetPlayerSkillLevel(abot.skill) >= abot.skillLevelReq)
                    {
                        UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+ I2.Loc.LocalizationManager.GetTranslation("Skill") + " " + I2.Loc.LocalizationManager.GetTranslation(skill.skillname), abot.skillLevelReq.ToString(),true);
                    }
                    else
                    {
                        UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("Skill")+" " + I2.Loc.LocalizationManager.GetTranslation(skill.skillname) ,  abot.skillLevelReq.ToString() ,true,UGUITooltip.Instance.itemStatLowerColour);
                    }
                }
                else
                {
                    Debug.LogError("Building Object Skill " + abot.skill + " can't be found");
                }
            }
            if (!abot.reqWeapon.Equals("~ none ~"))
            {
                if (((string)ClientAPI.GetPlayerObject().GetProperty("weaponType")).Equals(abot.reqWeapon))
                    UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("equiped weapon type"), I2.Loc.LocalizationManager.GetTranslation(abot.reqWeapon),true);
                else
                    UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("equiped weapon type") ,  I2.Loc.LocalizationManager.GetTranslation(abot.reqWeapon),true,UGUITooltip.Instance.itemStatLowerColour);
            }
#else
                UGUITooltip.Instance.AddAttributeTitle("Claim Object : " + abot.buildObjectName);
                UGUITooltip.Instance.AddAttributeTitle("Claim type : " + abot.validClaimTypes);
                UGUITooltip.Instance.AddAttributeTitle("Resources : ");
                for (int i = 0; i < abot.itemsReq.Count; i++)
                {
                    AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(abot.itemsReq[i]);
                    UGUITooltip.Instance.AddAttributeResource(item.name, abot.itemsReqCount[i].ToString(), item.icon, false);
                }
                UGUITooltip.Instance.AddAttributeSeperator();
                if (abot.skill > 0)
                {
                    Skill skill = Skills.Instance.GetSkillByID(abot.skill);
                    if (skill != null)
                    {
                        if (Skills.Instance.GetPlayerSkillLevel(abot.skill) >= abot.skillLevelReq)
                        {
                            UGUITooltip.Instance.AddAttribute("Required Skill " + skill.skillname, abot.skillLevelReq.ToString(), true);
                        }
                        else
                        {
                            UGUITooltip.Instance.AddAttribute("Required Skill " + skill.skillname, abot.skillLevelReq.ToString(), true, UGUITooltip.Instance.itemStatLowerColour);
                        }
                    }
                    else
                    {
                        Debug.LogError("Building Object Skill " + abot.skill + " can't be found");
                    }
                }
                if (!abot.reqWeapon.Equals("~ none ~"))
                {
                    if (((string)ClientAPI.GetPlayerObject().GetProperty("weaponType")).Equals(abot.reqWeapon))
                        UGUITooltip.Instance.AddAttribute("Required equiped weapon type: ", abot.reqWeapon, true);
                    else
                        UGUITooltip.Instance.AddAttribute("Required equiped weapon type: ", abot.reqWeapon, true, UGUITooltip.Instance.itemStatLowerColour);
                }
#endif
                UGUITooltip.Instance.SetDescription(tooltipDescription);
            }
            else if (GetEffectPositionsOfTypes("CraftsItem").Count > 0)
            {
                UGUITooltip.Instance.AddAttributeSeperator();

#if AT_I2LOC_PRESET
            string tooltipDescription = I2.Loc.LocalizationManager.GetTranslation("Items/" + tooltip);
#else
                string tooltipDescription = tooltip;
#endif
                int craftingRecipeID = int.Parse(itemEffectValues[GetEffectPositionsOfTypes("CraftsItem")[0]]);
                AtavismCraftingRecipe recipe = Inventory.Instance.GetCraftingRecipe(craftingRecipeID);
                // Crafts <item>
                AtavismInventoryItem itemCrafted = Inventory.Instance.GetItemByTemplateID(recipe.createsItems[0]);
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Crafts"), itemCrafted.name, true);
            UGUITooltip.Instance.AddAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Resources")+" : ");
            for (int r = 0; r < recipe.itemsReq.Count; r++)
            { 
                AtavismInventoryItem it = Inventory.Instance.GetItemByTemplateID(recipe.itemsReq[r]);
                UGUITooltip.Instance.AddAttributeResource(I2.Loc.LocalizationManager.GetTranslation("Items/" + it.name), recipe.itemsReqCounts[r].ToString(),it.icon, false);
            }
            UGUITooltip.Instance.AddAttributeSeperator();
            UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("Station"), recipe.stationReq, true);
            if (recipe.skillID > 0)
            {
                Skill skill = Skills.Instance.GetSkillByID(recipe.skillID);
                if (skill != null)
                {
                    if (Skills.Instance.GetPlayerSkillLevel(recipe.skillID) >= recipe.skillLevelReq)
                    {
                        UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Required")+" "+ I2.Loc.LocalizationManager.GetTranslation("Skill")+" "+ I2.Loc.LocalizationManager.GetTranslation(Skills.Instance.GetSkillByID(recipe.skillID).skillname), recipe.skillLevelReq.ToString(), true);
                    }
                    else
                    {
                        UGUITooltip.Instance.AddAttribute( I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("Skill")+""+ I2.Loc.LocalizationManager.GetTranslation(Skills.Instance.GetSkillByID(recipe.skillID).skillname) ,  recipe.skillLevelReq.ToString(), true,UGUITooltip.Instance.itemStatLowerColour);
                    }
                }
                else
                {
                    Debug.LogError("Craft Skill " + recipe.skillID + " can't be found");
                }
            }
          
#else
                UGUITooltip.Instance.AddAttribute("Crafts", itemCrafted.name, true);
                UGUITooltip.Instance.AddAttributeTitle("Resources : ");
                for (int r = 0; r < recipe.itemsReq.Count; r++)
                {
                    AtavismInventoryItem it = Inventory.Instance.GetItemByTemplateID(recipe.itemsReq[r]);
                    UGUITooltip.Instance.AddAttributeResource(it.name, recipe.itemsReqCounts[r].ToString(), it.icon, false);
                }
                UGUITooltip.Instance.AddAttributeSeperator();
                UGUITooltip.Instance.AddAttribute("Required Station", recipe.stationReq, true);
                if (recipe.skillID > 0)
                {
                    Skill skill = Skills.Instance.GetSkillByID(recipe.skillID);
                    if (skill != null)
                    {
                        if (Skills.Instance.GetPlayerSkillLevel(recipe.skillID) >= recipe.skillLevelReq)
                        {
                            UGUITooltip.Instance.AddAttribute("Required Skill " + Skills.Instance.GetSkillByID(recipe.skillID).skillname, recipe.skillLevelReq.ToString(), true);
                        }
                        else
                        {
                            UGUITooltip.Instance.AddAttribute("Required Skill " + Skills.Instance.GetSkillByID(recipe.skillID).skillname, recipe.skillLevelReq.ToString(), true, UGUITooltip.Instance.itemStatLowerColour);
                        }
                    }
                    else
                    {
                        Debug.LogError("Craft Skill " + recipe.skillID + " can't be found");
                    }
                }
#endif

                UGUITooltip.Instance.AddAttributeSeperator();
                UGUITooltip.Instance.SetDescription(tooltipDescription);
                showAdditionalTooltip(recipe.createsItems[0]);
            }
            else
            {
                UGUITooltip.Instance.AddAttributeSeperator();

#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetDescription(I2.Loc.LocalizationManager.GetTranslation("Items/" + tooltip));
#else
                UGUITooltip.Instance.SetDescription(tooltip);
#endif
            }
            //check ability ie learned
            if (GetEffectPositionsOfTypes("UseAbility").Count > 0)
            {
                if (name.IndexOf("TeachAbility") > -1)
                {
                    int abilityID = int.Parse(itemEffectNames[GetEffectPositionsOfTypes("UseAbility")[0]]);
                    AtavismAbility aa = Abilities.Instance.GetAbility(abilityID);
                    AtavismAbility paa = Abilities.Instance.GetPlayerAbility(abilityID);
                    if (paa != null)
                    {

#if AT_I2LOC_PRESET
                        UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("taught") , "", true,UGUITooltip.Instance.itemStatLowerColour);
#else
                        UGUITooltip.Instance.AddAttribute("Taught", "", true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                    }
                    aa.ShowAdditionalTooltip();
                }
            }



            UGUITooltip.Instance.Show(target);
        }






        /// <summary>
        /// Show Aditional tooltip
        /// </summary>
        /// <param name="Id"></param>
        void showAdditionalTooltip(int Id)
        {
            AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(Id);
            showAdditionalTooltip(item);
        }
        public void showAdditionalTooltip(AtavismInventoryItem item)
        {
            //  Debug.LogError("showAdditionalTooltip");
            //   string defaultColor = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));


#if AT_I2LOC_PRESET
        UGUITooltip.Instance.SetAdditionalTitle((item.enchantLeval > 0?" +"+ item.enchantLeval : "")+" "+I2.Loc.LocalizationManager.GetTranslation("Items/"+item.name));
#else
            UGUITooltip.Instance.SetAdditionalTitle((item.enchantLeval > 0 ? " +" + item.enchantLeval : "") + " " + item.name);
#endif
            if (item.icon != null)
            {
                UGUITooltip.Instance.SetAdditionalIcon(item.icon);
            }
            UGUITooltip.Instance.SetAdditionalQuality(item.quality);
            UGUITooltip.Instance.SetAdditionalTitleColour(AtavismSettings.Instance.ItemQualityColor(item.quality));
            string slotName = Inventory.Instance.GetItemByTemplateID(item.TemplateId).slot;
            /*
                    switch (item.slot)
                    {
                        case "PrimaryWeapon":
                            slotName = "Main Hand";
                            break;
                        case "SecondaryWeapon":
                            slotName = "Off Hand";
                            break;
                        case "PrimaryRing":
                            slotName = "Ring";
                            break;
                        case "SecondaryRing":
                            slotName = "Ring";
                            break;
                        case "PrimaryEarring":
                            slotName = "Earring";
                            break;
                        case "SecondaryEarring":
                            slotName = "Earring";
                            break;
                    }*/

            if (item.itemType == "Armor")
            {
#if AT_I2LOC_PRESET
      		UGUITooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Slot") + ": "+I2.Loc.LocalizationManager.GetTranslation(slotName));
#else
                UGUITooltip.Instance.SetAdditionalType(slotName);
#endif
            }
            else if (item.itemType == "Weapon")
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType(" "+I2.Loc.LocalizationManager.GetTranslation(item.subType));
            UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation(slotName),"",true);
#else
                UGUITooltip.Instance.SetAdditionalType(" " + item.subType);
                UGUITooltip.Instance.AddAdditionalAttribute(slotName, "", true);
#endif
            }
            else
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation(item.itemType));
#else
                UGUITooltip.Instance.SetAdditionalType(item.itemType);
#endif
            }
            UGUITooltip.Instance.SetAdditionalTypeColour(UGUITooltip.Instance.itemTypeColour);
            if (item.Weight > 0)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalWeight(I2.Loc.LocalizationManager.GetTranslation("Weight")+": " + item.Weight + "(" + (item.Weight * item.count) + ")");
#else
                UGUITooltip.Instance.SetAdditionalWeight("Weight: " + item.Weight + " (" + (item.Weight * item.count) + ")");
#endif
            }
            else
            {
                UGUITooltip.Instance.SetAdditionalWeight("");
            }
            if (item.itemType == "Weapon" || item.itemEffectTypes.Contains("Stat"))
            {
                UGUITooltip.Instance.AddAdditionalAttributeSeperator();
#if AT_I2LOC_PRESET
        UGUITooltip.Instance.AddAdditionalAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Stats"), UGUITooltip.Instance.itemSectionTitleColour);
#else
                UGUITooltip.Instance.AddAdditionalAttributeTitle("Stats", UGUITooltip.Instance.itemSectionTitleColour);
#endif
            }
            // Color colour = UGUITooltip.Instance.defaultTextColour;
            //string defaultColourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(colour.r), ToByte(colour.g), ToByte(colour.b));
            if (item.itemType == "Weapon")
            {
                // string colourText = "";
                //colour = UGUITooltip.Instance.defaultTextColour;
                string textDamage = item.damageValue.ToString();
                string mark = "";
                if (item.damageMaxValue - item.damageValue > 0)
                    textDamage += " - " + item.damageMaxValue.ToString();


                if (item.enchantStats.ContainsKey("dmg-base"))
                    if ((item.damageMaxValue + item.damageValue) / 2 - (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2 != 0)
                        textDamage += " (" + ((item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2 - (item.damageMaxValue + item.damageValue) / 2) + ")";
                if (item.itemType == "Weapon" || item.itemType == "Armor")
                    //foreach (AtavismInventoryItem item in items)
                    // {
                    //        if ((damageMaxValue + damageValue) / 2 > (item.damageMaxValue + item.damageValue) / 2) mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                    //        if ((damageMaxValue + damageValue) / 2 < (item.damageMaxValue + item.damageValue) / 2) mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                    //         if ((damageMaxValue + damageValue) / 2 == (item.damageMaxValue + item.damageValue) / 2) mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                    if (enchantStats.ContainsKey("dmg-base"))
                    {
                        if (item.enchantStats.ContainsKey("dmg-base"))
                        {
                            if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 > (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 < (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 == (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        }
                        else
                        {
                            if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 > (item.damageMaxValue + item.damageValue) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 < (item.damageMaxValue + item.damageValue) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 == (item.damageMaxValue + item.damageValue) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        }
                    }
                    else
                    {
                        if (item.enchantStats.ContainsKey("dmg-base"))
                        {
                            if ((damageMaxValue + damageValue) / 2 > (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            if ((damageMaxValue + damageValue) / 2 < (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            if ((damageMaxValue + damageValue) / 2 == (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        }
                        else
                        {
                            if ((damageMaxValue + damageValue) / 2 > (item.damageMaxValue + item.damageValue) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            if ((damageMaxValue + damageValue) / 2 < (item.damageMaxValue + item.damageValue) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            if ((damageMaxValue + damageValue) / 2 == (item.damageMaxValue + item.damageValue) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        }
                    }
                //}

                //colour = UGUITooltip.Instance.defaultTextColour;
                string textSpeed = ((float)item.weaponSpeed / 1000).ToString();
                string mark2 = "";

                if (item.itemType == "Weapon" || item.itemType == "Armor")
                    //foreach (AtavismInventoryItem item in items)
                    //{
                    if (item.weaponSpeed < weaponSpeed)
                        mark2 += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                if (item.weaponSpeed > weaponSpeed)
                    mark2 += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                if (item.weaponSpeed == weaponSpeed)
                    mark2 += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                //}
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute(FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("Damage")) + " "+mark,  textDamage, true);
            UGUITooltip.Instance.AddAdditionalAttribute(FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("attack_speed")) + " "+mark2,  textSpeed, true);
#else
                UGUITooltip.Instance.AddAdditionalAttribute("Damage " + mark, textDamage, false, UGUITooltip.Instance.itemTypeColour);
                UGUITooltip.Instance.AddAdditionalAttribute("Speed " + mark2, textSpeed, false, UGUITooltip.Instance.itemTypeColour);
#endif
            }

            List<string> additonalStats = new List<string>();

            foreach (string st in item.enchantStats.Keys)
            {
                if (!item.itemEffectNames.Contains(st) && !additonalStats.Contains(st))
                {
                    // Debug.LogError("Stat " + st);
                    additonalStats.Add(st);
                }
            }
            foreach (int statPos in item.GetEffectPositionsOfTypes("Stat"))
            {

                string statName = item.itemEffectNames[statPos];
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));
#else
                string[] statNames = statName.Split('_');
                if (statNames.Length > 1)
                {
                    statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                }
                else
                {
                    statName = FirstCharToUpper(statNames[0]);
                }
#endif
                // colour = UGUITooltip.Instance.defaultTextColour;
                string textParam = "";
                if (int.Parse(item.itemEffectValues[statPos]) != 0)
                    textParam = item.itemEffectValues[statPos] + " ";
                if (item.enchantStats.ContainsKey(item.itemEffectNames[statPos]))
                    if (int.Parse(item.itemEffectValues[statPos]) - item.enchantStats[item.itemEffectNames[statPos]] != 0)
                        textParam += "(" + (item.enchantStats[item.itemEffectNames[statPos]]) + ")";
                string mark = "";

                bool printStat = false;
                if (textParam.Length > 0)
                    printStat = true;
                if ((item.itemType == "Weapon" || item.itemType == "Armor") && (itemType == item.itemType))
                {
                    //   foreach (AtavismInventoryItem item in items)
                    //  {
                    //Testy wyszukania
                    if (itemEffectNames.Contains(item.itemEffectNames[statPos]))
                    {

                        int itemStatIndex = itemEffectNames.IndexOf(item.itemEffectNames[statPos]);
                        //   if (int.Parse(item.itemEffectValues[statPos]) < int.Parse(itemEffectValues[itemStatIndex])) mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                        //   if (int.Parse(item.itemEffectValues[statPos]) > int.Parse(itemEffectValues[itemStatIndex])) mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                        //   if (int.Parse(item.itemEffectValues[statPos]) == int.Parse(itemEffectValues[itemStatIndex])) mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        if (enchantStats.ContainsKey(item.itemEffectNames[statPos]))
                        {
                            if (item.enchantStats.ContainsKey(item.itemEffectNames[statPos]))
                            {
                                if (int.Parse(item.itemEffectValues[statPos]) + item.enchantStats[item.itemEffectNames[statPos]] < int.Parse(itemEffectValues[itemStatIndex]) + enchantStats[item.itemEffectNames[statPos]])
                                    mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                if (int.Parse(item.itemEffectValues[statPos]) + item.enchantStats[item.itemEffectNames[statPos]] > int.Parse(itemEffectValues[itemStatIndex]) + enchantStats[item.itemEffectNames[statPos]])
                                    mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                if (int.Parse(item.itemEffectValues[statPos]) + item.enchantStats[item.itemEffectNames[statPos]] == int.Parse(itemEffectValues[itemStatIndex]) + enchantStats[item.itemEffectNames[statPos]])
                                    mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                            }
                            else
                            {
                                if (int.Parse(item.itemEffectValues[statPos]) < int.Parse(itemEffectValues[itemStatIndex]) + enchantStats[item.itemEffectNames[statPos]])
                                    mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                if (int.Parse(item.itemEffectValues[statPos]) > int.Parse(itemEffectValues[itemStatIndex]) + enchantStats[item.itemEffectNames[statPos]])
                                    mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                if (int.Parse(item.itemEffectValues[statPos]) == int.Parse(itemEffectValues[itemStatIndex]) + enchantStats[item.itemEffectNames[statPos]])
                                    mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                            }
                        }
                        else
                        {
                            if (item.enchantStats.ContainsKey(item.itemEffectNames[statPos]))
                            {
                                if (int.Parse(item.itemEffectValues[statPos]) + item.enchantStats[item.itemEffectNames[statPos]] < int.Parse(itemEffectValues[itemStatIndex]))
                                    mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                if (int.Parse(item.itemEffectValues[statPos]) + item.enchantStats[item.itemEffectNames[statPos]] > int.Parse(itemEffectValues[itemStatIndex]))
                                    mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                if (int.Parse(item.itemEffectValues[statPos]) + item.enchantStats[item.itemEffectNames[statPos]] == int.Parse(itemEffectValues[itemStatIndex]))
                                    mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                            }
                            else
                            {
                                if (int.Parse(item.itemEffectValues[statPos]) < int.Parse(itemEffectValues[itemStatIndex]))
                                    mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                if (int.Parse(item.itemEffectValues[statPos]) > int.Parse(itemEffectValues[itemStatIndex]))
                                    mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                if (int.Parse(item.itemEffectValues[statPos]) == int.Parse(itemEffectValues[itemStatIndex]))
                                    mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                            }
                        }
                        //    colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(colour.r), ToByte(colour.g), ToByte(colour.b));
                        //   textParam += " (<color=" + colourText + ">" + (int.Parse(itemEffectValues[statPos]) - int.Parse(item.itemEffectValues[itemStatIndex])) + "</color>" + ")";
                        //     if (int.Parse(item.itemEffectValues[itemStatIndex]) != 0)
                        //     printStat = true;
                    }
                    else
                    {
                        mark += "<sprite=" + UGUITooltip.Instance.newSpriteId + ">";
                        ;
                    }

                }
                if (!string.IsNullOrEmpty(item.itemEffectValues[statPos]))
                    if (int.Parse(item.itemEffectValues[statPos]) > 0)
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute(statName + "  " + mark, textParam, false);
                    }
                    else if (int.Parse(item.itemEffectValues[statPos]) < 0)
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute(statName + "  " + mark, textParam, false);
                    }
                    else if (int.Parse(item.itemEffectValues[statPos]) == 0 && printStat)
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute(statName + "  " + mark, textParam, false);
                    }

            }
            //<sprite=1>
            //add stats from equiped items thats not in check item
            foreach (string addStatName in additonalStats)
            {
                bool printStat = false;
                string textParam = "";
                string mark = "";
                if (!addStatName.Equals("dmg-base") && !addStatName.Equals("dmg-max"))
                {
                    printStat = true;
                    textParam = "(" + item.enchantStats[addStatName] + ")";
                    //   foreach (AtavismInventoryItem item in items)
                    //  {
                    if (item.itemEffectNames.Contains(addStatName))
                    {
                        int itemStatIndex = item.itemEffectNames.IndexOf(addStatName);

                        //      if (int.Parse(item.itemEffectValues[itemStatIndex]) < enchantStats[addStatName]) mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                        //     if (int.Parse(item.itemEffectValues[itemStatIndex]) > enchantStats[addStatName]) mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                        //    if (int.Parse(item.itemEffectValues[itemStatIndex]) == enchantStats[addStatName]) mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        if (item.enchantStats.ContainsKey(addStatName))
                        {
                            if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[addStatName] < enchantStats[addStatName])
                                mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[addStatName] > enchantStats[addStatName])
                                mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[addStatName] == enchantStats[addStatName])
                                mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";

                        }
                        else
                        {
                            if (int.Parse(item.itemEffectValues[itemStatIndex]) < enchantStats[addStatName])
                                mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            if (int.Parse(item.itemEffectValues[itemStatIndex]) > enchantStats[addStatName])
                                mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            if (int.Parse(item.itemEffectValues[itemStatIndex]) == enchantStats[addStatName])
                                mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        }
                    }
                    else
                    {
                        mark += "<sprite=" + UGUITooltip.Instance.newSpriteId + ">";
                        ;
                    }
                    // }
                }
                if (printStat)
                {
#if AT_I2LOC_PRESET
                string _addStatName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(addStatName));
#else
                    string[] statNames = addStatName.Split('_');
                    string _addStatName = FirstCharToUpper(statNames[0]);
                    if (statNames.Length > 1)
                    {
                        _addStatName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                    }
#endif
                    UGUITooltip.Instance.AddAdditionalAttribute(_addStatName + mark, textParam, false);
                }
            }


            if (item.socketSlots.Count > 0)
            {
                UGUITooltip.Instance.AddAdditionalAttributeSeperator();
                foreach (string key in item.socketSlots.Keys)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Sockets of")+" " + I2.Loc.LocalizationManager.GetTranslation(key), UGUITooltip.Instance.itemSectionTitleColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttributeTitle("Sockets of " + key, UGUITooltip.Instance.itemSectionTitleColour);
#endif
                    foreach (int it in item.socketSlots[key].Keys)
                    {
                        //  Debug.LogError("Socket " + key + " slot: " + it + " item:" + socketSlots[key][it]);
                        if (item.socketSlots[key][it] > 0)
                        {
                            AtavismInventoryItem aii = Inventory.Instance.GetItemByTemplateID(item.socketSlots[key][it]);
                            if (aii == null)
                            {
#if AT_I2LOC_PRESET
                        UGUITooltip.Instance.AddAdditionalAttributeSocket(I2.Loc.LocalizationManager.GetTranslation("Empty"), null, false);
#else
                                UGUITooltip.Instance.AddAdditionalAttributeSocket("Empty", null, false);
#endif
                                continue;
                            }
                            string socketStat = "";
                            foreach (int statPos in aii.GetEffectPositionsOfTypes("Stat"))
                            {
                                //Debug.LogError("Socket " + key + " slot: " + it + " item:" + socketSlots[key][it]+" Stat pos:"+ statPos+ " count:"+itemEffectNames.Count);

                                string statName = aii.itemEffectNames[statPos];
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));
#else
                                string[] statNames = statName.Split('_');
                                if (statNames.Length > 1)
                                {
                                    statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                                }
                                else
                                {
                                    statName = FirstCharToUpper(statNames[0]);
                                }
#endif
                                socketStat += statName + " " + (int.Parse(aii.itemEffectValues[statPos]) > 0 ? "+" + aii.itemEffectValues[statPos] : int.Parse(aii.itemEffectValues[statPos]) < 0 ? "-" + aii.itemEffectValues[statPos] : "0") + "\n";
                            }
                            UGUITooltip.Instance.AddAdditionalAttributeSocket(socketStat, aii.icon, false);

                        }
                        else
                        {
#if AT_I2LOC_PRESET
                        UGUITooltip.Instance.AddAdditionalAttributeSocket(I2.Loc.LocalizationManager.GetTranslation("Empty"), null, false);
#else
                            UGUITooltip.Instance.AddAdditionalAttributeSocket("Empty", null, false);
#endif
                        }
                    }
                }
            }
            if (item.setId > 0)
            {
                AtavismInventoryItemSet aiis = Inventory.Instance.GetItemBySetID(item.setId);
                UGUITooltip.Instance.AddAdditionalAttributeSeperator();
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Set Effects")+" "+I2.Loc.LocalizationManager.GetTranslation(aiis.Name),UGUITooltip.Instance.itemSectionTitleColour);
#else
                UGUITooltip.Instance.AddAdditionalAttributeTitle("Set Effects " + aiis.Name, UGUITooltip.Instance.itemSectionTitleColour);
#endif

                foreach (AtavismInventoryItemSetLevel level in aiis.levelList)
                {
                    Color setColor = UGUITooltip.Instance.itemSetColour;
                    Color setTitleColor = UGUITooltip.Instance.itemSectionTitleColour;
                    if (item.setCount < level.NumerOfParts)
                    {
                        setColor = UGUITooltip.Instance.itemInactiveSetColour;
                        setTitleColor = UGUITooltip.Instance.itemInactiveSetColour;

                    }
#if AT_I2LOC_PRESET
                    UGUITooltip.Instance.AddAdditionalAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Set")+" "+level.NumerOfParts+" "+I2.Loc.LocalizationManager.GetTranslation("parts"), setTitleColor);
#else
                    UGUITooltip.Instance.AddAdditionalAttributeTitle("Set " + level.NumerOfParts + " parts", setTitleColor);
#endif
                    if (level.DamageValue != 0)
                    {
                        string statName = "Damage";
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("Damage"));
#else
                        string[] statNames = statName.Split('_');
                        if (statNames.Length > 1)
                        {
                            statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                        }
                        else
                        {
                            statName = FirstCharToUpper(statNames[0]);
                        }
#endif

                        if (level.DamageValue > 0)
                            UGUITooltip.Instance.AddAdditionalAttribute(statName, "+" + level.DamageValue.ToString(), false, setColor);
                        if (level.DamageValue < 0)
                            UGUITooltip.Instance.AddAdditionalAttribute(statName, "-" + level.DamageValue.ToString(), false, setColor);
                    }
                    if (level.DamageValuePercentage != 0)
                    {
                        string statName = "Damage";
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("Damage"));
#else
                        string[] statNames = statName.Split('_');
                        if (statNames.Length > 1)
                        {
                            statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                        }
                        else
                        {
                            statName = FirstCharToUpper(statNames[0]);
                        }
#endif

                        if (level.DamageValuePercentage > 0)
                            UGUITooltip.Instance.AddAdditionalAttribute(statName, "+" + level.DamageValuePercentage.ToString() + "%", false, setColor);
                        if (level.DamageValuePercentage < 0)
                            UGUITooltip.Instance.AddAdditionalAttribute(statName, "-" + level.DamageValuePercentage.ToString() + "%", false, setColor);
                    }




                    for (int k = 0; k < level.itemStatName.Count; k++)
                    {
                        string statName = level.itemStatName[k];
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));
#else
                        string[] statNames = statName.Split('_');
                        if (statNames.Length > 1)
                        {
                            statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                        }
                        else
                        {
                            statName = FirstCharToUpper(statNames[0]);
                        }
#endif
                        if (level.itemStatValues[k] > 0)
                            UGUITooltip.Instance.AddAdditionalAttribute(statName, "+" + level.itemStatValues[k].ToString(), false, setColor);
                        if (level.itemStatValues[k] < 0)
                            UGUITooltip.Instance.AddAdditionalAttribute(statName, "-" + level.itemStatValues[k].ToString(), false, setColor);
                        if (level.itemStatValuesPercentage[k] > 0)
                            UGUITooltip.Instance.AddAdditionalAttribute(statName, "+" + level.itemStatValuesPercentage[k].ToString() + "%", false, setColor);
                        if (level.itemStatValuesPercentage[k] < 0)
                            UGUITooltip.Instance.AddAdditionalAttribute(statName, "-" + level.itemStatValuesPercentage[k].ToString() + "%", false, setColor);
                    }

                }
            }


            UGUITooltip.Instance.AddAdditionalAttributeSeperator();
            if (item.itemReqTypes.Count > 0)


                for (int r = 0; r < item.itemReqTypes.Count; r++)
                {
                    if (item.itemReqTypes[r].Equals("Level"))
                    {
                        if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < int.Parse(item.itemReqValues[r]))
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") ,  item.itemReqValues[r] , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute("Required Level ", itemReqValues[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                        else
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ",  item.itemReqValues[r] , true);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute("Required Level ", item.itemReqValues[r], true);
#endif
                    }

                    if (item.itemReqTypes[r].Equals("Class"))
                    {
                        if (((int)ClientAPI.GetPlayerObject().GetProperty("aspect")) == int.Parse(item.itemReqValues[r]))
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredClass") + " ", I2.Loc.LocalizationManager.GetTranslation(item.itemReqNames[r]) , true);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute("Required Class ", item.itemReqNames[r], true);
#endif
                        else
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredClass") , I2.Loc.LocalizationManager.GetTranslation(item.itemReqNames[r]) , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute("Required Class", item.itemReqNames[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif

                    }
                    if (item.itemReqTypes[r].Equals("Race"))
                    {
                        if (((int)ClientAPI.GetPlayerObject().GetProperty("race")) == int.Parse(item.itemReqValues[r]))
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredRace") , I2.Loc.LocalizationManager.GetTranslation(item.itemReqNames[r]) , true);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute("Required Race ", item.itemReqNames[r], true);
#endif
                        else
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredRace") , I2.Loc.LocalizationManager.GetTranslation(item.itemReqNames[r]) , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute("Required Race ", item.itemReqNames[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                    }

                    if (item.itemReqTypes[r].Equals("Skill Level"))
                    {


                        if (Skills.Instance.GetPlayerSkillLevel(item.itemReqNames[r]) < int.Parse(item.itemReqValues[r]))
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+ I2.Loc.LocalizationManager.GetTranslation(item.itemReqNames[r]), item.itemReqValues[r] , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute("Required " + item.itemReqNames[r], item.itemReqValues[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                        else
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+ I2.Loc.LocalizationManager.GetTranslation(item.itemReqNames[r]),  item.itemReqValues[r], true);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute("Required " + item.itemReqNames[r], item.itemReqValues[r], true);
#endif
                    }

                    if (item.itemReqTypes[r].Equals("Stat"))
                    {
                        int val = 0;
                        if (ClientAPI.GetPlayerObject().PropertyExists(item.itemReqNames[r]))
                        {
                            //   Debug.LogError("Stat: r:" + r + " itemReqNames:" + itemReqNames[r] + " player prop:" + ClientAPI.GetPlayerObject().GetProperty(itemReqNames[r]));
                            val = (int)ClientAPI.GetPlayerObject().GetProperty(item.itemReqNames[r]);
                        }

                        string statName = item.itemReqNames[r];
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));
#else
                        string[] statNames = statName.Split('_');
                        if (statNames.Length > 1)
                        {
                            statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                        }
                        else
                        {
                            statName = FirstCharToUpper(statNames[0]);
                        }
#endif
                        if (val < int.Parse(item.itemReqValues[r]))
#if AT_I2LOC_PRESET
                    UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+ statName  ,  item.itemReqValues[r] , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute("Required " + statName, item.itemReqValues[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                        else
#if AT_I2LOC_PRESET
                    UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+statName , item.itemReqValues[r] , true);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute("Required " + statName, item.itemReqValues[r], true);
#endif
                    }
                }

            if (item.Unique)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Unique"),"",true);
#else
                UGUITooltip.Instance.AddAdditionalAttribute("Unique", "", true);
#endif
            }

            if (item.isBound)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Soulbound"), "", true,UGUITooltip.Instance.itemStatLowerColour);
#else
                UGUITooltip.Instance.AddAdditionalAttribute("Soulbound", "", true, UGUITooltip.Instance.itemStatLowerColour);
#endif
            }
            else if (item.binding > 0)
            {
                if (item.binding == 1)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("SoulboundOnEquip"), "", true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Soulbound On Equip", "", true);
#endif
                }
                else if (item.binding == 2)
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("SoulboundOnPickup"), "", true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Soulbound On Pickup", "", true);
#endif
            }
            if (item.sellable)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Sellable"), "", true);
#else
                UGUITooltip.Instance.AddAdditionalAttribute("Sellable", "", true);
#endif
            }
            else
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Not Sellable"), "", true,UGUITooltip.Instance.itemStatLowerColour);
#else
                UGUITooltip.Instance.AddAdditionalAttribute("Not Sellable", "", true, UGUITooltip.Instance.itemStatLowerColour);
#endif

            }
            if ((item.itemType == "Weapon" || item.itemType == "Armor"))

                if (item.enchantId > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Enchantable"), "", true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Enchantable", "", true);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Not Enchantable"), "", true,UGUITooltip.Instance.itemStatLowerColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Not Enchantable", "", true, UGUITooltip.Instance.itemStatLowerColour);
#endif

                }
            if (item.maxDurability > 0)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute( I2.Loc.LocalizationManager.GetTranslation("Durability")+" ", item.durability + "/" + item.maxDurability, true, UGUITooltip.Instance.itemTypeColour);
#else
                UGUITooltip.Instance.AddAdditionalAttribute(" Durability", item.durability + "/" + item.maxDurability, true, UGUITooltip.Instance.itemTypeColour);
#endif
            }
            if (item.GetEffectPositionsOfTypes("ClaimObject").Count > 0)
            {
                UGUITooltip.Instance.AddAdditionalAttributeSeperator();
#if AT_I2LOC_PRESET
            string tooltipDescription = I2.Loc.LocalizationManager.GetTranslation("Items/" + item.tooltip);
#else
                string tooltipDescription = item.tooltip;
#endif
                int templateId = int.Parse(item.itemEffectValues[item.GetEffectPositionsOfTypes("ClaimObject")[0]]);
                AtavismBuildObjectTemplate abot = WorldBuilder.Instance.GetBuildObjectTemplate(templateId);
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Claim Object")+" : " + I2.Loc.LocalizationManager.GetTranslation(abot.buildObjectName));
            UGUITooltip.Instance.AddAdditionalAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Claim type") + " : " + I2.Loc.LocalizationManager.GetTranslation(abot.validClaimTypes.ToString()));
            UGUITooltip.Instance.AddAdditionalAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Resources")+" : ");
            for (int i = 0; i < abot.itemsReq.Count; i++)
            {
                AtavismInventoryItem itemc = Inventory.Instance.GetItemByTemplateID(abot.itemsReq[i]);
                UGUITooltip.Instance.AddAdditionalAttributeResource(I2.Loc.LocalizationManager.GetTranslation(itemc.name), abot.itemsReqCount[i].ToString(),itemc.icon,false);
            }
            UGUITooltip.Instance.AddAdditionalAttributeSeperator();
            if (abot.skill > 0)
            {
                Skill skill = Skills.Instance.GetSkillByID(abot.skill);
                if (skill != null)
                {
                    if (Skills.Instance.GetPlayerSkillLevel(abot.skill) >= abot.skillLevelReq)
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+ I2.Loc.LocalizationManager.GetTranslation("Skill") + " " + I2.Loc.LocalizationManager.GetTranslation(skill.skillname), abot.skillLevelReq.ToString(),true);
                    }
                    else
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("Skill")+" " + I2.Loc.LocalizationManager.GetTranslation(skill.skillname) ,  abot.skillLevelReq.ToString() ,true,UGUITooltip.Instance.itemStatLowerColour);
                    }
                }
                else
                {
                    Debug.LogError("Building Object Skill " + abot.skill + " can't be found");
                }
            }
            if (!abot.reqWeapon.Equals("~ none ~"))
            {
                if (((string)ClientAPI.GetPlayerObject().GetProperty("weaponType")).Equals(abot.reqWeapon))
                    UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("equiped weapon type"), I2.Loc.LocalizationManager.GetTranslation(abot.reqWeapon),true);
                else
                    UGUITooltip.Instance.AddAdditionalAttribute( I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("equiped weapon type") , I2.Loc.LocalizationManager.GetTranslation(abot.reqWeapon) ,true,UGUITooltip.Instance.itemStatLowerColour);
            }
#else
                UGUITooltip.Instance.AddAdditionalAttributeTitle("Claim Object : " + abot.buildObjectName, UGUITooltip.Instance.itemSectionTitleColour);
                UGUITooltip.Instance.AddAdditionalAttributeTitle("Claim type : " + abot.validClaimTypes);
                UGUITooltip.Instance.AddAdditionalAttributeTitle("Resources : ");
                for (int i = 0; i < abot.itemsReq.Count; i++)
                {
                    AtavismInventoryItem itemc = Inventory.Instance.GetItemByTemplateID(abot.itemsReq[i]);
                    UGUITooltip.Instance.AddAdditionalAttributeResource(itemc.name, abot.itemsReqCount[i].ToString(), itemc.icon, false);
                }
                UGUITooltip.Instance.AddAdditionalAttributeSeperator();
                if (abot.skill > 0)
                {
                    Skill skill = Skills.Instance.GetSkillByID(abot.skill);
                    if (skill != null)
                    {
                        if (Skills.Instance.GetPlayerSkillLevel(abot.skill) >= abot.skillLevelReq)
                        {
                            UGUITooltip.Instance.AddAdditionalAttribute("Required Skill " + skill.skillname, abot.skillLevelReq.ToString(), true);
                        }
                        else
                        {
                            UGUITooltip.Instance.AddAdditionalAttribute("Required Skill " + skill.skillname, abot.skillLevelReq.ToString(), true, UGUITooltip.Instance.itemStatLowerColour);
                        }
                    }
                    else
                    {
                        Debug.LogError("Building Object Skill " + abot.skill + " can't be found");
                    }
                }
                if (!abot.reqWeapon.Equals("~ none ~"))
                {
                    if (((string)ClientAPI.GetPlayerObject().GetProperty("weaponType")).Equals(abot.reqWeapon))
                        UGUITooltip.Instance.AddAdditionalAttribute("Required equiped weapon type: ", abot.reqWeapon, true);
                    else
                        UGUITooltip.Instance.AddAdditionalAttribute("Required equiped weapon type:", abot.reqWeapon, true, UGUITooltip.Instance.itemStatLowerColour);
                }
#endif
                UGUITooltip.Instance.SetAdditionalDescription(tooltipDescription);
            }
            else if (item.GetEffectPositionsOfTypes("CraftsItem").Count > 0)
            {
                UGUITooltip.Instance.AddAdditionalAttributeSeperator();

#if AT_I2LOC_PRESET
            string tooltipDescription = I2.Loc.LocalizationManager.GetTranslation("Items/" + item.tooltip);
#else
                string tooltipDescription = item.tooltip;
#endif
                int craftingRecipeID = int.Parse(item.itemEffectValues[item.GetEffectPositionsOfTypes("CraftsItem")[0]]);
                AtavismCraftingRecipe recipe = Inventory.Instance.GetCraftingRecipe(craftingRecipeID);
                // Crafts <item>
                AtavismInventoryItem itemCrafted = Inventory.Instance.GetItemByTemplateID(recipe.createsItems[0]);
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Crafts"), itemCrafted.name, true);
            UGUITooltip.Instance.AddAdditionalAttributeTitle(I2.Loc.LocalizationManager.GetTranslation("Resources")+" : ");
            for (int r = 0; r < recipe.itemsReq.Count; r++)
            {
                 AtavismInventoryItem it = Inventory.Instance.GetItemByTemplateID(recipe.itemsReq[r]);
               UGUITooltip.Instance.AddAdditionalAttributeResource(I2.Loc.LocalizationManager.GetTranslation("Items/" + it.name), recipe.itemsReqCounts[r].ToString(),it.icon, false);
            }
            UGUITooltip.Instance.AddAdditionalAttributeSeperator();
            UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("Station"), recipe.stationReq, true);
            if (recipe.skillID > 0)
            {
                Skill skill = Skills.Instance.GetSkillByID(recipe.skillID);
                if (skill != null)
                {
                    if (Skills.Instance.GetPlayerSkillLevel(recipe.skillID) >= recipe.skillLevelReq)
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Required")+" "+ I2.Loc.LocalizationManager.GetTranslation("Skill")+" "+ I2.Loc.LocalizationManager.GetTranslation(Skills.Instance.GetSkillByID(recipe.skillID).skillname), recipe.skillLevelReq.ToString(), true);
                    }
                    else
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute( I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("Skill")+""+ I2.Loc.LocalizationManager.GetTranslation(Skills.Instance.GetSkillByID(recipe.skillID).skillname) ,  recipe.skillLevelReq.ToString() , true, UGUITooltip.Instance.itemStatLowerColour);
                    }
                }
                else
                {
                    Debug.LogError("Craft Skill " + recipe.skillID + " can't be found");
                }
            }
          
#else
                UGUITooltip.Instance.AddAdditionalAttribute("Crafts", itemCrafted.name, true);
                UGUITooltip.Instance.AddAdditionalAttributeTitle("Resources : ");
                for (int r = 0; r < recipe.itemsReq.Count; r++)
                {
                    AtavismInventoryItem it = Inventory.Instance.GetItemByTemplateID(recipe.itemsReq[r]);
                    UGUITooltip.Instance.AddAdditionalAttributeResource(it.name, recipe.itemsReqCounts[r].ToString(), it.icon, false);
                }
                UGUITooltip.Instance.AddAdditionalAttributeSeperator();
                UGUITooltip.Instance.AddAdditionalAttribute("Required Station", recipe.stationReq, true);
                if (recipe.skillID > 0)
                {
                    Skill skill = Skills.Instance.GetSkillByID(recipe.skillID);
                    if (skill != null)
                    {
                        if (Skills.Instance.GetPlayerSkillLevel(recipe.skillID) >= recipe.skillLevelReq)
                        {
                            UGUITooltip.Instance.AddAdditionalAttribute("Required Skill " + Skills.Instance.GetSkillByID(recipe.skillID).skillname, recipe.skillLevelReq.ToString(), true);
                        }
                        else
                        {
                            UGUITooltip.Instance.AddAdditionalAttribute("Required Skill " + Skills.Instance.GetSkillByID(recipe.skillID).skillname, recipe.skillLevelReq.ToString(), true, UGUITooltip.Instance.itemStatLowerColour);
                        }
                    }
                    else
                    {
                        Debug.LogError("Craft Skill " + recipe.skillID + " can't be found");
                    }
                }
#endif

                UGUITooltip.Instance.AddAdditionalAttributeSeperator();
                UGUITooltip.Instance.SetAdditionalDescription(tooltipDescription);
                showAdditionalTooltip2(recipe.createsItems[0]);
            }
            else
            {
                UGUITooltip.Instance.AddAdditionalAttributeSeperator();

#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalDescription(I2.Loc.LocalizationManager.GetTranslation("Items/" + item.tooltip));
#else
                UGUITooltip.Instance.SetAdditionalDescription(item.tooltip);
#endif
            }
            //check ability ie learned
            if (item.GetEffectPositionsOfTypes("UseAbility").Count > 0)
            {
                if (item.name.IndexOf("TeachAbility") > -1)
                {
                    int abilityID = int.Parse(item.itemEffectNames[item.GetEffectPositionsOfTypes("UseAbility")[0]]);
                    AtavismAbility aa = Abilities.Instance.GetAbility(abilityID);
                    AtavismAbility paa = Abilities.Instance.GetPlayerAbility(abilityID);
                    if (paa != null)
                    {

#if AT_I2LOC_PRESET
                        UGUITooltip.Instance.AddAdditionalAttribute( I2.Loc.LocalizationManager.GetTranslation("taught") , "", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute("Taught", "", true);
#endif
                    }
                    aa.ShowAdditionalTooltip2();
                }
            }
            UGUITooltip.Instance.ShowAdditionalTooltip();
        }






        /// <summary>
        /// Show Aditional tooltip
        /// </summary>
        /// <param name="Id"></param>
        void showAdditionalTooltip2(int Id)
        {

            AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(Id);
            showAdditionalTooltip2(item);
        }

        void showAdditionalTooltip2(AtavismInventoryItem item)
        {
            // Debug.LogError("showAdditionalTooltip2");

            // string defaultColor = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));


#if AT_I2LOC_PRESET
        UGUITooltip.Instance.SetAdditionalTitle2((item.enchantLeval > 0?" +"+ item.enchantLeval : "")+" "+I2.Loc.LocalizationManager.GetTranslation("Items/"+item.name));
#else
            UGUITooltip.Instance.SetAdditionalTitle2((item.enchantLeval > 0 ? " +" + item.enchantLeval : "") + " " + item.name);
#endif
            if (item.icon != null)
            {
                UGUITooltip.Instance.SetAdditionalIcon2(item.icon);
            }
            UGUITooltip.Instance.SetAdditionalQuality2(item.quality);
            UGUITooltip.Instance.SetAdditionalTitleColour2(AtavismSettings.Instance.ItemQualityColor(item.quality));
            string slotName = Inventory.Instance.GetItemByTemplateID(item.TemplateId).slot;
            /*string slotName = "";
            switch (item.slot)
            {
                case "PrimaryWeapon":
                    slotName = "Main Hand";
                    break;
                case "SecondaryWeapon":
                    slotName = "Off Hand";
                    break;
                case "PrimaryRing":
                    slotName = "Ring";
                    break;
                case "SecondaryRing":
                    slotName = "Ring";
                    break;
                case "PrimaryEarring":
                    slotName = "Earring";
                    break;
                case "SecondaryEarring":
                    slotName = "Earring";
                    break;
            }*/
            if (item.itemType == "Armor")
            {
#if AT_I2LOC_PRESET
      		UGUITooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Slot") + ": "+I2.Loc.LocalizationManager.GetTranslation(slotName));
#else
                UGUITooltip.Instance.SetAdditionalType2(slotName);
#endif
            }
            else if (item.itemType == "Weapon")
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType2(" "+I2.Loc.LocalizationManager.GetTranslation(item.subType));
            UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation(slotName),"",true);
#else
                UGUITooltip.Instance.SetAdditionalType2(" " + item.subType);
                UGUITooltip.Instance.AddAdditionalAttribute2(slotName, "", true);
#endif
            }
            else
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation(item.itemType));
#else
                UGUITooltip.Instance.SetAdditionalType2(item.itemType);
#endif
            }
            UGUITooltip.Instance.SetAdditionalTypeColour2(UGUITooltip.Instance.itemTypeColour);
            if (item.Weight > 0)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalWeight2(I2.Loc.LocalizationManager.GetTranslation("Weight")+": " + item.Weight + "(" + (item.Weight * item.count) + ")");
#else
                UGUITooltip.Instance.SetAdditionalWeight2("Weight: " + item.Weight + " (" + (item.Weight * item.count) + ")");
#endif
            }
            else
            {
                UGUITooltip.Instance.SetAdditionalWeight2("");
            }
            if (item.itemType == "Weapon" || item.itemEffectTypes.Contains("Stat"))
            {
                UGUITooltip.Instance.AddAdditionalAttributeSeperator2();
#if AT_I2LOC_PRESET
        UGUITooltip.Instance.AddAdditionalAttributeTitle2(I2.Loc.LocalizationManager.GetTranslation("Stats"), UGUITooltip.Instance.itemSectionTitleColour);
#else
                UGUITooltip.Instance.AddAdditionalAttributeTitle2("Stats", UGUITooltip.Instance.itemSectionTitleColour);
#endif
            }
            //  Color colour = UGUITooltip.Instance.defaultTextColour;
            //  string defaultColourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(colour.r), ToByte(colour.g), ToByte(colour.b));
            if (item.itemType == "Weapon")
            {
                // string colourText = "";
                //colour = UGUITooltip.Instance.defaultTextColour;
                string textDamage = item.damageValue.ToString();
                string mark = "";
                if (item.damageMaxValue - item.damageValue > 0)
                    textDamage += " - " + item.damageMaxValue.ToString();


                if (item.enchantStats.ContainsKey("dmg-base"))
                    if ((item.damageMaxValue + item.damageValue) / 2 - (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2 != 0)
                        textDamage += " (" + ((item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2 - (item.damageMaxValue + item.damageValue) / 2) + ")";
                if (item.itemType == "Weapon" || item.itemType == "Armor")
                    //foreach (AtavismInventoryItem item in items)
                    // {
                    //        if ((damageMaxValue + damageValue) / 2 > (item.damageMaxValue + item.damageValue) / 2) mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                    //        if ((damageMaxValue + damageValue) / 2 < (item.damageMaxValue + item.damageValue) / 2) mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                    //         if ((damageMaxValue + damageValue) / 2 == (item.damageMaxValue + item.damageValue) / 2) mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                    if (enchantStats.ContainsKey("dmg-base"))
                    {
                        if (item.enchantStats.ContainsKey("dmg-base"))
                        {
                            if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 > (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 < (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 == (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        }
                        else
                        {
                            if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 > (item.damageMaxValue + item.damageValue) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 < (item.damageMaxValue + item.damageValue) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            if ((enchantStats["dmg-base"] + enchantStats["dmg-max"]) / 2 == (item.damageMaxValue + item.damageValue) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        }
                    }
                    else
                    {
                        if (item.enchantStats.ContainsKey("dmg-base"))
                        {
                            if ((damageMaxValue + damageValue) / 2 > (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            if ((damageMaxValue + damageValue) / 2 < (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            if ((damageMaxValue + damageValue) / 2 == (item.enchantStats["dmg-base"] + item.enchantStats["dmg-max"]) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        }
                        else
                        {
                            if ((damageMaxValue + damageValue) / 2 > (item.damageMaxValue + item.damageValue) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            if ((damageMaxValue + damageValue) / 2 < (item.damageMaxValue + item.damageValue) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            if ((damageMaxValue + damageValue) / 2 == (item.damageMaxValue + item.damageValue) / 2)
                                mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        }
                    }
                //}

                //colour = UGUITooltip.Instance.defaultTextColour;
                string textSpeed = ((float)item.weaponSpeed / 1000).ToString();
                string mark2 = "";

                if (item.itemType == "Weapon" || item.itemType == "Armor")
                    //foreach (AtavismInventoryItem item in items)
                    //{
                    if (item.weaponSpeed < weaponSpeed)
                        mark2 += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                if (item.weaponSpeed > weaponSpeed)
                    mark2 += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                if (item.weaponSpeed == weaponSpeed)
                    mark2 += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                //}
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute2(FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("Damage")) + " "+mark,  textDamage, true);
            UGUITooltip.Instance.AddAdditionalAttribute2(FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("attack_speed")) + " "+mark2,  textSpeed, true);
#else
                UGUITooltip.Instance.AddAdditionalAttribute2("Damage " + mark, textDamage, false, UGUITooltip.Instance.itemTypeColour);
                UGUITooltip.Instance.AddAdditionalAttribute2("Speed " + mark2, textSpeed, false, UGUITooltip.Instance.itemTypeColour);
#endif
            }

            List<string> additonalStats = new List<string>();

            foreach (string st in item.enchantStats.Keys)
            {
                if (!item.itemEffectNames.Contains(st) && !additonalStats.Contains(st))
                {
                    // Debug.LogError("Stat " + st);
                    additonalStats.Add(st);
                }
            }
            foreach (int statPos in item.GetEffectPositionsOfTypes("Stat"))
            {

                string statName = item.itemEffectNames[statPos];
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));
#else
                string[] statNames = statName.Split('_');
                if (statNames.Length > 1)
                {
                    statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                }
                else
                {
                    statName = FirstCharToUpper(statNames[0]);
                }
#endif
                //    colour = UGUITooltip.Instance.defaultTextColour;
                string textParam = "";
                if (int.Parse(item.itemEffectValues[statPos]) != 0)
                    textParam = item.itemEffectValues[statPos] + " ";
                if (item.enchantStats.ContainsKey(item.itemEffectNames[statPos]))
                    if (int.Parse(item.itemEffectValues[statPos]) - item.enchantStats[item.itemEffectNames[statPos]] != 0)
                        textParam += "(" + (item.enchantStats[item.itemEffectNames[statPos]]) + ")";
                string mark = "";

                bool printStat = false;
                if (textParam.Length > 0)
                    printStat = true;
                if ((item.itemType == "Weapon" || item.itemType == "Armor") && (itemType == item.itemType))
                {
                    //   foreach (AtavismInventoryItem item in items)
                    //  {
                    //Testy wyszukania
                    if (itemEffectNames.Contains(item.itemEffectNames[statPos]))
                        if (itemEffectNames.Contains(item.itemEffectNames[statPos]))
                        {

                            int itemStatIndex = itemEffectNames.IndexOf(item.itemEffectNames[statPos]);
                            //   if (int.Parse(item.itemEffectValues[statPos]) < int.Parse(itemEffectValues[itemStatIndex])) mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            //   if (int.Parse(item.itemEffectValues[statPos]) > int.Parse(itemEffectValues[itemStatIndex])) mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            //   if (int.Parse(item.itemEffectValues[statPos]) == int.Parse(itemEffectValues[itemStatIndex])) mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                            if (enchantStats.ContainsKey(item.itemEffectNames[statPos]))
                            {
                                if (item.enchantStats.ContainsKey(item.itemEffectNames[statPos]))
                                {
                                    if (int.Parse(item.itemEffectValues[statPos]) + item.enchantStats[item.itemEffectNames[statPos]] < int.Parse(itemEffectValues[itemStatIndex]) + enchantStats[item.itemEffectNames[statPos]])
                                        mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[statPos]) + item.enchantStats[item.itemEffectNames[statPos]] > int.Parse(itemEffectValues[itemStatIndex]) + enchantStats[item.itemEffectNames[statPos]])
                                        mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[statPos]) + item.enchantStats[item.itemEffectNames[statPos]] == int.Parse(itemEffectValues[itemStatIndex]) + enchantStats[item.itemEffectNames[statPos]])
                                        mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                                }
                                else
                                {
                                    if (int.Parse(item.itemEffectValues[statPos]) < int.Parse(itemEffectValues[itemStatIndex]) + enchantStats[item.itemEffectNames[statPos]])
                                        mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[statPos]) > int.Parse(itemEffectValues[itemStatIndex]) + enchantStats[item.itemEffectNames[statPos]])
                                        mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[statPos]) == int.Parse(itemEffectValues[itemStatIndex]) + enchantStats[item.itemEffectNames[statPos]])
                                        mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                                }
                            }
                            else
                            {
                                if (item.enchantStats.ContainsKey(item.itemEffectNames[statPos]))
                                {
                                    if (int.Parse(item.itemEffectValues[statPos]) + item.enchantStats[item.itemEffectNames[statPos]] < int.Parse(itemEffectValues[itemStatIndex]))
                                        mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[statPos]) + item.enchantStats[item.itemEffectNames[statPos]] > int.Parse(itemEffectValues[itemStatIndex]))
                                        mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[statPos]) + item.enchantStats[item.itemEffectNames[statPos]] == int.Parse(itemEffectValues[itemStatIndex]))
                                        mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                                }
                                else
                                {
                                    if (int.Parse(item.itemEffectValues[statPos]) < int.Parse(itemEffectValues[itemStatIndex]))
                                        mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[statPos]) > int.Parse(itemEffectValues[itemStatIndex]))
                                        mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                                    if (int.Parse(item.itemEffectValues[statPos]) == int.Parse(itemEffectValues[itemStatIndex]))
                                        mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                                }
                            }
                            //    colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(colour.r), ToByte(colour.g), ToByte(colour.b));
                            //   textParam += " (<color=" + colourText + ">" + (int.Parse(itemEffectValues[statPos]) - int.Parse(item.itemEffectValues[itemStatIndex])) + "</color>" + ")";
                            //     if (int.Parse(item.itemEffectValues[itemStatIndex]) != 0)
                            //     printStat = true;
                        }
                        else
                        {
                            mark += "<sprite=" + UGUITooltip.Instance.newSpriteId + ">";
                            ;
                        }

                }
                if (!string.IsNullOrEmpty(item.itemEffectValues[statPos]))
                    if (int.Parse(item.itemEffectValues[statPos]) > 0)
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute2(statName + "  " + mark, textParam, false);
                    }
                    else if (int.Parse(item.itemEffectValues[statPos]) < 0)
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute2(statName + "  " + mark, textParam, false);
                    }
                    else if (int.Parse(item.itemEffectValues[statPos]) == 0 && printStat)
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute2(statName + "  " + mark, textParam, false);
                    }

            }
            //<sprite=1>
            //add stats from equiped items thats not in check item
            foreach (string addStatName in additonalStats)
            {
                bool printStat = false;
                string textParam = "";
                string mark = "";
                if (!addStatName.Equals("dmg-base") && !addStatName.Equals("dmg-max"))
                {
                    printStat = true;
                    textParam = "(" + item.enchantStats[addStatName] + ")";
                    //   foreach (AtavismInventoryItem item in items)
                    //  {
                    if (item.itemEffectNames.Contains(addStatName))
                    {
                        int itemStatIndex = item.itemEffectNames.IndexOf(addStatName);

                        //      if (int.Parse(item.itemEffectValues[itemStatIndex]) < enchantStats[addStatName]) mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                        //     if (int.Parse(item.itemEffectValues[itemStatIndex]) > enchantStats[addStatName]) mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                        //    if (int.Parse(item.itemEffectValues[itemStatIndex]) == enchantStats[addStatName]) mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        if (item.enchantStats.ContainsKey(addStatName))
                        {
                            if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[addStatName] < enchantStats[addStatName])
                                mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[addStatName] > enchantStats[addStatName])
                                mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            if (int.Parse(item.itemEffectValues[itemStatIndex]) + item.enchantStats[addStatName] == enchantStats[addStatName])
                                mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";

                        }
                        else
                        {
                            if (int.Parse(item.itemEffectValues[itemStatIndex]) < enchantStats[addStatName])
                                mark += "<sprite=" + UGUITooltip.Instance.lowerSpriteId + ">";
                            if (int.Parse(item.itemEffectValues[itemStatIndex]) > enchantStats[addStatName])
                                mark += "<sprite=" + UGUITooltip.Instance.greaterSpriteId + ">";
                            if (int.Parse(item.itemEffectValues[itemStatIndex]) == enchantStats[addStatName])
                                mark += "<sprite=" + UGUITooltip.Instance.equalSpriteId + ">";
                        }
                    }
                    else
                    {
                        mark += "<sprite=" + UGUITooltip.Instance.newSpriteId + ">";
                        ;
                    }
                    // }
                }
                if (printStat)
                {
#if AT_I2LOC_PRESET
                string _addStatName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(addStatName));
#else
                    string[] statNames = addStatName.Split('_');
                    string _addStatName = FirstCharToUpper(statNames[0]);
                    if (statNames.Length > 1)
                    {
                        _addStatName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                    }
#endif
                    UGUITooltip.Instance.AddAdditionalAttribute2(_addStatName + mark, textParam, false);
                }
            }


            if (item.socketSlots.Count > 0)
            {
                UGUITooltip.Instance.AddAdditionalAttributeSeperator2();
                foreach (string key in item.socketSlots.Keys)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttributeTitle2(I2.Loc.LocalizationManager.GetTranslation("Sockets of")+" " + I2.Loc.LocalizationManager.GetTranslation(key), UGUITooltip.Instance.itemSectionTitleColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttributeTitle2("Sockets of " + key, UGUITooltip.Instance.itemSectionTitleColour);
#endif
                    foreach (int it in item.socketSlots[key].Keys)
                    {
                        //  Debug.LogError("Socket " + key + " slot: " + it + " item:" + socketSlots[key][it]);
                        if (item.socketSlots[key][it] > 0)
                        {
                            AtavismInventoryItem aii = Inventory.Instance.GetItemByTemplateID(item.socketSlots[key][it]);
                            if (aii == null)
                            {
#if AT_I2LOC_PRESET
                        UGUITooltip.Instance.AddAdditionalAttributeSocket2(I2.Loc.LocalizationManager.GetTranslation("Empty"), null, false);
#else
                                UGUITooltip.Instance.AddAdditionalAttributeSocket2("Empty", null, false);
#endif
                                continue;
                            }
                            string socketStat = "";
                            foreach (int statPos in aii.GetEffectPositionsOfTypes("Stat"))
                            {
                                //Debug.LogError("Socket " + key + " slot: " + it + " item:" + socketSlots[key][it]+" Stat pos:"+ statPos+ " count:"+itemEffectNames.Count);

                                string statName = aii.itemEffectNames[statPos];
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));
#else
                                string[] statNames = statName.Split('_');
                                if (statNames.Length > 1)
                                {
                                    statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                                }
                                else
                                {
                                    statName = FirstCharToUpper(statNames[0]);
                                }
#endif
                                socketStat += statName + " " + (int.Parse(aii.itemEffectValues[statPos]) > 0 ? "+" + aii.itemEffectValues[statPos] : int.Parse(aii.itemEffectValues[statPos]) < 0 ? "-" + aii.itemEffectValues[statPos] : "0") + "\n";
                            }
                            UGUITooltip.Instance.AddAdditionalAttributeSocket2(socketStat, aii.icon, false);

                        }
                        else
                        {
#if AT_I2LOC_PRESET
                        UGUITooltip.Instance.AddAdditionalAttributeSocket2(I2.Loc.LocalizationManager.GetTranslation("Empty"), null, false);
#else
                            UGUITooltip.Instance.AddAdditionalAttributeSocket2("Empty", null, false);
#endif
                        }
                    }
                }
            }
            if (item.setId > 0)
            {
                AtavismInventoryItemSet aiis = Inventory.Instance.GetItemBySetID(item.setId);
                UGUITooltip.Instance.AddAdditionalAttributeSeperator2();
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttributeTitle2(I2.Loc.LocalizationManager.GetTranslation("Set Effects")+" "+I2.Loc.LocalizationManager.GetTranslation(aiis.Name),UGUITooltip.Instance.itemSectionTitleColour);
#else
                UGUITooltip.Instance.AddAdditionalAttributeTitle2("Set Effects " + aiis.Name, UGUITooltip.Instance.itemSectionTitleColour);
#endif

                foreach (AtavismInventoryItemSetLevel level in aiis.levelList)
                {
                    Color setColor = UGUITooltip.Instance.itemSetColour;
                    Color setTitleColor = UGUITooltip.Instance.itemSectionTitleColour;
                    if (item.setCount < level.NumerOfParts)
                    {
                        setColor = UGUITooltip.Instance.itemInactiveSetColour;
                        setTitleColor = UGUITooltip.Instance.itemInactiveSetColour;

                    }
#if AT_I2LOC_PRESET
                    UGUITooltip.Instance.AddAdditionalAttributeTitle2(I2.Loc.LocalizationManager.GetTranslation("Set")+" "+level.NumerOfParts+" "+I2.Loc.LocalizationManager.GetTranslation("parts"),setTitleColor);
#else
                    UGUITooltip.Instance.AddAdditionalAttributeTitle2("Set " + level.NumerOfParts + " parts", setTitleColor);
#endif
                    if (level.DamageValue != 0)
                    {
                        string statName = "Damage";
#if AT_I2LOC_PRESET
                    statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("Damage"));
#else
                        string[] statNames = statName.Split('_');
                        if (statNames.Length > 1)
                        {
                            statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                        }
                        else
                        {
                            statName = FirstCharToUpper(statNames[0]);
                        }
#endif

                        if (level.DamageValue > 0)
                            UGUITooltip.Instance.AddAdditionalAttribute2(statName, "+" + level.DamageValue.ToString(), false, setColor);
                        if (level.DamageValue < 0)
                            UGUITooltip.Instance.AddAdditionalAttribute2(statName, "-" + level.DamageValue.ToString(), false, setColor);
                    }

                    if (level.DamageValuePercentage != 0)
                    {
                        string statName = "Damage";
#if AT_I2LOC_PRESET
                    statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation("Damage"));
#else
                        string[] statNames = statName.Split('_');
                        if (statNames.Length > 1)
                        {
                            statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                        }
                        else
                        {
                            statName = FirstCharToUpper(statNames[0]);
                        }
#endif

                        if (level.DamageValuePercentage > 0)
                            UGUITooltip.Instance.AddAdditionalAttribute2(statName, "+" + level.DamageValuePercentage.ToString() + "%", false, setColor);
                        if (level.DamageValuePercentage < 0)
                            UGUITooltip.Instance.AddAdditionalAttribute2(statName, "-" + level.DamageValuePercentage.ToString() + "%", false, setColor);
                    }


                    for (int k = 0; k < level.itemStatName.Count; k++)
                    {
                        string statName = level.itemStatName[k];
#if AT_I2LOC_PRESET
                    statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));
#else
                        string[] statNames = statName.Split('_');
                        if (statNames.Length > 1)
                        {
                            statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                        }
                        else
                        {
                            statName = FirstCharToUpper(statNames[0]);
                        }
#endif
                        if (level.itemStatValues[k] > 0)
                            UGUITooltip.Instance.AddAdditionalAttribute2(statName, "+" + level.itemStatValues[k].ToString(), false, setColor);
                        if (level.itemStatValues[k] < 0)
                            UGUITooltip.Instance.AddAdditionalAttribute2(statName, "-" + level.itemStatValues[k].ToString(), false, setColor);
                        if (level.itemStatValuesPercentage[k] > 0)
                            UGUITooltip.Instance.AddAdditionalAttribute2(statName, "+" + level.itemStatValuesPercentage[k].ToString() + "%", false, setColor);
                        if (level.itemStatValuesPercentage[k] < 0)
                            UGUITooltip.Instance.AddAdditionalAttribute2(statName, "-" + level.itemStatValuesPercentage[k].ToString() + "%", false, setColor);
                    }

                }
            }


            UGUITooltip.Instance.AddAdditionalAttributeSeperator2();

            if (item.itemReqTypes.Count > 0)

                for (int r = 0; r < item.itemReqTypes.Count; r++)
                {
                    if (item.itemReqTypes[r].Equals("Level"))
                    {
                        if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < int.Parse(item.itemReqValues[r]))
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ",  item.itemReqValues[r] , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required Level ", itemReqValues[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                        else
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ",  item.itemReqValues[r] , true);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required Level ", item.itemReqValues[r], true);
#endif
                    }


                    if (item.itemReqTypes[r].Equals("Class"))
                    {
                        if (((int)ClientAPI.GetPlayerObject().GetProperty("aspect")) == int.Parse(item.itemReqValues[r]))
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredClass"), I2.Loc.LocalizationManager.GetTranslation(item.itemReqNames[r]), true);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required Class ", item.itemReqNames[r], true);
#endif
                        else
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredClass"), I2.Loc.LocalizationManager.GetTranslation(item.itemReqNames[r] ), true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required Class", item.itemReqNames[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif

                    }
                    if (item.itemReqTypes[r].Equals("Race"))
                    {
                        if (((int)ClientAPI.GetPlayerObject().GetProperty("race")) == int.Parse(item.itemReqValues[r]))
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredRace"), I2.Loc.LocalizationManager.GetTranslation(item.itemReqNames[r] ), true);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required Race ", item.itemReqNames[r], true);
#endif
                        else
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredRace") , I2.Loc.LocalizationManager.GetTranslation(item.itemReqNames[r]) , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required Race ", item.itemReqNames[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                    }

                    if (item.itemReqTypes[r].Equals("Skill Level"))
                    {


                        if (Skills.Instance.GetPlayerSkillLevel(item.itemReqNames[r]) < int.Parse(item.itemReqValues[r]))
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+ I2.Loc.LocalizationManager.GetTranslation(item.itemReqNames[r]),  item.itemReqValues[r] , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required " + item.itemReqNames[r], item.itemReqValues[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                        else
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+ I2.Loc.LocalizationManager.GetTranslation(item.itemReqNames[r]), item.itemReqValues[r] , true);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required " + item.itemReqNames[r], item.itemReqValues[r], true);
#endif
                    }

                    if (item.itemReqTypes[r].Equals("Stat"))
                    {
                        int val = 0;
                        if (ClientAPI.GetPlayerObject().PropertyExists(item.itemReqNames[r]))
                        {
                            val = (int)ClientAPI.GetPlayerObject().GetProperty(item.itemReqNames[r]);
                        }

                        string statName = item.itemReqNames[r];
#if AT_I2LOC_PRESET
            statName = FirstCharToUpper(I2.Loc.LocalizationManager.GetTranslation(statName));
#else
                        string[] statNames = statName.Split('_');
                        if (statNames.Length > 1)
                        {
                            statName = FirstCharToUpper(statNames[0]) + " " + FirstCharToUpper(statNames[1]);
                        }
                        else
                        {
                            statName = FirstCharToUpper(statNames[0]);
                        }
#endif
                        if (val < int.Parse(item.itemReqValues[r]))
#if AT_I2LOC_PRESET
                    UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+ statName ,  item.itemReqValues[r] , true,UGUITooltip.Instance.itemStatLowerColour);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required " + statName, item.itemReqValues[r], true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                        else
#if AT_I2LOC_PRESET
                    UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+statName ,  item.itemReqValues[r] , true);
#else
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required " + statName, item.itemReqValues[r], true);
#endif
                    }
                }

            if (item.Unique)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Unique"),"",true);
#else
                UGUITooltip.Instance.AddAdditionalAttribute2("Unique", "", true);
#endif
            }


            if (item.isBound)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Soulbound"), "", true,UGUITooltip.Instance.itemStatLowerColour);
#else
                UGUITooltip.Instance.AddAdditionalAttribute2("Soulbound", "", true, UGUITooltip.Instance.itemStatLowerColour);
#endif
            }
            else if (item.binding > 0)
            {
                if (item.binding == 1)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("SoulboundOnEquip"), "", true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Soulbound On Equip", "", true);
#endif
                }
                else if (item.binding == 2)
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("SoulboundOnPickup"), "", true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Soulbound On Pickup", "", true);
#endif
            }
            if (item.sellable)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Sellable"), "", true);
#else
                UGUITooltip.Instance.AddAdditionalAttribute2("Sellable", "", true);
#endif
            }
            else
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Not Sellable"), "", true,UGUITooltip.Instance.itemStatLowerColour);
#else
                UGUITooltip.Instance.AddAdditionalAttribute2("Not Sellable", "", true, UGUITooltip.Instance.itemStatLowerColour);
#endif

            }
            if ((item.itemType == "Weapon" || item.itemType == "Armor"))
                if (item.enchantId > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Enchantable"), "", true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Enchantable", "", true);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Not Enchantable"), "", true,UGUITooltip.Instance.itemStatLowerColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Not Enchantable", "", true, UGUITooltip.Instance.itemStatLowerColour);
#endif

                }
            if (item.maxDurability > 0)
            {
#if AT_I2LOC_PRESET
             UGUITooltip.Instance.AddAdditionalAttribute2( I2.Loc.LocalizationManager.GetTranslation("Durability")+" ", item.durability + "/" + item.maxDurability, true, UGUITooltip.Instance.itemTypeColour);
#else
                UGUITooltip.Instance.AddAdditionalAttribute2(" Durability", item.durability + "/" + item.maxDurability, true, UGUITooltip.Instance.itemTypeColour);
#endif
            }
            if (item.GetEffectPositionsOfTypes("ClaimObject").Count > 0)
            {
                UGUITooltip.Instance.AddAdditionalAttributeSeperator2();
#if AT_I2LOC_PRESET
            string tooltipDescription = I2.Loc.LocalizationManager.GetTranslation("Items/" + item.tooltip);
#else
                string tooltipDescription = item.tooltip;
#endif
                int templateId = int.Parse(item.itemEffectValues[item.GetEffectPositionsOfTypes("ClaimObject")[0]]);
                AtavismBuildObjectTemplate abot = WorldBuilder.Instance.GetBuildObjectTemplate(templateId);
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttributeTitle2(I2.Loc.LocalizationManager.GetTranslation("Claim Object")+" : " + I2.Loc.LocalizationManager.GetTranslation(abot.buildObjectName));
            UGUITooltip.Instance.AddAdditionalAttributeTitle2(I2.Loc.LocalizationManager.GetTranslation("Claim type") + " : " + I2.Loc.LocalizationManager.GetTranslation(abot.validClaimTypes.ToString()));
            UGUITooltip.Instance.AddAdditionalAttributeTitle2(I2.Loc.LocalizationManager.GetTranslation("Resources")+" : ");
            for (int i = 0; i < abot.itemsReq.Count; i++)
            {
                AtavismInventoryItem itemc = Inventory.Instance.GetItemByTemplateID(abot.itemsReq[i]);
                UGUITooltip.Instance.AddAdditionalAttributeResource2(I2.Loc.LocalizationManager.GetTranslation(itemc.name), abot.itemsReqCount[i].ToString(),itemc.icon,false);
            }
            UGUITooltip.Instance.AddAdditionalAttributeSeperator2();
            if (abot.skill > 0)
            {
                Skill skill = Skills.Instance.GetSkillByID(abot.skill);
                if (skill != null)
                {
                    if (Skills.Instance.GetPlayerSkillLevel(abot.skill) >= abot.skillLevelReq)
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Required") + " "+ I2.Loc.LocalizationManager.GetTranslation("Skill") + " " + I2.Loc.LocalizationManager.GetTranslation(skill.skillname), abot.skillLevelReq.ToString(),true);
                    }
                    else
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("Skill")+" " + I2.Loc.LocalizationManager.GetTranslation(skill.skillname),  abot.skillLevelReq.ToString() ,true, UGUITooltip.Instance.itemStatLowerColour);
                    }
                }
                else
                {
                    Debug.LogError("Building Object Skill " + abot.skill + " can't be found");
                }
            }
            if (!abot.reqWeapon.Equals("~ none ~"))
            {
                if (((string)ClientAPI.GetPlayerObject().GetProperty("weaponType")).Equals(abot.reqWeapon))
                    UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("equiped weapon type"), I2.Loc.LocalizationManager.GetTranslation(abot.reqWeapon),true);
                else
                    UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("equiped weapon type"),  I2.Loc.LocalizationManager.GetTranslation(abot.reqWeapon) ,true, UGUITooltip.Instance.itemStatLowerColour);
            }
#else
                UGUITooltip.Instance.AddAdditionalAttributeTitle2("Claim Object : " + abot.buildObjectName);
                UGUITooltip.Instance.AddAdditionalAttributeTitle2("Claim type : " + abot.validClaimTypes);
                UGUITooltip.Instance.AddAdditionalAttributeTitle2("Resources : ");
                for (int i = 0; i < abot.itemsReq.Count; i++)
                {
                    AtavismInventoryItem itemc = Inventory.Instance.GetItemByTemplateID(abot.itemsReq[i]);
                    UGUITooltip.Instance.AddAdditionalAttributeResource2(itemc.name, abot.itemsReqCount[i].ToString(), itemc.icon, false);
                }
                UGUITooltip.Instance.AddAdditionalAttributeSeperator2();
                if (abot.skill > 0)
                {
                    Skill skill = Skills.Instance.GetSkillByID(abot.skill);
                    if (skill != null)
                    {
                        if (Skills.Instance.GetPlayerSkillLevel(abot.skill) >= abot.skillLevelReq)
                        {
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required Skill " + skill.skillname, abot.skillLevelReq.ToString(), true);
                        }
                        else
                        {
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required Skill " + skill.skillname, abot.skillLevelReq.ToString(), true, UGUITooltip.Instance.itemStatLowerColour);
                        }
                    }
                    else
                    {
                        Debug.LogError("Building Object Skill " + abot.skill + " can't be found");
                    }
                }
                if (!abot.reqWeapon.Equals("~ none ~"))
                {
                    if (((string)ClientAPI.GetPlayerObject().GetProperty("weaponType")).Equals(abot.reqWeapon))
                        UGUITooltip.Instance.AddAdditionalAttribute2("Required equiped weapon type: ", abot.reqWeapon, true);
                    else
                        UGUITooltip.Instance.AddAdditionalAttribute2("Required equiped weapon type:", abot.reqWeapon, true, UGUITooltip.Instance.itemStatLowerColour);
                }
#endif
                UGUITooltip.Instance.SetAdditionalDescription2(tooltipDescription);
            }
            else if (item.GetEffectPositionsOfTypes("CraftsItem").Count > 0)
            {
                UGUITooltip.Instance.AddAdditionalAttributeSeperator2();

#if AT_I2LOC_PRESET
            string tooltipDescription = I2.Loc.LocalizationManager.GetTranslation("Items/" + item.tooltip);
#else
                string tooltipDescription = item.tooltip;
#endif
                int craftingRecipeID = int.Parse(item.itemEffectValues[item.GetEffectPositionsOfTypes("CraftsItem")[0]]);
                AtavismCraftingRecipe recipe = Inventory.Instance.GetCraftingRecipe(craftingRecipeID);
                // Crafts <item>
                AtavismInventoryItem itemCrafted = Inventory.Instance.GetItemByTemplateID(recipe.createsItems[0]);
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Crafts"), itemCrafted.name, true);
            UGUITooltip.Instance.AddAdditionalAttributeTitle2(I2.Loc.LocalizationManager.GetTranslation("Resources")+" : ");
            for (int r = 0; r < recipe.itemsReq.Count; r++)
            {
                 AtavismInventoryItem it = Inventory.Instance.GetItemByTemplateID(recipe.itemsReq[r]);
               UGUITooltip.Instance.AddAdditionalAttributeResource2(I2.Loc.LocalizationManager.GetTranslation("Items/" + it.name), recipe.itemsReqCounts[r].ToString(),it.icon, false);
            }
            UGUITooltip.Instance.AddAdditionalAttributeSeperator2();
            UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("Station"), recipe.stationReq, true);
            if (recipe.skillID > 0)
            {
                Skill skill = Skills.Instance.GetSkillByID(recipe.skillID);
                if (skill != null)
                {
                    if (Skills.Instance.GetPlayerSkillLevel(recipe.skillID) >= recipe.skillLevelReq)
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Required")+" "+ I2.Loc.LocalizationManager.GetTranslation("Skill")+" "+ I2.Loc.LocalizationManager.GetTranslation(Skills.Instance.GetSkillByID(recipe.skillID).skillname), recipe.skillLevelReq.ToString(), true);
                    }
                    else
                    {
                        UGUITooltip.Instance.AddAdditionalAttribute2( I2.Loc.LocalizationManager.GetTranslation("Required") + " " + I2.Loc.LocalizationManager.GetTranslation("Skill")+""+ I2.Loc.LocalizationManager.GetTranslation(Skills.Instance.GetSkillByID(recipe.skillID).skillname) ,  recipe.skillLevelReq.ToString() , true, UGUITooltip.Instance.itemStatLowerColour);
                    }
                }
                else
                {
                    Debug.LogError("Craft Skill " + recipe.skillID + " can't be found");
                }
            }
          
#else
                UGUITooltip.Instance.AddAdditionalAttribute2("Crafts", itemCrafted.name, true);
                UGUITooltip.Instance.AddAdditionalAttributeTitle2("Resources : ");
                for (int r = 0; r < recipe.itemsReq.Count; r++)
                {
                    AtavismInventoryItem it = Inventory.Instance.GetItemByTemplateID(recipe.itemsReq[r]);
                    UGUITooltip.Instance.AddAdditionalAttributeResource2(it.name, recipe.itemsReqCounts[r].ToString(), it.icon, false);

                }
                UGUITooltip.Instance.AddAdditionalAttributeSeperator2();
                UGUITooltip.Instance.AddAdditionalAttribute2("Required Station", recipe.stationReq, true);
                if (recipe.skillID > 0)
                {
                    Skill skill = Skills.Instance.GetSkillByID(recipe.skillID);
                    if (skill != null)
                    {
                        if (Skills.Instance.GetPlayerSkillLevel(recipe.skillID) >= recipe.skillLevelReq)
                        {
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required Skill " + Skills.Instance.GetSkillByID(recipe.skillID).skillname, recipe.skillLevelReq.ToString(), true);
                        }
                        else
                        {
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required Skill " + Skills.Instance.GetSkillByID(recipe.skillID).skillname, recipe.skillLevelReq.ToString(), true, UGUITooltip.Instance.itemStatLowerColour);
                        }
                    }
                    else
                    {
                        Debug.LogError("Craft Skill " + recipe.skillID + " can't be found");
                    }
                }
#endif

                UGUITooltip.Instance.AddAdditionalAttributeSeperator2();
                UGUITooltip.Instance.SetAdditionalDescription2(tooltipDescription);
            //    showAdditionalTooltip2(recipe.createsItems[0]);
            }
            else
            {
                UGUITooltip.Instance.AddAdditionalAttributeSeperator2();

#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalDescription2(I2.Loc.LocalizationManager.GetTranslation("Items/" + item.tooltip));
#else
                UGUITooltip.Instance.SetAdditionalDescription2(item.tooltip);
#endif
            }
            //check ability ie learned
            if (item.GetEffectPositionsOfTypes("UseAbility").Count > 0)
            {
                if (item.name.IndexOf("TeachAbility") > -1)
                {
                    int abilityID = int.Parse(item.itemEffectNames[item.GetEffectPositionsOfTypes("UseAbility")[0]]);
                    AtavismAbility aa = Abilities.Instance.GetAbility(abilityID);
                    AtavismAbility paa = Abilities.Instance.GetPlayerAbility(abilityID);
                    if (paa != null)
                    {

#if AT_I2LOC_PRESET
                        UGUITooltip.Instance.AddAdditionalAttribute2( I2.Loc.LocalizationManager.GetTranslation("taught") , "", true, UGUITooltip.Instance.itemStatLowerColour);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute2("Taught", "", true, UGUITooltip.Instance.itemStatLowerColour);
#endif
                    }
                    aa.ShowAdditionalTooltip2();
                }
            }
            UGUITooltip.Instance.ShowAdditionalTooltip2();
        }






        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            var arr = input.ToCharArray();
            arr[0] = char.ToUpperInvariant(arr[0]);
            return new string(arr);
        }

        public override Cooldown GetLongestActiveCooldown()
        {
            // Go through each item effect and see if it is an ability
            List<int> effectTypes = GetEffectPositionsOfTypes("UseAbility");
            foreach (int effectPos in effectTypes)
            {
                AtavismAbility ab = Abilities.Instance.GetAbility(int.Parse(itemEffectValues[effectPos]));
                if (ab != null)
                    return ab.GetLongestActiveCooldown();
            }

            return null;
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
        /// <summary>
        /// Gets or sets the name of the base.
        /// </summary>
        /// <value>The name of the base.</value>
        #region Properties
        public string BaseName
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public OID ItemId
        {
            get
            {
                return itemId;
            }
            set
            {
                itemId = value;
            }
        }

        public int TemplateId
        {
            get
            {
                return templateId;
            }
            set
            {
                templateId = value;
            }
        }

        public string Category
        {
            get
            {
                return category;
            }
            set
            {
                category = value;
            }
        }

        public string Subcategory
        {
            get
            {
                return subcategory;
            }
            set
            {
                subcategory = value;
            }
        }

        public int Count
        {
            get
            {
                return count /*- usedCount*/;
            }
            set
            {
                count = value;
            }
        }

        public int Quality
        {
            get
            {
                return quality;
            }
            set
            {
                quality = value;
            }
        }

        public int Binding
        {
            get
            {
                return binding;
            }
            set
            {
                binding = value;
            }
        }

        public bool IsBound
        {
            get
            {
                return isBound;
            }
            set
            {
                isBound = value;
            }
        }

        public bool Unique
        {
            get
            {
                return unique;
            }
            set
            {
                unique = value;
            }
        }

        public int StackLimit
        {
            get
            {
                return stackLimit;
            }
            set
            {
                stackLimit = value;
            }
        }

        public int CurrencyType
        {
            get
            {
                return currencyType;
            }
            set
            {
                currencyType = value;
            }
        }

        public int Cost
        {
            get
            {
                return cost;
            }
            set
            {
                cost = value;
            }
        }

        public bool Sellable
        {
            get
            {
                return sellable;
            }
            set
            {
                sellable = value;
            }
        }

        public int EnergyCost
        {
            get
            {
                return energyCost;
            }
            set
            {
                energyCost = value;
            }
        }

        public int Encumberance
        {
            get
            {
                return encumberance;
            }
            set
            {
                encumberance = value;
            }
        }

        public Dictionary<string, int> Resistances
        {
            get
            {
                return resistances;
            }
            set
            {
                resistances = value;
            }
        }

        public Dictionary<string, int> Stats
        {
            get
            {
                return stats;
            }
            set
            {
                stats = value;
            }
        }
        public Dictionary<string, int> EnchantStats
        {
            get
            {
                return enchantStats;
            }
            set
            {
                enchantStats = value;
            }
        }
        public Dictionary<string, Dictionary<int, int>> SocketSlots
        {
            get
            {
                return socketSlots;
            }
            set
            {
                socketSlots = value;
            }
        }
        public Dictionary<string, Dictionary<int, long>> SocketSlotsOid
        {
            get
            {
                return socketSlotsOid;
            }
            set
            {
                socketSlotsOid = value;
            }
        }

        public int DamageValue
        {
            get
            {
                return damageValue;
            }
            set
            {
                damageValue = value;
            }
        }
        public int DamageMaxValue
        {
            get
            {
                return damageMaxValue;
            }
            set
            {
                damageMaxValue = value;
            }
        }

        public string DamageType
        {
            get
            {
                return damageType;
            }
            set
            {
                damageType = value;
            }
        }

        public int WeaponSpeed
        {
            get
            {
                return weaponSpeed;
            }
            set
            {
                weaponSpeed = value;
            }
        }

        public int Durability
        {
            get
            {
                return durability;
            }
            set
            {
                durability = value;
            }
        }

        public int MaxDurability
        {
            get
            {
                return maxDurability;
            }
            set
            {
                maxDurability = value;
            }
        }

        public int Weight
        {
            get
            {
                if (GetEffectPositionsOfTypes("Weight").Count > 0)
                {
                    return int.Parse(itemEffectValues[GetEffectPositionsOfTypes("Weight")[0]]);
                }
                return 0;
            }
        }

        public int ReqLeval
        {
            get
            {
                return reqLevel;
            }
            set
            {
                reqLevel = value;
            }
        }
        public int SetId
        {
            get
            {
                return setId;
            }
            set
            {
                setId = value;
            }
        }
        public int EnchantId
        {
            get
            {
                return enchantId;
            }
            set
            {
                enchantId = value;
            }
        }
        #endregion Properties
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Atavism
{
    [Serializable]
    public class AtavismInventoryItemSetLevel
    {
        public int DamageValue = 0;
        public int DamageValuePercentage = 0;
        public int NumerOfParts = 0;
        public List<string> itemStatName = new List<string>();
        public List<int> itemStatValues = new List<int>();
        public List<int> itemStatValuesPercentage = new List<int>();
    }

    public class AtavismInventoryItemSet : MonoBehaviour
    {
        public int Setid = 0;
        public string Name = "name";        // The enchant profile name
                                            //  public List<SetLevelData> levelList = new List<SetLevelData>();
        public List<int> itemList = new List<int>();
        public List<AtavismInventoryItemSetLevel> levelList = new List<AtavismInventoryItemSetLevel>();

        public AtavismInventoryItemSet Clone(GameObject go)
        {
            AtavismInventoryItemSet clone = go.AddComponent<AtavismInventoryItemSet>();
            clone.Name = Name;
            clone.itemList = itemList;
            return clone;
        }


        /*
        public void AlterUseCount(int delta) {
            usedCount += delta;
        }

        public void ResetUseCount() {
            usedCount = 0;
        }

        public void AddItemEffect(string itemEffectType, string itemEffectName, string itemEffectValue) {
            itemEffectTypes.Add(itemEffectType);
            itemEffectNames.Add(itemEffectName);
            itemEffectValues.Add(itemEffectValue);
        }
        */
        public void ClearLevels()
        {
            levelList.Clear();

        }
        /*
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
        */
        /*  public List<int> GetEffectPositionsOfTypes(string effectType) {
              List<int> effectPositions = new List<int>();
              for (int i = 0; i < itemEffectTypes.Count; i++) {
                  if (itemEffectTypes[i] == effectType)
                      effectPositions.Add(i);
              }
              return effectPositions;
          }

        */
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
        /*  public string BaseName {
              get {
                  return name;
              }
              set {
                  name = value;
              }
          }

          public OID ItemId {
              get {
                  return itemId;
              }
              set {
                  itemId = value;
              }
          }

          public int TemplateId {
              get {
                  return templateId;
              }
              set {
                  templateId = value;
              }
          }

          public string Category {
              get {
                  return category;
              }
              set {
                  category = value;
              }
          }

          public string Subcategory {
              get {
                  return subcategory;
              }
              set {
                  subcategory = value;
              }
          }

          public int Count {
              get {
                  return count ;
              }
              set {
                  count = value;
              }
          }

          public int Quality {
              get {
                  return quality;
              }
              set {
                  quality = value;
              }
          }

          public int Binding {
              get {
                  return binding;
              }
              set {
                  binding = value;
              }
          }

          public bool IsBound {
              get {
                  return isBound;
              }
              set {
                  isBound = value;
              }
          }

          public bool Unique {
              get {
                  return unique;
              }
              set {
                  unique = value;
              }
          }

          public int StackLimit {
              get {
                  return stackLimit;
              }
              set {
                  stackLimit = value;
              }
          }

          public int CurrencyType {
              get {
                  return currencyType;
              }
              set {
                  currencyType = value;
              }
          }

          public int Cost {
              get {
                  return cost;
              }
              set {
                  cost = value;
              }
          }

          public bool Sellable {
              get {
                  return sellable;
              }
              set {
                  sellable = value;
              }
          }

          public int EnergyCost {
              get {
                  return energyCost;
              }
              set {
                  energyCost = value;
              }
          }

          public int Encumberance {
              get {
                  return encumberance;
              }
              set {
                  encumberance = value;
              }
          }

          public Dictionary<string, int> Resistances {
              get {
                  return resistances;
              }
              set {
                  resistances = value;
              }
          }

          public Dictionary<string, int> Stats {
              get {
                  return stats;
              }
              set {
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

          public int DamageValue {
              get {
                  return damageValue;
              }
              set {
                  damageValue = value;
              }
          }
          public int DamageMaxValue {
              get {
                  return damageMaxValue;
              }
              set {
                  damageMaxValue = value;
              }
          }

          public string DamageType {
              get {
                  return damageType;
              }
              set {
                  damageType = value;
              }
          }

          public int WeaponSpeed {
              get {
                  return weaponSpeed;
              }
              set {
                  weaponSpeed = value;
              }
          }

          public int Durability {
              get {
                  return durability;
              }
              set {
                  durability = value;
              }
          }

          public int MaxDurability {
              get {
                  return maxDurability;
              }
              set {
                  maxDurability = value;
              }
          }

          public int Weight {
              get {
                  if (GetEffectPositionsOfTypes("Weight").Count > 0)
                  {
                      return int.Parse(itemEffectValues[GetEffectPositionsOfTypes("Weight")[0]]);
                  }
                  return 0;
              }
          }

          public int ReqLeval {
              get {
                  return reqLevel;
              }
              set {
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
          }*/
        #endregion Properties
    }
}
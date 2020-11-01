using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class AtavismCraftingRecipe : MonoBehaviour
    {

        public int recipeID;
        public string recipeName;
        public Sprite icon;
        public int skillID = -1;
        public int skillLevelReq = -1;
        public string stationReq = "";
        public int creationTime = 0;

        public List<int> createsItems;
        public List<int> createsItemsCounts;
        public List<int> createsItems2;
        public List<int> createsItemsCounts2;
        public List<int> createsItems3;
        public List<int> createsItemsCounts3;
        public List<int> createsItems4;
        public List<int> createsItemsCounts4;
        public List<int> itemsReq;
        public List<int> itemsReqCounts;

        public void ShowTooltip(GameObject target)
        {
#if AT_I2LOC_PRESET
        UGUITooltip.Instance.SetTitle(I2.Loc.LocalizationManager.GetTranslation("Items/"+recipeName));
#else
            UGUITooltip.Instance.SetTitle(recipeName);
#endif
            if (icon != null)
            {
                UGUITooltip.Instance.SetIcon(icon);
            }
            //  UGUITooltip.Instance.SetQuality(quality);
            //  UGUITooltip.Instance.SetTitleColour(AtavismSettings.Instance.ItemQualityColor(quality));
            UGUITooltip.Instance.AddAttributeSeperator();

#if AT_I2LOC_PRESET
            string tooltipDescription ="";
#else
            string tooltipDescription = "";
#endif
            //  int craftingRecipeID = int.Parse(itemEffectValues[GetEffectPositionsOfTypes("CraftsItem")[0]]);
            AtavismCraftingRecipe recipe = Inventory.Instance.GetCraftingRecipe(recipeID);
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
            UGUITooltip.Instance.Show(target);

        }
        void showAdditionalTooltip(int Id)
        {
            AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(Id);
            AtavismInventoryItem item2 = GetComponent<AtavismInventoryItem>();
            if (item2 == null)
                item2 = gameObject.AddComponent<AtavismInventoryItem>();
            item2.showAdditionalTooltip(item);
        }


    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{
    public class CraftingComponent
    {
        public AtavismInventoryItem item = null;
        public int count = 1;
    }

    public class Blueprint
    {
        public int recipeID;
        public int resultID;
        public int recipeItemID;
        public string station;
        public List<List<CraftingComponent>> slots;
    }

    public class ResourceItem
    {
        public AtavismInventoryItem item;
        public int count;
    }

    public class Crafting : MonoBehaviour
    {

        static Crafting instance;

        public int gridSize = 3;
        //CraftingStationType stationType = CraftingStationType.None;
        string stationType = "None";

        GameObject station;
        string coordEffect;

        List<CraftingComponent> gridItems = new List<CraftingComponent>();
        AtavismInventoryItem dye = null;
        AtavismInventoryItem essence = null;
        int recipeID = -1;
        public string recipeName = "";
        int recipeItemID = -1;
        public List<AtavismInventoryItem> resultItems = new List<AtavismInventoryItem>();
        AtavismInventoryItem recipeItem = null;
        List<Blueprint> blueprints = new List<Blueprint>();

        List<ResourceItem> resourceLoot = new List<ResourceItem>();
        Dictionary<int, ResourceNode> resourceNodes = new Dictionary<int, ResourceNode>();
        Dictionary<int, bool> resourceNodesQueue = new Dictionary<int, bool>();

        int currentResourceNode = -1;
        [SerializeField] bool showAllKnownSkills = false;

        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            int gridCount = gridSize * gridSize;
            for (int i = 0; i < gridCount; i++)
            {
                gridItems.Add(new CraftingComponent());
            }

            // Listen for messages from the server
            NetworkAPI.RegisterExtensionMessageHandler("CraftingGridMsg", HandleCraftingGridResponse);
            NetworkAPI.RegisterExtensionMessageHandler("CraftingMsg", HandleCraftingMessage);
            NetworkAPI.RegisterExtensionMessageHandler("BlueprintMsg", HandleBlueprintMessage);
            NetworkAPI.RegisterExtensionMessageHandler("resource_drops", HandleResourceDropsMessage);
            NetworkAPI.RegisterExtensionMessageHandler("resource_state", HandleResourceStateMessage);

            NetworkAPI.RegisterExtensionMessageHandler("start_harvest_task", HandleStartHarvestTask);
            NetworkAPI.RegisterExtensionMessageHandler("harvest_task_interrupted", HandleInterruptHarvestTask);

            // Listen for inventory changes
            AtavismEventSystem.RegisterEvent("INVENTORY_UPDATE", this);
        }
        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("INVENTORY_UPDATE", this);

        }
        void Update()
        {
            if (station != null)
            {
                if (Vector3.Distance(ClientAPI.GetPlayerObject().Position, station.transform.position) > 5)
                {
                    ClearGrid();
                    station.SendMessage("CloseStation");
                    station = null;
                    string[] args = new string[1];
                    AtavismEventSystem.DispatchEvent("CLOSE_CRAFTING_STATION", args);
                }
            }

            if (currentResourceNode > 0 && resourceNodes.ContainsKey(currentResourceNode))
            {
                if (Vector3.Distance(ClientAPI.GetPlayerObject().Position, resourceNodes[currentResourceNode].gameObject.transform.position) > 5)
                {
                    currentResourceNode = -1;
                    string[] args = new string[1];
                    AtavismEventSystem.DispatchEvent("CLOSE_RESOURCE_LOOT_WINDOW", args);
                }
            }
        }

        void ClientReady()
        {
            ClientAPI.WorldManager.RegisterObjectPropertyChangeHandler("recipes", RecipesPropertyHandler);
        }

        public void RegisterResourceNode(ResourceNode resourceNode)
        {
            resourceNodes[resourceNode.id] = resourceNode;
            if (resourceNodesQueue.ContainsKey(resourceNode.id))
            {
                resourceNodes[resourceNode.id].Active = resourceNodesQueue[resourceNode.id];
                resourceNodes[resourceNode.id].ResetHighlight();
                if (resourceNodes[resourceNode.id].GetComponent<Rigidbody>() != null)
                {
                    resourceNodes[resourceNode.id].GetComponent<Rigidbody>().isKinematic = false;
                    resourceNodes[resourceNode.id].GetComponent<Rigidbody>().AddForce(0.1f, 0, 0.1f);
                }
                resourceNodesQueue.Remove(resourceNode.id);
            }
        }

        public void RemoveResourceNode(int id)
        {
            resourceNodes.Remove(id);
        }

        public ResourceNode GetResourceNode(int id)
        {
            if (resourceNodes.ContainsKey(id))
            {
                return resourceNodes[id];
            }
            else
            {
                return null;
            }
        }

        public void LootResource(AtavismInventoryItem item)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("resourceID", CurrentResourceNode);
            props.Add("gatherAll", false);
            props.Add("itemID", item.TemplateId);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "crafting.GATHER_RESOURCE", props);
        }

        public void HarvestResource(int resourceID)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("resourceID", resourceID);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "crafting.HARVEST_RESOURCE", props);
            CurrentResourceNode = resourceID;
        }

        public void RecipesPropertyHandler(object sender, ObjectPropertyChangeEventArgs args)
        {
            if (args.Oid != ClientAPI.GetPlayerOid())
                return;
            LinkedList<object> recipes_prop = (LinkedList<object>)ClientAPI.GetPlayerObject().GetProperty("recipes");
            Debug.Log("Got player recipes property change: " + recipes_prop);
            //LinkedList<string> recipeIDs = new LinkedList<string>();
            int numRecipes = 0;
            Dictionary<string, object> props = new Dictionary<string, object>();
            foreach (string recipeString in recipes_prop)
            {
                // Get items
                bool haveBlueprint = false;
                int recipeID = int.Parse(recipeString);
                foreach (Blueprint bp in blueprints)
                {
                    if (bp.recipeID == recipeID)
                        haveBlueprint = true;
                }
                if (!haveBlueprint)
                {
                    props.Add("recipe" + numRecipes, recipeID);
                    numRecipes++;
                }
            }

            props.Add("numRecipes", numRecipes);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "crafting.GET_BLUEPRINTS", props);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "INVENTORY_UPDATE")
            {
                // The inventory has updated, we need to see if it effects the crafting UI
                int gridCount = gridSize * gridSize;
                for (int i = 0; i < gridCount; i++)
                {
                    if (gridItems[i].item != null)
                    {
                        AtavismInventoryItem item = gridItems[i].item;
                        gridItems[i].item = Inventory.Instance.GetInventoryItem(item.ItemId);
                    }
                }

                if (station != null)
                    SendGridUpdated();
                //string[] args = new string[1];
                //AtavismEventSystem.DispatchEvent("CRAFTING_GRID_UPDATE", args);
            }
        }

        public void SetGridItem(int gridPos, AtavismInventoryItem item, bool send)
        {
            if (item == null)
            {
                //if (gridItems[gridPos].item != null)
                //	gridItems[gridPos].item.AlterUseCount(-gridItems[gridPos].count);
                gridItems[gridPos].item = null;
                gridItems[gridPos].count = 1;
            }/* else if (gridItems[gridPos].item == item) {
			gridItems[gridPos].count++;
		}*/ else
            {
                gridItems[gridPos].item = item;
                gridItems[gridPos].count = item.Count;
            }

            //if (item != null)
            //	item.AlterUseCount(1);
            if (send)
                SendGridUpdated();
        }

        public void SetRecipeItem(AtavismInventoryItem item)
        {
            recipeItem = item;
            SendGridUpdated();
        }

        public void SetDye(AtavismInventoryItem item)
        {
            dye = item;
            SendGridUpdated();
        }

        public void SetEssence(AtavismInventoryItem item)
        {
            essence = item;
            SendGridUpdated();
        }

        void SendGridUpdated()
        {
            // Send message to server to work out if we have a valid recipe
            Dictionary<string, object> props = new Dictionary<string, object>();
            LinkedList<object> itemIds = new LinkedList<object>();
            LinkedList<object> itemCounts = new LinkedList<object>();
            for (int i = 0; i < gridItems.Count; i++)
            {
                if (gridItems[i].item != null)
                {
                    itemIds.AddLast(gridItems[i].item.templateId);
                }
                else
                {
                    itemIds.AddLast(-1);
                }
                itemCounts.AddLast(gridItems[i].count);
            }
            props.Add("gridSize", gridSize);
            props.Add("componentIDs", itemIds);
            props.Add("componentCounts", itemCounts);
            props.Add("stationType", stationType.ToString());
            if (recipeItem != null)
            {
                props.Add("recipeItemID", recipeItem.templateId);
            }
            else
            {
                props.Add("recipeItemID", -1);
            }
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "crafting.GRID_UPDATED", props);

            //string[] args = new string[1];
            //AtavismEventSystem.DispatchEvent("INVENTORY_UPDATE", args);
        }

        public void CraftItem()
        {
            //    Debug.LogError("CraftItem");
            Dictionary<string, object> props = new Dictionary<string, object>();
            //properties["CraftType"] = craftType;
            LinkedList<object> items = new LinkedList<object>();
            LinkedList<object> itemCounts = new LinkedList<object>();
            for (int i = 0; i < gridItems.Count; i++)
            {
                if (gridItems[i].item != null)
                {
                    items.AddLast(gridItems[i].item.ItemId.ToLong());
                    itemCounts.AddLast(gridItems[i].count);
                }
            }
            props.Add("gridSize", gridSize);
            props.Add("components", items);
            props.Add("componentCounts", itemCounts);
            props.Add("RecipeId", recipeID);
            props.Add("stationType", stationType.ToString());
            if (recipeItem != null)
            {
                props.Add("recipeItemID", recipeItem.templateId);
            }
            else
            {
                props.Add("recipeItemID", -1);
            }
            props.Add("coordEffect", coordEffect);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "crafting.CRAFT_ITEM", props);
        }

        public void CraftItemBook(int recipeID, int count)
        {
            try
            {
                AtavismLogger.LogInfoMessage("Got Message CraftItemBook");
                Dictionary<string, object> props = new Dictionary<string, object>();
                //properties["CraftType"] = craftType;
                LinkedList<object> items = new LinkedList<object>();
                LinkedList<object> itemCounts = new LinkedList<object>();

                props.Add("count", count);
                props.Add("gridSize", gridSize);
                props.Add("components", items);
                props.Add("componentCounts", itemCounts);
                props.Add("RecipeId", recipeID);
                props.Add("stationType", stationType.ToString());
                if (recipeItem != null)
                {
                    props.Add("recipeItemID", recipeItem.templateId);
                }
                else
                {
                    props.Add("recipeItemID", -1);
                }
                props.Add("coordEffect", coordEffect);
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "crafting.CRAFT_ITEM", props);
                //    Debug.LogError("CraftItemBook End");
            }
            catch (System.Exception e)
            {
                AtavismLogger.LogError("Got Exeption " + e.Message);
            }
            AtavismLogger.LogInfoMessage("CraftItemBook End");

        }

        public void HandleCraftingGridResponse(Dictionary<string, object> props)
        {
            try
            {
                AtavismLogger.LogInfoMessage("Got Message HandleCraftingGridResponse");
                recipeID = (int)props["recipeID"];
                recipeName = (string)props["recipeName"];
                recipeItemID = (int)props["recipeItem"];
                int numResults = (int)props["numResults"];
                resultItems.Clear();
                for (int i = 0; i < numResults; i++)
                {
                    int itemID = (int)props["resultItem" + i];
                    AtavismInventoryItem resultItem = GetComponent<Inventory>().GetItemByTemplateID(itemID);
                    resultItem.Count = (int)props["resultItemCount" + i];
                    resultItems.Add(resultItem);
                }
                if (recipeItemID != -1)
                {
                    recipeItem = GetComponent<Inventory>().GetItemByTemplateID(recipeItemID);
                }
                else
                {
                    recipeItem = null;
                }
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("CRAFTING_GRID_UPDATE", args);

            }
            catch (System.Exception e)
            {
                AtavismLogger.LogError("Got Exeption " + e.Message);
            }
            AtavismLogger.LogInfoMessage("Got Message HandleCraftingGridResponse End");
        }

        void HandleCraftingMessage(Dictionary<string, object> props)
        {
            string msgType = (string)props["PluginMessageType"];

            switch (msgType)
            {

                case "CraftingStarted":
                    {
                        int creationTime = (int)props["creationTime"];
                        if (creationTime > 0)
                        {
                            // dispatch a ui event to tell the rest of the system
                            string[] args = new string[2];
                            args[0] = creationTime.ToString();
                            args[1] = OID.fromLong(ClientAPI.GetPlayerOid()).ToString();
                            AtavismEventSystem.DispatchEvent("CASTING_STARTED", args);
                        }
                        //ClearGrid();
                        //ui.GetComponent<CraftingUI>().StartProgressBar();
                        break;
                    }
                case "CraftingCompleted":
                    {
                        // Go through each item in the grid and set the item back to 0, reducing the count in the grid instead
                      //  int num = int.Parse((string)props["num"]);

                        int num = (int)props["num"];

                        for (int i = 0; i < num; i++)
                        {
                         //   int itemId = int.Parse((string)props["item" + i + "Id"]);
                          //  int itemCount = int.Parse((string)props["item" + i + "Count"]);

                            int itemId = (int)props["item" + i + "Id"];
                            int itemCount = (int)props["item" + i + "Count"];
                            string[] args = new string[1];
                            AtavismInventoryItem it = Inventory.Instance.GetItemByTemplateID(itemId);
                            if (it != null)
                            {
                                args[0] = "Item Created " + it.BaseName + " x" + itemCount;
                                AtavismEventSystem.DispatchEvent("ANNOUNCEMENT", args);
                            }
                        }
                        //	string[] args = new string[1];
                        //	AtavismEventSystem.DispatchEvent("CRAFTING_GRID_UPDATE", args);*/
                        //AtavismNGUIManager.Instance.craftingUI.StopCraftingTimer();
                        break;
                    }
                case "CraftingFailed":
                    {
                        /*Dictionary<string, object> errors = new Dictionary<string,object>();
                        errors.Add("ErrorText", (string)props["ErrorMsg"]);
                        GameObject ui = GameObject.Find("UI");
                        ui.GetComponent<ErrorMessage>().HandleErrorMessage(errors);
                        */
                        string[] args = new string[1];
                        args[0] = (string)props["ErrorMsg"];
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);

                        break;
                    }
                case "CraftingInterrupted":
                    {
                        Debug.LogWarning("Crafting was interrupted");
                        // dispatch a ui event to tell the rest of the system
                        string[] args = new string[2];
                        args[1] = OID.fromLong(ClientAPI.GetPlayerOid()).ToString();
                        AtavismEventSystem.DispatchEvent("CASTING_CANCELLED", args);
                        ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismMobController>().PlayAnimation("", 0);
                        break;
                    }
            }

            AtavismLogger.LogDebugMessage("Got A Crafting Message!");
        }

        public void HandleBlueprintMessage(Dictionary<string, object> props)
        {
            int numBlueprints = (int)props["numBlueprints"];

            for (int i = 0; i < numBlueprints; i++)
            {
                Blueprint bp = new Blueprint();
                bp.recipeID = (int)props["recipeID" + i];
                bp.resultID = (int)props["itemID" + i];
                bp.recipeItemID = (int)props["recipeItemID" + i];
                bp.station = (string)props["station" + i];
                int numRows = (int)props["numRows" + i];
                List<List<CraftingComponent>> slots = new List<List<CraftingComponent>>();
                for (int j = 0; j < numRows; j++)
                {
                    List<CraftingComponent> columnSlots = new List<CraftingComponent>();
                    int numColumns = (int)props["numColumns" + i + "_" + j];
                    for (int k = 0; k < numColumns; k++)
                    {
                        int itemID = (int)props["item" + i + "_" + j + "_" + k];
                        if (itemID == -1)
                            continue;
                        AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(itemID);
                        CraftingComponent component = new CraftingComponent();
                        component.item = item;
                        columnSlots.Add(component);
                    }
                    slots.Add(columnSlots);
                }
                bp.slots = slots;
                blueprints.Add(bp);
            }
        }

        void HandleResourceDropsMessage(Dictionary<string, object> props)
        {
            resourceLoot = new List<ResourceItem>();
            currentResourceNode = (int)props["resourceNode"];
            int numDrops = (int)props["numDrops"];

            for (int i = 0; i < numDrops; i++)
            {
                int itemID = (int)props["drop" + i];
                int count = (int)props["dropCount" + i];
                ResourceItem resourceItem = new ResourceItem();
                resourceItem.item = Inventory.Instance.GetItemByTemplateID(itemID);
                resourceItem.count = count;
                resourceLoot.Add(resourceItem);
            }
            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("RESOURCE_LOOT_UPDATE", args);
        }

        void HandleResourceStateMessage(Dictionary<string, object> props)
        {
            //AtavismLogger.
            int nodeID = (int)props["nodeID"];
            bool active = (bool)props["active"];
            if (resourceNodes.ContainsKey(nodeID))
            { 
                resourceNodes[nodeID].Active = active;
                resourceNodes[nodeID].ResetHighlight();
                if (resourceNodes[nodeID].GetComponent<Rigidbody>() != null)
                {
                    resourceNodes[nodeID].GetComponent<Rigidbody>().isKinematic = false;
                    resourceNodes[nodeID].GetComponent<Rigidbody>().AddForce(0.1f, 0, 0.1f);
                }
                else
                {
                    /*      if (resourceNodes[nodeID].isLODChild)
                          {
                              resourceNodes[nodeID].transform.parent.gameObject.SetActive(active);
                          }
                          else
                          {
                              resourceNodes[nodeID].gameObject.SetActive(active);
                          }*/
                }
            }
            else
            {
                if (resourceNodesQueue.ContainsKey(nodeID))
                    resourceNodesQueue[nodeID] = active;
                else
                    resourceNodesQueue.Add(nodeID, active);
             //   Debug.LogError("resourceNodes " + nodeID + " not found");
            }
        }
      

        public void HandleStartHarvestTask(Dictionary<string, object> props)
        {
            //ClientAPI.Write("Starting build task with length: " + (float)props["length"]);
            float length = (float)props["length"];
            string[] csArgs = new string[2];
            csArgs[0] = length.ToString();
            csArgs[1] = OID.fromLong(ClientAPI.GetPlayerOid()).ToString();
            AtavismEventSystem.DispatchEvent("CASTING_STARTED", csArgs);

            if (UGUICastingBar.Instance != null && UGUICastingBar.Instance.icon != null)
            {
                UGUICastingBar.Instance.icon.sprite = resourceNodes[currentResourceNode].selectedIcon;
            }
        }

        public void HandleInterruptHarvestTask(Dictionary<string, object> props)
        {
            string[] args = new string[2];
            args[0] = "";
            args[1] = OID.fromLong(ClientAPI.GetPlayerOid()).ToString();
            AtavismEventSystem.DispatchEvent("CASTING_CANCELLED", args);

            ClientAPI.GetPlayerObject().MobController.PlayAnimation("", 0);
        }

        public void ClearGrid()
        {
            int gridCount = gridSize * gridSize;
            for (int i = 0; i < gridCount; i++)
            {
                if (gridItems[i].item != null)
                {
                    gridItems[i].item.ResetUseCount();
                }
            }
            gridItems.Clear();
            for (int i = 0; i < gridCount; i++)
            {
                gridItems.Add(new CraftingComponent());
            }

            // Also clear special slots
            resultItems.Clear();
            dye = null;
            essence = null;
            recipeItem = null;
            recipeItemID = -1;

            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("CRAFTING_GRID_UPDATE", args);
            AtavismEventSystem.DispatchEvent("INVENTORY_UPDATE", args);
        }

        #region Properties
        public static Crafting Instance
        {
            get
            {
                return instance;
            }
        }
        public bool GetShowAllKnownSkills
        {
            get
            {
                return showAllKnownSkills;
            }
        }
        public List<CraftingComponent> GridItems
        {
            get
            {
                return gridItems;
            }
        }

        public List<AtavismInventoryItem> ResultItems
        {
            get
            {
                return resultItems;
            }
        }

        public AtavismInventoryItem RecipeItem
        {
            get
            {
                return recipeItem;
            }
        }

        /*	public CraftingStationType StationType {
                get {
                    return stationType;
                }
                set {
                    stationType = value;
                }
            }
            */
        public string StationType
        {
            get
            {
                return stationType;
            }
            set
            {
                stationType = value;
            }
        }
        public GameObject Station
        {
            get
            {
                return station;
            }
            set
            {
                station = value;
            }
        }

        public string CoordEffect
        {
            set
            {
                coordEffect = value;
            }
        }

        public int CurrentResourceNode
        {
            get
            {
                return currentResourceNode;
            }
            set
            {
                currentResourceNode = value;
            }
        }

        public List<ResourceItem> ResourceLoot
        {
            get
            {
                return resourceLoot;
            }
            set
            {
                resourceLoot = value;
            }
        }

        #endregion Properties
    }
}
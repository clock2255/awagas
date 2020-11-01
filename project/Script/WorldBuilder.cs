using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Atavism
{

    [System.Flags]
    public enum ClaimType
    {
        Any = (1 << 0),
        Residential = (1 << 1),
        Farm = (1 << 2),
    }

    public class ClaimPermission
    {
        public OID playerOid;
        public string playerName;
        public int permissionLevel;
    }

    public class Claim
    {
        public int id;
        public ClaimType claimType;
        public string name = "";
        public string ownerName = "";
        public Vector3 loc;
        public int sizeX = 30;
        public int sizeZ = 30;
        public Bounds bounds;
        public bool playerOwned;
        public bool forSale;
        public long cost;
        public int currency = 1;
        public int purchaseItemReq = -1;
        public int permissionlevel = 0;
        public Dictionary<int, int> resources = new Dictionary<int, int>();
        public Dictionary<int, ClaimObject> claimObjects = new Dictionary<int, ClaimObject>();
        public List<ClaimPermission> permissions = new List<ClaimPermission>();

        public void GenerateBounds()
        {
            bounds = new Bounds(loc, new Vector3(sizeX, sizeX * 3f, sizeZ));
        }

        public bool IsObjectFullyInsideClaim(Bounds b)
        {
            //Debug.Log("Bounds: " + bounds.ToString() + " against b: " + b.ToString());
            if (!bounds.Contains(b.min))
            {
                return false;
            }
            if (!bounds.Contains(b.max))
            {
                return false;
            }
            return true;
        }
    }

    public enum WorldBuildingState
    {
        PlaceItem,
        SelectItem,
        EditItem,
        MoveItem,
        SellClaim,
        PurchaseClaim,
        CreateClaim,
        CreateObject,
        Standard,
        Admin,
        None
    }

    public struct ClaimObjectData
    {
        public int objectID;
        public int templateID;
        public int claimID;
        public string prefabName;
        public Vector3 loc;
        public Quaternion orient;
        public string state;
        public int health;
        public int maxHealth;
        public bool complete;
    }

    public class WorldBuilder : MonoBehaviour
    {

        static WorldBuilder instance;

        public float deselectDistance = 6f;

        Dictionary<int, AtavismBuildObjectTemplate> buildObjectTemplates;
        WorldBuildingState buildingState = WorldBuildingState.None;
        private List<Claim> claims = new List<Claim>();
        private Claim activeClaim = null;
        bool showClaims = false;
        Dictionary<string,GameObject> claimGameObjects = new Dictionary<string,GameObject>();
        Dictionary<int, int> buildingResources = new Dictionary<int, int>();
        List<AtavismInventoryItem> itemsPlacedToUpgrade = new List<AtavismInventoryItem>();
        ClaimObject selectedObject;
        int selectedID = -1;

        List<ClaimObjectData> objectsToLoad = new List<ClaimObjectData>();
        int frameCount = 0;
        public bool showInConstructMaterialsFromBackpack = false;
        public bool itemsForUpgradeMustBeInserted = true;
        // Use this for initialization
        void Start()
        {
           
            if (instance != null)
            {
                return;
            }
            instance = this;

            AtavismEventSystem.RegisterEvent("CLAIM_ADDED", this);
            AtavismEventSystem.RegisterEvent("CLAIMED_REMOVED", this);
            AtavismEventSystem.RegisterEvent("LOGGED_OUT", this);

            // Register for messages relating to the claim system
            NetworkAPI.RegisterExtensionMessageHandler("claim_data", ClaimIDMessage);
            NetworkAPI.RegisterExtensionMessageHandler("remove_claim_data", ClaimRemoveDataMessage);
            NetworkAPI.RegisterExtensionMessageHandler("claim_updated", ClaimUpdatedMessage);
            NetworkAPI.RegisterExtensionMessageHandler("remove_claim", RemoveClaimMessage);
            NetworkAPI.RegisterExtensionMessageHandler("claim_deleted", RemoveClaimMessage);
            NetworkAPI.RegisterExtensionMessageHandler("claim_made", ClaimMadeMessage);

            NetworkAPI.RegisterExtensionMessageHandler("claim_object", ClaimObjectMessage);
            NetworkAPI.RegisterExtensionMessageHandler("claim_object_bulk", ClaimObjectBulkMessage);
            NetworkAPI.RegisterExtensionMessageHandler("move_claim_object", MoveClaimObjectMessage);
            NetworkAPI.RegisterExtensionMessageHandler("update_claim_object_state", UpdateClaimObjectStateMessage);
            NetworkAPI.RegisterExtensionMessageHandler("claim_object_info", ClaimObjectInfoMessage);
            NetworkAPI.RegisterExtensionMessageHandler("remove_claim_object", RemoveClaimObjectMessage);
            NetworkAPI.RegisterExtensionMessageHandler("buildingResources", HandleBuildingResources);
            NetworkAPI.RegisterExtensionMessageHandler("start_build_object", HandleStartBuildObject);
            NetworkAPI.RegisterExtensionMessageHandler("start_build_task", HandleStartBuildTask);
            NetworkAPI.RegisterExtensionMessageHandler("build_task_interrupted", HandleInterruptBuildTask);

            // Load in items
            buildObjectTemplates = new Dictionary<int, AtavismBuildObjectTemplate>();
            Object[] prefabs = Resources.LoadAll("Content/BuildObjects");
            foreach (Object displayPrefab in prefabs)
            {
                GameObject go = (GameObject)displayPrefab;
                AtavismBuildObjectTemplate displayData = go.GetComponent<AtavismBuildObjectTemplate>();
                if (!buildObjectTemplates.ContainsKey(displayData.id) && displayData.id > 0)
                {
                    buildObjectTemplates.Add(displayData.id, displayData);
                }
            }
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("CLAIM_ADDED", this);
            AtavismEventSystem.UnregisterEvent("CLAIMED_REMOVED", this);
            AtavismEventSystem.UnregisterEvent("LOGGED_OUT", this);
        }

        // Update is called once per frame
        void Update()
        {
            if (activeClaim == null /*&& buildingState != WorldBuildingState.None*/)
            {
                // Check against claims to see if a region has been entered
                foreach (Claim claim in claims)
                {
                 //   Debug.LogWarning("WorldBuilder "+claim.id+" "+claim.name+ " "+claim.permissionlevel+" "+claim.permissions+" | scena="+UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                    if (ClientAPI.GetPlayerObject() != null && InsideClaimArea(claim, ClientAPI.GetPlayerObject().Position))
                    {
                   //     Debug.LogWarning("WorldBuilder Selected ClaimId="+claim.id+" "+claim.name );
                        activeClaim = claim;
                        // dispatch a ui event to tell the rest of the system
                        string[] args = new string[1];
                        AtavismEventSystem.DispatchEvent("CLAIM_CHANGED", args);
                        break;
                    }
                }
            }
            else /*if (buildingState != WorldBuildingState.None) */
            {
                // Check if the player has left the claim
                if (ClientAPI.GetPlayerObject() != null && !InsideClaimArea(activeClaim, ClientAPI.GetPlayerObject().Position))
                {
                    activeClaim = null;
                    // dispatch a ui event to tell the rest of the system
                    string[] args = new string[1];
                    AtavismEventSystem.DispatchEvent("CLAIM_CHANGED", args);
                }
            }

            // Check distance for selected object - if too far away, unselect it
            if (selectedObject != null && buildingState != WorldBuildingState.MoveItem)
            {
                if (ClientAPI.GetPlayerObject() != null)
                {
                    if (Vector3.Distance(ClientAPI.GetPlayerObject().Position, selectedObject.transform.position) > deselectDistance)
                    {
                        SelectedObject = null;
                    }
                }
                else
                {
                    SelectedObject = null;
                }
            }

            if (frameCount == 3 && objectsToLoad.Count > 0)
            {
                SpawnClaimObject(objectsToLoad[0]);
                objectsToLoad.RemoveAt(0);
            }
            frameCount++;
            if (frameCount > 3)
                frameCount = 0;
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "CLAIM_ADDED")
            {
                GameObject claim = GameObject.Find(eData.eventArgs[0]);
                if (claim != null)
                {
                    if (claimGameObjects.ContainsKey(eData.eventArgs[0]))
                        if (claimGameObjects[eData.eventArgs[0]] == null)
                            claimGameObjects.Remove(eData.eventArgs[0]);
                    if (!claimGameObjects.ContainsKey(eData.eventArgs[0]))
                        claimGameObjects.Add(eData.eventArgs[0], claim);
                    claim.SetActive(showClaims);
                }
            }
            else if (eData.eventType == "CLAIM_REMOVED")
            {
                //GameObject claim = GameObject.Find(eData.eventArgs[0]);
                if(claimGameObjects.ContainsKey(eData.eventArgs[0]))
                claimGameObjects.Remove(eData.eventArgs[0]);
            }
            else if (eData.eventType == "LOGGED_OUT")
            {
                foreach (Claim claim in claims)
                {
                    foreach (ClaimObject cObject in claim.claimObjects.Values)
                    {
                        Destroy(cObject.gameObject);
                    }
                }
                claims.Clear();
            }
        }

        public bool InsideClaimArea(Claim claim, Vector3 point)
        {
            if (InRange(point.x, claim.loc.x - claim.sizeX / 2, claim.loc.x + claim.sizeX / 2) &&
                InRange(point.z, claim.loc.z - claim.sizeZ / 2, claim.loc.z + claim.sizeZ / 2))
            {
                return true;
            }
            return false;
        }

        bool InRange(float val, float min, float max)
        {
            return ((val >= min) && (val <= max));
        }

        public Claim GetClaim(int claimID)
        {
            foreach (Claim claim in claims)
            {
                if (claim.id == claimID)
                    return claim;
            }
            return null;
        }


        #region Message Senders
        public void CreateClaim(string name, int size, bool playerOwned, bool forSale, int currencyID, long cost , int ct)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("loc", ClientAPI.GetPlayerObject().Position);
            props.Add("name", name);
            props.Add("size", size);
            props.Add("owned", playerOwned);
            props.Add("forSale", forSale);
            props.Add("cost", (int)cost);
            props.Add("claimType", ct);
            props.Add("currency", currencyID);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.CREATE_CLAIM", props);
        }

        public void AddPermission(string playerName, int permissionLevel)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("claimID", activeClaim.id);
            props.Add("playerName", playerName);
            props.Add("action", "Add");
            props.Add("permissionLevel", permissionLevel);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.CLAIM_PERMISSION", props);
            //ClientAPI.Write("Sent add permission message");
        }

        public void RemovePermission(string playerName)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("claimID", activeClaim.id);
            props.Add("playerName", playerName);
            props.Add("action", "Remove");
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.CLAIM_PERMISSION", props);
            //ClientAPI.Write("Sent remove permission message");
        }

        public void SendEditClaim()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("claimID", activeClaim.id);
            props.Add("name", activeClaim.name);
            props.Add("forSale", activeClaim.forSale);
            props.Add("cost", activeClaim.cost);
            props.Add("currency", activeClaim.currency);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.EDIT_CLAIM", props);
            //ClientAPI.Write("Sent edit claim message");
        }

        public void PurchaseClaim()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("claimID", activeClaim.id);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.PURCHASE_CLAIM", props);
            //ClientAPI.Write("Sent purchase claim message");
        }

        public void SendPlaceClaimObject(int buildObjectID, AtavismInventoryItem item, Transform transform, int parent)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("claim", activeClaim.id);
            props.Add("loc", transform.position);
            props.Add("orient", transform.rotation);
            props.Add("parent", parent);

            if (item != null)
            {
                props.Add("buildObjectTemplateID", buildObjectID);
                props.Add("itemID", item.templateId);
                props.Add("itemOID", item.ItemId);
            }
            else
            {
                props.Add("buildObjectTemplateID", buildObjectID);
                props.Add("itemID", -1);
                props.Add("itemOID", null);
            }

            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.PLACE_CLAIM_OBJECT", props);
        }

        public void RequestClaimObjectInfo(int claimObjectID)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("claimID", activeClaim.id);
            props.Add("objectID", claimObjectID);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.GET_CLAIM_OBJECT_INFO", props);
        }

        public void SendEditObjectPosition(ClaimObject cObject, int parent)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("action", "save");
            props.Add("claimID", activeClaim.id);
            props.Add("objectID", cObject.ID);
            props.Add("loc", cObject.gameObject.transform.position);
            props.Add("orient", cObject.gameObject.transform.rotation);
            props.Add("parent", parent);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.EDIT_CLAIM_OBJECT", props);
        }

        public void ImproveBuildObject(GameObject objectBeingEdited, AtavismInventoryItem item, int count)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("claimID", activeClaim.id);
            props.Add("objectID", objectBeingEdited.GetComponent<ClaimObject>().ID);
            props.Add("itemID", item.templateId);
            props.Add("itemOID", item.ItemId);
            props.Add("count", count);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.UPGRADE_BUILDING_OBJECT", props);
        }

        public void ImproveBuildObject()
        {
            if (selectedObject == null)
                return;
            if (itemsForUpgradeMustBeInserted && (itemsPlacedToUpgrade.Count == 0  || selectedObject.ItemReqs.Count > itemsPlacedToUpgrade.Count))
                return;
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("claimID", activeClaim.id);
            props.Add("objectID", selectedObject.ID);
            props.Add("itemsCount", itemsPlacedToUpgrade.Count);
            for (int i = 0; i < itemsPlacedToUpgrade.Count; i++)
            {
                props.Add("itemID" + i, itemsPlacedToUpgrade[i].templateId);
                props.Add("itemOID" + i, itemsPlacedToUpgrade[i].ItemId);
                props.Add("count" + i, itemsPlacedToUpgrade[i].Count);
            }
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.UPGRADE_BUILDING_OBJECT", props);

            Debug.Log("Sending " + itemsPlacedToUpgrade.Count + " items");
        }

        public void PickupClaimObject()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("action", "convert");
            props.Add("claimID", activeClaim.id);
            props.Add("objectID", SelectedObject.ID);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.EDIT_CLAIM_OBJECT", props);
            BuildingState = WorldBuildingState.Standard;
            WorldBuilderInterface.Instance.ClearCurrentReticle(false);
        }
        #endregion Message Senders

        public void StartPlaceClaimObject(UGUIAtavismActivatable activatable)
        {
            AtavismInventoryItem item = (AtavismInventoryItem)activatable.ActivatableObject;
            StartPlaceClaimObject(item);
        }

        public void StartPlaceClaimObject(AtavismInventoryItem item)
        {
            if (activeClaim == null)
                return;
            string[] args = new string[1];
            args[0] = item.ItemId.ToString();
            AtavismEventSystem.DispatchEvent("PLACE_CLAIM_OBJECT", args);
        }

        void SpawnClaimObject(ClaimObjectData objectData)
        {
            // Add the gameObject to the claim
            Claim claim = GetClaim(objectData.claimID);
            if (claim == null)
            {
                AtavismLogger.LogWarning("No Claim found for Claim Object");
                return;
            }

            if (claim.claimObjects.ContainsKey(objectData.objectID))
            {
                return;
            }
            // Spawn the object in the world
            string prefabName = objectData.prefabName;
            int resourcePathPos = prefabName.IndexOf("Resources/");
            prefabName = prefabName.Substring(resourcePathPos + 10);
            prefabName = prefabName.Remove(prefabName.Length - 7);
            GameObject prefab = (GameObject)Resources.Load(prefabName);
            if (prefab == null)
            {
                Debug.LogError("Builder prefab " + prefabName + " not found in the resources");
                return;
            }

            GameObject claimObject = (GameObject)UnityEngine.Object.Instantiate(prefab, objectData.loc + claim.loc, objectData.orient);
            // TEMP: set claim object to don't delete on load
            DontDestroyOnLoad(claimObject);
            // Get the Claim Object Component
            ClaimObject cObject = claimObject.GetComponentInChildren<ClaimObject>();
            if (cObject == null)
            {
                cObject = claimObject.AddComponent<ClaimObject>();
            }
            cObject.ClaimID = objectData.claimID;
            cObject.StateUpdated(objectData.state);
            cObject.ID = objectData.objectID;
            cObject.TemplateID = objectData.templateID;
            cObject.Health = objectData.health;
            cObject.MaxHealth = objectData.maxHealth;
            cObject.Complete = objectData.complete;
            //cObject.ItemReqs = objectData.
            claim.claimObjects.Add(objectData.objectID, cObject);

            if (cObject.ID == selectedID)
            {
                SelectedObject = cObject;
            }
        }

        public void AttackClaimObject(GameObject obj)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("claimID", obj.GetComponent<ClaimObject>().ClaimID);
            props.Add("objectID", obj.GetComponent<ClaimObject>().ID);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.ATTACK_BUILDING_OBJECT", props);
            //ClientAPI.Write("Sent attack claim object");
        }

        public List<AtavismBuildObjectTemplate> GetBuildObjectsOfCategory(int category, bool nonItemsOnly)
        {
            List<AtavismBuildObjectTemplate> buildObjects = new List<AtavismBuildObjectTemplate>();
            foreach (AtavismBuildObjectTemplate template in buildObjectTemplates.Values)
            {
                if ((template.category == category || category == -1) && (!nonItemsOnly || !template.onlyAvailableFromItem))
                {
                    buildObjects.Add(template);
                }
            }

            return buildObjects;
        }

        #region Message Handlers

        /// <summary>
        /// Handles the Claim Action Message from the server. Passes the data onto the voxel editor.
        /// </summary>
        /// <param name="props">Properties.</param>
        public void ClaimObjectMessage(Dictionary<string, object> props)
        {
          //  Debug.LogError("ClaimObjectMessage ");
            try
            {
                int objectID = (int)props["id"];
                int templateID = (int)props["templateID"];
                string prefabName = (string)props["gameObject"];
                Vector3 loc = (Vector3)props["loc"];
                Quaternion orient = (Quaternion)props["orient"];
                int claimID = (int)props["claimID"];
                //string state = (string)props["state"];

                AtavismLogger.LogDebugMessage("Got claim object: " + gameObject);
                //SpawnClaimObject(objectID, claimID, prefabName, loc, orient);
                ClaimObjectData objectData = new ClaimObjectData();
                objectData.objectID = objectID;
                objectData.claimID = claimID;
                objectData.templateID = templateID;
                objectData.prefabName = prefabName;
                objectData.loc = loc;
                objectData.orient = orient;
                objectData.state = "";
                objectData.health = (int)props["health"];
                objectData.maxHealth = (int)props["maxHealth"];
                objectData.complete = (bool)props["complete"];
                objectsToLoad.Add(objectData);
            }
            catch (System.Exception e )
            {

                Debug.LogError("ClaimObjectMessage Exception=" + e);
            }
          //  Debug.LogError("ClaimObjectMessage objectsToLoad=" + objectsToLoad.Count);
        }

        /// <summary>
        /// Handles the Claim Action Bulk Message which is used to transfer large amounts of actions at once.
        /// </summary>
        /// <param name="props">Properties.</param>
        public void ClaimObjectBulkMessage(Dictionary<string, object> props)
        {
          //  Debug.LogError("ClaimObjectBulkMessage ");
            try
            {
                int numObjects = (int)props["numObjects"];
                AtavismLogger.LogDebugMessage("Got numObjects: " + numObjects);
                for (int i = 0; i < numObjects; i++)
                {
                    string actionString = (string)props["object_" + i];
                    string[] actionData = actionString.Split(';');
                    string objectID = actionData[0];
                    string templateID = actionData[1];
                    string gameObject = actionData[2];
                    string[] locData = actionData[3].Split(',');
                //    Debug.LogError("ClaimObjectBulkMessage Loc=>"+string.Join("|",locData) +"< string="+ actionData[3]);
                    Vector3 loc = new Vector3(float.Parse(locData[0]), float.Parse(locData[1]), float.Parse(locData[2]));
                    string[] normalData = actionData[4].Split(',');
                 //   Debug.LogError("ClaimObjectBulkMessage Rot=>" + string.Join("|", normalData) + "< string=" + actionData[4]);
                    Quaternion orient = new Quaternion(float.Parse(normalData[0]), float.Parse(normalData[1]), float.Parse(normalData[2]), float.Parse(normalData[3]));
                    string state = actionData[5];
                    int health = int.Parse(actionData[6]);
                    int maxHealth = int.Parse(actionData[7]);
                    bool complete = bool.Parse(actionData[8]);
                    //SpawnClaimObject(int.Parse(objectID), int.Parse(claimID), gameObject, loc, orient);
                    ClaimObjectData objectData = new ClaimObjectData();
                    objectData.objectID = int.Parse(objectID);
                    objectData.templateID = int.Parse(templateID);
                    objectData.claimID = (int)props["claimID"];
                    objectData.prefabName = gameObject;
                    objectData.loc = loc;
                    objectData.orient = orient;
                    objectData.state = state;
                    objectData.health = health;
                    objectData.maxHealth = maxHealth;
                    objectData.complete = complete;
                    objectsToLoad.Add(objectData);
                }
            }
            catch (System.Exception e)
            {

                Debug.LogError("ClaimObjectBulkMessage Exception=" + e);
            }
         //   Debug.LogError("ClaimObjectBulkMessage objectsToLoad=" + objectsToLoad.Count);
        }

        public void MoveClaimObjectMessage(Dictionary<string, object> props)
        {
            int objectID = (int)props["id"];
            Vector3 loc = (Vector3)props["loc"];
            Quaternion orient = (Quaternion)props["orient"];
            int claimID = (int)props["claimID"];

            Claim claim = GetClaim(claimID);
            if (claim != null)
            {
                claim.claimObjects[objectID].transform.position = loc + claim.loc;
                claim.claimObjects[objectID].transform.rotation = orient;
            }

            //Debug.Log("Got claim object: " + gameObject);
            //SpawnClaimObject(objectID, claimID, prefabName, loc, orient);
        }

        public void UpdateClaimObjectStateMessage(Dictionary<string, object> props)
        {
            int objectID = (int)props["id"];
            string state = (string)props["state"];
            int claimID = (int)props["claimID"];

            Claim claim = GetClaim(claimID);
            if (claim != null)
            {
                claim.claimObjects[objectID].StateUpdated(state);
            }

            //Debug.Log("Got claim object: " + gameObject);
            //SpawnClaimObject(objectID, claimID, prefabName, loc, orient);
        }

        public void ClaimObjectInfoMessage(Dictionary<string, object> props)
        {
            itemsPlacedToUpgrade.Clear();
            int objectID = (int)props["id"];
            int claimID = (int)props["claimID"];
            int health = (int)props["health"];
            int maxHealth = (int)props["maxHealth"];
            int itemCount = (int)props["itemCount"];
            bool complete = (bool)props["complete"];

            Claim claim = GetClaim(claimID);
            if (claim != null)
            {
                if (!claim.claimObjects.ContainsKey(objectID))
                {
                    return;
                }
                claim.claimObjects[objectID].Health = health;
                claim.claimObjects[objectID].MaxHealth = maxHealth;
                claim.claimObjects[objectID].Complete = complete;
                Dictionary<int, int> itemReqs = new Dictionary<int, int>();
                for (int i = 0; i < itemCount; i++)
                {
                    itemReqs.Add((int)props["item" + i], (int)props["itemCount" + i]);
                }
                claim.claimObjects[objectID].ItemReqs = itemReqs;

                // Work out how much time is left in the current task
                long timeCompleted = (long)props["timeCompleted"];
                if (timeCompleted > 0)
                {
                    long timeLeft = (long)props["timeLeft"];
                    float secondsCompleted = (float)timeCompleted / 1000f;
                    float secondsLeft = (float)timeLeft / 1000f;
                    string[] csArgs = new string[2];
                    csArgs[0] = secondsCompleted.ToString();
                    csArgs[1] = secondsLeft.ToString();
                    AtavismEventSystem.DispatchEvent("GROWING_STARTED", csArgs);

                }
                else
                {
                    string[] csArgs = new string[2];
                    csArgs[0] = "0";
                    csArgs[1] = "0";
                    AtavismEventSystem.DispatchEvent("GROWING_STARTED", csArgs);
                }
            }

            // Dispatch event to be picked up by UI
            string[] args = new string[1];
            args[0] = "";
            AtavismEventSystem.DispatchEvent("CLAIM_OBJECT_UPDATED", args);
        }

        public void RemoveClaimObjectMessage(Dictionary<string, object> props)
        {
            int objectID = (int)props["id"];
            int claimID = (int)props["claimID"];

            Claim claim = GetClaim(claimID);
            if (claim != null)
            {
                if (selectedObject != null && objectID == selectedObject.ID)
                {
                    selectedID = objectID;
                    selectedObject = null;
                    // Dispatch event to be picked up by UI
                    string[] args = new string[1];
                    args[0] = "";
                    AtavismEventSystem.DispatchEvent("CLAIM_OBJECT_UPDATED", args);
                }
                Destroy(claim.claimObjects[objectID].gameObject);
                claim.claimObjects.Remove(objectID);
            }
        }

        public void ClaimIDMessage(Dictionary<string, object> props)
        {
            int claimID = (int)props["claimID"];
         //   Debug.LogWarning("WorldBuilder Got Cliam id="+claimID);
            Claim claim = new Claim();
            if (GetClaim(claimID) != null)
            {
                claim = GetClaim(claimID);
            }
            else
            {
                claim.id = claimID;
            }

            claim.name = (string)props["claimName"];
            claim.sizeX = (int)props["claimSizeX"];
            claim.sizeZ = (int)props["claimSizeZ"];
            claim.loc = (Vector3)props["claimLoc"];
            claim.GenerateBounds();
            claim.claimType = (ClaimType)props["claimType"];
            claim.ownerName = (string)props["ownerName"];
            claim.forSale = (bool)props["forSale"];
            claim.permissionlevel = (int)props["permissionLevel"];
            if (claim.forSale)
            {
                claim.cost = (int)props["cost"];
                claim.currency = (int)props["currency"];
            }
            claim.purchaseItemReq = (int)props["purchaseItemReq"];
            claim.playerOwned = (bool)props["myClaim"];
            // Player permissions
            claim.permissions.Clear();
            int permissionCount = (int)props["permissionCount"];
            for (int i = 0; i < permissionCount; i++)
            {
                ClaimPermission per = new ClaimPermission();
                per.playerName = (string)props["permission_" + i];
                per.permissionLevel = (int)props["permissionLevel_" + i];
                claim.permissions.Add(per);
            }

            if (GetClaim(claimID) == null)
            {
               // Debug.LogWarning("WorldBuilder add to Cliam list id=" + claim.id);
                claims.Add(claim);
            }

            if (claim == activeClaim)
            {
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("CLAIM_CHANGED", args);
            }
            AtavismLogger.LogDebugMessage("Got new claim data: " + claim.id);
        }

        public void ClaimRemoveDataMessage(Dictionary<string, object> props)
        {
            int claimID = (int)props["claimID"];
            if (GetClaim(claimID) != null)
            {
                claims.Remove(GetClaim(claimID));
            }
            AtavismLogger.LogDebugMessage("Removed claim data: " + claimID);
        }

        public void ClaimUpdatedMessage(Dictionary<string, object> props)
        {
            int claimID = (int)props["claimID"];
            Claim claim = GetClaim(claimID);
            claim.forSale = (bool)props["forSale"];
            if (claim.forSale)
            {
                claim.cost = (int)props["cost"];
                claim.currency = (int)props["currency"];
            }
            claim.playerOwned = (bool)props["myClaim"];
            claim.purchaseItemReq = (int)props["purchaseItemReq"];
            AtavismLogger.LogDebugMessage("Got claim update data");
            if (claim == activeClaim)
            {
                string[] args = new string[1];
                AtavismEventSystem.DispatchEvent("CLAIM_CHANGED", args);
            }
        }

        /// <summary>
        /// Handles the Remove Claim Message which means a player is no longer in the radius for a claim
        /// so the client no longer needs to check if they are in its edit radius.
        /// </summary>
        /// <param name="props">Properties.</param>
        public void RemoveClaimMessage(Dictionary<string, object> props)
        {
            AtavismLogger.LogDebugMessage("Got remove claim data "+ claims.Count);
            try
            {
                int claimID = (int)props["claimID"];
             //   Debug.LogWarning("WorldBuilder Got Cliam to remove id="+claimID);
                Claim claimToRemove = null;
                foreach (Claim claim in claims)
                {
                    if (claim.id == claimID)
                    {
                     //   Debug.LogWarning("WorldBuilder Cliam to remove id=" + claimID);
                        /*int itemID = (int)props["resource"];
                                 int count = (int)props["resourceCount"];
                                 claim.resources[itemID] = count;*/
                        foreach (ClaimObject cObject in claim.claimObjects.Values)
                        {
                          //  Debug.LogWarning("WorldBuilder Cliam to remove id=" + claimID+" COid"+cObject.ID+" name="+ cObject.name);
                            DestroyImmediate(cObject.gameObject);
                        }
                        claimToRemove = claim;
                        break;
                    }
                }
                if (claimToRemove != null)
                {
                    if (claimToRemove == activeClaim)
                    {
                        activeClaim = null;
                        string[] args = new string[1];
                        AtavismEventSystem.DispatchEvent("CLAIM_CHANGED", args);
                    }
                    claims.Remove(claimToRemove);
                    if (claimGameObjects.ContainsKey("Claim" + claimID))
                    {
                        DestroyImmediate(claimGameObjects["Claim" + claimID]);
                        claimGameObjects.Remove("Claim" + claimID);
                    }
                }

            }
            catch (System.Exception e)
            {
                AtavismLogger.LogError("Got remove claim data Exception "+e.Message+" "+e);
            }
            AtavismLogger.LogDebugMessage("Got remove claim data end " + claims.Count);
        }

        /// <summary>
        /// Temporary hack to remove the claim deed item
        /// </summary>
        /// <param name="props">Properties.</param>
        public void ClaimMadeMessage(Dictionary<string, object> props)
        {
            // Something to be doing?

        }

        public void HandleBuildingResources(Dictionary<string, object> props)
        {
            buildingResources.Clear();
            int numResources = (int)props["numResources"];
            for (int i = 0; i < numResources; i++)
            {
                string resourceID = (string)props["resource" + i + "ID"];
                int resourceCount = (int)props["resource" + i + "Count"];
                buildingResources.Add(int.Parse(resourceID), resourceCount);
            }
            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("RESOURCE_UPDATE", args);
        }

        public void HandleStartBuildObject(Dictionary<string, object> props)
        {
            // Make sure player is in a claim they own
            if (activeClaim == null)
                return;

            // Get props and send event out to start the placement of the object
            int buildObjectTemplate = (int)props["buildObjectTemplate"];
            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            args[0] = "" + buildObjectTemplate;
            AtavismEventSystem.DispatchEvent("START_BUILD_CLAIM_OBJECT", args);
        }

        public void HandleStartBuildTask(Dictionary<string, object> props)
        {
            //ClientAPI.Write("Starting build task with length: " + (float)props["length"]);
            float length = (float)props["length"];
            string[] csArgs = new string[2];
            csArgs[0] = length.ToString();
            if (selectedObject == null || GetBuildObjectTemplate(selectedObject.TemplateID).buildTaskReqPlayer)
            {
                csArgs[1] = OID.fromLong(ClientAPI.GetPlayerOid()).ToString();
                AtavismEventSystem.DispatchEvent("CASTING_STARTED", csArgs);
            }
            else
            {
                csArgs = new string[2];
                csArgs[0] = "0";
                csArgs[1] = length.ToString();
                AtavismEventSystem.DispatchEvent("GROWING_STARTED", csArgs);
            }
        }

        public void HandleInterruptBuildTask(Dictionary<string, object> props)
        {
            int objectID = (int)props["id"];
            int claimID = (int)props["claimID"];

            // What do we do now?
            string[] args = new string[2];
            args[0] = "";
            args[1] = OID.fromLong(ClientAPI.GetPlayerOid()).ToString();
            if (selectedObject == null || GetBuildObjectTemplate(selectedObject.TemplateID).buildTaskReqPlayer)
            {
                AtavismEventSystem.DispatchEvent("CASTING_CANCELLED", args);
            }
            else
            {
                AtavismEventSystem.DispatchEvent("GROWING_CANCELLED", args);
            }

            ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismMobController>().PlayAnimation("", 0);
        }

        #endregion Message Handlers

        public void StartPlaceBuildObject(int buildObjectTemplateID)
        {
            // Make sure player is in a claim they own
            if (activeClaim == null)
                return;

            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            args[0] = "" + buildObjectTemplateID;
            AtavismEventSystem.DispatchEvent("START_BUILD_CLAIM_OBJECT", args);
        }

        public AtavismBuildObjectTemplate GetBuildObjectTemplate(int templateID)
        {
            if (!buildObjectTemplates.ContainsKey(templateID))
            {
                Debug.LogError("Build Object Template not found " + templateID);
                return null;
            }

            return buildObjectTemplates[templateID];
        }

        public int GetBuildingMaterialCount(int materialID)
        {
            if (buildingResources.ContainsKey(materialID))
            {
                return buildingResources[materialID];
            }
            else
            {
                return 0;
            }
        }

        public void ClaimAppeared(GameObject claim)
        {
            claimGameObjects.Add(claim.name,claim);
            claim.SetActive(showClaims);
        }

        public void ClaimRemoved(GameObject claim)
        {
            claimGameObjects.Remove(claim.name);
        }

        public void AddItemPlacedForUpgrade(AtavismInventoryItem item)
        {
            itemsPlacedToUpgrade.Add(item);
        }

        public void RemoveItemPlacedForUpgrade(AtavismInventoryItem item)
        {
            itemsPlacedToUpgrade.Remove(item);
        }

        #region Properties
        public static WorldBuilder Instance
        {
            get
            {
                return instance;
            }
        }

        public List<Claim> Claims
        {
            get
            {
                return claims;
            }
        }

        public Claim ActiveClaim
        {
            get
            {
                return activeClaim;
            }
        }

        public bool ShowClaims
        {
            get
            {
                return showClaims;
            }
            set
            {
                showClaims = value;
                foreach (GameObject claim in claimGameObjects.Values)
                {
                    if (claim != null)
                    {
                        claim.SetActive(showClaims);
                    }
                }
            }
        }

        public WorldBuildingState BuildingState
        {
            get
            {
                return buildingState;
            }
            set
            {
                buildingState = value;
            }
        }

        public Dictionary<int, AtavismBuildObjectTemplate> BuildObjectTemplates
        {
            get
            {
                return buildObjectTemplates;
            }
        }

        public ClaimObject SelectedObject
        {
            get
            {
                return selectedObject;
            }
            set
            {
                if (selectedObject != null)
                {
                    selectedObject.ResetHighlight();
                }
                if (buildingState == WorldBuildingState.EditItem && selectedObject != value)
                {
                    buildingState = WorldBuildingState.None;
                }

                selectedObject = value;
                if (selectedObject != null)
                {
                    selectedObject.Highlight();
                    RequestClaimObjectInfo(selectedObject.ID);
                    if (buildingState == WorldBuildingState.SelectItem)
                    {
                        buildingState = WorldBuildingState.EditItem;
                    }

                    // Dispatch event to be picked up by UI
                    string[] args = new string[1];
                    args[0] = "";
                    AtavismEventSystem.DispatchEvent("CLAIM_OBJECT_SELECTED", args);
                }
            }
        }

        #endregion Properties

        public const int PERMISSION_INTERACTION = 1;
        public const int PERMISSION_ADD_ONLY = 2;
        public const int PERMISSION_ADD_DELETE = 3;
        public const int PERMISSION_ADD_USERS = 4;
        public const int PERMISSION_MANAGE_USERS = 5;
        public const int PERMISSION_OWNER = 6;
    }
}
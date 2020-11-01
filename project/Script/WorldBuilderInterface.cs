using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public enum MouseWheelBuildMode
    {
        Rotate,
        MoveVertical
    }

    /// <summary>
    /// Interface class for World Builder. Contains the functions for placing and moving world building objects.
    /// If you want to change how players make their buildings, this is the class you will want to change.
    /// </summary>
    public class WorldBuilderInterface : MonoBehaviour
    {

        static WorldBuilderInterface instance;

        GameObject currentReticle;
        ClaimObject currentClaimObject;
        Bounds currentReticleBounds;
        private Vector3 hitPoint;
        private Vector3 hitNormal;
        private GameObject hitObject;
        private RaycastHit hit;
        private Vector3 moveObjectOrgPosition;
        private Quaternion moveObjectOrgRotation;

        AtavismInventoryItem itemBeingPlaced;
        AtavismBuildObjectTemplate buildTemplate;
        //string buildObjectGameObject = "";

        public bool snap = true;
        public float snapGap = 0.5f;
        public float rotationAmount = 90;
        public MouseWheelBuildMode mouseWheelBuildMode = MouseWheelBuildMode.Rotate;
        public float verticalMovementAmount = 0.5f;
        public float baseOverTerrainThreshold = 1f; // How high base objects can be placed over the terrain
        public LayerMask collisionPlacementLayer;
        public LayerMask objectSelectLayers;
        public CoordinatedEffect placementCoordEffect;
        float currentVerticalOffset = 0f;
        ClaimObject hoverObject = null;
        //	bool snapped = false;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            // Register for 
            AtavismEventSystem.RegisterEvent("PLACE_CLAIM_OBJECT", this);
            AtavismEventSystem.RegisterEvent("CLAIM_OBJECT_CLICKED", this);
            AtavismEventSystem.RegisterEvent("START_BUILD_CLAIM_OBJECT", this);
            AtavismEventSystem.RegisterEvent("CLAIM_CHANGED", this);
        }

        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("PLACE_CLAIM_OBJECT", this);
            AtavismEventSystem.UnregisterEvent("CLAIM_OBJECT_CLICKED", this);
            AtavismEventSystem.UnregisterEvent("START_BUILD_CLAIM_OBJECT", this);
            AtavismEventSystem.UnregisterEvent("CLAIM_CHANGED", this);
        }

        // Update is called once per frame
        void Update()
        {
            if (AtavismCursor.Instance == null || AtavismCursor.Instance.IsMouseOverUI())
                return;
            if (WorldBuilder.Instance.ActiveClaim != null)
                if (WorldBuilder.Instance.ActiveClaim.permissionlevel < 1 && !WorldBuilder.Instance.ActiveClaim.playerOwned)
                {
                    return;
                }
            if (GetBuildingState() == WorldBuildingState.SelectItem)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                // Casts the ray and get the first game object hit
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, objectSelectLayers))
                {
                    ClaimObject cObject = hit.transform.gameObject.GetComponent<ClaimObject>();
                    if (cObject == null)
                        cObject = hit.transform.gameObject.GetComponentInChildren<ClaimObject>();
                    if (cObject == null && hit.transform.parent != null)
                        cObject = hit.transform.parent.GetComponent<ClaimObject>();
                    if (cObject != null)
                    {
                        int objectID = cObject.ID;
                        if (WorldBuilder.Instance.ActiveClaim.claimObjects.ContainsKey(objectID))
                        {
                            if (hoverObject != cObject)
                            {
                                if (hoverObject != null)
                                {
                                    hoverObject.ResetHighlight();
                                }
                                hoverObject = cObject;
                                cObject.Highlight();
                            }
                            if (Input.GetMouseButtonDown(0))
                            {
                                StopSelectObject();
                                //SetBuildingState(WorldBuildingState.EditItem);
                                //objectBeingEdited = activeClaim.claimObjects[objectID].gameObject;
                                WorldBuilder.Instance.RequestClaimObjectInfo(objectID);
                                cObject.Highlight();
                                WorldBuilder.Instance.SelectedObject = cObject;
                            }
                        }
                    }
                    else if (hoverObject != null)
                    {
                        hoverObject.ResetHighlight();
                        hoverObject = null;
                    }
                }
                else if (hoverObject != null)
                {
                    hoverObject.ResetHighlight();
                    hoverObject = null;
                }
            }

            if ((GetBuildingState() == WorldBuildingState.PlaceItem || GetBuildingState() == WorldBuildingState.MoveItem) && currentReticle != null)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    ClearCurrentReticle(true);
                    WorldBuilder.Instance.BuildingState = WorldBuildingState.None;
                    return;
                }

                GetHitLocation();
                bool legalPlacement = IsLegalClaimObjectPlacement();
                if (!legalPlacement)
                {
                    currentReticle.GetComponentInChildren<ClaimObject>().HighlightError();
                }
                else
                {
                    currentReticle.GetComponentInChildren<ClaimObject>().ResetHighlight();
                }

                float delta = Input.GetAxis("Mouse ScrollWheel");
                if (mouseWheelBuildMode == MouseWheelBuildMode.Rotate)
                {
                    Vector3 currentRotation = currentReticle.transform.rotation.eulerAngles;
                    if (delta > 0)
                    {
                        //currentReticle.transform.Rotate (new Vector3 (0, -rotationAmount, 0));
                        currentRotation.y -= 90;
                    }
                    else if (delta < 0)
                    {
                        //currentReticle.transform.Rotate (new Vector3 (0, rotationAmount, 0));
                        currentRotation.y += 90;
                    }
                    Quaternion rotation = Quaternion.Euler(currentRotation);
                    currentReticle.transform.rotation = rotation;
                }
                else if (mouseWheelBuildMode == MouseWheelBuildMode.MoveVertical)
                {
                    if (delta > 0)
                    {
                        if (currentClaimObject.verticalSnapDistance > 0)
                        {
                            currentVerticalOffset += currentClaimObject.verticalSnapDistance;
                        }
                        else
                        {
                            currentVerticalOffset += verticalMovementAmount;
                        }
                    }
                    else if (delta < 0)
                    {
                        if (currentClaimObject.verticalSnapDistance > 0)
                        {
                            currentVerticalOffset -= currentClaimObject.verticalSnapDistance;
                        }
                        else
                        {
                            currentVerticalOffset -= verticalMovementAmount;
                        }
                    }
                }

                //snapped = false;
                if ((currentReticle != null) && (WorldBuilder.Instance.ActiveClaim != null) &&
                    (WorldBuilder.Instance.InsideClaimArea(WorldBuilder.Instance.ActiveClaim, hitPoint)))
                {

                    if (snap)
                    {
                        float newX = Mathf.Round(hitPoint.x * (1 / snapGap)) / (1 / snapGap);
                        float newZ = Mathf.Round(hitPoint.z * (1 / snapGap)) / (1 / snapGap);
                        float newY = hitPoint.y + currentVerticalOffset;
                        // If the claimobject has a horizontal snap value, override default snap values
                        if (currentClaimObject.horizontalSnapDistance > 0)
                        {
                            newX = Mathf.Round(hitPoint.x * (1 / currentClaimObject.horizontalSnapDistance)) / (1 / currentClaimObject.horizontalSnapDistance);
                            newZ = Mathf.Round(hitPoint.z * (1 / currentClaimObject.horizontalSnapDistance)) / (1 / currentClaimObject.horizontalSnapDistance);
                        }
                        // If the claimobject has a vertical snap value, set that as well
                        if (currentClaimObject.verticalSnapDistance > 0)
                        {
                            newY = Mathf.Round((hitPoint.y + currentVerticalOffset) * (1 / currentClaimObject.verticalSnapDistance)) / (1 / currentClaimObject.verticalSnapDistance);
                        }
                        hitPoint = new Vector3(newX, newY, newZ);
                        //	snapped = true;
                    }

                    if (currentClaimObject.placementType == BuildObjectPlacementType.Terrain && hitObject != null
                        && (hitObject.GetComponent<ClaimObject>() != null || hitObject.GetComponentInParent<ClaimObject>() != null))
                    {
                        // Custom snap system to line them up with each other
                        // Get size of block
                        Vector3 baseSize = GetSizeOfBase(currentReticle).size;
                        // Move block along z axis by size / 2, set y to same as hit object
                        if (hitNormal == Vector3.back)
                        {
                            hitPoint = hitObject.transform.position;
                            hitPoint.z -= baseSize.z;
                            //	snapped = true;
                        }
                        else if (hitNormal == Vector3.forward)
                        {
                            hitPoint = hitObject.transform.position;
                            hitPoint.z += baseSize.z;
                            //	snapped = true;
                        }
                        else if (hitNormal == Vector3.right)
                        {
                            hitPoint = hitObject.transform.position;
                            hitPoint.x += baseSize.x;
                            //	snapped = true;
                        }
                        else if (hitNormal == Vector3.left)
                        {
                            hitPoint = hitObject.transform.position;
                            hitPoint.x -= baseSize.x;
                            //	snapped = true;
                        }

                        // Check terrain height
                        if (baseOverTerrainThreshold > 0)
                        {
                            float height = Terrain.activeTerrain.SampleHeight(hitPoint) + Terrain.activeTerrain.GetPosition().y;
                            //Debug.Log("Hitpoint y = " + hitPoint.y + " with terrain height: " + height);
                            if (hitPoint.y - baseOverTerrainThreshold > height)
                            {
                                legalPlacement = false;
                                currentClaimObject.HighlightError();
                                Debug.Log("Too high above terrain");
                            }
                        }
                    }
                    currentReticle.transform.position = hitPoint;

                    if (currentClaimObject.autoRotateToHitNormal)
                    {
                        //Rotate the object to match normals
                        Vector3 currentRotation = currentReticle.transform.rotation.eulerAngles;
                        if (hitNormal == Vector3.back)
                        {
                            currentRotation.y = 180;
                            //currentReticle.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
                            //hitPoint = new Vector3(newX, newY, hitPoint.z);
                        }
                        else if (hitNormal == Vector3.forward)
                        {
                            currentRotation.y = 0;
                        }
                        else if (hitNormal == Vector3.right)
                        {
                            currentRotation.y = 90;
                        }
                        else if (hitNormal == Vector3.left)
                        {
                            currentRotation.y = -90;
                        }
                        Quaternion rotation = Quaternion.Euler(currentRotation);
                        currentReticle.transform.rotation = rotation;
                    }
                }

                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    if (mouseWheelBuildMode == MouseWheelBuildMode.MoveVertical)
                    {
                        mouseWheelBuildMode = MouseWheelBuildMode.Rotate;
#if AT_I2LOC_PRESET
                    ClientAPI.Write(I2.Loc.LocalizationManager.GetTranslation("Changed to Rotate Mode"));
#else
                        ClientAPI.Write("Changed to Rotate Mode");
#endif
                    }
                    else
                    {
                        mouseWheelBuildMode = MouseWheelBuildMode.MoveVertical;
#if AT_I2LOC_PRESET
                    ClientAPI.Write(I2.Loc.LocalizationManager.GetTranslation("Changed to Move Vertical Mode"));
#else
                        ClientAPI.Write("Changed to Move Vertical Mode");
#endif
                    }
                }

                if (Input.GetMouseButtonDown(0) && !AtavismUiSystem.IsMouseOverFrame())
                {
                    if (!legalPlacement)
                    {
                        string[] args = new string[1];
#if AT_I2LOC_PRESET
                    args[0] = I2.Loc.LocalizationManager.GetTranslation("That object cannot be placed there");
#else
                        args[0] = "That object cannot be placed there";
#endif
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                    }
                    else if (GetBuildingState() == WorldBuildingState.PlaceItem)
                    {
                        int parent = -1;
                        if (currentClaimObject.canHaveParent && hitObject.GetComponentInParent<ClaimObject>() != null)
                            parent = hitObject.GetComponentInParent<ClaimObject>().ID;

                        WorldBuilder.Instance.SendPlaceClaimObject(buildTemplate.id, itemBeingPlaced, currentReticle.transform, parent);
                        // Play a coord effect
                        if (placementCoordEffect != null)
                        {
                            Dictionary<string, object> props = new Dictionary<string, object>();
                            props["gameObject"] = ClientAPI.GetPlayerObject().GameObject;
                            CoordinatedEffectSystem.ExecuteCoordinatedEffect(placementCoordEffect.name, props);
                        }

#if AT_I2LOC_PRESET
                    ClientAPI.Write(I2.Loc.LocalizationManager.GetTranslation("Send place claim object message"));
#else
                        ClientAPI.Write("Send place claim object message");
#endif
                        ClearCurrentReticle(true);
                        itemBeingPlaced = null;
                        SetBuildingState(WorldBuildingState.Standard);
                        AtavismCursor.Instance.ClearItemClickedOverride(WorldBuilder.Instance.StartPlaceClaimObject);
                    }
                    else if (GetBuildingState() == WorldBuildingState.MoveItem)
                    {
                        int parent = -1;
                        if (hitObject.GetComponent<ClaimObject>() != null)
                            parent = hitObject.GetComponent<ClaimObject>().ID;
                        WorldBuilder.Instance.SendEditObjectPosition(WorldBuilder.Instance.SelectedObject, parent);
                        //SetBuildingState(WorldBuildingState.EditItem);
                        SetBuildingState(WorldBuildingState.Standard);
                        ClearCurrentReticle(false);
                        itemBeingPlaced = null;
                        //objectBeingEdited.GetComponent<ClaimObject>().ResetHighlight();
                        //objectBeingEdited = null;
                    }

                }
            }
        }

        public Bounds GetSizeOfBase(GameObject basePrefab)
        {
            // First find a center for your bounds.
            Collider collider = basePrefab.GetComponent<Collider>();
            if (!collider.enabled)
            {
                collider.enabled = true;
                Bounds bounds = collider.bounds;
                collider.enabled = false;
                return bounds;
            }
            else
            {
                return collider.bounds;
            }
        }

        bool GetHitLocation()
        {
            //Debug.Log("GetHitLocation");
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            bool hitSomething = Physics.Raycast(ray, out hit, 200f, collisionPlacementLayer);
            if (hitSomething)
            {
                hitObject = hit.collider.gameObject;
            }
            else
            {
                hitObject = null;
            }

            if (WorldBuilder.Instance.ActiveClaim != null && ((WorldBuilder.Instance.ActiveClaim.sizeX == 0) ||
                    WorldBuilder.Instance.InsideClaimArea(WorldBuilder.Instance.ActiveClaim, hit.point)))
            {
                hitNormal = hit.normal;
                hitPoint = hit.point;
                return hitSomething;
            }
            else
            {
                return false;
            }
        }

        public bool IsLegalClaimObjectPlacement()
        {
            // First check if the objects bounds are fully encapsulated within the claim
            currentReticleBounds = new Bounds(currentReticle.transform.position, Vector3.zero);
            Renderer[] colliders = currentReticle.GetComponents<Renderer>();
            if (currentClaimObject.placementMeshes.Count > 0)
            {
                foreach (Renderer rend in currentClaimObject.placementMeshes)
                {
                    currentReticleBounds.Encapsulate(rend.bounds);
                }
            }
            else
            {
                foreach (Renderer col in colliders)
                {
                    currentReticleBounds.Encapsulate(col.bounds);
                }
                colliders = currentReticle.GetComponentsInChildren<Renderer>();
                foreach (Renderer col in colliders)
                {
                    currentReticleBounds.Encapsulate(col.bounds);
                }
            }
            if (!WorldBuilder.Instance.ActiveClaim.IsObjectFullyInsideClaim(currentReticleBounds))
            {
                Debug.Log("Placement failed due to object not fully inside claim");
                return false;
            }

            //Debug.Log("Intersected objects count: " + ClientAPI.ScriptObject.GetComponent<WorldBuilder>().IntersectedObjects.Count);
            // Check if the placement is intersecting any other objects
            /*if (ClientAPI.ScriptObject.GetComponent<WorldBuilder>().IntersectedObjects.Count > 0) {
                Debug.Log("Placement failed due to intersecting another object. Count = " + ClientAPI.ScriptObject.GetComponent<WorldBuilder>().IntersectedObjects.Count);
                //foreach(Collider collider in ClientAPI.ScriptObject.GetComponent<WorldBuilder>().IntersectedObjects) {
                //	Debug.Log("Intersecting another object: " + collider.name);
                //}
                return false;
            }*/
            // Check if the hit point is on another object and if it allows children and that this object wants a parent
            if (hitObject != null)
            {
                ClaimObject cObject = hitObject.GetComponent<ClaimObject>();
                if (cObject == null && currentClaimObject.requiresParent)
                {
                    if (hitObject.GetComponent<ClaimObjectChild>() == null)
                    {
                        Debug.Log("Placement failed due to no claimobject script on hit object");
                        return false;
                    }
                }
                if (cObject != null)
                {
                    if (cObject.placementType == BuildObjectPlacementType.Terrain && currentClaimObject.placementType == BuildObjectPlacementType.Terrain)
                    {
                        return true;
                    }
                    if (!currentClaimObject.canHaveParent)
                    {
                        Debug.Log("Placement failed due to not allowed a parent");
                        return false;
                    }
                    if (!cObject.allowChildren)
                    {
                        Debug.Log("Placement failed due to parent not allowing children");
                        return false;
                    }
                }
            }
            else if (currentClaimObject.requiresParent)
            {
                Debug.Log("Placement failed due to no hit object");
                return false;
            }

            // Does the object intersect with any existing claim objects
            foreach (ClaimObject cObject in WorldBuilder.Instance.ActiveClaim.claimObjects.Values)
            {
                if (cObject == null || cObject.gameObject == currentReticle)
                    continue;
                if (cObject.blockingCollisionVolumes != null && cObject.blockingCollisionVolumes.Count > 0)
                {
                    foreach (Collider c in cObject.blockingCollisionVolumes)
                    {
                        if (c != null)
                        {
                            if (c.bounds.Intersects(currentReticleBounds))
                            {
                                Debug.Log("Placement failed due to intersecting another object");
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    foreach (Collider c in cObject.gameObject.GetComponentsInChildren<Collider>())
                    {
                        if (c.bounds.Intersects(currentReticleBounds))
                        {
                            Debug.Log("Placement failed due to intersecting another object");
                            return false;
                        }
                    }
                }
            }

            //if (buildTemplate.objectType == "Base") {
            //	return true;
            //}
            // Check if the placement type is valid
            /*if (snapped) {
                Debug.Log ("Got snapped, so returning true");
                return true;
            }*/

            if (currentClaimObject.placementType == BuildObjectPlacementType.Terrain
                || currentClaimObject.placementType == BuildObjectPlacementType.TerrainOrFloor)
            {
                if (hitObject != null && hitObject.GetComponent<Terrain>())
                {
                    return true;
                }
            }

            if (currentClaimObject.placementType == BuildObjectPlacementType.Floor
                || currentClaimObject.placementType == BuildObjectPlacementType.TerrainOrFloor)
            {
                if (hitNormal == Vector3.up)
                    return true;
            }
            if (currentClaimObject.placementType == BuildObjectPlacementType.Ceiling)
            {
                if (hitNormal == Vector3.down)
                    return true;
            }
            if (currentClaimObject.placementType == BuildObjectPlacementType.Wall)
            {
                if (hitNormal == Vector3.back || hitNormal == Vector3.forward || hitNormal == Vector3.right || hitNormal == Vector3.left)
                    return true;
            }
            //Debug.Log("Placement failed due to invalid placement location");
            return false;
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "PLACE_CLAIM_OBJECT")
            {
                //if (GetBuildingState() != WorldBuildingState.PlaceItem)
                //	return;

                OID itemOID = OID.fromString(eData.eventArgs[0]);
                itemBeingPlaced = Inventory.Instance.GetInventoryItem(itemOID);
                // Does the item have a ClaimObject effect?
                List<int> effectPositions = itemBeingPlaced.GetEffectPositionsOfTypes("ClaimObject");
                if (effectPositions.Count == 0)
                {
                    itemBeingPlaced = null;
                    return;
                }
                else
                {
                    SetBuildingState(WorldBuildingState.PlaceItem);
                    int buildObjectID = int.Parse(itemBeingPlaced.itemEffectValues[effectPositions[0]]);
                    buildTemplate = WorldBuilder.Instance.GetBuildObjectTemplate(buildObjectID);

                    string prefabName = buildTemplate.gameObject.Remove(0, 17);
                    prefabName = prefabName.Remove(prefabName.Length - 7);
                    // Create an instance of the game Object
                    GameObject prefab = (GameObject)Resources.Load(prefabName);
                    GetHitLocation();
                    SetCurrentReticle((GameObject)UnityEngine.Object.Instantiate(prefab, hitPoint, prefab.transform.rotation));
                }
            }
            else if (eData.eventType == "START_BUILD_CLAIM_OBJECT")
            {
                int buildTemplateID = int.Parse(eData.eventArgs[0]);
                buildTemplate = WorldBuilder.Instance.GetBuildObjectTemplate(buildTemplateID);
                Debug.Log("Checking claim type: " + WorldBuilder.Instance.ActiveClaim.claimType + " against: " + buildTemplate.validClaimTypes);
                // Verify the claim type matches
                ClaimType activeClaimType = WorldBuilder.Instance.ActiveClaim.claimType;
                if (activeClaimType != ClaimType.Any && buildTemplate.validClaimTypes != ClaimType.Any)
                {
                    if (activeClaimType != buildTemplate.validClaimTypes)
                    {
                        string[] args = new string[1];
#if AT_I2LOC_PRESET
                    args[0] = I2.Loc.LocalizationManager.GetTranslation("That object cannot be placed in this Claim");
#else
                        args[0] = "That object cannot be placed in this Claim";
#endif
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                        return;
                    }
                }
                SetBuildingState(WorldBuildingState.PlaceItem);
                itemBeingPlaced = null;
                StartPlaceBuildObject(buildTemplateID);
            }
            else if (eData.eventType == "CLAIM_OBJECT_CLICKED")
            {
                int objectID = int.Parse(eData.eventArgs[0]);
                if (GetBuildingState() == WorldBuildingState.SelectItem && WorldBuilder.Instance.ActiveClaim.claimObjects.ContainsKey(objectID))
                {
                    SetCurrentReticle(WorldBuilder.Instance.ActiveClaim.claimObjects[objectID].gameObject);
                }
            }
            else if (eData.eventType == "CLAIM_CHANGED")
            {
                ClearCurrentReticle(true);
                WorldBuilder.Instance.BuildingState = WorldBuildingState.None;
            }
        }

        public void StartPlaceBuildObject(int buildTemplateID)
        {
            buildTemplate = WorldBuilder.Instance.GetBuildObjectTemplate(buildTemplateID);
            if (buildTemplate == null)
                return;
            ClearCurrentReticle(true);
            string prefabName = buildTemplate.gameObject;
            prefabName = prefabName.Remove(0, 17);
            prefabName = prefabName.Remove(prefabName.Length - 7);
            // Create an instance of the game Object
            GameObject prefab = (GameObject)Resources.Load(prefabName);
            SetBuildingState(WorldBuildingState.PlaceItem);
            GetHitLocation();
            SetCurrentReticle((GameObject)UnityEngine.Object.Instantiate(prefab, hitPoint, prefab.transform.rotation));
        }

        public void SetCurrentReticle(GameObject obj)
        {
            //Debug.LogError("Building SetCurrentReticle");
            if (currentReticle != null)
                DestroyImmediate(currentReticle);
            currentReticle = obj;
            if (GetBuildingState() == WorldBuildingState.MoveItem)
            {
                moveObjectOrgPosition = obj.transform.position;
                moveObjectOrgRotation = obj.transform.rotation;
            }

            currentClaimObject = obj.GetComponentInChildren<ClaimObject>();
            // Generate Bounds before turning colliders off
            currentReticleBounds = new Bounds(currentReticle.transform.position, Vector3.zero);
            Collider[] colliders = currentReticle.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                currentReticleBounds.Encapsulate(col.bounds);
                col.enabled = false;
            }
            colliders = currentReticle.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                currentReticleBounds.Encapsulate(col.bounds);
                col.enabled = false;
            }
            // Disable mouse wheel scroll
            ClientAPI.GetInputController().MouseWheelDisabled = true;
        }

        public void ClearCurrentReticle(bool destroyObj)
        {
         //   Debug.LogError("Building ClearCurrentReticle destroyObj=" + destroyObj);
            if (currentReticle == null)
                return;
            if (GetBuildingState() == WorldBuildingState.MoveItem)
            {
                currentReticle.transform.position = moveObjectOrgPosition;
                currentReticle.transform.rotation = moveObjectOrgRotation;
                currentReticle.GetComponentInChildren<ClaimObject>().ResetHighlight();
                destroyObj = false;
            }
            if (destroyObj)
            {
                Destroy(currentReticle);
            }
            else if (currentReticle != null)
            {
                Collider[] colliders = currentReticle.GetComponents<Collider>();
                foreach (Collider col in colliders)
                    col.enabled = true;
                colliders = currentReticle.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                    col.enabled = true;
            }

            currentReticle = null;
            if (ClientAPI.GetInputController() != null)
            {
                ClientAPI.GetInputController().MouseWheelDisabled = false;
            }
        }

        public void StartSelectObject()
        {
            ClearCurrentReticle(true);
            if (WorldBuilder.Instance.ActiveClaim != null && WorldBuilder.Instance.ActiveClaim.playerOwned)
            {
                SetBuildingState(WorldBuildingState.SelectItem);
                foreach (ClaimObject cObject in WorldBuilder.Instance.ActiveClaim.claimObjects.Values)
                {
                    cObject.Active = true;
                    cObject.ResetHighlight();
                }
            }
        }

        void StopSelectObject()
        {
            foreach (ClaimObject cObject in WorldBuilder.Instance.ActiveClaim.claimObjects.Values)
            {
                cObject.Active = false;
            }
        }

        /// <summary>
        /// Gets the BuildingState from the World Builder
        /// </summary>
        /// <returns>The building state.</returns>
        WorldBuildingState GetBuildingState()
        {
            return WorldBuilder.Instance.BuildingState;
        }

        /// <summary>
        /// Tells the WorldBuilder to update the BuildingState
        /// </summary>
        /// <param name="newState">New state.</param>
        void SetBuildingState(WorldBuildingState newState)
        {
            WorldBuilder.Instance.BuildingState = newState;
        }

        public static WorldBuilderInterface Instance
        {
            get
            {
                return instance;
            }
        }

        public MouseWheelBuildMode MouseWheelBuildMode
        {
            get
            {
                return mouseWheelBuildMode;
            }
            set
            {
                mouseWheelBuildMode = value;
            }
        }
    }
}
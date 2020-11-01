/*
 * See below for credits to original camera
 * Code based on existing Atavism Target, but changed to reflect tab targeting (custom index, starts at closest then tabs outwards)
 * Also adds soft targeting, which borrows heavily from the shooter camera for determining aim points, then uses atavism traits to
 * obtain OIDs and assign the target to the player
 */

/*
 * USAGE NOTES
 * In Atavism3dPersonInput, make these public:
 * 		Make mouseLookLocked public (can hide in inspector)
 * 		Make cameraTargetOffset public
 * 		Make cameraFirstPerson public (but hide in inspector)
 * 
 * In Atavism3rdPersonInput, add a public SDETargeting variable
 * In its Start method, check if the variable is null
 * if it isn't, create an SDETargeting component and add it to the Atavism3rdPersonInput's game object
 * 
 * Additionally, the UpdateVisualCursor method must be changed (just one part) so soft targeting can hide the default cursor
 * You must add 
 * || sdeTarget.softTargetMode 
 * to the if statement so it is properly hidden.
 * 
 * Original:
 * ----------------------------------------------------------------
 * if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) )
 * ----------------------------------------------------------------
 * 
 * 
 * 
 * Example Start Code:
 * ===================================================================
 * 	sdeTarget = camera.gameObject.GetComponent<SDETargeting> ();
	if (sdeTarget == null) {
		sdeTarget = camera.gameObject.AddComponent<SDETargeting> ();
		sdeTarget.A3PI = this;
	}
	===================================================================
 */ 

// Camera code is from this Project:
// http://unity3d.com/support/resources/example-projects/3rdpersonshooter
// Thank you guys.

// Add this script to the main camera.
// In the inspector, there is a variable "player" - drag your player on it.
// Press Play!

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    // 3rd person game-like camera controller
    // keeps camera behind the player and aimed at aiming point
    public class SDETargeting : MonoBehaviour
    {

        Transform player;
        public KeyCode tabTargetKey;
        public GameObject crosshair; // crosshair - removed it for quick and easy setup. ben0bi
                                     // if you add the crosshair, you need to drag a crosshair texture on the "crosshair" variable in the inspector 

        //private float angleH = 0;
        //private float angleV = 0;
        private Transform cam;
        //	private float maxCamDist = 3;
        private LayerMask mask;
        public LayerMask targetableMask;
        //	private Vector3 smoothPlayerPos;

        public float maxTargetDistance = 60f;
        public float sphereCastRadius = 0.25f;
        private int tabIndex = 1;
        public Vector3 cameraOffset = new Vector3(.25f, .25f, 1.75f);
        private Vector3 oldOffset = new Vector3(0f, 0f, 0f);
        [HideInInspector] public Atavism3rdPersonInput A3PI;
        public bool softTargetMode = false;
        public bool LoseLock = false;  //If you want the target to be unset when you move off of the target, set this to true
        public float minZoomInDistance = 1f;
        public float maxZoomInDistance = 10f;
        float oldMinDistance;
        float oldMaxDistance;

        long lastTarget = -1;
        float lastSelectTarget = 0f;
        [SerializeField] float diffTimeResetTarget = 2f;
        [SerializeField] bool targetNeutral = false;
        [SerializeField] bool targetFrendly = false;

        // Use this for initialization
        void Start()
        {
            SetTarget(ClientAPI.GetPlayerObject().GameObject.transform);
            // [edit] no aimtarget gameobject needs to be placed anymore - ben0bi
            // Add player's own layer to mask
            //mask = 1;
            // Add Ignore Raycast layer to mask
            /*if (crosshair == null) {
                crosshair = Instantiate(Resources.Load("TargetPrefab/Crosshairs")) as GameObject;
                crosshair.transform.parent = transform;
                crosshair.transform.localPosition = new Vector3(0f, 0f, 0f);
            }*/

            mask |= 1 << LayerMask.NameToLayer("Ignore Raycast");
            // Invert mask
            mask = ~mask;

            cam = transform;
            //smoothPlayerPos = player.position;
        }

        public void Initiate()
        {
            oldOffset = A3PI.cameraTargetOffset;
            oldMaxDistance = A3PI.cameraMaxDist;
            oldMinDistance = A3PI.cameraMinDist;
        }

        void Update()
        {

            if (Input.GetKeyDown(tabTargetKey))
            {
                TabTarget();
                //AtavismTarget.TargetNearestEnemy();
            }/* else if (Input.GetKeyDown (KeyCode.Slash)) {
			//A3PI.mouseLookLocked = !A3PI.mouseLookLocked;
			softTargetMode = !softTargetMode;
			crosshair.renderer.enabled = softTargetMode;
		}*/

            //cameraFree = !cameraFree;
            if (softTargetMode && !A3PI.cameraFirstPerson)
            {
                A3PI.cameraTargetOffset = oldOffset + cameraOffset;
                A3PI.cameraMaxDist = maxZoomInDistance;
                A3PI.cameraMinDist = minZoomInDistance;
            }
            else
            {
                A3PI.cameraTargetOffset = oldOffset;
                A3PI.cameraMaxDist = oldMaxDistance;
                A3PI.cameraMinDist = oldMinDistance;
            }

        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (player == null && ClientAPI.GetPlayerObject() != null)
            {
                SetTarget(ClientAPI.GetPlayerObject().GameObject.transform);
            }

            if (Time.deltaTime == 0 || Time.timeScale == 0 || player == null || softTargetMode == false)
                return;

            //Debug.Log ("Doing work.");
            // Before changing camera, store the prev aiming distance.
            // If we're aiming at nothing (the sky), we'll keep this distance.
            float prevDist = (transform.position - cam.position).magnitude;

            // Find far and close position for the camera
            //	smoothPlayerPos = player.position;

            // Make sure camera doesn't intersect geometry
            // Move camera towards closeOffset if ray back towards camera position intersects something 
            RaycastHit hit;
            bool gotTarget = false;

            // Do a raycast from the camera to find the distance to the point we're aiming at.
            float aimTargetDist = 5;
            float skipZone = (cam.position - player.transform.position).magnitude;
            if (Physics.Raycast(new Ray(cam.position + cam.forward * skipZone, cam.forward), out hit, maxTargetDistance, targetableMask))
            {
                Debug.DrawLine(cam.position, hit.point, Color.red, 3f);
                gotTarget = UpdateTarget(hit);
            }
            else if (Physics.SphereCast(cam.position, sphereCastRadius, cam.forward, out hit, maxTargetDistance, targetableMask) && hit.collider.transform.name != player.name)
            {
                Debug.DrawLine(cam.position + cam.forward * skipZone, hit.point, Color.red, 3f);
                gotTarget = UpdateTarget(hit);
            }

            if (!gotTarget)
            {
                // If we're aiming at nothing, keep prev dist but make it at least 5.
                aimTargetDist = Mathf.Max(5, Mathf.Max(aimTargetDist, prevDist));

                //Debug.DrawLine (cam.position, cam.position + cam.forward * aimTargetDist, Color.blue, 3f);
                //crosshair.transform.position = cam.position + cam.forward * aimTargetDist;
                if (LoseLock)
                {
                    AtavismClient.Instance.WorldManager.TargetId = -1;
                    lastTarget = -1;
                }
            }
        }

        bool UpdateTarget(RaycastHit hit)
        {
            // Double check there is no obstacles in the way
            RaycastHit lineHit;
            float skipZone = (cam.position - player.transform.position).magnitude;
            if (Physics.Linecast(cam.position + cam.forward * skipZone, hit.point, out lineHit, mask))
            {
                if (lineHit.collider != hit.collider)
                    return false;
            }

            //Debug.Log (hit.collider.name);

            //	float aimTargetDist = hit.distance + 0.05f;
            //crosshair.transform.position = hit.point;

            var AN = hit.transform.gameObject.GetComponent<AtavismNode>();
            AtavismObjectNode worldObj = null;
            AtavismMobNode mobObj = null;

            if (AN != null)
                worldObj = ClientAPI.WorldManager.GetObjectNode(AN.Oid);

            if (worldObj != null)
                mobObj = worldObj as AtavismMobNode;
            if (mobObj != null && mobObj.PropertyExists("health"))
            {
                var health = (int)mobObj.GetProperty("health");
                if (health > 0)
                {
                    if (mobObj.Oid != lastTarget)
                    {
                        MobSoundSet soundSet = mobObj.GameObject.GetComponent<MobSoundSet>();
                        if (soundSet != null)
                        {
                            soundSet.PlaySoundEvent(MobSoundEvent.Response);
                        }
                        lastTarget = mobObj.Oid;
                        AtavismClient.Instance.WorldManager.TargetId = mobObj.Oid;
                    }


                }
            }
            else
            {
                if (LoseLock)
                {
                    AtavismClient.Instance.WorldManager.TargetId = -1;
                    lastTarget = -1;
                }
            }
            return true;
        }

        public void TabTarget()
        {
            Debug.Log("Running tab target");
            List<AtavismObjectNode> worldObjects = new List<AtavismObjectNode>();
            List<long> worldObjOIDs = ClientAPI.WorldManager.GetObjectOidList();
            foreach (long worldObjOID in worldObjOIDs)
            {
                AtavismObjectNode worldObj = ClientAPI.WorldManager.GetObjectNode(worldObjOID);
                if (worldObj != null && worldObj.Oid != ClientAPI.GetPlayerOid() &&
                    worldObj.PropertyExists("attackable") && worldObj.CheckBooleanProperty("attackable") && worldObj.PropertyExists("reaction") &&
                    (((int)worldObj.GetProperty("reaction") < 0) || ((int)worldObj.GetProperty("reaction") == 0 && targetNeutral) || ((int)worldObj.GetProperty("reaction") == 1 && targetFrendly)))
                {
                    var mobObj = worldObj as AtavismMobNode;
                    if (mobObj != null)
                        if (mobObj.PropertyExists("health"))
                        {
                            var health = (int)mobObj.GetProperty("health");
                            if (health > 0)
                                worldObjects.Add(worldObj);
                        }
                }
            }
            if (worldObjects.Count > 0)
                worldObjects = TestVisible(worldObjects, maxTargetDistance);

            if (AtavismClient.Instance.WorldManager.TargetId == -1)
            {
                tabIndex = 0;
            }
            else if (tabIndex < worldObjects.Count)
            {
                if (Time.time > lastSelectTarget + diffTimeResetTarget)
                    tabIndex = 0;
                else
                    tabIndex++;
            }
            if (tabIndex >= worldObjects.Count)
            {
                tabIndex = 0;
            }
            lastSelectTarget = Time.time;
            AtavismClient.Instance.WorldManager.TargetId = TabTargetNearestEnemy(tabIndex, worldObjects);

        }

        public static List<AtavismObjectNode> TestVisible(List<AtavismObjectNode> entities, float maxDistance)
        {
            Camera cam = Camera.main;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
            for (int i = entities.Count - 1; i >= 0; i--)
            {

                if (entities[i] == null || entities[i].GameObject == null)
                {
                    entities.RemoveAt(i);
                }
                else if (entities[i].GameObject.GetComponentInChildren<Renderer>() == null)
                {
                    entities.RemoveAt(i);
                }
                else if (!GeometryUtility.TestPlanesAABB(planes, entities[i].GameObject.GetComponentInChildren<Renderer>().bounds))
                {
                    entities.RemoveAt(i);
                }
                else if (Vector3.Distance(cam.transform.position, entities[i].Position) > maxDistance)
                {
                    entities.RemoveAt(i);
                }
            }
            return entities;
        }

        //Target the tabIndex enemy if they are visible, sorted by distance	
        public static long TabTargetNearestEnemy(int tabIndex, List<AtavismObjectNode> wo)
        {
            List<AtavismObjectNode> worldObjects = wo;
            Vector3 playerPos = ClientAPI.GetPlayerObject().Position;
            worldObjects.Sort((AtavismObjectNode x, AtavismObjectNode y) =>
            {
                Vector3 distanceA = x.Position - playerPos;
                Vector3 distanceB = y.Position - playerPos;

                if (distanceA.sqrMagnitude > distanceB.sqrMagnitude)
                {
                    return 1;
                }
                else if (distanceB.sqrMagnitude > distanceA.sqrMagnitude)
                {
                    return -1;
                }

                return 0;
            });
            //worldObjects.sort((lambda x, y: _DistanceComparerHelper(playerPos, x, y)), false);
            // I now have a list of world objects, sorted based on their distance from the player
            if (tabIndex < worldObjects.Count)
            {
                return worldObjects[tabIndex].Oid;
            }
            return -1;
        }

        // so you can change the camera from a static observer (level loading) or something else
        // to your player or something else. I needed that for network init... ben0bi
        public void SetTarget(Transform t)
        {
            player = t;
            //foreach (Transform tt in player.gameObject.GetComponentsInChildren<Transform>()){
            t.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            //}
        }

    }
}
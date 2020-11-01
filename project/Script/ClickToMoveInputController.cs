using UnityEngine;
using System.Collections;

namespace Atavism
{

    public class ClickToMoveInputController : AtavismInputController
    {
        public LayerMask layerMask;
        public LayerMask ignoreLayers;

        public bool allowKeyMovement = true;
        public float targetRadius = 2f;
        private bool isControllable = true;
        private Vector3 playerAccel;
        private Vector3 position;
        private GameObject clickedTarget;
        private float distanceToStopAt = 0.25f;
        private bool sentStrike = false;
        private float activateTime = -1f;
        public float harvestDelay = 0.5f;

        public static bool attack;
        public static bool die;

        public float minDistance = 5f;
        public float maxDistance = 20f;
        //public float minHeight = 5f;
        //public float maxHeight = 20f;
        // The distance in the x-z plane to the target
        public float distance = 10.0f;
        // the height we want the camera to be above the target
        public float height = 8.0f;
        float heightDif = 0f;
        // How much we 
        float heightDamping = 2.0f;
        public float MouseWheelVelocity = -1.0f;
        public Vector3 cameraRotation = Vector3.forward;

        // target indicator
        public ParticleSystem particleIndicator;
        public float particleLife = 1f;
        public float particleYOffset = 0.2f;
        GameObject currentParticle = null;
        float particleExpiration = -1f;

        // Use this for initialization
        void Start()
        {
            // Need to tell the client that this is the new active input controller
            ClientAPI.InputControllerActivated(this);

            // Set start point to players current location
            position = ClientAPI.GetPlayerObject().Position;

            heightDif = distance - height;
        }

        // Update is called once per frame
        void Update()
        {
            if (particleExpiration != -1 && Time.time > particleExpiration)
            {
                DestroyImmediate(currentParticle);
                particleExpiration = -1;
            }
            if (target == null)
                return;
            UpdateCamera();
        }

        public void TargetClicked(GameObject target)
        {
            //clickedTarget = target;
        }

        public override Vector3 GetPlayerMovement()
        {
            if (allowKeyMovement)
            {
                HandleImmediateKeys(UnityEngine.Time.deltaTime, UnityEngine.Time.time);
                if (playerAccel != Vector3.zero)
                {
                    target.transform.rotation = Quaternion.LookRotation(playerAccel);
                    position = target.position;
                    clickedTarget = null;
                    return playerAccel;
                }
            }

            if (target == null)
                return Vector3.zero;

            if (!attack && !die)
            {
                if (Input.GetMouseButton(0) && !AtavismCursor.Instance.IsMouseOverUI() && !AtavismCursor.Instance.UguiIconBeingDragged)
                {
                    //Locate where the player clicked on the terrain
                    LocatePosition();
                }
                else if (Input.GetMouseButtonUp(0) && !AtavismCursor.Instance.IsMouseOverUI() && !AtavismCursor.Instance.UguiIconBeingDragged)
                {
                    //Locate where the player clicked on the terrain
                    LocateTarget();
                }

                if (!AtavismCursor.Instance.IsMouseOverUI())
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (currentParticle != null)
                        {
                            DestroyImmediate(currentParticle);
                        }
                        if (particleIndicator != null)
                        {
                            Vector3 particlePosition = position;
                            if (clickedTarget != null)
                            {
                                particlePosition = clickedTarget.transform.position;
                            }
                            currentParticle = (GameObject)Instantiate(particleIndicator.gameObject, particlePosition + new Vector3(0, particleYOffset, 0), Quaternion.identity);
                            particleExpiration = Time.time + particleLife;
                        }
                    }

                    if (distance >= minDistance && distance <= maxDistance)
                    {
                        float mult = Mathf.Max(.1f, distance);
                        float d = MouseWheelVelocity * mult * Input.GetAxis("Mouse ScrollWheel");
                        //Debug.Log("Mousewheel: " + d + "; with velocity: " + MouseWheelVelocity + " and input: " + Input.GetAxis ("Mouse ScrollWheel"));
                        float newDistance = distance + d;
                        if (newDistance >= minDistance && newDistance <= maxDistance)
                        {
                            distance = newDistance;
                            height += d;
                        }


                        //distance = Mathf.Min (maxDistance, distance);
                        //distance = Mathf.Max (minDistance, distance);

                        //height = Mathf.Min (maxDistance, height);
                        //height = Mathf.Max (minDistance, height);
                    }

                    /*if (Mathf.Abs(distance - height) > Mathf.Abs(heightDif)) {
                        height -= heightDif; 
                    }*/
                }

                //Move the player to the position
                return moveToPosition();
            }
            else
            {
                return Vector3.zero;
            }
        }

        void LocatePosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, ~ignoreLayers))
            {
                if (hit.collider.GetComponent<ClickToMoveTargetableObject>())
                {
                    GameObject tempTarget = hit.collider.gameObject;
                    float distanceToStop = tempTarget.GetComponent<ClickToMoveTargetableObject>().distanceToStopAt;
                    if (Vector3.Distance(target.position, hit.transform.position) > distanceToStop + 0.25f)
                    {
                        return;
                    }
                }

                clickedTarget = null;
                distanceToStopAt = 0;
                ClientAPI.SetTarget(-1);
                NetworkAPI.SendAttackMessage(ClientAPI.GetTargetOid(), "strike", false);

                if (hit.collider.tag != "Player" && hit.collider.tag != "Enemy" && IsInLayerMask(hit.collider.gameObject, layerMask)
                   || clickedTarget != null)
                {
                    position = hit.point;
                }
            }
        }

        void LocateTarget()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, ~ignoreLayers))
            {
                // Make sure the target is far enough away to care
                if (hit.collider.GetComponent<ClickToMoveTargetableObject>() && hit.collider.gameObject != ClientAPI.GetPlayerObject().GameObject)
                {
                    float distanceToStop = hit.collider.gameObject.GetComponent<ClickToMoveTargetableObject>().distanceToStopAt;
                    TargetFound(hit.collider.gameObject, distanceToStop);
                }
                else
                {
                    // Check if there is a mob within 2m of the hit 
                    foreach (Collider col in Physics.OverlapSphere(hit.point, targetRadius, ~ignoreLayers))
                    {
                        if (col.gameObject != ClientAPI.GetPlayerObject().GameObject && col.gameObject.GetComponent<AtavismNode>() != null
                                && col.gameObject.GetComponent<AtavismNode>().GetProperty("attackable") != null)
                        {
                            float distanceToStop = GetDistanceToMob(col.gameObject);
                            TargetFound(col.gameObject, distanceToStop);
                            return;
                        }
                    }
                    clickedTarget = null;
                    distanceToStopAt = 0;
                }
            }
        }

        float GetDistanceToMob(GameObject target)
        {
            // Is the target attackable?
            if (((target.GetComponent<AtavismNode>().PropertyExists("targetType") && (int)target.GetComponent<AtavismNode>().GetProperty("targetType") < 1) ||
                  !target.GetComponent<AtavismNode>().PropertyExists("targetType"))
                  && (bool)target.GetComponent<AtavismNode>().GetProperty("attackable"))
            {
                // Work out range of auto attack ability
                int autoAbilityID = (int)ClientAPI.GetPlayerObject().GetProperty("combat.autoability");
                int distance = Abilities.Instance.GetAbility(autoAbilityID).distance;
                if (distance > 4)
                {
                    return distance;
                }
                else
                {
                    return target.GetComponent<ClickToMoveTargetableObject>().distanceToStopAt;
                }
            }
            else
            {
                // Go to the target
                return target.GetComponent<ClickToMoveTargetableObject>().distanceToStopAt;
            }
        }

        void TargetFound(GameObject newTarget, float distanceToStop)
        {
            if (newTarget.GetComponent<AtavismNode>() != null && newTarget.GetComponent<AtavismNode>().GetProperty("attackable") != null)
            {
                distanceToStop = GetDistanceToMob(newTarget);
                ClientAPI.SetTarget(newTarget.GetComponent<AtavismNode>().Oid);
            }
            if (Vector3.Distance(target.position, newTarget.transform.position) > distanceToStop + 0.25f)
            {
                clickedTarget = newTarget;
                distanceToStopAt = distanceToStop;
                sentStrike = false;
                activateTime = -1f;
            }
            else
            {
                sentStrike = false;
                activateTime = 1; // Set to 1 so it will trigger right away
                ActivateTarget(newTarget);
                position = target.position;
            }
        }

        private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
        {
            // Convert the object's layer to a bitfield for comparison
            int objLayerMask = (1 << obj.layer);
            if ((layerMask.value & objLayerMask) > 0)  // Extra round brackets required!
                return true;
            else
                return false;
        }

        Vector3 moveToPosition()
        {
            if (clickedTarget != null)
            {
                position = clickedTarget.transform.position;
                target.LookAt(clickedTarget.transform);
                Quaternion newRotation = target.rotation;
                newRotation.x = 0f;
                newRotation.z = 0f;
                target.rotation = newRotation;
            }
            Vector3 newPosition = position;
            newPosition.y = target.position.y;
            //Game Object is moving
            if (Vector3.Distance(target.position, newPosition) > (distanceToStopAt + 0.5f))
            {
                Quaternion newRotation = Quaternion.LookRotation(position - target.position);

                newRotation.x = 0f;
                newRotation.z = 0f;

                target.rotation = newRotation; //Quaternion.Slerp(target.rotation, newRotation, Time.deltaTime * 10);
                return target.forward;
            }
            //Game Object is not moving
            else
            {
                if (clickedTarget != null && !sentStrike)
                {
                    ActivateTarget(clickedTarget);
                }
                return Vector3.zero;
            }
        }

        void ActivateTarget(GameObject tempTarget)
        {
            if (tempTarget.GetComponent<AtavismNode>() != null
                && tempTarget.GetComponent<AtavismNode>().CheckBooleanProperty("attackable"))
            {
                // Send strike command
                NetworkAPI.SendAttackMessage(tempTarget.GetComponent<AtavismNode>().Oid, "strike", true);
                sentStrike = true;
            }
            else if (tempTarget.GetComponent<ClickToMoveTargetableObject>().activateTarget != null)
            {
                GameObject activateTarget = tempTarget.GetComponent<ClickToMoveTargetableObject>().activateTarget;
                if (activateTarget.GetComponent<ResourceNode>() != null)
                {
                    if (activateTime == -1)
                    {
                        activateTime = Time.time + harvestDelay;
                    }
                    else if (Time.time > activateTime)
                    {
                        activateTarget.GetComponent<ResourceNode>().HarvestResource();
                        sentStrike = true;
                        activateTime = -1f;
                    }
                }
                else if (activateTarget.GetComponent<CraftingStation>() != null)
                {
                    if (activateTime == -1)
                    {
                        activateTime = Time.time + harvestDelay;
                    }
                    else if (Time.time > activateTime)
                    {
                        activateTarget.GetComponent<CraftingStation>().ActivateCraftingStation();
                        sentStrike = true;
                        activateTime = -1f;
                    }
                }
            }
        }

        #region Keyboard Input
        protected enum MoveEnum : int
        {
            Left = 0,
            Right = 1,
            Forward = 2,
            Back = 3,
            Count = 4
        }

        protected bool[] movement = new bool[(int)MoveEnum.Count];

        public void MoveForward(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Forward] = true;
                movement[(int)MoveEnum.Back] = false;
            }
            else
            {
                movement[(int)MoveEnum.Forward] = false;
            }
        }

        public void MoveBackward(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Forward] = false;
                movement[(int)MoveEnum.Back] = true;
            }
            else
            {
                movement[(int)MoveEnum.Back] = false;
            }
        }

        public void TurnLeft(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Left] = true;
                movement[(int)MoveEnum.Right] = false;
            }
            else
            {
                movement[(int)MoveEnum.Left] = false;
            }
        }

        public void TurnRight(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Left] = false;
                movement[(int)MoveEnum.Right] = true;
            }
            else
            {
                movement[(int)MoveEnum.Right] = false;
            }
        }

        /// <summary>
        ///   Handle the keyboard and mouse input for movement of the player and camera.
        ///   This method name says immediate, but really it is acting on a keyboard state
        ///   that may have been filled in by buffered or immediate input.
        /// </summary>
        /// <param name="timeSinceLastFrame">This is supposed to be in milliseconds, but seems to be in seconds.</param>
        protected void HandleImmediateKeys(float timeSinceLastFrame, float now)
        {
            // Now handle movement and stuff

            // Ignore the input if we're in the loading state
            //if (Client.Instance.LoadingState)
            //	return;

            // reset acceleration zero
            playerAccel = Vector3.zero;

            if (isControllable && !ClientAPI.UIHasFocus())
            {
                // Check key states
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                    MoveForward(true);
                if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
                    MoveForward(false);
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                    MoveBackward(true);
                if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
                    MoveBackward(false);
                // Turning keys
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                    TurnLeft(true);
                if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
                    TurnLeft(false);
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                    TurnRight(true);
                if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
                    TurnRight(false);
            }

            // Apply acceleration
            if (ClientAPI.WorldManager.Player.CanMove())
            {
                if (movement[(int)MoveEnum.Forward])
                    playerAccel.z += 1.0f;
                if (movement[(int)MoveEnum.Back])
                    playerAccel.z -= 1.0f;
                if (movement[(int)MoveEnum.Right])
                    playerAccel.x += 1.0f;
                if (movement[(int)MoveEnum.Left])
                    playerAccel.x -= 1.0f;
            }
        }
        #endregion Keyboard Input

        #region Camera Movement
        public override void RunCameraUpdate()
        {
            UpdateCamera();
        }

        /// <summary>
        ///   Move the camera based on the new position of the camera target (player)
        /// </summary>
        /// <param name="playerPos"></param>
        /// <param name="playerOrient"></param>
        protected void UpdateCamera()
        {
            Camera camera = Camera.main;
            // Calculate the current rotation angles
            float wantedHeight = target.position.y + height;

            float currentHeight = camera.transform.position.y;

            // Damp the rotation around the y-axis

            // Damp the height
            //currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);
            currentHeight = wantedHeight;

            // Convert the angle into a rotation

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            camera.transform.position = target.position;
            camera.transform.position -= cameraRotation * distance;

            // Set the height of the camera
            camera.transform.position = new Vector3(camera.transform.position.x, currentHeight, camera.transform.position.z);

            // Always look at the target
            camera.transform.LookAt(target);
        }
        #endregion Camera Movement

        public bool IsControllable
        {
            get
            {
                return isControllable;
            }
            set
            {
                isControllable = value;
            }
        }

    }
}
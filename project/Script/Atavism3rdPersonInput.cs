using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Atavism
{
    public class Atavism3rdPersonInput : AtavismInputController
    {

        //public KeyCode walkToggleKey = KeyCode.Backslash;
        // Walk on
        public bool walk = false;
        // The speed when walking
        //	float walkSpeed = 2.0f;
        // when pressing "Fire3" button (cmd) we start running
        //	float runSpeed = 6.0f;
        Vector3 playerAccel = Vector3.zero;
        //	Quaternion playerRotation = Quaternion.identity;
        public float rotateSpeed = 5.0f;

        // Are we moving backwards (This locks the camera to not do a 180 degree spin)
        private bool movingBack = false;
        // Is the user pressing any keys?
        //private bool isMoving = false;

        private bool isControllable = true;

        // Last time the jump button was clicked down
        //	private float lastJumpButtonTime = -10.0f;
        // Last time we performed a jump
        //	private float lastJumpTime = -1.0f;

        #region Camera Fields
        public float MouseVelocity = 0.035f;
        public float MouseWheelVelocity = -1.0f;

        // Camera parameters - - exposed via the ParameterRegistry
        //	bool playerVisible = true;
        public bool cameraFirstPerson = false;
        bool cameraFree = false;
        //	bool cameraMotionYaw = true;

        private bool mouseOverUI = false;
        /// <summary>
        ///   This variable allows me to lock the interface to mouse look mode.
        ///   Essentially, this treats the interface as though the right mouse
        ///   button is down all the time for purposes of the camera updates.
        ///   The movement still follows 
        /// </summary>
        public bool mouseLookLocked = false;
        bool leftButtonDown = false;
        bool rightButtonDown = false;
        bool mouseRun = false;
        //	float minPlayerVisibleDistance = 2f;
        float minThirdPersonDistance = 1f;
        public float headHeightAbovePlayerOrigin = 1.8f;
        public Vector3 cameraTargetOffset;
        // cameraPosition is only used if cameraFree is true

        //	bool playerOrientationInitialized = false;
        Quaternion cameraOrientation;
        Vector3 cameraPosition = Vector3.zero;

        // Camera pitch (in degrees)
        public float maxPitch = 85.0f; //275.0f;
        public float minPitch = 275.0f; //85.0f;
        public float idealPitch = 20.0f;
        public float cameraDist = 5;
        public float cameraMaxDist = 20;
        public float cameraMinDist = 0;

        public LayerMask obstacleLayers = 0, groundLayers = 512;

        Vector3 mousePosition = Vector3.zero;

        public SDETargeting sdeTarget;

        #endregion Camera Fields

        //   bool moveForwardLeft = false;
        //   bool moveForwardRight = false;
        //   bool moveBackwardLeft = false;
        //   bool moveBackwardRight = false;
        bool mouseRightButtonClicked = false;
        bool mouseLeftButtonClicked = false;
        bool cameraFollowChar = true;
        public float rotateCameraSpeed = 80.0f;
        Vector3 forcePower = Vector3.zero;
        bool noMove = false;
        bool dead = false;
        string state = "";


        // Use this for initialization
        void Start()
        {
            // Need to tell the client that this is the new active input controller
            ClientAPI.InputControllerActivated(this);

            cameraTargetOffset = headHeightAbovePlayerOrigin * Vector3.up;
            //cameraOrientation = transform.rotation * Vector3.back;
            //cameraOrientation = Quaternion.AngleAxis (Mathf.Deg2Rad * 20.0f, Vector3.up);
            AtavismEventSystem.RegisterEvent("MOUSE_SENSITIVE", this);
            MouseVelocity = AtavismSettings.Instance.GetGeneralSettings().sensitivityMouse;
            MouseWheelVelocity = AtavismSettings.Instance.GetGeneralSettings().sensitivityWheelMouse;
            Camera camera = Camera.main;
            if (PlayerYaw >= 180)
                CameraYaw = this.PlayerYaw - 180.0f;
            else
                CameraYaw = this.PlayerYaw + 180.0f;
            Vector3 cameraDir = camera.transform.rotation * Vector3.forward;
            Vector3 cameraTarget = target.position + target.rotation * cameraTargetOffset;
            camera.transform.position = (cameraTarget + cameraDir * cameraDist);

            sdeTarget = gameObject.GetComponent<SDETargeting>();
            if (sdeTarget == null)
            {
                sdeTarget = gameObject.AddComponent<SDETargeting>();
            }

            if (sdeTarget != null)
            {
                sdeTarget.A3PI = this;
                sdeTarget.Initiate();
            }
        }
        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("MOUSE_SENSITIVE", this);

        }
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "MOUSE_SENSITIVE")
            {
                MouseVelocity = AtavismSettings.Instance.GetGeneralSettings().sensitivityMouse;
                MouseWheelVelocity = AtavismSettings.Instance.GetGeneralSettings().sensitivityWheelMouse;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (target == null)
                return;
            // Update camera
            UpdateCamera();
            UpdateCamera(target.position, target.rotation);

            // Mouse code
            if (ClientAPI.mouseLook && Cursor.lockState != CursorLockMode.Locked)
            {
                if (Vector3.Distance(Input.mousePosition, mousePosition) > 100)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        public override Vector3 GetPlayerMovement()
        {
            if (!isControllable)
            {
                // kill all inputs if not controllable.
                //Input.ResetInputAxes ();
            }

            // Check if mouse is over UI for this frame
            mouseOverUI = AtavismCursor.Instance.IsMouseOverUI();
            //mouseOverUI = AtavismUiSystem.IsMouseOverFrame ();

            //UpdateSmoothedMovementDirection ();
            HandleImmediateKeys(UnityEngine.Time.deltaTime, UnityEngine.Time.time);

            if (playerAccel.z < -0.2)
                movingBack = true;
            else
                movingBack = false;

            ClientAPI.GetPlayerObject().GameObject.SendMessage("MovingBack", movingBack);

            return (ClientAPI.GetPlayerObject().Orientation * playerAccel);
        }

        public override void RunCameraUpdate()
        {
            UpdateCamera();
            UpdateCamera(target.position, target.rotation);
        }

        #region Movement

        protected enum MoveEnum : int
        {
            Left = 0,
            Right = 1,
            Forward = 2,
            Back = 3,
            StrafeLeft = 4,
            StrafeRight = 5,
            Up = 6,
            Down = 7,
            AutoRun = 8,
            Count = 9
        }

        protected bool[] movement = new bool[(int)MoveEnum.Count];

        public void MoveForward(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Forward] = true;
                movement[(int)MoveEnum.Back] = false;
                movement[(int)MoveEnum.AutoRun] = false;
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
                movement[(int)MoveEnum.AutoRun] = false;
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

        public void StrafeLeft(bool status)
        {
            movement[(int)MoveEnum.StrafeLeft] = status;
        }

        public void StrafeRight(bool status)
        {
            movement[(int)MoveEnum.StrafeRight] = status;
        }

        public void MoveUp(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Up] = true;
                movement[(int)MoveEnum.Down] = false;
                movement[(int)MoveEnum.AutoRun] = false;
            }
            else
            {
                movement[(int)MoveEnum.Up] = false;
            }
        }

        public void MoveDown(bool status)
        {
            if (status)
            {
                movement[(int)MoveEnum.Up] = false;
                movement[(int)MoveEnum.Down] = true;
                movement[(int)MoveEnum.AutoRun] = false;
            }
            else
            {
                movement[(int)MoveEnum.Down] = false;
            }
        }

        public void ToggleAutorun()
        {
            movement[(int)MoveEnum.AutoRun] = !movement[(int)MoveEnum.AutoRun];
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
                /*			if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                                MoveForward(true);
                            if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
                                MoveForward(false);
                            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                                MoveBackward(true);
                            if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
                                MoveBackward(false);
                */
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().moveForward) || Input.GetKey(AtavismSettings.Instance.GetKeySettings().altMoveForward))
                    MoveForward(true);
                else
                    MoveForward(false);
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().moveBackward) || Input.GetKey(AtavismSettings.Instance.GetKeySettings().altMoveBackward))
                    MoveBackward(true);
                else
                    MoveBackward(false);

                // Strafing keys
                /*           if (Input.GetKeyDown(KeyCode.Q))
                               StrafeLeft(true);
                           if (Input.GetKeyUp(KeyCode.Q))
                               StrafeLeft(false);
                           if (Input.GetKeyDown(KeyCode.E))
                               StrafeRight(true);
                           if (Input.GetKeyUp(KeyCode.E))
                               StrafeRight(false);
               */
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().strafeLeft))
                    StrafeLeft(true);
                else
                    StrafeLeft(false);
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().strafeRight))
                    StrafeRight(true);
                else
                    StrafeRight(false);


                // Turning keys
                /*            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                                TurnLeft(true);
                            if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
                                TurnLeft(false);
                            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                                TurnRight(true);
                            if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
                                TurnRight(false);
                */
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().turnLeft) || Input.GetKey(AtavismSettings.Instance.GetKeySettings().altTurnLeft))
                    TurnLeft(true);
                else
                    TurnLeft(false);
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().turnRight) || Input.GetKey(AtavismSettings.Instance.GetKeySettings().altTurnRight))
                    TurnRight(true);
                else
                    TurnRight(false);

                // Auto run
                /*   if (Input.GetKeyDown(KeyCode.Numlock))
                       ToggleAutorun();
       */
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().autoRun))
                    ToggleAutorun();

                // Update character state
                /*     if (Input.GetKeyDown(walkToggleKey)) {
                         // Toggle walking state
                         walk = !walk;
                     }*/
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().walkRun))
                {
                    // Toggle walking state
                    walk = !walk;
                }

                ClientAPI.GetPlayerObject().GameObject.SendMessage("SetWalk", walk);
                /*	
                    if (Input.GetButtonDown ("Jump")) {
                        ClientAPI.GetPlayerObject().GameObject.SendMessage("StartJump");
                    }
                    */
                if (Input.GetKey(AtavismSettings.Instance.GetKeySettings().jump))
                {
                    ClientAPI.GetPlayerObject().GameObject.SendMessage("StartJump");
                }

            }
            else
            {
                MoveForward(false);
                MoveBackward(false);
                StrafeLeft(false);
                StrafeRight(false);
                TurnLeft(false);
                TurnRight(false);
            }

            // Mouse state
            if (Input.GetMouseButtonDown(0) && !mouseOverUI)
                leftButtonDown = true;
            if (Input.GetMouseButtonUp(0))
                leftButtonDown = false;
            if (Input.GetMouseButtonDown(1) && !mouseOverUI)
            {
                rightButtonDown = true;
            }
            if (Input.GetMouseButtonUp(1))
            {
                rightButtonDown = false;
            }

            if (leftButtonDown && rightButtonDown)
            {
                mouseRun = true;
            }
            else if (mouseRun)
            {
                mouseRun = false;
                movement[(int)MoveEnum.AutoRun] = false;
            }

            if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().PropertyExists("deadstate"))
            {
                dead = (bool)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().GetProperty("deadstate");
            }
            if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().PropertyExists("state"))
            {
                state = (string)ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().GetProperty("state");
            }

            // Apply acceleration
            if (ClientAPI.WorldManager.Player.CanMove() && (!dead || state == "spirit"))
            {
                if (movement[(int)MoveEnum.Forward] || movement[(int)MoveEnum.AutoRun] || mouseRun)
                    playerAccel.z += 1.0f;
                if (movement[(int)MoveEnum.Back])
                    playerAccel.z -= 1.0f;
                if (movement[(int)MoveEnum.StrafeLeft])
                    playerAccel.x -= 0.5f;
                if (movement[(int)MoveEnum.StrafeRight])
                    playerAccel.x += 0.5f;
                /*if (movement[(int)MoveEnum.Up])
                    playerAccel.y += 0.5f;
                if (movement[(int)MoveEnum.Down])
                    playerAccel.y -= 0.5f;*/

                if (playerAccel.z < -0.2)
                    movingBack = true;
                else
                    movingBack = false;
            }

            //log.DebugFormat ("HandleImmediateKeys: playerAccel = {0}", playerAccel);

            // If mouse2 (button1) is down, left and right are strafe.
            // Otherwise, they are rotation.
            if (ClientAPI.WorldManager.Player.CanMove() && (mouseLookLocked || (rightButtonDown && !mouseOverUI)) && (!dead || state == "spirit"))
            {
                // Apply the left and right as strafe
                if (movement[(int)MoveEnum.Left])
                    playerAccel.x -= 0.5f;
                if (movement[(int)MoveEnum.Right])
                    playerAccel.x += 0.5f;

                if (ClientAPI.WorldManager.Player.MobController.RotatingDirection != 0)
                {
                    ClientAPI.WorldManager.SendPlayerRotationDiration(0);
                    ClientAPI.WorldManager.Player.MobController.RotatingDirection = 0;
                }
            }
            else
            {
                float rotationDirection = 0;
                if (ClientAPI.WorldManager.Player.CanTurn() && (!dead || state == "spirit"))
                {
                    // Apply the left and right as rotate
                    if (movement[(int)MoveEnum.Left])
                    {
                        rotationDirection = -1;
                    }
                    //this.PlayerYaw -= rotateSpeed * timeSinceLastFrame;
                    if (movement[(int)MoveEnum.Right])
                    {
                        rotationDirection = 1;
                    }

                    if (rotationDirection != 0)
                    {
                        target.RotateAround(Vector3.up, rotateSpeed * rotationDirection * timeSinceLastFrame);
                      //  target.Rotate(Vector3.up, rotateSpeed * rotationDirection * timeSinceLastFrame);
                    }
                    //this.PlayerYaw += rotateSpeed * timeSinceLastFrame;
                }
                if (rotateSpeed * rotationDirection != ClientAPI.WorldManager.Player.MobController.RotatingDirection)
                {
                    ClientAPI.WorldManager.SendPlayerRotationDiration(rotateSpeed * rotationDirection);
                    ClientAPI.WorldManager.Player.MobController.RotatingDirection = rotateSpeed * rotationDirection;
                }

                // If the left mouse is not down, rotate the camera, but 
                // otherwise, leave it alone
                if (!leftButtonDown && cameraFollowChar)
                {
                    Camera camera = Camera.main;
                    if (movement[(int)MoveEnum.Left])
                        camera.transform.RotateAround(Vector3.up, -rotateSpeed * timeSinceLastFrame);
                       // camera.transform.Rotate(Vector3.up, -rotateSpeed * timeSinceLastFrame);
                    //this.CameraYaw -= rotateSpeed * timeSinceLastFrame;
                    if (movement[(int)MoveEnum.Right])
                        camera.transform.RotateAround(Vector3.up, rotateSpeed * timeSinceLastFrame);
                     //      camera.transform.Rotate(Vector3.up, rotateSpeed * timeSinceLastFrame);
                    //this.CameraYaw += rotateSpeed * timeSinceLastFrame;
                    // Make a smooth transition back to being behind the player over approximately 1.5 seconds for a full 180 change
                    if (playerAccel == Vector3.zero)
                        return;
                    float targetYaw = PlayerYaw;
                    if (Mathf.Abs(this.CameraYaw - targetYaw) > 1)
                    {
                        float difference = this.CameraYaw - targetYaw;
                        if (difference > 360)
                            difference -= 360;
                        float alteration = timeSinceLastFrame * 120; // 120 degrees per second
                        if (Mathf.Abs(difference) > alteration)
                        {
                            if ((difference > 0 && difference < 180) || difference < -180)
                                this.CameraYaw = this.CameraYaw - alteration;
                            else
                                this.CameraYaw = this.CameraYaw + alteration;
                            if (AtavismLogger.logLevel <= LogLevel.Info)
                                AtavismLogger.LogInfoMessage("Difference between camera and player is: " + difference + " with alteration: " + alteration + " and playerYaw : " + PlayerYaw + " and targetYaw: " + targetYaw);
                        }
                        else
                        {
                            this.CameraYaw = targetYaw;
                        }
                    }
                    // Also try move camera to the ideal pitch
                    if (Mathf.Abs(this.CameraPitch - idealPitch) > 1)
                    {
                        float difference = this.CameraPitch - idealPitch;
                        if (difference > 360)
                            difference -= 360;
                        float alteration = timeSinceLastFrame * 40; // 120 degrees per second
                        if (Mathf.Abs(difference) > alteration)
                        {
                            if ((difference > 0 && difference < 180) || difference < -180)
                                this.CameraPitch = this.CameraPitch - alteration;
                            else
                                this.CameraPitch = this.CameraPitch + alteration;
                            if (AtavismLogger.logLevel <= LogLevel.Info)
                                AtavismLogger.LogInfoMessage("Difference between camera and player is: " + difference + " with alteration: " + alteration + " and playerYaw : " + PlayerYaw + " and targetYaw: " + targetYaw);
                        }
                        else
                        {
                            this.CameraPitch = idealPitch;
                        }
                    }
                }
            }
        }

        #endregion Movement

        #region Camera

        /// <summary>
        ///   This is called from the MouseMoved handler.  It updates the 
        ///   cameraPitch, cameraYaw and cameraDist variables (which are 
        ///   later used to modify the camera).  It may also update the 
        ///   PlayerYaw.
        /// </summary>
        /// <param name="e"></param>
        protected void UpdateCamera()
        {
            UpdateCursorVisibility(leftButtonDown || rightButtonDown);
            if (rightButtonDown)
            {
                mouseRightButtonClicked = true;
                cameraFollowChar = true;
            }
            if (leftButtonDown && playerAccel == Vector3.zero)
            {
                mouseLeftButtonClicked = true;
                cameraFollowChar = false;
            }
            if (!cameraFollowChar && !leftButtonDown && !AtavismSettings.Instance.GetGeneralSettings().freeCamera)
            {
                // mouseRightButtonClicked = true;
                cameraFollowChar = true;
            }
            //if (playerAccel != Vector3.zero &&  !leftButtonDown)
            if (playerAccel != Vector3.zero && ((mouseRightButtonClicked && !rightButtonDown) || (mouseLeftButtonClicked && !leftButtonDown)))
            {
                mouseRightButtonClicked = false;
                mouseLeftButtonClicked = false;
                //     Quaternion q = Quaternion.AngleAxis(Mathf.PI, Vector3.up);
                target.rotation = Camera.main.transform.rotation;// * q;
            }

            if (mouseLookLocked || (rightButtonDown /*&& !mouseOverUI*/))
            {
                // If they are holding down the right mouse button, 
                // rotate both the camera and the player
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                ApplyMouseToCamera(mouseX, mouseY);

                // Whenever we use second mouse button or movement keys to 
                // rotate, reset the player's orientation to match the camera
                // Since the player model's default orientation is facing the 
                // camera, spin the camera an extra 180 degrees.
                if (/*(mouseX != 0 || mouseY != 0) && */playerAccel != Vector3.zero)
                {
                    //    Quaternion q = Quaternion.AngleAxis(Mathf.PI, Vector3.up);
                    if(ClientAPI.GetPlayerObject().MobController.MovementState==1)
                        target.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
                    else
                        target.rotation = Camera.main.transform.rotation;// * q;
                }

                //PlayerYaw = CameraYaw; 
                if (AtavismLogger.logLevel <= LogLevel.Info)
                    AtavismLogger.LogInfoMessage("Setting playerYaw to: " + PlayerYaw + " with cameraYaw: " + CameraYaw);
            }
            else if (leftButtonDown /*&& !mouseOverUI*/)
            {
                // If they are holding down the left mouse button, 
                // rotate the camera around the player.
                ApplyMouseToCamera(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }

            // If the mob is to follow the terrain, set their rotation to 0 for x and z axes
            // or is the right mouse button is not down
            if (ClientAPI.GetPlayerObject() != null)
                if (ClientAPI.GetPlayerObject().MobController.FollowTerrain || !rightButtonDown)
                    target.rotation = Quaternion.Euler(0, PlayerYaw, 0);

            // Set the lower bound on the treatment of camera distance so we
            // act as though we are at least 10cm awway.
            float mult = Mathf.Max(.1f, cameraDist);
            // a non-linear distance transform here for the scroll wheel
            if (!mouseOverUI && !mouseWheelDisabled)
            {
                float d = MouseWheelVelocity * mult * Input.GetAxis("Mouse ScrollWheel");
                if (AtavismLogger.logLevel <= LogLevel.Info)
                    AtavismLogger.LogInfoMessage("Mousewheel: " + d + "; with velocity: " + MouseWheelVelocity + " and input: " + Input.GetAxis("Mouse ScrollWheel"));
                cameraDist += d;
            }
            if (cameraDist < 0)
                cameraDist = 0;

            // limit the range of camera movement
            cameraDist = Mathf.Min(cameraMaxDist, cameraDist);
            cameraDist = Mathf.Max(cameraMinDist, cameraDist);

            // Check to see if the player should be visible
            //	playerVisible = cameraDist > minPlayerVisibleDistance;
        }

        /// <summary>
        ///   Use the information from the mouse movement to update the camera.
        /// </summary>
        /// <param name="x">the relative x offset of the mouse (in mouse units)</param>
        /// <param name="y">the relative y offset of the mouse (in mouse units)</param>
        protected void ApplyMouseToCamera(float x, float y)
        {
            if (cameraGrabbed)
                return;
            Camera camera = Camera.main;
            camera.transform.RotateAround(Vector3.up, x * MouseVelocity);
            // camera.transform.Rotate(Vector3.up, x * MouseVelocity);
            //this.CameraYaw += x * MouseVelocity;
            //float cameraPitch = camera.transform.eulerAngles.x;
            float cameraPitch = this.CameraPitch;
            cameraPitch -= y * MouseVelocity * 100;
            if (cameraPitch < 0)
                cameraPitch = 360 + cameraPitch;
            if (AtavismLogger.logLevel <= LogLevel.Info)
                AtavismLogger.LogInfoMessage("Camera pitch: " + CameraPitch + " and new pitch: " + cameraPitch);
            idealPitch = cameraPitch;
            CameraPitch = cameraPitch;
        }

        protected void UpdateCursorVisibility(bool mouseButtonDown)
        {
            if (cameraGrabbed)
                return;
            if (mouseOverUI)
            {
                if (AtavismLogger.logLevel <= LogLevel.Info)
                    AtavismLogger.LogInfoMessage("Mouse over UI");
                if (!mouseButtonDown)
                    Cursor.lockState = CursorLockMode.None;
                //return;
            }
            else if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                mousePosition = UnityEngine.Input.mousePosition;
                //Screen.lockCursor = true;
                ClientAPI.mouseLook = true;
            }

            if (!mouseButtonDown)
            {
                Cursor.lockState = CursorLockMode.None;
                ClientAPI.mouseLook = false;
            }
        }

        /// <summary>
        ///   Move the camera based on the new position of the camera target (player)
        /// </summary>
        /// <param name="playerPos"></param>
        /// <param name="playerOrient"></param>
        protected void UpdateCamera(Vector3 playerPos, Quaternion playerOrient)
        {
            // Convenience to avoid calling client.Camera everywhere
            Camera camera = Camera.main;

            Vector3 cameraDir = camera.transform.rotation * Vector3.back;
            if (AtavismLogger.logLevel <= LogLevel.Info)
                AtavismLogger.LogInfoMessage("Camera rotation1 :" + camera.transform.rotation);

            // Look at a point that is above the player's base - this should ideally be
            // around the character's head.
            Vector3 cameraTarget = playerPos + playerOrient * cameraTargetOffset;

            if (cameraGrabbed)
            {
                // if the camera is grabbed, then dont do anything
            }
            else if (cameraFree)
            {
                // If the camera is free, just set the position and direction
                camera.transform.position = cameraPosition;
                camera.transform.forward = -cameraDir;
                cameraDist = (playerPos - cameraPosition).magnitude;
                //client.Player.SceneNode.IsVisible = cameraDist > minThirdPersonDistance;
                //log.DebugFormat ("Camera is free; cameraPosition = {0}, cameraOrientation = {1}",
                //                    cameraPosition, cameraOrientation.EulerString);
            }
            else
            {
                // Put the camera cameraDist behind the player
                Vector3 cameraPos = cameraTarget + cameraDir * cameraDist;
                Vector3 targetDir = (cameraPos - cameraTarget).normalized;
                float len = FindAcceptableCameraPosition(camera, cameraPos, cameraTarget, targetDir);
                forcePower = Vector3.Lerp(forcePower, Vector3.zero, Time.deltaTime * 5);
                Vector3 cameraChange = new Vector3(Mathf.Cos(Time.time * 80) * forcePower.x, Mathf.Cos(Time.time * 80) * forcePower.y, Mathf.Cos(Time.time * 80) * forcePower.z);

                // The player is visible if the camera is further
                // than the minimum player visible distance
                //       playerVisible = len > minPlayerVisibleDistance;
                // Record if we were first person last frame
                //	bool formerlyFirstPerson = cameraFirstPerson;
                // We shift to first-person mode if the distance
                // is less than the minimum third-person distance
                cameraFirstPerson = len < minThirdPersonDistance;
                // Set the camera position to the target if we're
                // in first person mode, else use the calculated
                // position.
                camera.transform.position = (cameraFirstPerson ? cameraTarget : cameraTarget + cameraDir * len);
                camera.transform.localPosition = camera.transform.localPosition + cameraChange;

                if (cameraFirstPerson)
                    camera.transform.TransformDirection(-cameraDir);
                else
                    camera.transform.LookAt(cameraTarget);
                if (AtavismLogger.logLevel <= LogLevel.Info)
                    AtavismLogger.LogInfoMessage("Camera rotation2 :" + camera.transform.rotation);

                //client.Player.SceneNode.IsVisible = playerVisible;
            }
        }

        protected float FindAcceptableCameraPosition(Camera camera, Vector3 cameraPos, Vector3 cameraTarget, Vector3 cameraDir)
        {
            float len = (cameraPos - cameraTarget).magnitude;

            // Send a ray from the players head in the direction of the camera
            Vector3 intersection = Vector3.zero;
            Vector3 newCameraPos = cameraPos;
            Vector3 unitTowardCamera = (newCameraPos - cameraTarget).normalized;
            Ray ray = new Ray(cameraTarget, cameraDir);
            RaycastHit hit;
            if (Physics.SphereCast(ray, 0.25f, out hit, len, obstacleLayers | groundLayers))
            {
                if (AtavismLogger.logLevel <= LogLevel.Info)
                    AtavismLogger.LogInfoMessage("Camera ray hit object with distance: " + hit.distance);
                if (hit.distance < len)
                    return hit.distance;
            }
            return len;
        }

        public bool IsMouseLook()
        {
            return mouseLookLocked;
        }

        #endregion Camera

        public float CameraPitch
        {
            get
            {
                float pitch;
                Camera camera = Camera.main;
                pitch = camera.transform.rotation.eulerAngles.x;
                return pitch;
            }
            set
            {
                Camera camera = Camera.main;
                Vector3 pitchYawRoll = camera.transform.eulerAngles;
                if (value > 180 && value < minPitch)
                    value = minPitch;
                else if (value < 180 && value > maxPitch)
                    value = maxPitch;
                pitchYawRoll.x = value;
                camera.transform.eulerAngles = pitchYawRoll;
            }
        }

        public float CameraYaw
        {
            get
            {
                float yaw;
                Camera camera = Camera.main;
                yaw = camera.transform.rotation.eulerAngles.y;
                //yaw = cameraOrientation.eulerAngles.y;
                return yaw;
            }
            set
            {
                Camera camera = Camera.main;
                Vector3 pitchYawRoll = camera.transform.eulerAngles;
                pitchYawRoll.y = value;
                camera.transform.eulerAngles = pitchYawRoll;
            }
        }

        public float PlayerYaw
        {
            get
            {
                float yaw;
                yaw = target.rotation.eulerAngles.y;
                return yaw;
            }
            set
            {
                Camera camera = Camera.main;
                Vector3 pitchYawRoll = target.eulerAngles;
                pitchYawRoll.y = value;
                target.eulerAngles = pitchYawRoll;
            }
        }

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
        public bool IsMovingBack
        {
            get
            {
                return movingBack;
            }

        }
        void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus == false)
            {
                MoveForward(false);
                MoveBackward(false);
                StrafeLeft(false);
                StrafeRight(false);
                TurnLeft(false);
                TurnRight(false);
            }
        }
        public void Shake(Vector3 power)
        {
            forcePower = -power;
        }
        public void NoMove(bool value)
        {
            noMove = value;
        }

    }
}
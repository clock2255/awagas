using UnityEngine;
using System.Collections;

namespace Atavism
{
    public class ExampleInputController : AtavismInputController
    {

        // Use this for initialization
        void Start()
        {
            // Need to tell the client that this is the new active input controller
            ClientAPI.InputControllerActivated(this);
        }

        public override Vector3 GetPlayerMovement()
        {
            Vector3 playerMovementVector = Vector3.zero;
            // Do your input handling here, and update the playerMovementVector to contain the 
            // desired movement. The vector gets normalized in the MobController, then the players speed property will be 
            // applied to it.
            return playerMovementVector;
        }

        public override void RunCameraUpdate()
        {
            // Work out where the camera should now be, and set Main Camera to that position/rotation
        }
    }
}
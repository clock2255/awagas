using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{
    [System.Obsolete("Class Obsotete", true)]
    public class AnimationCommands : MonoBehaviour
    {

        static AnimationCommands instance;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            // First check if the UI has focus (as in an InputText object), if so, return
            if (ClientAPI.UIHasFocus())
                return;

            if (Input.GetKey(KeyCode.P))
            {
                PlayPoint();
            }
            if (Input.GetKey(KeyCode.Z))
            {
                PlayLieDown();
            }
        }

        public void PlayWave()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("coordEffect", "PlayWaveAnimation"); // Put name of Coord Effect Prefab to play here
            props.Add("hasTarget", false);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.PLAY_COORD_EFFECT", props);
        }

        public void PlayPoint()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("coordEffect", "PlayPointAnimation"); // Put name of Coord Effect Prefab to play here
            props.Add("hasTarget", false);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.PLAY_COORD_EFFECT", props);
        }

        public void PlayLieDown()
        {
            // First check if the player is already lying down
            bool playerLyingDown = false;
            if (ClientAPI.GetPlayerObject().PropertyExists("currentAnim"))
            {
                if ((string)ClientAPI.GetPlayerObject().GetProperty("currentAnim") == "lie")
                    playerLyingDown = true;
            }

            if (playerLyingDown)
            {
                // Player is lying down so reset anim back to normal
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/setStringProperty currentAnim null");
            }
            else
            {
                // Player is not lying down so set anim to lie
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/setStringProperty currentAnim lie");
            }
        }

        public static AnimationCommands Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
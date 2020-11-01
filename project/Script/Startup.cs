using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{
    public class Startup : MonoBehaviour
    {

        public static Startup instance;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            instance = this;
            //Set the scripts object to not be destroyed
            DontDestroyOnLoad(gameObject);
            // Load Coordinated Effects
            //CoordinatedEffectSystem.RegisterCoordinatedEffect("CoordMeleeStrikeEffect", new CoordMeleeStrikeEffect());
            //CoordinatedEffectSystem.RegisterCoordinatedEffect("Backflip", new BackflipEffect());
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Raises the level was loaded event. This is used to tell the server that the client is ready to go.
        /// </summary>
        /// <param name='level'>
        /// Level.
        /// </param>
        /*void OnLevelWasLoaded(int level) {
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.CLIENT_LEVEL_LOADED", props);
        }*/
    }
}
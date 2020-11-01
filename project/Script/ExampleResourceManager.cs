using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class ExampleResourceManager : AtavismResourceManager
    {

        // Use this for initialization
        void Start()
        {
            // Don't let this object be destroyed as it will mess things up when changing scenes
            DontDestroyOnLoad(this.gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Loads the asset for the specified path and filename. Used for loading player and mob prefabs.
        /// </summary>
        /// <returns>The asset.</returns>
        /// <param name="node">The ObjectNode of the player/mob.</param>
        /// <param name="path">Path.</param>
        /// <param name="fileName">File name.</param>
        public override object LoadAsset(AtavismObjectNode node, string path, string fileName)
        {
            string prefabName = path + fileName;
            if (prefabName.Contains(".prefab"))
            {
                prefabName = prefabName.Remove(prefabName.Length - 7);
                prefabName = prefabName.Remove(0, 17);
                return Resources.Load(prefabName);
            }
            else
            {
                return Resources.Load(prefabName);
            }
        }

        /// <summary>
        /// Loads the asset for the specified path and filename. Used for loading coordinated effects.
        /// </summary>
        /// <returns>The asset.</returns>
        /// <param name="props">The properties of the coordinated effect.</param>
        /// <param name="path">Path.</param>
        /// <param name="fileName">File name.</param>
        public override object LoadAsset(Dictionary<string, object> props, string path, string fileName)
        {
            string prefabName = path + fileName;
            if (prefabName.Contains(".prefab"))
            {
                prefabName = prefabName.Remove(prefabName.Length - 7);
                prefabName = prefabName.Remove(0, 17);
                return Resources.Load(prefabName);
            }
            else
            {
                return Resources.Load(prefabName);
            }
        }
    }
}
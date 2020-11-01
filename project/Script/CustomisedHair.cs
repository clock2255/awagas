using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{
    public class CustomisedHair : MonoBehaviour
    {

        public List<GameObject> hairModels; // Allows the user to place the different hair model options in a list
        public Transform parentJoint; // Where to attach the hairModel on the character
        public string hairPropertyName = "HairTest"; // The name of the property that will be saved to the character
        public string hairDirectory = "";
        int switchHair = 0;
        GameObject activeHair; // The currently active hairModel - this should be stored so it can be removed later

        // Use this for initialization
        void Start()
        {
        }

        // Using the ObjectNodeReady function means this will run when the node is all set up and ready to go
        protected void ObjectNodeReady()
        {
            if (GetComponent<AtavismNode>() != null)
            {
                // Register a property changer for the hair model so when the character loads in the game it will show up
                GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler(hairPropertyName, HandleHairChange);
                // The property may have already been loaded, so do a check now as well
                if (GetComponent<AtavismNode>().PropertyExists(hairPropertyName))
                {
                    string hairModel = (string)GetComponent<AtavismNode>().GetProperty(hairPropertyName);
                    UpdateHairModel(hairModel);
                }
            }
        }

        // It's good practice to remove any property handlers when an object is being destroyed
        void OnDestroy()
        {
            if (GetComponent<AtavismNode>() != null)
            {
                // Register a property changer for the hair model so when the character loads in the game it will show up
                GetComponent<AtavismNode>().RemoveObjectPropertyChangeHandler(hairPropertyName, HandleHairChange);
            }
        }

        public void SwitchHairForward()
        {
            switchHair++;
            if (switchHair == hairModels.Count)
                switchHair = 0;
            UpdateHairModel(hairModels[switchHair].name);
        }

        // This will run when the game gets a new hair property - it will get the property value then run the UpdateHairModel function.
        public void HandleHairChange(object sender, PropertyChangeEventArgs args)
        {
            string hairModel = (string)GetComponent<AtavismNode>().GetProperty(hairPropertyName);
            UpdateHairModel(hairModel);
        }

        // Changes the hair model when run
        public void UpdateHairModel(string hairPrefabName)
        {
            if (activeHair != null)
            {
                Destroy(activeHair);
            }

            // No hair selected, just return
            if (hairPrefabName == null || hairPrefabName == "")
            {
                return;
            }

            GameObject hairPrefab;
            // Load in the hair prefab from the resources folder (or subfolder if specified)
            if (hairDirectory == "")
            {
                hairPrefab = (GameObject)Resources.Load(hairPrefabName);
            }
            else
            {
                hairPrefab = (GameObject)Resources.Load(hairDirectory + "/" + hairPrefabName);
            }

            activeHair = (GameObject)Instantiate(hairPrefab, parentJoint.position, parentJoint.rotation);
            activeHair.name = hairPrefab.name; // To get rid of the Clone that gets added to the end of the name
            activeHair.transform.parent = parentJoint;
        }

        // Used to get the currently active hair model so it can be saved to the characters properties
        public GameObject ActiveHair
        {
            get
            {
                return activeHair;
            }
        }

    }
}
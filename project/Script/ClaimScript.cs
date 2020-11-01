using UnityEngine;
using System.Collections;

namespace Atavism
{
    public class ClaimScript : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            string[] args = new string[1];
            args[0] = gameObject.name;
            AtavismEventSystem.DispatchEvent("CLAIM_ADDED", args);
        }

        void ObjectNodeReady()
        {
            GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("scale", ScaleHandler);
            if (GetComponent<AtavismNode>().PropertyExists("scale"))
            {
                AtavismLogger.LogDebugMessage("Got scale");
                Vector3 scaleObj = (Vector3)GetComponent<AtavismNode>().GetProperty("scale");
                gameObject.transform.localScale = scaleObj;
            }
        }

        void OnDestroy()
        {
            string[] args = new string[1];
            args[0] = gameObject.name;
            AtavismEventSystem.DispatchEvent("CLAIM_REMOVED", args);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ScaleHandler(object sender, PropertyChangeEventArgs args)
        {
            AtavismLogger.LogDebugMessage("Got scale");
            Vector3 scaleObj = (Vector3)GetComponent<AtavismNode>().GetProperty("scale");
            gameObject.transform.localScale = scaleObj;
        }
    }
}
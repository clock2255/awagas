using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class InteractiveObjectsManager : MonoBehaviour
    {

        static InteractiveObjectsManager instance;

        Dictionary<int, InteractiveObject> interactiveObjects = new Dictionary<int, InteractiveObject>();

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            NetworkAPI.RegisterExtensionMessageHandler("interactive_object_state", HandleInteractiveObjectStateMessage);
        }

        public void RegisterInteractiveObject(InteractiveObject iObj)
        {
            interactiveObjects[iObj.id] = iObj;
        }

        public void RemoveInteractiveObject(int id)
        {
            interactiveObjects.Remove(id);
        }

        void HandleInteractiveObjectStateMessage(Dictionary<string, object> props)
        {
            int nodeID = (int)props["nodeID"];
            bool active = (bool)props["active"];
            string state = (string)props["state"];
            interactiveObjects[nodeID].Active = active;
            interactiveObjects[nodeID].ResetHighlight();

            if (interactiveObjects[nodeID].isLODChild)
            {
                interactiveObjects[nodeID].transform.parent.gameObject.SetActive(active);
            }
            else
            {
                interactiveObjects[nodeID].gameObject.SetActive(active);
            }

            if (active)
            {
                interactiveObjects[nodeID].StateUpdated(state);
            }
        }

        public static InteractiveObjectsManager Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
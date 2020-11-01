using UnityEngine;
using System.Collections;

namespace Atavism
{
    public class SpawnMarker : MonoBehaviour
    {

        int markerID;
        private Vector3 screenPoint;
        private Vector3 offset;
        bool editingPosition = false;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /*void OnMouseDown ()
        {
            MobCreator mobCreator = Camera.main.GetComponentInChildren<MobCreator>();
            if (mobCreator != null)
                mobCreator.SpawnSelected(markerID);
        }*/

        public void StartEditingPosition()
        {
            editingPosition = true;
        }

        public void StopEditingPosition()
        {
            editingPosition = false;
        }

        public int MarkerID
        {
            get
            {
                return markerID;
            }
            set
            {
                markerID = value;
            }
        }
    }
}
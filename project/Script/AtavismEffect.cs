using UnityEngine;
using System.Collections;

namespace Atavism
{
    public class AtavismEffect : MonoBehaviour
    {

        public int id;
        public string name;
        public Sprite icon;
        public string tooltip;
        public bool isBuff;
        int stackSize = 1;
        float length;
        float expiration = -1;
        bool active = false;
        bool passive = false;

        // Use this for initialization
        void Start()
        {

        }

        public AtavismEffect Clone(GameObject go)
        {
            AtavismEffect clone = go.AddComponent<AtavismEffect>();
            clone.id = id;
            clone.name = name;
            clone.icon = icon;
            clone.tooltip = tooltip;
            clone.Length = Length;
            return clone;
        }

        public int StackSize
        {
            get
            {
                return stackSize;
            }
            set
            {
                stackSize = value;
            }
        }

        public float Length
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
            }
        }

        public float Expiration
        {
            get
            {
                return expiration;
            }
            set
            {
                expiration = value;
            }
        }

        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
            }
        }
        public bool Passive
        {
            get
            {
                return passive;
            }
            set
            {
                passive = value;
            }
        }
    }
}
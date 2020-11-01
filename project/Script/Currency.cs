using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Atavism
{
    public class Currency : MonoBehaviour
    {

        public int id = -1;
        public string name = "";
        public Sprite icon;
        public int group = 1;
        public int position = 1;
        public int convertsTo = -1;
        public int conversionAmountReq = 1;
        long current = 0;
        public long max = 999999;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public long Current
        {
            get
            {
                return current;
            }
            set
            {
                current = value;
            }
        }
    }
}
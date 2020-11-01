using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class AtavismBuildObjectTemplate : MonoBehaviour
    {

        public int id = 0;
        public string buildObjectName = "";
        public Sprite icon;
        public string gameObject = "";
        public int category = 0;
        public int skill = -1;
        public int skillLevelReq = 0;
        public float distanceReq = 1f;
        public bool buildTaskReqPlayer = true;
        public ClaimType validClaimTypes = ClaimType.Any;
        public bool onlyAvailableFromItem = false;
        public List<int> itemsReq;
        public List<int> itemsReqCount;
        public List<int> upgradeItemsReq;
        public Dictionary<int, int> itemReqs = new Dictionary<int, int>();
        public string reqWeapon = "";
    }
}
using UnityEngine;
using System.Collections;

namespace Atavism
{
    public class ShootWeapon : MonoBehaviour
    {

        public KeyCode shootKey;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(shootKey))
            {
                Debug.Log("sending shoot");
                //int id = (int)ClientAPI.GetPlayerObject().GetProperty("combat.autoability");
                int id = 5;
                NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/ability " + id);
                //NetworkAPI.SendAttackMessage (ClientAPI.GetTargetOid(), "strike", true);
                //NetworkAPI.SendAttackMessage (ClientAPI.GetTargetOid(), "strike", false);
            }
        }
    }
}
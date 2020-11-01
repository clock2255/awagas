using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{
    /// <summary>
    /// This module contains the various methods for dealing with the target
    /// </summary>
    public class AtavismTarget
    {

        #region Target API

        public static void AttackTarget(long objectId)
        {
            /*if (_currentTarget == null) {
                ClientAPI.Write ("No target selected");
                return;
            }*/
            AutoAttackMessage message = new AutoAttackMessage();
            message.ObjectId = objectId;
            message.AttackType = "strike";
            message.AttackStatus = true;
            AtavismClient.Instance.NetworkHelper.SendMessage(message);
            //ClientAPI.Network.SendAttackMessage (_currentTarget.OID, "strike", True);
        }

        public static void ClearTarget()
        {
            _UpdateTarget(null);
        }

        public static void TargetByName(string name)
        {
            _UpdateTarget(ClientAPI.WorldManager.GetObjectNode(name));
        }

        public static void TargetUnit(string unit)
        {
            //_UpdateTarget (MarsUnit._GetUnit (unit));
        }

        public static void TargetLastEnemy()
        {
            _UpdateTarget(_lastEnemy);
        }

        public static void TargetLastTarget()
        {
            _UpdateTarget(_lastTarget);
        }

        public static void TargetNearestEnemy(bool reverse)
        {
            List<long> worldObjOIDs = ClientAPI.WorldManager.GetObjectOidList();
            List<AtavismObjectNode> worldObjects = new List<AtavismObjectNode>();
            foreach (long worldObjOID in worldObjOIDs)
            {
                AtavismObjectNode worldObj = ClientAPI.WorldManager.GetObjectNode(worldObjOID);
                if (worldObj != null && worldObj.CheckBooleanProperty("attackable"))
                    worldObjects.Add(worldObj);
            }
            Vector3 playerPos = ClientAPI.GetPlayerObject().Position;
            //worldObjects.sort((lambda x, y: _DistanceComparerHelper(playerPos, x, y)), reverse=reverse)
            // I now have a list of world objects, sorted based on their distance from the player
            AtavismObjectNode last = null;
            if (_currentTarget != null && _currentTarget.CheckBooleanProperty("attackable"))
                last = _currentTarget;
            else
                last = _lastEnemy;
            int index = -1;
            if (worldObjects.Contains(last))
                index = worldObjects.IndexOf(last);
            else
                last = null;
            if (last == null)
            {
                if (worldObjects.Count > 0)
                    _UpdateTarget(worldObjects[0]);
                return;
            }
            if (worldObjects.Count > index + 1)
                _UpdateTarget(worldObjects[index + 1]);
            else
                _UpdateTarget(worldObjects[0]);
        }

        /*int _DistanceComparerHelper (Vector3 pos, Vector2 x, Vector2 y)
        {
            float xDistSquared = (pos - x.Position).sqrMagnitude;
            float yDistSquared = (pos - y.Position).sqrMagnitude;
            if (xDistSquared < yDistSquared)
                return -1;
            else if (xDistSquared > yDistSquared)
                return 1;
            return 0;
        }*/

        public static void TargetByOID(OID oid)
        {
            _UpdateTarget(ClientAPI.WorldManager.GetObjectNode(oid));
        }

        public static AtavismObjectNode GetCurrentTarget()
        {
            return _currentTarget;
        }

        #endregion Target API

        #region Fields

        // info about the current target, the last target, the last enemy target, and the mouseover object
        static AtavismObjectNode _currentTarget;
        static AtavismObjectNode _lastTarget;
        static AtavismObjectNode _lastEnemy;
        static AtavismObjectNode _mouseoverTarget;

        #endregion Fields


        #region Helper methods

        static void _UpdateTarget(AtavismObjectNode obj)
        {
            if (_currentTarget != obj)
            {
                if (_currentTarget != null)
                {
                    _lastTarget = _currentTarget;
                    if (_currentTarget.CheckBooleanProperty("attackable"))
                        _lastEnemy = _currentTarget;
                }
                _currentTarget = obj;
                //ClientAPI.Interface.DispatchEvent("PLAYER_TARGET_CHANGED", []);
            }
        }

        static void _HandleObjectRemoved(AtavismObjectNode worldObj)
        {
            if (_currentTarget == worldObj)
                _UpdateTarget(null);
            if (_lastTarget == worldObj)
                _lastTarget = null;
            if (_lastEnemy == worldObj)
                _lastEnemy = null;
            if (_mouseoverTarget == worldObj)
                _mouseoverTarget = null;
        }

        /*void _UpdateMouseoverTarget (object sender, int eventData)
        {
            _mouseoverTarget = ClientAPI.GetMouseoverTarget ();
        }*/

        #endregion Helper Methods

        // Register for frame started events, so we can update our mouseover target
        //ClientAPI.RegisterEventHandler("FrameStarted", _UpdateMouseoverTarget);

        // Register for object removed messages, so we can clear our target if needed
        //ClientAPI.World.RegisterEventHandler('ObjectRemoved', _HandleObjectRemoved);

    }
}
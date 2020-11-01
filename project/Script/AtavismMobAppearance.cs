using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{
    public enum AttachmentSocket
    {
        Root,
        LeftFoot,
        RightFoot,
        Pelvis,
        LeftHip,
        RightHip,
        MainHand,
        MainHand2,
        OffHand,
        OffHand2,
        MainHandRest,
        MainHandRest2,
        OffHandRest,
        OffHandRest2,
        Shield,
        Shield2,
        ShieldRest,
        ShieldRest2,
        Chest,
        Back,
        LeftShoulder,
        RightShoulder,
        Head,
        Neck,
        Mouth,
        LeftEye,
        RightEye,
        Overhead,
        MainWeapon,
        SecondaryWeapon,
        None
    }

    public class ActiveEquipmentDisplay
    {
        public EquipmentDisplay equipDisplay;
        public GameObject attachedObject;
        public AttachmentSocket socket;
        public Material baseMaterial;

        public ActiveEquipmentDisplay()
        {
        }

        public ActiveEquipmentDisplay(EquipmentDisplay equipDisplay, GameObject attachedObject, AttachmentSocket socket)
        {
            this.equipDisplay = equipDisplay;
            this.attachedObject = attachedObject;
            this.socket = socket;
        }

        public ActiveEquipmentDisplay(EquipmentDisplay equipDisplay, GameObject attachedObject, Material baseMaterial)
        {
            this.equipDisplay = equipDisplay;
            this.attachedObject = attachedObject;
            this.baseMaterial = baseMaterial;
        }
    }

    public class AtavismMobAppearance : MonoBehaviour
    {

        // Icon
        public Sprite portraitIcon;

        // Sockets for attaching weapons (and particles)
        public Transform mainHand;
        public Transform mainHand2;
        public Transform offHand;
        public Transform offHand2;
        public Transform mainHandRest;
        public Transform mainHandRest2;
        public Transform offHandRest;
        public Transform offHandRest2;
        public Transform shield;
        public Transform shield2;
        public Transform shieldRest;
        public Transform shieldRest2;
        public Transform head;
        public Transform leftShoulderSocket;
        public Transform rightShoulderSocket;

        // Sockets for particles
        public Transform rootSocket;
        public Transform leftFootSocket;
        public Transform rightFootSocket;
        public Transform pelvisSocket;
        public Transform leftHipSocket;
        public Transform rightHipSocket;
        public Transform chestSocket;
        public Transform backSocket;
        public Transform neckSocket;
        public Transform mouthSocket;
        public Transform leftEyeSocket;
        public Transform rightEyeSocket;
        public Transform overheadSocket;

        List<string> displayProperties = new List<string>() {
        "weaponDisplayID",
        "weapon2DisplayID",
        "legDisplayID",
        "chestDisplayID",
        "headDisplayID",
        "feetDisplayID",
        "handDisplayID",
        "capeDisplayID",
        "shoulderDisplayID",
        "beltDisplayID",
        "ringDisplayID",
        "ring2DisplayID",
        "earringDisplayID",
        "earring2DisplayID",
        "rangedDisplayID",
        "neckDisplayID",
        "slot1DisplayID",
        "slot2DisplayID",
        "slot3DisplayID",
        "slot4DisplayID",
        "slot5DisplayID",
        "slot6DisplayID",
        "slot7DisplayID",
        "slot8DisplayID",
        "slot9DisplayID",
        "slot10DisplayID",
        "slot11DisplayID",
        "slot12DisplayID",
        "slot13DisplayID",
        "slot14DisplayID",
        "slot15DisplayID",
        "slot16DisplayID",
        "slot17DisplayID",
        "slot18DisplayID",
        "slot19DisplayID",
        "slot20DisplayID",
        "fashionDisplayID"
    };

        protected Dictionary<string, List<ActiveEquipmentDisplay>> activeEquipDisplays = new Dictionary<string, List<ActiveEquipmentDisplay>>();
        protected bool inCombat = false;
        //  string weaponType = "";
        public float combatCloseDelay = 0.5f;
        float toRestTime = 0;
        bool toRest = false;
        float hideTime = 0;
        bool hidedWeapon = false;
        bool dead = false;
        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        protected void Update()
        {
            if (toRestTime < Time.time && toRest && toRestTime != 0)
            {
                toRest = false;
                if (inCombat == false)
                    SetWeaponsAttachmentSlot();
            }
            if (hideTime < Time.time && hideTime != 0 && hidedWeapon)
            {
                hidedWeapon = false;
                ShowWeapon();
            }
        }


        public Transform GetSocketTransform(AttachmentSocket slot)
        {
            switch (slot)
            {
                case AttachmentSocket.MainHand:
                    if (mainHand != null)
                        return mainHand;
                    else
                        return transform;
                case AttachmentSocket.MainHand2:
                    if (mainHand2 != null)
                        return mainHand2;
                    else
                        return transform;
                case AttachmentSocket.OffHand:
                    if (offHand != null)
                        return offHand;
                    else
                        return transform;
                case AttachmentSocket.OffHand2:
                    if (offHand2 != null)
                        return offHand2;
                    else
                        return transform;
                case AttachmentSocket.MainHandRest:
                    if (mainHandRest != null)
                        return mainHandRest;
                    else
                        return transform;
                case AttachmentSocket.MainHandRest2:
                    if (mainHandRest2 != null)
                        return mainHandRest2;
                    else
                        return transform;
                case AttachmentSocket.OffHandRest:
                    if (offHandRest != null)
                        return offHandRest;
                    else
                        return transform;
                case AttachmentSocket.OffHandRest2:
                    if (offHandRest2 != null)
                        return offHandRest2;
                    else
                        return transform;
                case AttachmentSocket.Shield:
                    if (shield != null)
                        return shield;
                    else
                        return transform;
                case AttachmentSocket.Shield2:
                    if (shield2 != null)
                        return shield2;
                    else
                        return transform;
                case AttachmentSocket.ShieldRest:
                    if (shieldRest != null)
                        return shieldRest;
                    else
                        return transform;
                case AttachmentSocket.ShieldRest2:
                    if (shieldRest2 != null)
                        return shieldRest2;
                    else
                        return transform;
                case AttachmentSocket.Head:
                    if (head != null)
                        return head;
                    else
                        return transform;
                case AttachmentSocket.LeftShoulder:
                    if (leftShoulderSocket != null)
                        return leftShoulderSocket;
                    else
                        return transform;
                case AttachmentSocket.RightShoulder:
                    if (rightShoulderSocket != null)
                        return rightShoulderSocket;
                    else
                        return transform;
                case AttachmentSocket.Root:
                    return transform;
                case AttachmentSocket.LeftFoot:
                    if (leftFootSocket != null)
                        return leftFootSocket;
                    else
                        return transform;
                case AttachmentSocket.RightFoot:
                    if (rightFootSocket != null)
                        return rightFootSocket;
                    else
                        return transform;
                case AttachmentSocket.Pelvis:
                    if (pelvisSocket != null)
                        return pelvisSocket;
                    else
                        return transform;
                case AttachmentSocket.LeftHip:
                    if (leftHipSocket != null)
                        return leftHipSocket;
                    else
                        return transform;
                case AttachmentSocket.RightHip:
                    if (rightHipSocket != null)
                        return rightHipSocket;
                    else
                        return transform;
                case AttachmentSocket.Chest:
                    if (chestSocket != null)
                        return chestSocket;
                    else
                        return transform;
                case AttachmentSocket.Back:
                    if (backSocket != null)
                        return backSocket;
                    else
                        return transform;
                case AttachmentSocket.Neck:
                    if (neckSocket != null)
                        return neckSocket;
                    else
                        return transform;
                case AttachmentSocket.Mouth:
                    if (mouthSocket != null)
                        return mouthSocket;
                    else
                        return transform;
                case AttachmentSocket.LeftEye:
                    if (leftEyeSocket != null)
                        return leftEyeSocket;
                    else
                        return transform;
                case AttachmentSocket.RightEye:
                    if (rightEyeSocket != null)
                        return rightEyeSocket;
                    else
                        return transform;
                case AttachmentSocket.Overhead:
                    if (overheadSocket != null)
                        return overheadSocket;
                    else
                        return transform;
                case AttachmentSocket.MainWeapon:
                    if (mainHand != null && mainHand.childCount > 0)
                        return mainHand.GetChild(0).Find("socket");
                    else
                        return null;
                case AttachmentSocket.SecondaryWeapon:
                    if (offHand != null)
                        return offHand.GetChild(0).Find("socket");
                    else
                        return null;
            }
            return null;
        }

        protected virtual void OnDestroy()
        {
            if (GetComponent<AtavismNode>())
            {
                foreach (string displayProperty in displayProperties)
                {
                    GetComponent<AtavismNode>().RemoveObjectPropertyChangeHandler(displayProperty, EquipPropertyHandler);
                }
                GetComponent<AtavismNode>().RemoveObjectPropertyChangeHandler("combatstate", HandleCombatState);
                GetComponent<AtavismNode>().RemoveObjectPropertyChangeHandler("model", ModelHandler);
                GetComponent<AtavismNode>().RemoveObjectPropertyChangeHandler("weaponType", HandleWeaponType);
                GetComponent<AtavismNode>().RemoveObjectPropertyChangeHandler("deadstate", HandleDeadState);
            }
        }

        protected void ObjectNodeReady()
        {
            GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("model", ModelHandler);

            foreach (string displayProperty in displayProperties)
            {
                GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler(displayProperty, EquipPropertyHandler);
                if (GetComponent<AtavismNode>().PropertyExists(displayProperty))
                {
                    string displayID = (string)GetComponent<AtavismNode>().GetProperty(displayProperty);
                    UpdateEquipDisplay(displayProperty, displayID);
                }
            }

            GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("combatstate", HandleCombatState);
            if (GetComponent<AtavismNode>().PropertyExists("combatstate"))
            {
                inCombat = (bool)GetComponent<AtavismNode>().GetProperty("combatstate");
                SetWeaponsAttachmentSlot();
            }
            AtavismLogger.LogInfoMessage("Registered display properties for: " + name);


            GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("weaponType", HandleWeaponType);
            if (GetComponent<AtavismNode>().PropertyExists("weaponType"))
            {
                //   weaponType = (string)GetComponent<AtavismNode>().GetProperty("weaponType");
            }
            GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("deadstate", HandleDeadState);
            if (GetComponent<AtavismNode>().PropertyExists("deadstate"))
            {
                dead = (bool)GetComponent<AtavismNode>().GetProperty("deadstate");
            }
        }

        public void HandleWeaponType(object sender, PropertyChangeEventArgs args)
        {
            //   weaponType = (string)GetComponent<AtavismNode>().GetProperty("weaponType");
        }

        public void ModelHandler(object sender, PropertyChangeEventArgs args)
        {
            AtavismLogger.LogDebugMessage("Got model");
            UpdateModel((string)GetComponent<AtavismNode>().GetProperty(args.PropertyName));
        }

        public void UpdateModel(string prefabName)
        {
            if (prefabName.Contains(".prefab"))
            {
                int resourcePathPos = prefabName.IndexOf("Resources/");
                prefabName = prefabName.Substring(resourcePathPos + 10);
                prefabName = prefabName.Remove(prefabName.Length - 7);
            }

            GameObject prefab = (GameObject)Resources.Load(prefabName);
            GameObject newCharacter = (GameObject)UnityEngine.Object.Instantiate(prefab, transform.position, transform.rotation);
            newCharacter.name = name;
            GetComponent<AtavismNode>().ReplaceGameObject(newCharacter);

            // Check if the player should be hidden as they are in spirit world
            if (GetComponent<AtavismNode>().PropertyExists("state"))
            {
                string state = (string)GetComponent<AtavismNode>().GetProperty("state");
                if (state == "spirit")
                    newCharacter.SetActive(false);
            }
        }

        public void HandleCombatState(object sender, PropertyChangeEventArgs args)
        {
            inCombat = (bool)GetComponent<AtavismNode>().GetProperty(args.PropertyName);
            // Reset weapon attached weapons based on combat state
            if (!inCombat)
            {
                toRestTime = Time.time + combatCloseDelay;
                toRest = true;
                return;
            }
            SetWeaponsAttachmentSlot();
        }

        public void SetWeaponsAttachmentSlot()
        {
            SetWeaponsAttachmentSlot(false);
        }

        public void SetWeaponsAttachmentSlot(bool force)
        {
            foreach (List<ActiveEquipmentDisplay> activeDisplays in activeEquipDisplays.Values)
            {
                foreach (ActiveEquipmentDisplay activeDisplay in activeDisplays)
                {
                    if (activeDisplay.equipDisplay.equipDisplayType != EquipDisplayType.AttachedObject)
                        continue;

                    //TODO: Handle items that can be both primary and off hand
                    if (inCombat || activeDisplay.equipDisplay.restSocket == AttachmentSocket.None)
                    {
                        if (activeDisplay.socket == activeDisplay.equipDisplay.socket && !force)
                        {
                            toRestTime = 0;
                            toRest = false;
                            continue;
                        }
                        // Set the weapons socket to the main socket for the display
                        activeDisplay.socket = activeDisplay.equipDisplay.socket;
                        GameObject weapon = activeDisplay.attachedObject;
                        /*
                        weapon.transform.parent = null;
                        weapon.transform.position = GetSocketTransform(activeDisplay.socket).position;
                        weapon.transform.rotation = GetSocketTransform(activeDisplay.socket).rotation;
                        weapon.transform.parent = GetSocketTransform(activeDisplay.socket);
                        */
                        weapon.transform.parent = GetSocketTransform(activeDisplay.socket);
                        weapon.transform.localPosition = Vector3.zero;
                        weapon.transform.localRotation = new Quaternion(0, 0, 0, 0);
                        weapon.transform.localScale = Vector3.one;
                    }
                    else if (force || activeDisplay.socket != activeDisplay.equipDisplay.restSocket)
                    {
                        // Set the weapons socket to the rest socket for the display
                        activeDisplay.socket = activeDisplay.equipDisplay.restSocket;
                        GameObject weapon = activeDisplay.attachedObject;
                        /*
                        weapon.transform.parent = null;
                        weapon.transform.position = GetSocketTransform(activeDisplay.socket).position;
                        weapon.transform.rotation = GetSocketTransform(activeDisplay.socket).rotation;
                        */
                        weapon.transform.parent = GetSocketTransform(activeDisplay.socket);
                        weapon.transform.localPosition = Vector3.zero;
                        weapon.transform.localRotation = new Quaternion(0, 0, 0, 0);
                        weapon.transform.localScale = Vector3.one;
                        //coretion position an rotation
                        weapon.transform.localPosition += activeDisplay.equipDisplay.restPosition;
                        weapon.transform.Rotate(activeDisplay.equipDisplay.restRotation.x, activeDisplay.equipDisplay.restRotation.y, activeDisplay.equipDisplay.restRotation.z, Space.Self);
                        //Disable Trails 
                        /*    MeleeWeaponTrail[] mwts = weapon.GetComponentsInChildren<MeleeWeaponTrail>();
                            foreach (MeleeWeaponTrail mwt in mwts)
                            {
                                if (mwt != null)
                                    mwt.gameObject.SetActive(false);
                            }*/
                    }
                }
            }
        }
        /// <summary>
        /// Function move equiped weapon to main slot
        /// </summary>
        /// <param name="time"></param>
        public void GetWeapon(float time)
        {
            foreach (List<ActiveEquipmentDisplay> activeDisplays in activeEquipDisplays.Values)
            {
                foreach (ActiveEquipmentDisplay activeDisplay in activeDisplays)
                {
                    toRestTime = Time.time + time;
                    toRest = true;
                    if (activeDisplay.socket == activeDisplay.equipDisplay.socket)
                        continue;
                    // Set the weapons socket to the main socket for the display
                    activeDisplay.socket = activeDisplay.equipDisplay.socket;
                    GameObject weapon = activeDisplay.attachedObject;
                    weapon.transform.parent = GetSocketTransform(activeDisplay.socket);
                    weapon.transform.localPosition = Vector3.zero;
                    weapon.transform.localRotation = new Quaternion(0, 0, 0, 0);
                    weapon.transform.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// Function turn off object of equiped weapon and dieable trails
        /// </summary>
        /// <param name="t"></param>
        public void HideWeapon(float t)
        {
            foreach (List<ActiveEquipmentDisplay> activeDisplays in activeEquipDisplays.Values)
            {
                foreach (ActiveEquipmentDisplay activeDisplay in activeDisplays)
                {
                    GameObject weapon = activeDisplay.attachedObject;
                    //Disable trails
                    /*      XWeaponTrail trail = weapon.GetComponentInChildren<XWeaponTrail>();
                          if (trail != null) trail.Deactivate();*/
                    weapon.SetActive(false);
                    hideTime = Time.time + t;
                    hidedWeapon = true;
                }
            }
        }

        /// <summary>
        /// Function return model of equiped weapon
        /// </summary>
        /// <returns></returns>
        public GameObject GetWeaponObjectModel()
        {
            foreach (List<ActiveEquipmentDisplay> activeDisplays in activeEquipDisplays.Values)
                foreach (ActiveEquipmentDisplay activeDisplay in activeDisplays)
                    return activeDisplay.equipDisplay.model;
            return null;
        }

        /// <summary>
        /// Function return object of equiped weapon
        /// </summary>
        /// <returns></returns>
        public GameObject GetWeaponObject()
        {
            foreach (List<ActiveEquipmentDisplay> activeDisplays in activeEquipDisplays.Values)
                foreach (ActiveEquipmentDisplay activeDisplay in activeDisplays)
                    return activeDisplay.attachedObject;
            return null;
        }

        /// <summary>
        /// Function return name of model equiped weapon 
        /// </summary>
        /// <returns></returns>
        public string GetWeaponName()
        {
            foreach (List<ActiveEquipmentDisplay> activeDisplays in activeEquipDisplays.Values)
                foreach (ActiveEquipmentDisplay activeDisplay in activeDisplays)
                    return activeDisplay.equipDisplay.model.name;
            return null;
        }

        /// <summary>
        /// Function turn on trails on equiped weapon
        /// </summary>
        public void ShowTrail()
        {
            foreach (List<ActiveEquipmentDisplay> activeDisplays in activeEquipDisplays.Values)
                foreach (ActiveEquipmentDisplay activeDisplay in activeDisplays)
                {
                    /*  GameObject weapon = activeDisplay.attachedObject;
                       XWeaponTrail trail = weapon.GetComponentInChildren<XWeaponTrail>(true);
                      if (trail != null) trail.Activate();*/
                }
        }

        /// <summary>
        /// Function trun off trails on equiped weapon
        /// </summary>
        public void HideTrail()
        {
            foreach (List<ActiveEquipmentDisplay> activeDisplays in activeEquipDisplays.Values)
                foreach (ActiveEquipmentDisplay activeDisplay in activeDisplays)
                {
                    /*    GameObject weapon = activeDisplay.attachedObject;
                        XWeaponTrail trail = weapon.GetComponentInChildren<XWeaponTrail>();
                        if (trail != null) trail.Deactivate();*/
                }
        }

        /// <summary>
        /// Function turn on object of equiped weapon
        /// </summary>
        public void ShowWeapon()
        {
            foreach (List<ActiveEquipmentDisplay> activeDisplays in activeEquipDisplays.Values)
                foreach (ActiveEquipmentDisplay activeDisplay in activeDisplays)
                    activeDisplay.attachedObject.SetActive(true);
        }

        public void EquipPropertyHandler(object sender, PropertyChangeEventArgs args)
        {
            string displayID = (string)GetComponent<AtavismNode>().GetProperty(args.PropertyName);
            if (AtavismSettings.Instance.CharacterAvatar != null)
            {
                if (GetComponent<AtavismNode>() != null)
                    if (GetComponent<AtavismNode>().Oid == ClientAPI.GetPlayerOid())
                        AtavismSettings.Instance.CharacterAvatar.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay(args.PropertyName, displayID);
            }
            UpdateEquipDisplay(args.PropertyName, displayID);
        }

        public void UpdateEquipDisplay(string propName, string displayID)
        {
            if (activeEquipDisplays.ContainsKey(propName))
            {
                foreach (ActiveEquipmentDisplay activeDisplay in activeEquipDisplays[propName])
                {
                    if (activeDisplay.equipDisplay.equipDisplayType == EquipDisplayType.AttachedObject)
                    {
                        RemoveAttachedObject(activeDisplay);
                    }
                    else if (activeDisplay.equipDisplay.equipDisplayType == EquipDisplayType.ActivatedModel)
                    {
                        DeactivateModel(activeDisplay);
                    }
                    else if (activeDisplay.equipDisplay.equipDisplayType == EquipDisplayType.BaseTextureSwap)
                    {
                        ResetBaseTexture(activeDisplay);
                    }
                }
                activeEquipDisplays.Remove(propName);
            }
            if (displayID != null && displayID != "")
            {
                List<EquipmentDisplay> displays = Inventory.Instance.LoadEquipmentDisplay(displayID);
                if (displays == null || displays.Count == 0)
                    return;
                List<ActiveEquipmentDisplay> activeDisplays = new List<ActiveEquipmentDisplay>();
                foreach (EquipmentDisplay display in displays)
                {
                    if (display.equipDisplayType == EquipDisplayType.AttachedObject)
                    {
                        if (display.model == null)
                        {
                            Debug.LogError("AttachObject model is null propName="+ propName+ " displayID=" + displayID);

                        }
                        else
                            activeDisplays.Add(AttachObject(display));
                    }
                    else if (display.equipDisplayType == EquipDisplayType.ActivatedModel)
                    {
                        activeDisplays.Add(ActivateModel(display));
                    }
                    else if (display.equipDisplayType == EquipDisplayType.BaseTextureSwap)
                    {
                        activeDisplays.Add(SwapBaseModelTexture(display));
                    }
                }
                activeEquipDisplays.Add(propName, activeDisplays);
                SetWeaponsAttachmentSlot();
            }
        }

        protected ActiveEquipmentDisplay AttachObject(EquipmentDisplay equipDisplay)
        {


            GameObject weapon = (GameObject)Instantiate(equipDisplay.model, GetSocketTransform(equipDisplay.socket).position,
                                                         GetSocketTransform(equipDisplay.socket).rotation);
            if (equipDisplay.material != null)
            {
                if (weapon.GetComponent<Renderer>() != null)
                    weapon.GetComponent<Renderer>().material = equipDisplay.material;
            }
            weapon.transform.parent = GetSocketTransform(equipDisplay.socket);
            return new ActiveEquipmentDisplay(equipDisplay, weapon, equipDisplay.socket);
        }

        protected ActiveEquipmentDisplay ActivateModel(EquipmentDisplay equipDisplay)
        {
            if (string.IsNullOrEmpty(equipDisplay.modelName) && equipDisplay.model != null)
                equipDisplay.modelName = equipDisplay.model.name;
            Transform newModel = transform.Find(equipDisplay.modelName);
            ActiveEquipmentDisplay activeDisplay = new ActiveEquipmentDisplay(equipDisplay, null, null);
            if (newModel != null)
            {
                newModel.gameObject.SetActive(true);
                if (equipDisplay.material != null)
                {
                    activeDisplay.baseMaterial = newModel.GetComponent<Renderer>().material;
                    newModel.GetComponent<Renderer>().material = equipDisplay.material;
                }
            }
            return activeDisplay;
        }

        protected ActiveEquipmentDisplay SwapBaseModelTexture(EquipmentDisplay equipDisplay)
        {
            if (string.IsNullOrEmpty(equipDisplay.modelName) && equipDisplay.model != null)
                equipDisplay.modelName = equipDisplay.model.name;
            Transform model = transform.Find(equipDisplay.modelName);
            // Store the base material first
            ActiveEquipmentDisplay activeDisplay = new ActiveEquipmentDisplay(equipDisplay, null, model.GetComponent<Renderer>().material);
            model.GetComponent<Renderer>().material = equipDisplay.material;
            return activeDisplay;
        }

        protected void RemoveAttachedObject(ActiveEquipmentDisplay activeDisplay)
        {
            Destroy(activeDisplay.attachedObject);
        }

        protected void DeactivateModel(ActiveEquipmentDisplay activeDisplay)
        {
            if (string.IsNullOrEmpty(activeDisplay.equipDisplay.modelName) && activeDisplay.equipDisplay.model != null)
                activeDisplay.equipDisplay.modelName = activeDisplay.equipDisplay.model.name;
            Transform model = transform.Find(activeDisplay.equipDisplay.modelName);
            if (model != null)
            {
                if (activeDisplay.baseMaterial != null)
                {
                    model.GetComponent<Renderer>().material = activeDisplay.baseMaterial;
                }
                model.gameObject.SetActive(false);
            }
        }

        protected void ResetBaseTexture(ActiveEquipmentDisplay activeDisplay)
        {
            if (string.IsNullOrEmpty(activeDisplay.equipDisplay.modelName) && activeDisplay.equipDisplay.model != null)
                activeDisplay.equipDisplay.modelName = activeDisplay.equipDisplay.model.name;
            Transform model = transform.Find(activeDisplay.equipDisplay.modelName);
            model.GetComponent<Renderer>().material = activeDisplay.baseMaterial;
        }


        public void HandleDeadState(object sender, PropertyChangeEventArgs args)
        {
            dead = (bool)GetComponent<AtavismNode>().GetProperty(args.PropertyName);
        }


        public void SetupSockets(AtavismMobSockets ams)
        {
            // Sockets for attaching weapons (and particles)
            this.mainHand = ams.mainHand;
            this.mainHand2 = ams.mainHand2;
            this.offHand = ams.offHand;
            this.offHand2 = ams.offHand2;
            this.mainHandRest = ams.mainHandRest;
            this.mainHandRest2 = ams.mainHandRest2;
            this.offHandRest = ams.offHandRest;
            this.offHandRest2 = ams.offHandRest2;
            this.shield = ams.shield;
            this.shield2 = ams.shield2;
            this.shieldRest = ams.shieldRest;
            this.shieldRest2 = ams.shieldRest2;
            this.head = ams.head;
            this.leftShoulderSocket = ams.leftShoulderSocket;
            this.rightShoulderSocket = ams.rightShoulderSocket;

            // Sockets for particles
            this.rootSocket = ams.rootSocket;
            this.leftFootSocket = ams.leftFootSocket;
            this.rightFootSocket = ams.rightFootSocket;
            this.pelvisSocket = ams.pelvisSocket;
            this.leftHipSocket = ams.leftHipSocket;
            this.rightHipSocket = ams.rightHipSocket;
            this.chestSocket = ams.chestSocket;
            this.backSocket = ams.backSocket;
            this.neckSocket = ams.neckSocket;
            this.mouthSocket = ams.mouthSocket;
            this.leftEyeSocket = ams.leftEyeSocket;
            this.rightEyeSocket = ams.rightEyeSocket;
            this.overheadSocket = ams.overheadSocket;
            SetWeaponsAttachmentSlot(true);
        }

    }
}
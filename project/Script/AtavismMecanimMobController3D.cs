using UnityEngine;
using System.Collections;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif
using UnityEngine.SceneManagement;


namespace Atavism
{

    public class AtavismMecanimMobController3D : MobController3D
    {

        private Animator _animator;
        string overrideAnimationName;
        float overrideAnimationExpires;
        string overrideIntAnimationName;
        float overrideIntAnimationExpires;
        int overrideIntAnimationAfter;
        string overrideFloatAnimationName;
        float overrideFloatAnimationExpires;
        float overrideFloatAnimationAfter;
        //  float prevRotationAngle = 0f;

        protected string actionState = "";
        string weaponType;
        string weapon2Type;
        [SerializeField]
        protected bool characterNoMove = false;
        float startDelay = 0f;
        float speed = 0f;
        float speed2d = 0f;
        // Use this for initialization
        void Start()
        {
            if (SceneManager.GetActiveScene().name.Equals(ClientAPI.Instance.characterSceneName))
                return;
            _animator = (Animator)GetComponentInChildren(typeof(Animator));
            if (weaponType == null || weaponType == "")
            {
                if (GetComponent<AtavismNode>() != null)
                {
                    GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("weaponType", HandleWeaponType);
                    if (GetComponent<AtavismNode>().PropertyExists("weaponType"))
                    {
                        weaponType = (string)GetComponent<AtavismNode>().GetProperty("weaponType");
                    }
                    GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("weapon2Type", HandleWeapon2Type);
                    if (GetComponent<AtavismNode>().PropertyExists("weapon2Type"))
                    {
                        weapon2Type = (string)GetComponent<AtavismNode>().GetProperty("weapon2Type");
                    }
                }
            }
            if (weaponType == null || weaponType == "")
            {
                if (GetComponent<AtavismNode>() != null)
                {
                    GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("weapon2Type", HandleWeapon2Type);
                    if (GetComponent<AtavismNode>().PropertyExists("weapon2Type"))
                    {
                        weapon2Type = (string)GetComponent<AtavismNode>().GetProperty("weapon2Type");
                    }
                }
            }
            startDelay = Time.time + 2f;
        }

        protected override void ObjectNodeReady()
        {
            base.ObjectNodeReady();
            GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("weaponType", HandleWeaponType);
            // Get weaponType property
            if (GetComponent<AtavismNode>().PropertyExists("weaponType"))
            {
                weaponType = (string)GetComponent<AtavismNode>().GetProperty("weaponType");
                /*if (weaponType != null && weaponType != "" && weaponType != "Unarmed") {
                    _animator.SetBool (weaponType, true);
                }*/
            }
            GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("weapon2Type", HandleWeapon2Type);
            // Get weaponType property
            if (GetComponent<AtavismNode>().PropertyExists("weapon2Type"))
            {
                weapon2Type = (string)GetComponent<AtavismNode>().GetProperty("weapon2Type");

            }
        }

        void OnDestroy()
        {
            AtavismNode aNode = GetComponent<AtavismNode>();
            if (aNode != null)
            {
                aNode.RemoveObjectPropertyChangeHandler("weaponType", HandleWeaponType);
                aNode.RemoveObjectPropertyChangeHandler("weapon2Type", HandleWeapon2Type);
                aNode.RemoveObjectPropertyChangeHandler("deadstate", HandleDeadState);
                aNode.RemoveObjectPropertyChangeHandler("combatstate", HandleCombatState);
                aNode.RemoveObjectPropertyChangeHandler("movement_state", MovementStateHandler);
                aNode.RemoveObjectPropertyChangeHandler("mount", HandleMount);
                aNode.RemoveObjectPropertyChangeHandler("waterHeight", HandleWaterHeight);
                aNode.RemoveObjectPropertyChangeHandler("state", HandleState);
                aNode.RemoveObjectPropertyChangeHandler("aggressive", HandleAggressive);

            }
            if (mount != null)
                Destroy(mount);
        }
      //  CharacterController controller;
        // Update is called once per frame
        void Update()
        {
            if (SceneManager.GetActiveScene().name.Equals(ClientAPI.Instance.characterSceneName))
                return;
            Profiler.BeginSample("AMMC3D Update");
            Profiler.BeginSample("AMMC3D Update DoMovement");
            if (!characterNoMove || (characterNoMove && groundDistance > 0.07f) || (startDelay > Time.time))
            {
                base.DoMovement();
            }
            Profiler.EndSample();
            Profiler.BeginSample("AMMC3D Update 2");
            if (controller == null)
                controller = GetComponent<CharacterController>();
            if (controller == null)
                return;
            if (_animator == null)
            {
                _animator = (Animator)GetComponentInChildren(typeof(Animator));
            }
            // Debug.Log("Using animator for mob: " + name);
            Profiler.BeginSample("AMMC3D Update Animator");
            if (_animator && (dead && state != "spirit"))
            {
                _animator.SetBool("Dead", true);
                _animator.SetBool("SpecialAttack2", false);
                _animator.SetBool("Attack", false);
            }
            else if (_animator)
            {
                 speed2d = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
                _animator.SetInteger("MovementState", movementState);
                _animator.SetBool("Dead", false);
                if (pathInterpolator != null)
                {
                    _animator.SetFloat("Speed", pathInterpolator.Speed);
                }
                else
                {
                    _animator.SetFloat("Speed", controller.velocity.magnitude);
                    speed = controller.velocity.magnitude;
                }
                
               // if (_animator.GetFloat("Speed2D") != null)
                    _animator.SetFloat("Speed2D", new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude);
               //  speed2d = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
               // if (_animator.GetFloat("GroundDistance") != null)
                    _animator.SetFloat("GroundDistance", groundDistance);
                Profiler.BeginSample("AMMC3D Update Diraction");
                if (controller.velocity.magnitude > 0.1f && rotatingDirection == 0)
                {
                    /*
                        // Direction setting if you want to use it
                        float dot = Vector3.Dot(transform.forward, movement.normalized);
                        //Debug.Log("dot:" + dot);
                        if(dot > 0.8) {
                            // going forward direction
                            _animator.SetFloat ("Direction", 0);
                        } else if (dot < - 0.8) { 
                            // going backwards
                            _animator.SetFloat ("Direction", 45);
                        } else {
                            Vector3 cross = Vector3.Cross(transform.forward, movement.normalized);
                            //Debug.Log("cross:" + cross);
                            if(cross.y < 0) {
                                // going left 
                                _animator.SetFloat ("Direction", 225);
                            } else {
                                // going right 
                                _animator.SetFloat ("Direction", 135);
                            }
                        }
                        */
                    float dot = Vector3.Dot(transform.forward, movement.normalized);
                    Vector3 cross = Vector3.Cross(transform.forward, movement.normalized);
                    if (dot > 0.8 && cross.y < 0.28 && cross.y > -0.28)
                    {//forward
                        _animator.SetFloat("Direction", 0);
                    }
                    else if (dot > 0.3 && (cross.y > 0.28 || cross.y < -0.28))
                    {//forward Right|Left
                        _animator.SetFloat("Direction", cross.y < 0 ? 45 : -45);
                    }
                    else if (dot < 0.3 && dot > -0.3 && (cross.y > 0.28 || cross.y < -0.28))
                    {//Right|Left
                        _animator.SetFloat("Direction", cross.y < 0 ? 90 : -90);
                    }
                    else if (dot < -0.3 && (cross.y > 0.28 || cross.y < -0.28))
                    {//Backwards Right|Left
                        _animator.SetFloat("Direction", cross.y < 0 ? 135 : -135);
                    }
                    else if (dot < -0.3 && cross.y < 0.28 && cross.y > -0.28)
                    {//Backwards
                        _animator.SetFloat("Direction", 180);
                    }
                }
                else
                {
                    float dot = Vector3.Dot(transform.forward, movement.normalized);
                    _animator.SetFloat("Direction", dot < -0.4 ? 180 : 0);
                    //_animator.SetFloat ("Direction", 0);
                    _animator.SetFloat("RotatingDirection", rotatingDirection);
                }
                Profiler.EndSample();

                if (mount != null)
                {
                    _animator.SetBool(mountAnim, true);
                }
                else
                {
                    _animator.SetBool(mountAnim, false);
                }
                if (jumping)
                {
                    _animator.SetBool("Jump", true);
                }
                else
                {
                    _animator.SetBool("Jump", false);
                }
                _animator.SetBool("Combat", inCombat);
                if (weaponType != null && weaponType != "" && weaponType != "Unarmed")
                {
                    foreach (AnimatorControllerParameter param in _animator.parameters)
                    {
                        if (param.name == weaponType)
                            _animator.SetBool(weaponType, true);
                    }
                }
                if (weapon2Type != null && weapon2Type != "" && weapon2Type != "Unarmed")
                {
                    foreach (AnimatorControllerParameter param in _animator.parameters)
                    {
                        if (param.name == weapon2Type)
                            _animator.SetBool(weapon2Type, true);
                    }
                }
                if (_animator.GetBool("IsGrounded"))
                    _animator.SetBool("IsGrounded", IsGrounded());
                if (overrideAnimationName == "waving")
                {
                    if (Time.time > overrideAnimationExpires)
                    {
                        _animator.SetBool("Waving", false);
                        overrideAnimationName = "";
                    }
                    else
                    {
                        _animator.SetBool("Waving", true);
                    }
                }
                else if (overrideAnimationName == "mining")
                {
                    if (Time.time > overrideAnimationExpires)
                    {
                        _animator.SetBool("Mining", false);
                        overrideAnimationName = "";
                    }
                    else
                    {
                        _animator.SetBool("Mining", true);
                    }
                }
                else if (overrideAnimationName == "attack_normal")
                {
                    if (Time.time > overrideAnimationExpires)
                    {
                        _animator.SetBool("Attack", false);
                        overrideAnimationName = "";
                    }
                    else
                    {
                        _animator.SetBool("Wound", false);
                        _animator.SetBool("SpecialAttack2", false);
                        _animator.SetBool("Attack", true);
                    }
                }
                else if (overrideAnimationName == "attack_special")
                {
                    if (Time.time > overrideAnimationExpires)
                    {
                        _animator.SetBool("SpecialAttack", false);
                        overrideAnimationName = "";
                    }
                    else
                    {
                        _animator.SetBool("SpecialAttack", true);
                    }
                }
                else if (overrideAnimationName == "attack_special2")
                {
                    if (Time.time > overrideAnimationExpires)
                    {
                        _animator.SetBool("SpecialAttack2", false);
                        overrideAnimationName = "";
                    }
                    else
                    {
                        _animator.SetBool("Wound", false);
                        _animator.SetBool("Attack", false);
                        _animator.SetBool("SpecialAttack2", true);
                    }
                }
                else if (overrideAnimationName == "wound")
                {
                    if (Time.time > overrideAnimationExpires)
                    {
                        _animator.SetBool("Wound", false);
                        overrideAnimationName = "";
                    }
                    else
                    {
                        _animator.SetBool("Wound", true);
                    }
                }
                else if (overrideAnimationName != null && overrideAnimationName != "")
                {
                    if (Time.time > overrideAnimationExpires)
                    {
                        _animator.SetBool(overrideAnimationName, false);
                        overrideAnimationName = "";
                    }
                    else
                    {
                        _animator.SetBool(overrideAnimationName, true);
                    }
                }
                else if (actionState != "")
                {
                    _animator.SetBool(actionState, true);
                }
                if (overrideIntAnimationName != null && overrideIntAnimationName != "")
                {
                    if (Time.time > overrideIntAnimationExpires)
                    {
                        _animator.SetInteger(overrideIntAnimationName, overrideIntAnimationAfter);
                        overrideIntAnimationName = "";
                    }
                }
                if (overrideFloatAnimationName != null && overrideFloatAnimationName != "")
                {
                    if (Time.time > overrideFloatAnimationExpires)
                    {
                        _animator.SetFloat(overrideFloatAnimationName, overrideFloatAnimationAfter);
                        overrideFloatAnimationName = "";
                    }
                }

            }
            Profiler.EndSample();
            Profiler.EndSample();
            Profiler.EndSample();
        }

        public void HandleWeaponType(object sender, PropertyChangeEventArgs args)
        {
            if (weaponType != null && weaponType != "" && weaponType != "Unarmed")
            {
                _animator.SetBool(weaponType, false);
            }
            weaponType = (string)AtavismClient.Instance.WorldManager.GetObjectNode(oid).GetProperty("weaponType");
            if (_animator != null && weaponType != "" && weaponType != "Unarmed")
            {
                //        Debug.LogError("setting combat state for animator weaponType:" + weaponType);
                _animator.SetBool(weaponType, true);
            }
        }

        public void HandleWeapon2Type(object sender, PropertyChangeEventArgs args)
        {
            if (weapon2Type != null && weapon2Type != "" && weapon2Type != "Unarmed")
            {
                _animator.SetBool(weapon2Type, false);
            }
            weapon2Type = (string)AtavismClient.Instance.WorldManager.GetObjectNode(oid).GetProperty("weapon2Type");
            if (_animator != null && weapon2Type != "" && weapon2Type != "Unarmed")
            {
                //  Debug.LogError("setting combat state for animator weapon2Type:" + weapon2Type);
                _animator.SetBool(weapon2Type, true);
            }
        }


        public void ActionStateHandler(object sender, PropertyChangeEventArgs args)
        {
            AtavismLogger.LogDebugMessage("Got actionstate");
            //	AtavismObjectNode node = (AtavismObjectNode)sender;
            string newState = (string)GetComponent<AtavismNode>().GetProperty(args.PropertyName);
            if (_animator != null && newState != actionState)
            {
                AtavismLogger.LogDebugMessage("clearing old actionstate");
                _animator.SetBool(actionState, false);
            }
            actionState = newState;
        }

        public override void PlayMeleeAttackAnimation(string attackType, string result)
        {
            if (attackType == "normal")
            {
                overrideAnimationName = "attack_normal";
            }
            else if (attackType == "special")
            {
                overrideAnimationName = "attack_special";
            }
            else if (attackType == "special2")
            {
                overrideAnimationName = "attack_special2";
            }
            overrideAnimationExpires = Time.time + 1.0f; //overrideAnimation.length;
        }

        public override void PlayMeleeRecoilAnimation(string result)
        {
            overrideAnimationName = "wound";
            overrideAnimationExpires = Time.time + 0.5f;
        }

        public override void PlayAnimation(string animationName, float length)
        {
            //   Debug.LogWarning("PlayAnimation: >" + animationName + "< length:" + length);
            if (_animator != null && overrideAnimationName != null && overrideAnimationName != "")
            {
                AtavismLogger.LogDebugMessage("clearing old animation");
                _animator.SetBool(overrideAnimationName, false);
            }
            overrideAnimationName = animationName;
            overrideAnimationExpires = Time.time + length;
        }

        public override void PlayAnimationInt(string animationName, int value, float length, int valueAfter)
        {
            if (_animator != null && animationName != null && animationName != "")
            {
                _animator.SetInteger(animationName, value);
                overrideIntAnimationName = animationName;
                overrideIntAnimationExpires = Time.time + length;
                overrideIntAnimationAfter = valueAfter;

            }
        }


        public override void PlayAnimationTrigger(string animNameTrigger)
        {
            if (_animator != null && animNameTrigger != null && animNameTrigger != "")
            {
                _animator.SetTrigger(animNameTrigger);
            }
        }


        public override void PlayAnimationFloat(string animNameParamFloat, float value, float length, float valueAfter)
        {
            if (_animator != null && animNameParamFloat != null && animNameParamFloat != "")
            {
                _animator.SetFloat(animNameParamFloat, value);
                overrideFloatAnimationName = animNameParamFloat;
                overrideFloatAnimationExpires = Time.time + length;
                overrideFloatAnimationAfter = valueAfter;
            }
        }



    }
}
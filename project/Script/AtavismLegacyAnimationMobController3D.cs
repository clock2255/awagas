using UnityEngine;
using System.Collections;

namespace Atavism
{

    public class AtavismLegacyAnimationMobController3D : MobController3D
    {

        public AnimationClip idleAnimation;
        public AnimationClip walkAnimation;
        public AnimationClip runAnimation;
        public AnimationClip jumpPoseAnimation;
        public AnimationClip combatIdleAnimation;
        public AnimationClip unarmedAttackedAnimation;
        public AnimationClip specialAttack;
        public AnimationClip deathAnimation;
        public AnimationClip swimIdleAnimation;
        public AnimationClip swimAnimation;
        public AnimationClip mountAnimation;
        public AnimationClip waveAnimation;
        public AnimationClip lieAnimation;

        public float walkMaxAnimationSpeed = 0.75f;
        public float trotMaxAnimationSpeed = 1.0f;
        public float runMaxAnimationSpeed = 1.0f;
        public float jumpAnimationSpeed = 1.15f;
        public float landAnimationSpeed = 1.0f;
        private Animation _animation;

        AnimationClip overrideAnimation;
        float overrideAnimationExpires;

        AnimationClip currentAnimation = null;

        // Use this for initialization
        void Start()
        {
            _animation = (Animation)GetComponent("Animation");
        }

        protected override void ObjectNodeReady()
        {
            base.ObjectNodeReady();

            GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("currentAnim", HandleCurrentAnim);
            // If the currentAnim property already exists, run the handler now
            if (GetComponent<AtavismNode>().PropertyExists("currentAnim"))
            {
                HandleCurrentAnim(null, null);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_animation == null)
            {
                _animation = (Animation)GetComponentInChildren<Animation>();
            }
            base.DoMovement();
            if (_animation == null)
                return;
            CharacterController controller = GetComponent<CharacterController>();
            //Debug.Log("Using animation for mob: " + name);
            if (dead && state != "spirit")
            {
                if (currentAnimation != deathAnimation)
                    _animation.CrossFade(deathAnimation.name);
                currentAnimation = deathAnimation;
            }
            else if (mount != null && mountAnimation != null)
            {
                _animation.CrossFade(mountAnimation.name);
            }
            else if (jumping)
            {
                if (!jumpingReachedApex)
                {
                    _animation[jumpPoseAnimation.name].speed = jumpAnimationSpeed;
                    _animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
                    _animation.CrossFade(jumpPoseAnimation.name);
                }
                else
                {
                    _animation[jumpPoseAnimation.name].speed = -landAnimationSpeed;
                    _animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
                    _animation.CrossFade(jumpPoseAnimation.name);
                }
            }
            else if (movementState == MOVEMENT_STATE_SWIMMING)
            {
                if (controller.velocity.sqrMagnitude > 0.1)
                {
                    _animation[swimAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f, runMaxAnimationSpeed);
                    _animation.CrossFade(swimAnimation.name);
                }
                else
                {
                    _animation.CrossFade(swimIdleAnimation.name);
                }
            }
            else
            {
                if (controller.velocity.sqrMagnitude > 0.1)
                {
                    if (controller.velocity.magnitude > runThreshold)
                    {
                        _animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f, runMaxAnimationSpeed);
                        _animation.CrossFade(runAnimation.name);
                    }
                    else
                    {
                        _animation[walkAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0.0f, walkMaxAnimationSpeed);
                        _animation.CrossFade(walkAnimation.name);
                    }
                }
                else if (overrideAnimation != null)
                {
                    //_animation [overrideAnimation.name].speed = Mathf.Clamp (controller.velocity.magnitude, 0.0f, runMaxAnimationSpeed);
                    _animation.CrossFade(overrideAnimation.name);
                    if (Time.time > overrideAnimationExpires)
                    {
                        overrideAnimation = null;
                    }
                }
                else
                {
                    _animation.CrossFade(idleAnimation.name);
                    currentAnimation = _animation.clip;
                }
            }


            if (isPlayer && controller.velocity.sqrMagnitude > 0.1 && ClientAPI.GetPlayerObject().PropertyExists("currentAnim"))
            {
                if ((string)ClientAPI.GetPlayerObject().GetProperty("currentAnim") != "null")
                    NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/setStringProperty currentAnim null");
            }

        }

        public override void PlayMeleeAttackAnimation(string attackType, string result)
        {
            overrideAnimation = unarmedAttackedAnimation;
            overrideAnimationExpires = Time.time + 1.0f; //overrideAnimation.length;
        }

        public override void PlayMeleeRecoilAnimation(string result)
        {
            //overrideAnimationName = "wound";
            overrideAnimationExpires = Time.time + 0.5f;
        }

        public override void PlayAnimation(string animationName, float length)
        {
            overrideAnimationExpires = Time.time + length;
            //	if (GetComponent<Animation>()) {
            if (animationName == "attack_normal" || animationName == "Attack")
            {
                overrideAnimation = unarmedAttackedAnimation;
            }
            else if (animationName == "Waving")
            {
                overrideAnimation = waveAnimation;
            }
            else if (animationName == "special_attack")
            {
                overrideAnimation = specialAttack;
            }
            //	}
        }
        public override void PlayAnimationInt(string animationName, int value, float length, int valueAfter)
        {
            AtavismLogger.LogError("Was Run function PlayAnimationInt on Legacy Controller :" + animationName);
        }

        public override void PlayAnimationTrigger(string animNameTrigger)
        {
            AtavismLogger.LogError("Was Run function PlayAnimationTrigger on Legacy Controller :" + animNameTrigger);
        }

        public override void PlayAnimationFloat(string animNameParamFloat, float value, float length, float valueAfter)
        {
            AtavismLogger.LogError("Was Run function PlayAnimationFloat on Legacy Controller :" + animNameParamFloat);
        }

        public void HandleCurrentAnim(object sender, PropertyChangeEventArgs args)
        {
            // Get the value of the currentAnim property
            string currentAnim = (string)AtavismClient.Instance.WorldManager.GetObjectNode(oid).GetProperty("currentAnim");
            // Check if the value is "null", if so, set the overrideAnimationExpires time to -1 to reset it
            if (currentAnim == "null")
            {
                overrideAnimationExpires = -1;
            }
            else
            {
                // Otherwise call the PlayAnimation function with a very large amount of time
                PlayAnimation(currentAnim, float.MaxValue);
            }
        }
    }
}
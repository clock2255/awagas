using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Atavism
{
    public class AtavismMobSockets : MonoBehaviour
    {
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
    }
}
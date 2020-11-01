using UnityEngine;
using System.Collections;

namespace Atavism
{
    public enum EquipDisplayType
    {
        AttachedObject,
        ActivatedModel,
        BaseTextureSwap
    }

    public class EquipmentDisplay : MonoBehaviour
    {

        int id;
        public EquipDisplayType equipDisplayType;
        public string modelName;
        public GameObject model;
        public Material material;
        public AttachmentSocket socket = AttachmentSocket.None;
        public AttachmentSocket restSocket = AttachmentSocket.None;
        public Vector3 restRotation = Vector3.zero;
        public Vector3 restPosition = Vector3.zero;
    }
}
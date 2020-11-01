using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if AT_PPS2_PRESET
using UnityEngine.Rendering.PostProcessing;
#endif

namespace Atavism
{
    public class AtavismPostProcessing : MonoBehaviour
    {
#if AT_PPS2_PRESET
    public List<PostProcessVolume> volumes = new List<PostProcessVolume>();// Use this for initialization
    void Start () {
        AtavismSettings.Instance.PostProcessVolumes = volumes;
    }
	
#endif
    }

}
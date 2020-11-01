using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if AT_MASTERAUDIO_PRESET
using DarkTonic.MasterAudio;
#endif

namespace Atavism
{
    public class AtavismAudioPlay : MonoBehaviour
    {
        public void PlayAudioClip(AudioClip clip, Transform pos)
        {
#if AT_MASTERAUDIO_PRESET
    MasterAudio.PlaySound3DAtTransformAndForget(clip.name, pos, 1f);
#endif

        }
    }
}
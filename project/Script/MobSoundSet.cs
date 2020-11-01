using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if AT_MASTERAUDIO_PRESET
using DarkTonic.MasterAudio;
#endif
namespace Atavism
{

    public enum MobSoundEvent
    {
        Response,
        Death,
        Aggro,
        Attack,
        Jump,
    }

    public class MobSoundSet : MonoBehaviour
    {

        public List<AudioClip> responseSound;
        public List<AudioClip> responseSoundFemale;
        public List<AudioClip> deathSound;
        public List<AudioClip> deathSoundFemale;
        public List<AudioClip> aggroSound;
        public List<AudioClip> aggroSoundFemale;
        public List<AudioClip> attackSound;
        public List<AudioClip> attackSoundFemale;
        public List<AudioClip> jumpSound;
        public List<AudioClip> jumpSoundFemale;
        public List<string> responseSoundName;
        public List<string> responseSoundFemaleName;
        public List<string> deathSoundName;
        public List<string> deathSoundFemaleName;
        public List<string> aggroSoundName;
        public List<string> aggroSoundFemaleName;
        public List<string> attackSoundName;
        public List<string> attackSoundFemaleName;
        public List<string> jumpSoundName;
        public List<string> jumpSoundFemaleName;
        public string mixerGroupName = "SFX";
        public int maxDistance = 100;
        //  Random rand = new Random();

        // Use this for initialization
        void Start()
        {

        }

        void ObjectNodeReady()
        {
            AtavismNode aNode = GetComponent<AtavismNode>();
            if (aNode == null && transform != null && transform.parent != null)
                aNode = transform.parent.GetComponent<AtavismNode>();
            if (aNode != null)
            {
                aNode.RegisterObjectPropertyChangeHandler("deadstate", HandleDeadState);
            }
        }

        void OnDestroy()
        {
            AtavismNode aNode = GetComponent<AtavismNode>();
            if (aNode == null && transform != null && transform.parent != null)
                aNode = transform.parent.GetComponent<AtavismNode>();
            if (aNode != null)
            {
                aNode.RemoveObjectPropertyChangeHandler("deadstate", HandleDeadState);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PlaySoundEvent(MobSoundEvent soundEvent)
        {
            // Play sound clip on the mob
            AtavismMobAppearance ama = GetComponent<AtavismMobAppearance>();
            if (ama == null && transform != null && transform.parent != null)
                ama = transform.parent.GetComponent<AtavismMobAppearance>();
            Transform slotTransform = ama.GetSocketTransform(AttachmentSocket.Root);
            GameObject soundObject = new GameObject();
            soundObject.transform.position = slotTransform.position;
            soundObject.transform.parent = slotTransform;
            float duration = 2;
            AtavismNode an = GetComponent<AtavismNode>();
            if (an == null && transform != null && transform.parent != null)
                an = transform.parent.GetComponent<AtavismNode>();
            AtavismObjectNode aNode = ClientAPI.WorldManager.GetObjectNode(an.Oid);

            if (soundEvent == MobSoundEvent.Aggro)
            {
                if (aggroSound.Count > 0 || aggroSoundFemale.Count > 0)
                {
                    AudioSource audioSource = soundObject.AddComponent<AudioSource>();
                    if (aNode.Properties.ContainsKey("gender") && aNode.Properties["gender"].Equals("Female") && aggroSoundFemale.Count > 0)
                        audioSource.clip = aggroSoundFemale[(int)Random.Range(0, aggroSoundFemale.Count)];
                    else if (aggroSound.Count > 0)
                        audioSource.clip = aggroSound[(int)Random.Range(0, aggroSound.Count)];
                    audioSource.spatialBlend = 1.0f;
                    audioSource.rolloffMode = AudioRolloffMode.Linear;
                    audioSource.maxDistance = maxDistance;
                    audioSource.volume = 1f;// SoundSystem.SoundEffectVolume;
                    if (AtavismSettings.Instance.masterMixer != null)
                        audioSource.outputAudioMixerGroup = AtavismSettings.Instance.masterMixer.FindMatchingGroups(mixerGroupName)[0];
                    audioSource.Play();
                    if (audioSource.clip != null)
                        duration = audioSource.clip.length + 1f;
                    else
                        duration = 1f;
                }
#if AT_MASTERAUDIO_PRESET
            if (aNode.Properties.ContainsKey("gender") && aNode.Properties["gender"].Equals("Female")) {
                if (aggroSoundFemaleName.Count > 0) {
                    MasterAudio.PlaySound3DAtTransform(aggroSoundFemaleName[Random.Range(0, aggroSoundFemaleName.Count)], slotTransform, 1f);
                }
            } else if (aggroSoundName.Count > 0) {
                MasterAudio.PlaySound3DAtTransform(aggroSoundName[Random.Range(0, aggroSoundName.Count)], slotTransform, 1f);
            }
#endif
            }
            else if (soundEvent == MobSoundEvent.Attack)
            {
                if (attackSound.Count > 0 || attackSoundFemale.Count > 0)
                {
                    AudioSource audioSource = soundObject.AddComponent<AudioSource>();
                    if (aNode.Properties.ContainsKey("gender") && aNode.Properties["gender"].Equals("Female") && attackSoundFemale.Count > 0)
                        audioSource.clip = attackSoundFemale[(int)Random.Range(0, attackSoundFemale.Count)];
                    else if (attackSound.Count > 0)
                        audioSource.clip = attackSound[(int)Random.Range(0, attackSound.Count)];
                    audioSource.spatialBlend = 1.0f;
                    audioSource.rolloffMode = AudioRolloffMode.Linear;
                    audioSource.maxDistance = maxDistance;
                    audioSource.volume = 1f;//SoundSystem.SoundEffectVolume;
                    if (AtavismSettings.Instance.masterMixer != null)
                        audioSource.outputAudioMixerGroup = AtavismSettings.Instance.masterMixer.FindMatchingGroups(mixerGroupName)[0];
                    audioSource.Play();
                    if (audioSource.clip != null)
                        duration = audioSource.clip.length + 1f;
                    else
                        duration = 1f;
                }
#if AT_MASTERAUDIO_PRESET
            if (aNode.Properties.ContainsKey("gender") && aNode.Properties["gender"].Equals("Female")) {
                if (attackSoundFemaleName.Count > 0) {
                    MasterAudio.PlaySound3DAtTransform(attackSoundFemaleName[Random.Range(0, attackSoundFemaleName.Count)], slotTransform, 1f);
                }
            } else if (attackSoundName.Count > 0) {
                MasterAudio.PlaySound3DAtTransform(attackSoundName[Random.Range(0, attackSoundName.Count)], slotTransform, 1f);
            }
#endif
            }
            else if (soundEvent == MobSoundEvent.Death)
            {
                if (deathSound.Count > 0 || deathSoundFemale.Count > 0)
                {
                    AudioSource audioSource = soundObject.AddComponent<AudioSource>();
                    if (aNode.Properties.ContainsKey("gender") && aNode.Properties["gender"].Equals("Female") && deathSoundFemale.Count > 0)
                        audioSource.clip = deathSoundFemale[(int)Random.Range(0, deathSoundFemale.Count)];
                    else if (deathSound.Count > 0)
                        audioSource.clip = deathSound[(int)Random.Range(0, deathSound.Count)];
                    audioSource.spatialBlend = 1.0f;
                    audioSource.rolloffMode = AudioRolloffMode.Linear;
                    audioSource.maxDistance = maxDistance;
                    audioSource.volume = 1f;//SoundSystem.SoundEffectVolume;
                    if (AtavismSettings.Instance.masterMixer != null)
                        audioSource.outputAudioMixerGroup = AtavismSettings.Instance.masterMixer.FindMatchingGroups(mixerGroupName)[0];
                    audioSource.Play();
                    if (audioSource.clip != null)
                        duration = audioSource.clip.length + 1f;
                    else
                        duration = 1f;
                }
#if AT_MASTERAUDIO_PRESET
            if (aNode.Properties.ContainsKey("gender") && aNode.Properties["gender"].Equals("Female")) {
                if (deathSoundFemaleName.Count > 0) {
                    MasterAudio.PlaySound3DAtTransform(deathSoundFemaleName[Random.Range(0, deathSoundFemaleName.Count)], slotTransform, 1f);
                }
            } else if (deathSoundName.Count > 0) {
                MasterAudio.PlaySound3DAtTransform(deathSoundName[Random.Range(0, deathSoundName.Count)], slotTransform, 1f);
            }
#endif
            }
            else if (soundEvent == MobSoundEvent.Response)
            {
                if (responseSound.Count > 0 || responseSoundFemale.Count > 0)
                {
                    AudioSource audioSource = soundObject.AddComponent<AudioSource>();
                    //int soundChoice = Random.Range(0, responseSound.Count);
                    if (aNode.Properties.ContainsKey("gender") && aNode.Properties["gender"].Equals("Female") && responseSoundFemale.Count > 0)
                        audioSource.clip = responseSoundFemale[(int)Random.Range(0, responseSoundFemale.Count)];
                    else if (responseSound.Count > 0)
                        audioSource.clip = responseSound[(int)Random.Range(0, responseSound.Count)];
                    audioSource.spatialBlend = 1.0f;
                    audioSource.rolloffMode = AudioRolloffMode.Linear;
                    audioSource.maxDistance = maxDistance;
                    audioSource.volume = 1f;// SoundSystem.SoundEffectVolume;
                    if (AtavismSettings.Instance.masterMixer != null)
                        audioSource.outputAudioMixerGroup = AtavismSettings.Instance.masterMixer.FindMatchingGroups(mixerGroupName)[0];
                    audioSource.Play();
                    if (audioSource.clip != null)
                        duration = audioSource.clip.length + 1f;
                    else
                        duration = 1f;
                }
#if AT_MASTERAUDIO_PRESET
            if (aNode.Properties.ContainsKey("gender") && aNode.Properties["gender"].Equals("Female")) {
                if (responseSoundFemaleName.Count > 0) {
                    MasterAudio.PlaySound3DAtTransform(responseSoundFemaleName[Random.Range(0, responseSoundFemaleName.Count)], slotTransform, 1f);
                }
            } else if (responseSoundName.Count > 0) {
                MasterAudio.PlaySound3DAtTransform(responseSoundName[Random.Range(0, responseSoundName.Count)], slotTransform, 1f);
            }
#endif
            }
            else if (soundEvent == MobSoundEvent.Jump)
            {
                if (jumpSound.Count > 0 || jumpSoundFemale.Count > 0)
                {
                    AudioSource audioSource = soundObject.AddComponent<AudioSource>();

                    if (aNode.Properties.ContainsKey("gender") && aNode.Properties["gender"].Equals("Female") && jumpSoundFemale.Count > 0)
                        audioSource.clip = jumpSoundFemale[(int)Random.Range(0, jumpSoundFemale.Count)];
                    else if (jumpSound.Count > 0)
                        audioSource.clip = jumpSound[(int)Random.Range(0, jumpSound.Count)];
                    audioSource.spatialBlend = 1.0f;
                    audioSource.rolloffMode = AudioRolloffMode.Linear;
                    audioSource.maxDistance = maxDistance;
                    audioSource.volume = 1f;// SoundSystem.SoundEffectVolume;
                    if (AtavismSettings.Instance.masterMixer != null)
                        audioSource.outputAudioMixerGroup = AtavismSettings.Instance.masterMixer.FindMatchingGroups(mixerGroupName)[0];
                    audioSource.Play();

                    if (audioSource.clip != null)
                        duration = audioSource.clip.length + 1f;
                    else
                        duration = 1f;
                }
#if AT_MASTERAUDIO_PRESET
            if (aNode.Properties.ContainsKey("gender") && aNode.Properties["gender"].Equals("Female")) {
                if (jumpSoundFemaleName.Count > 0) {
                   int soundChoice = Random.Range(0, jumpSoundFemaleName.Count);
                    MasterAudio.PlaySound3DAtTransform(jumpSoundFemaleName[soundChoice], slotTransform, 1f);
                }
            } else if (jumpSoundName.Count > 0) {
                MasterAudio.PlaySound3DAtTransform(jumpSoundName[Random.Range(0, jumpSoundName.Count)], slotTransform, 1f);
            }
#endif
            }
            Destroy(soundObject, duration);
        }

        public void HandleDeadState(object sender, PropertyChangeEventArgs args)
        {
            //Debug.Log ("Got dead update: " + oid);
            AtavismNode an = GetComponent<AtavismNode>();
            if (an == null && transform != null && transform.parent != null)
                an = transform.parent.GetComponent<AtavismNode>();
            long oid = an.Oid;
            bool dead = (bool)AtavismClient.Instance.WorldManager.GetObjectNode(oid).GetProperty("deadstate");
            if (dead)
            {
                PlaySoundEvent(MobSoundEvent.Death);
            }
        }
        public void DidJump()
        {
            PlaySoundEvent(MobSoundEvent.Jump);
        }
    }

}
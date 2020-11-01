using UnityEngine;
using System.Collections;

namespace Atavism
{
    public class SoundSystem
    {

        static float masterVolume;
        static float musicVolume;
        static float soundEffectVolume;
        static AudioSource music;

        // Use this for initialization
        public static void LoadSoundSettings()
        {
            if (PlayerPrefs.HasKey("music_volume"))
            {
                musicVolume = PlayerPrefs.GetFloat("music_volume");
            }
            else
            {
                musicVolume = 0.5f;
                PlayerPrefs.SetFloat("music_volume", musicVolume);
            }
            if (PlayerPrefs.HasKey("sound_volume"))
            {
                soundEffectVolume = PlayerPrefs.GetFloat("sound_volume");
            }
            else
            {
                soundEffectVolume = 0.5f;
                PlayerPrefs.SetFloat("sound_volume", soundEffectVolume);
            }
        }

        public static void SaveSoundSettings()
        {
            PlayerPrefs.SetFloat("music_volume", musicVolume);
            PlayerPrefs.SetFloat("sound_volume", soundEffectVolume);
            PlayerPrefs.Save();
        }

        public static void ResetSoundSettings()
        {
            musicVolume = PlayerPrefs.GetFloat("music_volume");
            soundEffectVolume = PlayerPrefs.GetFloat("sound_volume");
        }

        public static void PlayMusic(AudioSource musicSource)
        {
            if (music != null)
                music.Stop();
            if (musicSource == null)
                return;
            music = musicSource;
            music.volume = musicVolume;
            music.Play();
        }

        public static void PlaySound(AudioSource soundSource)
        {
            if (soundSource == null)
                return;
            soundSource.volume = soundEffectVolume;
            soundSource.Play();
        }

        public static float MusicVolume
        {
            get
            {
                return musicVolume;
            }
            set
            {
                if (value != musicVolume)
                {
                    musicVolume = value;
                    if (music != null)
                        music.volume = musicVolume;

                    string[] args = new string[2];
                    AtavismEventSystem.DispatchEvent("MUSIC_VOLUME_ADJUSTED", args);
                }
            }
        }

        public static float SoundEffectVolume
        {
            get
            {
                return soundEffectVolume;
            }
            set
            {
                soundEffectVolume = value;
            }
        }
    }
}
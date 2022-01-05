using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagnumFoundation2.System.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class s_soundmanager : s_singleton<s_soundmanager>
    {
        public static s_soundmanager sound;
        public AudioSource src;
        public AudioSource music;
        public List<AudioClip> clips;
        public float soundVolume = 1;

        void Start()
        {
            src = GetComponent<AudioSource>();
            sound = GetComponent<s_soundmanager>();
        }

        public AudioClip GetClip(string sound) {
            return clips.Find(x => x.name == sound);
        }

        public void PlaySound(string sound)
        {
            if (sound == null)
                return;
            src.PlayOneShot(clips.Find(x => x.name == sound));
        }
        public void PlaySound(AudioClip sound)
        {
            if (sound == null)
                return;
            src.PlayOneShot(sound);
        }
        public void PlaySound(ref AudioClip sound, float volume, bool isBGM)
        {
            if (sound == null)
                return;
            if (!isBGM)
            {
                src.PlayOneShot(sound, volume);
            }
        }

        public void Update()
        {
        }

    }
}
